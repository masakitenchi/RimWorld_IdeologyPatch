using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class WorldObject_ICBMfission : WorldObject_Missile
	{
		private MapParent mapParent;

		private StringBuilder sb = new StringBuilder();

		public override void Arrived()
		{
			DubUtils.GetResearch().GatherData("ICBMfission", 50f);
			try
			{
				NukemRico();
			}
			catch (Exception ex)
			{
				Log.Warning("Something went wrong during nuke blast\n" + ex);
			}
			Find.WorldObjects.Remove(this);
		}

		public void MakeFallout(int tile)
		{
			if (TileFinder.TryFindPassableTileWithTraversalDistance(tile, 1, 2, out var result))
			{
				WorldObject_Fallout worldObject_Fallout = (WorldObject_Fallout)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("Fallout"));
				worldObject_Fallout.Tile = tile;
				worldObject_Fallout.conditionDef = "NuclearFallout";
				worldObject_Fallout.lifeSpan = 60000 * Rand.Range(2, 40);
				worldObject_Fallout.destinationTile = result;
				Find.WorldObjects.Add(worldObject_Fallout);
			}
		}

		public void DropWarheadInMap()
		{
			if (!destinationCell.IsValid)
			{
				destinationCell = CellFinderLoose.TryFindCentralCell(mapParent.Map, 25, 1000);
			}
			NuclearStrike nuclearStrike = (NuclearStrike)ThingMaker.MakeThing(DubDef.ICBMStrike);
			nuclearStrike.Yield = yield;
			SkyfallerMaker.SpawnSkyfaller(ThingDef.Named("NuclearWarheadIncoming"), nuclearStrike, destinationCell, mapParent.Map);
		}

		public IEnumerable<string> DebugPossibleSignals(Quest quest)
		{
			string input = Scribe.saver.DebugOutputFor(quest);
			foreach (Match item in Regex.Matches(input, ">(Quest" + quest.id + "\\.[a-zA-Z0-9/\\-\\.]*)<"))
			{
				yield return item.Groups[1].Value;
			}
		}

		public void NukemRico()
		{
			if (yield > 150f)
			{
				for (int i = 0; i < 6; i++)
				{
					MakeFallout(destinationTile);
				}
			}
			if (!Find.WorldObjects.AnyMapParentAt(destinationTile))
			{
				if (DebugSettings.godMode)
				{
					Log.Warning("No map parents for nuke");
				}
				Messages.Message("critBombDet".Translate(), MessageTypeDefOf.NeutralEvent);
				DubDef.hugeExplosionDistant.PlayOneShotOnCamera();
				WorldObject worldObject = Find.WorldObjects.WorldObjectAt<WorldObject>(destinationTile);
				if (worldObject != null && worldObject.Faction != null && !worldObject.Faction.IsPlayer)
				{
					worldObject.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -999);
				}
				PeaceTalks peaceTalks = Find.WorldObjects.WorldObjectAt<PeaceTalks>(destinationTile);
				if (peaceTalks != null)
				{
					try
					{
						peaceTalks.Outcome_Disaster((Caravan)null);
					}
					catch (Exception)
					{
					}
					Find.WorldObjects.Remove(peaceTalks);
				}
				return;
			}
			mapParent = Find.WorldObjects.MapParentAt(destinationTile);
			if (mapParent.Faction != null && !mapParent.Faction.IsPlayer)
			{
				mapParent.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -999);
			}
			if (mapParent.HasMap)
			{
				if (DebugSettings.godMode)
				{
					Log.Warning("dropping nuke inside map");
				}
				DropWarheadInMap();
				return;
			}
			DubDef.hugeExplosionDistant.PlayOneShotOnCamera();
			if (mapParent is DestroyedSettlement)
			{
				if (DebugSettings.godMode)
				{
					Log.Warning("map parent was a destroyed settlement");
				}
				Messages.Message("critBombDet".Translate(), MessageTypeDefOf.NeutralEvent);
				return;
			}
			Messages.Message("critBombDetFac".Translate(mapParent.Label), mapParent, MessageTypeDefOf.NegativeEvent);
			DefeatAllEnemiesQuestComp component = mapParent.GetComponent<DefeatAllEnemiesQuestComp>();
			if (component != null)
			{
				try
				{
					component.GiveRewardsAndSendLetter();
				}
				catch (Exception ex2)
				{
					Log.Warning("DefeatAllEnemiesQuestComp GiveRewardsAndSendLetter Err\n" + ex2);
				}
				try
				{
					component.StopQuest();
				}
				catch (Exception ex3)
				{
					Log.Warning("DefeatAllEnemiesQuestComp StopQuest Err\n" + ex3);
				}
			}
			if (mapParent is Settlement settlement)
			{
				if (settlement.Faction == Faction.OfPlayer)
				{
					return;
				}
				try
				{
					sb.Clear();
					sb.Append("LetterFactionBaseDefeated".Translate(settlement.Label, ""));
					if (!HasAnyOtherBase(settlement))
					{
						settlement.Faction.defeated = true;
						sb.AppendLine();
						sb.AppendLine();
						sb.Append("LetterFactionBaseDefeated_FactionDestroyed".Translate(settlement.Faction.Name));
					}
					Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), sb.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(settlement.Tile));
					DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
					destroyedSettlement.Tile = settlement.Tile;
					Find.WorldObjects.Add(destroyedSettlement);
				}
				catch (Exception ex4)
				{
					Log.Warning("Failed to end settlement\n" + ex4);
				}
			}
			Find.WorldObjects.Remove(mapParent);
			foreach (Quest item in Find.QuestManager.QuestsListForReading)
			{
				if (item.Historical || item.dismissed || !item.QuestLookTargets.Contains(mapParent))
				{
					continue;
				}
				string text = DebugPossibleSignals(item).Distinct().FirstOrDefault((string x) => x.Contains("AllEnemiesDefeated"));
				if (text != null)
				{
					Find.SignalManager.SendSignal(new Signal(text));
					continue;
				}
				string text2 = DebugPossibleSignals(item).Distinct().FirstOrDefault((string x) => x.Contains("Destroyed"));
				if (!string.IsNullOrEmpty(text2))
				{
					Find.SignalManager.SendSignal(new Signal(text2));
				}
			}
		}

		private static bool HasAnyOtherBase(Settlement defeatedFactionBase)
		{
			List<Settlement> settlementBases = Find.WorldObjects.SettlementBases;
			for (int i = 0; i < settlementBases.Count; i++)
			{
				Settlement settlement = settlementBases[i];
				if (settlement.Faction == defeatedFactionBase.Faction && settlement != defeatedFactionBase)
				{
					return true;
				}
			}
			return false;
		}
	}
}
