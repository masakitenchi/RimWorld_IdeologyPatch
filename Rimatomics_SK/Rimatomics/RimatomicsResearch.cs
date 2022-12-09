using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class RimatomicsResearch : GameComp_RimatomicsResearch
	{
		public static int SilverForShip = 1000;

		public bool BuggerMe;

		public int NukeLaunches;

		public List<CreudMinxident> queuedIncidents = new List<CreudMinxident>();

		public bool ScrambleMode;

		public bool ThreatDetectionMode = true;

		public int TicksToShipArrive;

		public int TraderCooldown;

		public bool updateMessage;

		public static RimatomicsResearch _instance;

		public RimatomicsResearch(World world)
			: base(world)
		{
			_instance = this;
		}

		public void DonkeyRubarb(IncidentParms parms, ref bool res)
		{
			if (queuedIncidents.NullOrEmpty())
			{
				queuedIncidents = new List<CreudMinxident>();
			}
			if (queuedIncidents.Any((CreudMinxident x) => x.FiringIncident.parms == parms))
			{
				return;
			}
			GatherData("ATOMproject", 10f);
			if (!HarmonyPatches.Pstrike && parms.faction.HostileTo(Faction.OfPlayer) && !parms.forced && DubUtils.GetResearch().ThreatDetectionMode && parms.points > 0f && parms.target is Map map && map.Rimatomics().AtomActive)
			{
				float num = Mathf.Min(parms.points, 7000f);
				float num2 = Rand.Range(num * 6f, num * 40f);
				if (parms.raidArrivalMode != null && (parms.raidArrivalMode == PawnsArrivalModeDefOf.CenterDrop || parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeDrop))
				{
					num2 = Mathf.Min(num2 * 0.25f, 45000f);
				}
				int num3 = Mathf.RoundToInt(num2);
				CreudMinxident creudMinxident = new CreudMinxident(new FiringIncident(IncidentDefOf.RaidEnemy, null, parms), Find.TickManager.TicksGame + num3);
				creudMinxident.TickFound = Find.TickManager.TicksGame;
				creudMinxident.detection = "ATOMdetection".Translate();
				if (Rand.Value > 0.55f)
				{
					creudMinxident.factionKnown = true;
				}
				if (Rand.Value > 0.75f)
				{
					creudMinxident.verboseTime = true;
				}
				Find.LetterStack.ReceiveLetter(creudMinxident.detection, creudMinxident.ReportThreat(), LetterDefOf.ThreatSmall, new TargetInfo(parms.spawnCenter, map));
				queuedIncidents.Add(creudMinxident);
				res = false;
				if (HarmonyPatches.Harmony_ScenPart_CreateIncident.repeater != null)
				{
					HarmonyPatches.Harmony_ScenPart_CreateIncident.detected = true;
				}
			}
		}

		public void FactionUpdates()
		{
			if (queuedIncidents.NullOrEmpty())
			{
				return;
			}
			for (int num = queuedIncidents.Count - 1; num >= 0; num--)
			{
				CreudMinxident creudMinxident = queuedIncidents[num];
				if (!creudMinxident.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer))
				{
					queuedIncidents.Remove(creudMinxident);
				}
			}
		}

		public void ForceRaids()
		{
			foreach (CreudMinxident queuedIncident in queuedIncidents)
			{
				queuedIncident.fireTick = Find.TickManager.TicksGame;
			}
		}

		public void IncidentQueueTick()
		{
			if (queuedIncidents.NullOrEmpty())
			{
				return;
			}
			for (int num = queuedIncidents.Count - 1; num >= 0; num--)
			{
				CreudMinxident creudMinxident = queuedIncidents[num];
				if (!((Map)queuedIncidents[num].FiringIncident.parms.target).Parent.Spawned)
				{
					queuedIncidents.Remove(creudMinxident);
				}
				if (!creudMinxident.TriedToFire)
				{
					if (creudMinxident.FireTick <= Find.TickManager.TicksGame)
					{
						if (!creudMinxident.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer))
						{
							queuedIncidents.Remove(creudMinxident);
						}
						else
						{
							bool flag = false;
							try
							{
								flag = Find.Storyteller.TryFire(creudMinxident.FiringIncident);
							}
							catch (Exception ex)
							{
								flag = true;
								Log.Warning(creudMinxident.FiringIncident.ToString());
								Log.Warning(ex.ToString());
							}
							creudMinxident.Notify_TriedToFire();
							if (flag || creudMinxident.RetryDurationTicks == 0)
							{
								queuedIncidents.Remove(creudMinxident);
							}
						}
					}
				}
				else if (creudMinxident.FireTick + creudMinxident.RetryDurationTicks <= Find.TickManager.TicksGame)
				{
					queuedIncidents.Remove(creudMinxident);
				}
				else if (Find.TickManager.TicksGame % 833 == Rand.RangeSeeded(0, 833, creudMinxident.FireTick))
				{
					if (!creudMinxident.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer))
					{
						queuedIncidents.Remove(creudMinxident);
					}
					else
					{
						bool flag2 = false;
						try
						{
							flag2 = Find.Storyteller.TryFire(creudMinxident.FiringIncident);
						}
						catch (Exception ex2)
						{
							flag2 = true;
							Log.Warning(creudMinxident.FiringIncident.ToString());
							Log.Warning(ex2.ToString());
						}
						creudMinxident.Notify_TriedToFire();
						if (flag2)
						{
							queuedIncidents.Remove(creudMinxident);
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref queuedIncidents, "queuedIncidents2", LookMode.Deep);
			Scribe_Values.Look(ref BuggerMe, "BuggerMe", defaultValue: false);
			Scribe_Values.Look(ref ScrambleMode, "ScrambleMode", defaultValue: false);
			Scribe_Values.Look(ref ThreatDetectionMode, "ThreatDetectionMode", defaultValue: true);
			Scribe_Values.Look(ref TicksToShipArrive, "TicksToShipArrive", 0);
			Scribe_Values.Look(ref TraderCooldown, "TraderCooldown", 0);
			Scribe_Values.Look(ref NukeLaunches, "NukeLaunches", 0);
			Scribe_Values.Look(ref updateMessage, "updateMessage", defaultValue: false);
		}

		public override void WorldComponentTick()
		{
			if (TicksToShipArrive > 0)
			{
				TicksToShipArrive--;
				if (TicksToShipArrive == 0)
				{
					SpawnTradeShip();
				}
			}
			if (TraderCooldown > 0)
			{
				TraderCooldown--;
			}
			if (GenDate.DaysPassed < 1)
			{
				updateMessage = true;
			}
			IncidentQueueTick();
		}

		public bool SpawnTradeShip()
		{
			Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
			TradeShip tradeShip = new TradeShip(DubDef.Orbital_Rimatomics)
			{
				name = "Rimatomics"
			};
			if (anyPlayerHomeMap.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
			{
				Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION"))), LetterDefOf.NeutralEvent);
			}
			anyPlayerHomeMap.passingShipManager.AddShip(tradeShip);
			tradeShip.GenerateThings();
			return true;
		}
	}
}
