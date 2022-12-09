using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class NuclearStrike : ThingWithComps
	{
		private static readonly List<IntVec3> openCells = new List<IntVec3>();

		private static readonly List<IntVec3> adjWallCells = new List<IntVec3>();

		public int BlastRadius = 20;

		private readonly List<IntVec3> cellsToAffect = new List<IntVec3>();

		private int duration = 6000;

		public List<Thing> hitThings = new List<Thing>();

		public int shockwave = 20;

		private int startTick;

		public List<Thing> thingsToAffect = new List<Thing>();

		public float Yield = 250f;

		protected int TicksPassed => Find.TickManager.TicksGame - startTick;

		protected int TicksLeft => duration - TicksPassed;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Values.Look(ref Yield, "Yield", 0f);
		}

		public override void Draw()
		{
			Comps_PostDraw();
			WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, GenMath.LerpDoubleClamped(0f, BlastRadius, 5f, 0f, shockwave), 0.008f, GraphicsCache.bigFlashMat);
		}

		public override void Tick()
		{
			base.Tick();
			if (startTick == 0 && base.Spawned)
			{
				StartStrike();
			}
			if (startTick != 0 && TicksPassed >= duration)
			{
				Destroy();
			}
		}

		public virtual void StartStrike()
		{
			if (!base.Spawned)
			{
				Log.Error("Called StartStrike() on unspawned thing.");
				return;
			}
			if (Find.CurrentMap == base.Map)
			{
				DubDef.nuclearBlastInMap.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			else
			{
				DubDef.hugeExplosionDistant.PlayOneShotOnCamera();
			}
			Find.TickManager.slower.SignalForceNormalSpeed();
			if (Yield > 150f)
			{
				duration = 6000;
			}
			else
			{
				duration = 240;
			}
			startTick = Find.TickManager.TicksGame;
			GetComp<CompAffectsSky>().StartFadeInHoldFadeOut(30, 15, duration - 30 - 15);
			if (Yield > 150f)
			{
				Rand.PushState();
				SmokeThrown obj = (SmokeThrown)ThingMaker.MakeThing(DubDef.Mote_NukeCloud);
				obj.exactPosition = base.Position.ToVector3();
				obj.AtomicScale = 2.2f;
				GenSpawn.Spawn(obj, base.Position, base.Map);
				Rand.PopState();
			}
			else
			{
				Rand.PushState();
				SmokeThrown obj2 = (SmokeThrown)ThingMaker.MakeThing(DubDef.Mote_NukeCloud);
				obj2.exactPosition = base.Position.ToVector3();
				obj2.AtomicScale = 1.2f;
				GenSpawn.Spawn(obj2, base.Position, base.Map);
				Rand.PopState();
			}
			if (Yield > 150f)
			{
				if (base.Map.GameConditionManager.ConditionIsActive(DubDef.RadioactiveFallout))
				{
					base.Map.GameConditionManager.GetActiveCondition(DubDef.RadioactiveFallout).End();
				}
				if (!base.Map.GameConditionManager.ConditionIsActive(DubDef.NuclearFallout))
				{
					int num = Mathf.RoundToInt(Rand.Range(10f, 30f) * 60000f);
					GameCondition cond = GameConditionMaker.MakeCondition(DubDef.NuclearFallout, num);
					base.Map.GameConditionManager.RegisterCondition(cond);
				}
			}
			if (Yield > 150f)
			{
				LongEventHandler.QueueLongEvent(StripRoofs, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(Flash, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(GetCells, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWavePlants, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWaveLeaves, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWavePawn, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWaveThing, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWavePawn, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BlastWaveThing, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(BurnSurface, "Detonating", doAsynchronously: false, null);
				LongEventHandler.QueueLongEvent(LeaveAsh, "Detonating", doAsynchronously: false, null);
			}
			else
			{
				GoBang(base.Map);
			}
		}

		public static void ExplosionDamageTerrain(IntVec3 c, Map map)
		{
			if (map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				map.terrainGrid.Notify_TerrainDestroyed(c);
			}
		}

		public void GoBang(Map Map)
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(base.Position, 55f, useCenter: true))
			{
				if (item.InBounds(Map) && base.Position.GetRoof(Map) != RoofDefOf.RoofRockThick)
				{
					Map.roofGrid.SetRoof(item, null);
				}
			}
			List<IntVec3> list = GenRadial.RadialCellsAround(base.Position, 25f, useCenter: true).ToList();
			thingsToAffect.Clear();
			hitThings.Clear();
			foreach (IntVec3 item2 in list)
			{
				if (!item2.InBounds(Map))
				{
					continue;
				}
				List<Thing> list2 = item2.GetThingList(Map).ToList();
				for (int i = 0; i < list2.Count; i++)
				{
					Thing thing = list2[i];
					if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal && thing != this)
					{
						thingsToAffect.Add(thing);
					}
				}
			}
			foreach (Thing item3 in thingsToAffect)
			{
				if (hitThings.Contains(item3))
				{
					continue;
				}
				hitThings.Add(item3);
				try
				{
					if (item3 is IThingHolder thingHolder && !item3.def.IsCorpse)
					{
						try
						{
							thingHolder.GetDirectlyHeldThings().ClearAndDestroyContents();
						}
						catch (Exception)
						{
						}
					}
					if (item3.def.destroyable && !item3.Destroyed)
					{
						item3.Destroy();
					}
				}
				catch (Exception)
				{
				}
			}
			thingsToAffect.Clear();
			hitThings.Clear();
			GenExplosion.DoExplosion(base.Position, Map, 55f, DubDef.Bomb_PlasmaToroid, null, 600, 100f, DubDef.CoreBlast.soundExplosion, null, null, null, ThingDefOf.Filth_Ash, 0.6f, 1, null, applyDamageToExplosionCellsNeighbors: true, null, 0f, 1, 1f);
		}

		public float DamageAt(IntVec3 c)
		{
			float num = base.Position.DistanceTo(c);
			if (num < 1f)
			{
				return 2500f;
			}
			return Yield * (0f - Mathf.Log(num / Yield));
		}

		public void StripRoofs()
		{
			foreach (IntVec3 allCell in base.Map.AllCells)
			{
				if (allCell.GetRoof(base.Map) == RoofDefOf.RoofConstructed)
				{
					base.Map.roofGrid.SetRoof(allCell, null);
				}
			}
		}

		public void GetCells()
		{
			cellsToAffect.Clear();
			openCells.Clear();
			adjWallCells.Clear();
			Map map = base.Map;
			foreach (IntVec3 allCell in map.AllCells)
			{
				if (allCell.InBounds(map) && !allCell.Roofed(map))
				{
					openCells.Add(allCell);
				}
			}
			for (int i = 0; i < openCells.Count; i++)
			{
				IntVec3 intVec = openCells[i];
				if (!intVec.Walkable(map))
				{
					continue;
				}
				for (int j = 0; j < 4; j++)
				{
					IntVec3 intVec2 = intVec + GenAdj.CardinalDirections[j];
					if (intVec2.InBounds(map) && !intVec2.Standable(map) && intVec2.GetEdifice(map) != null && !openCells.Contains(intVec2) && adjWallCells.Contains(intVec2))
					{
						adjWallCells.Add(intVec2);
					}
				}
			}
			cellsToAffect.AddRange(openCells.Concat(adjWallCells));
		}

		public void Flash()
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(base.Position, Yield / 10f, useCenter: true))
			{
				try
				{
					base.Map.roofGrid.SetRoof(item, null);
					base.Map.terrainGrid.RemoveTopLayer(item, doLeavings: false);
					TerrainDef driesTo = base.Map.terrainGrid.TerrainAt(item).driesTo;
					if (driesTo != null)
					{
						base.Map.terrainGrid.SetTerrain(item, driesTo);
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat("Flash could not affect cell ", item, ": ", ex));
				}
			}
		}

		public void BlastWaveThing()
		{
			foreach (IntVec3 item in cellsToAffect)
			{
				float num = DamageAt(item);
				if (!(num > 0f))
				{
					continue;
				}
				List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
				for (int i = 0; i < list.Count; i++)
				{
					try
					{
						Thing thing = list[i];
						if ((thing.def.category != ThingCategory.Building && thing.def.category != ThingCategory.Item) || thing == this || thing.Destroyed || !thing.def.destroyable)
						{
							continue;
						}
						if (num > 1000f)
						{
							if (thing is IThingHolder thingHolder && !thing.def.IsCorpse)
							{
								try
								{
									thingHolder.GetDirectlyHeldThings().ClearAndDestroyContents();
								}
								catch (Exception)
								{
								}
							}
							thing.Destroy();
							continue;
						}
						DamageInfo dinfo = new DamageInfo(DubDef.Bomb_PlasmaToroid, num, 1f);
						thing.TakeDamage(dinfo);
						if (!thing.Destroyed)
						{
							CompRottable compRottable = thing.TryGetComp<CompRottable>();
							if (compRottable != null && (int)compRottable.Stage < 2)
							{
								compRottable.RotProgress += 3000f;
							}
						}
					}
					catch (Exception ex2)
					{
						Log.Warning(string.Concat("Explosion could not affect cell ", item, ": ", ex2));
					}
				}
			}
		}

		public void BlastWavePawn()
		{
			foreach (IntVec3 item in cellsToAffect)
			{
				try
				{
					float num = DamageAt(item);
					if (!(num > 0f))
					{
						continue;
					}
					List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
					for (int i = 0; i < list.Count; i++)
					{
						if (!(list[i] is Pawn pawn))
						{
							continue;
						}
						DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, num);
						dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
						pawn.TakeDamage(dinfo);
						DamageInfo dinfo2 = new DamageInfo(DubDef.Bomb_PlasmaToroid, num);
						pawn.TakeDamage(dinfo2);
						if (pawn.Dead)
						{
							CompRottable compRottable = pawn.Corpse.TryGetComp<CompRottable>();
							if (compRottable != null)
							{
								compRottable.RotProgress = 1E+10f;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat("BlastWavePawn could not affect cell ", item, ": ", ex));
				}
			}
		}

		public void BlastWaveLeaves()
		{
			foreach (IntVec3 item in cellsToAffect)
			{
				try
				{
					if (!(DamageAt(item) > 0f))
					{
						continue;
					}
					List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] is Plant plant)
						{
							plant.MakeLeafless(Plant.LeaflessCause.Poison);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat("BlastWaveLeaves could not affect cell ", item, ": ", ex));
				}
			}
		}

		public void BlastWavePlants()
		{
			foreach (IntVec3 item in cellsToAffect)
			{
				try
				{
					float num = DamageAt(item);
					if (!(num > 0f))
					{
						continue;
					}
					List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].def.category != ThingCategory.Plant || !(list[i] is Plant plant))
						{
							continue;
						}
						DamageInfo dinfo = new DamageInfo(DubDef.Bomb_PlasmaToroid, num);
						plant.TakeDamage(dinfo);
						if (!plant.Destroyed && Rand.Value * 300f > num)
						{
							plant.Destroy();
							if (plant.def.plant.IsTree && plant.LifeStage != 0 && plant.def != ThingDefOf.BurnedTree)
							{
								((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, plant.Position, base.Map)).Growth = plant.Growth;
							}
						}
						else if (Rand.Chance(0.25f))
						{
							Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
							obj.fireSize = 1f;
							GenSpawn.Spawn(obj, plant.Position, base.Map, Rot4.North);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat("BlastWavePlants could not affect cell ", item, ": ", ex));
				}
			}
		}

		public void BurnSurface()
		{
			foreach (IntVec3 item in cellsToAffect)
			{
				try
				{
					ExplosionDamageTerrain(item, base.Map);
					if (base.Map.snowGrid.GetDepth(item) > 0f)
					{
						base.Map.snowGrid.SetDepth(item, 0f);
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat("BurnSurface could not affect cell ", item, ": ", ex));
				}
			}
		}

		public void LeaveAsh()
		{
			Rand.PushState();
			foreach (IntVec3 item in cellsToAffect)
			{
				float num = base.Position.DistanceTo(item);
				if (Rand.Value * 100f > num)
				{
					FilthMaker.TryMakeFilth(item, base.Map, ThingDefOf.Filth_Ash);
				}
			}
			Rand.PopState();
		}

		public void GenShockwave(int center, int radius)
		{
			Find.WorldFloodFiller.FloodFill(center, (int tile) => true, delegate(int tile, int dist)
			{
				if (dist > radius + 1)
				{
					return true;
				}
				if (dist == radius + 1)
				{
					Find.WorldGrid.tiles[tile].biome = BiomeDefOf.Tundra;
					Settlement settlement = Find.WorldObjects.SettlementBases.FirstOrDefault((Settlement x) => x.Tile == tile);
					if (settlement != null)
					{
						Find.WorldObjects.Remove(settlement);
					}
					_ = Rand.Value;
					_ = 0f;
				}
				return false;
			});
		}

		public bool HasProtection(Pawn pawn)
		{
			return (pawn.apparel?.FirstApparelOnBodyPartGroup(BodyPartGroupDefOf.FullHead))?.def.thingCategories.Contains(DubDef.Mopp) ?? false;
		}
	}
}
