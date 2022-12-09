using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200001E RID: 30
	public static class AdvancedAI_Classes
	{
		// Token: 0x06000117 RID: 279 RVA: 0x00019B5C File Offset: 0x00017D5C
		public static MapComponent_SkyAI MapComp(Pawn pawn)
		{
			bool flag = pawn != null;
			if (flag)
			{
				Map map = pawn.Map ?? Find.CurrentMap;
				bool flag2 = map != null;
				if (flag2)
				{
					return map.GetComponent<MapComponent_SkyAI>();
				}
			}
			return null;
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00019B9C File Offset: 0x00017D9C
		public static void CheckPawnLord(Pawn pawn)
		{
			bool flag = pawn.Faction != null && pawn.Faction != Faction.OfPlayer;
			if (flag)
			{
				try
				{
					Lord lord = pawn.GetLord();
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
					bool flag2 = mapComponent_SkyAI == null;
					if (!flag2)
					{
						bool flag3 = lord != null;
						if (flag3)
						{
							bool flag4 = mapComponent_SkyAI.lords != null;
							if (flag4)
							{
								bool flag5 = mapComponent_SkyAI.lords.NullOrEmpty<Lord>();
								if (flag5)
								{
									mapComponent_SkyAI.Generated = true;
									mapComponent_SkyAI.lords.Add(lord);
								}
								else
								{
									bool flag6 = !mapComponent_SkyAI.lords.NullOrEmpty<Lord>();
									if (flag6)
									{
										bool flag7 = !mapComponent_SkyAI.lords.Any((Lord l) => l != null && l.loadID == lord.loadID);
										if (flag7)
										{
											mapComponent_SkyAI.Generated = true;
											mapComponent_SkyAI.lords.Add(lord);
										}
									}
								}
							}
						}
						else
						{
							bool flag8 = lord == null;
							if (flag8)
							{
								RaidData raidData = AdvancedAI.PawnRaidData(pawn);
								bool flag9 = raidData != null && !AdvancedAI.HasExitJob(pawn);
								if (flag9)
								{
									List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
									bool flag10 = list != null;
									if (flag10)
									{
										Func<Pawn, float> <>9__3;
										Func<Pawn, float> <>9__4;
										Func<Pawn, bool> <>9__1;
										foreach (Lord lord3 in list)
										{
											bool flag11 = !lord3.ownedPawns.NullOrEmpty<Pawn>();
											if (flag11)
											{
												IEnumerable<Pawn> ownedPawns = lord3.ownedPawns;
												Func<Pawn, bool> predicate;
												if ((predicate = <>9__1) == null)
												{
													predicate = (<>9__1 = ((Pawn p) => p != pawn && !p.IsSlave && !p.IsPrisoner && !raidData.leaderGuards.Contains(p)));
												}
												IEnumerable<Pawn> enumerable = ownedPawns.Where(predicate);
												IEnumerable<Pawn> source = from p in enumerable
												where !AdvancedAI.HasExitJob(p)
												select p;
												Func<Pawn, float> selector;
												if ((selector = <>9__3) == null)
												{
													selector = (<>9__3 = ((Pawn p2) => p2.Position.DistanceTo(pawn.Position)));
												}
												Pawn pawn2;
												source.TryMinBy(selector, out pawn2);
												bool flag12 = pawn2 != null;
												if (flag12)
												{
													bool debugLog = SkyAiCore.Settings.debugLog;
													if (debugLog)
													{
														Log.Message(string.Format("{0} {1}: CheckPawnLord. PawnRaidData. Added {2} to existing not flee lord: {3} with id: {4}", new object[]
														{
															pawn,
															pawn.Position,
															pawn,
															list[0].LordJob,
															list[0].loadID
														}));
													}
													lord3.AddPawn(pawn);
													bool flag13 = pawn.jobs != null;
													if (flag13)
													{
														pawn.jobs.StopAll(false, true);
													}
													return;
												}
												IEnumerable<Pawn> source2 = enumerable;
												Func<Pawn, float> selector2;
												if ((selector2 = <>9__4) == null)
												{
													selector2 = (<>9__4 = ((Pawn p2) => p2.Position.DistanceTo(pawn.Position)));
												}
												source2.TryMinBy(selector2, out pawn2);
												bool flag14 = pawn2 != null;
												if (flag14)
												{
													bool debugLog2 = SkyAiCore.Settings.debugLog;
													if (debugLog2)
													{
														Log.Message(string.Format("{0} {1}: CheckPawnLord. PawnRaidData. Added {2} to existing flee lord: {3} with id: {4}", new object[]
														{
															pawn,
															pawn.Position,
															pawn,
															list[0].LordJob,
															list[0].loadID
														}));
													}
													lord3.AddPawn(pawn);
													bool flag15 = pawn.jobs != null;
													if (flag15)
													{
														pawn.jobs.StopAll(false, true);
													}
													return;
												}
											}
										}
									}
								}
								IEnumerable<Pawn> enumerable2 = AdvancedAI.PawnsOfFactionOnMap(pawn);
								bool flag16 = !enumerable2.EnumerableNullOrEmpty<Pawn>();
								if (flag16)
								{
									IEnumerable<Pawn> source3 = enumerable2;
									Func<Pawn, bool> <>9__5;
									Func<Pawn, bool> predicate2;
									if ((predicate2 = <>9__5) == null)
									{
										predicate2 = (<>9__5 = ((Pawn p2) => p2 != pawn && !p2.IsPrisoner && !p2.IsSlave && !AdvancedAI.HasExitJob(p2) && !AdvancedAI.PawnIsGuard(p2)));
									}
									foreach (Pawn p3 in source3.Where(predicate2))
									{
										Lord lord2 = p3.GetLord();
										bool flag17 = lord2 != null;
										if (flag17)
										{
											bool debugLog3 = SkyAiCore.Settings.debugLog;
											if (debugLog3)
											{
												Log.Message(string.Format("{0} {1}: CheckPawnLord. Added {2} to existing lord: {3} with id: {4}", new object[]
												{
													pawn,
													pawn.Position,
													pawn,
													lord2.LordJob,
													lord2.loadID
												}));
											}
											lord2.AddPawn(pawn);
											bool flag18 = pawn.jobs != null;
											if (flag18)
											{
												pawn.jobs.StopAll(false, true);
											}
											break;
										}
									}
								}
							}
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error(string.Format("{0} {1}: CheckPawnLord exception: {2}", pawn, pawn.Position, arg));
				}
			}
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0001A2CC File Offset: 0x000184CC
		public static void StealDebug(Pawn pawn)
		{
			bool flag = SkyAiCore.Settings.debugStealCells && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag)
			{
				foreach (Building building in pawn.Map.listerBuildings.allBuildingsColonist)
				{
					bool flag2 = AdvancedAI.IsStealableItem(pawn, building, 100f);
					if (flag2)
					{
						pawn.Map.debugDrawer.FlashCell(building.Position, 0.77f, "", SkyAiCore.Settings.flashCellDelay);
					}
				}
				foreach (Thing thing in pawn.Map.listerThings.AllThings)
				{
					bool flag3 = AdvancedAI.IsStealableItem(pawn, thing, 100f);
					if (flag3)
					{
						pawn.Map.debugDrawer.FlashCell(thing.Position, 0.77f, "", SkyAiCore.Settings.flashCellDelay);
					}
				}
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0001A410 File Offset: 0x00018610
		public static void ExtinguishFire(Pawn pawn)
		{
			AdvancedAI_Classes.<>c__DisplayClass3_0 CS$<>8__locals1 = new AdvancedAI_Classes.<>c__DisplayClass3_0();
			CS$<>8__locals1.pawn = pawn;
			try
			{
				float maxDistance = (float)(AdvancedAI.InDangerousCombat(CS$<>8__locals1.pawn, 30f) ? 4 : 12);
				Thing thing = GenClosest.ClosestThingReachable(CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(CS$<>8__locals1.pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), maxDistance, new Predicate<Thing>(CS$<>8__locals1.<ExtinguishFire>g__isBurningPawn|0), null, 0, -1, false, RegionType.Set_Passable, false);
				bool flag = thing != null;
				if (flag)
				{
					LocomotionUrgency locomotionUrgency = SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;
					Job job = JobMaker.MakeJob(JobDefOf.BeatFire);
					job.targetA = thing;
					job.locomotionUrgency = locomotionUrgency;
					bool flag2 = job != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1} Extinguish: {2} on {3}", new object[]
							{
								CS$<>8__locals1.pawn,
								CS$<>8__locals1.pawn.Position,
								job.targetA,
								job.targetA.Cell
							}));
						}
						bool flag4 = !AdvancedAI.AlreadyHasSameJob(CS$<>8__locals1.pawn, JobDefOf.BeatFire);
						if (flag4)
						{
							CS$<>8__locals1.pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
						}
					}
					else
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: Extinguish job failed", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: ExtinguishFire exception: {2}", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, arg));
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0001A634 File Offset: 0x00018834
		public static void ScanInventoryForBetterWeapon(Pawn pawn, bool enemyCamperFound)
		{
			try
			{
				bool flag = false;
				Thing enemyTarget = pawn.mindState.enemyTarget;
				bool flag2 = enemyTarget != null;
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: Weapon switching decisions. Target: {2} {3} Dist: {4}", new object[]
						{
							pawn,
							pawn.Position,
							enemyTarget,
							enemyTarget.Position,
							pawn.Position.DistanceTo(enemyTarget.Position)
						}));
					}
					ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
					CompInventory compInventory = pawn.TryGetComp<CompInventory>();
					bool flag4 = compInventory != null;
					if (flag4)
					{
						float num = pawn.Position.DistanceTo(enemyTarget.Position);
						bool flag5 = AdvancedAI.IsInCloseWithTarget(pawn, enemyTarget) && thingWithComps != null && !thingWithComps.def.IsMeleeWeapon;
						if (flag5)
						{
							List<ThingWithComps> list = AdvancedAI.InventoryMeleeWeaponList(compInventory);
							bool flag6 = !list.NullOrEmpty<ThingWithComps>();
							if (flag6)
							{
								bool flag7 = compInventory != null;
								if (flag7)
								{
									bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag8)
									{
										Log.Message(string.Format("{0} {1}: Switch to melee! Target: {2} {3} Dist: {4}", new object[]
										{
											pawn,
											pawn.Position,
											enemyTarget,
											enemyTarget.Position,
											pawn.Position.DistanceTo(enemyTarget.Position)
										}));
									}
									compInventory.TrySwitchToWeapon(list.FirstOrDefault<ThingWithComps>());
									flag = true;
								}
							}
						}
						bool flag9 = AdvancedAI.AnyAllyNearby(pawn, 5f) || Rand.Chance(0.33f);
						bool flag10 = !flag && thingWithComps != null && !AdvancedAI.IsGrenade(thingWithComps) && (enemyCamperFound || flag9);
						if (flag10)
						{
							ThingWithComps thingWithComps2 = AdvancedAI.GrenadeInInventory(compInventory);
							IEnumerable<int> source = Enumerable.Range(6, 12);
							bool flag11 = thingWithComps2 != null && !AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, enemyTarget, 5f, 0f);
							if (flag11)
							{
								List<Verb> list2 = AdvancedAI.WeaponVerbs(thingWithComps2);
								bool flag12 = !list2.NullOrEmpty<Verb>();
								if (flag12)
								{
									Verb verb = list2.MaxBy((Verb v) => v.verbProps.range);
									source = Enumerable.Range(Mathf.RoundToInt(verb.verbProps.ai_AvoidFriendlyFireRadius), Mathf.RoundToInt(verb.verbProps.range));
								}
								bool flag13 = source.Contains(Mathf.RoundToInt(num));
								if (flag13)
								{
									bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag14)
									{
										Log.Message(string.Format("{0} {1}: Switch to grenades! Target: {2} {3} Dist: {4}", new object[]
										{
											pawn,
											pawn.Position,
											enemyTarget,
											enemyTarget.Position,
											pawn.Position.DistanceTo(enemyTarget.Position)
										}));
									}
									compInventory.TrySwitchToWeapon(thingWithComps2);
									flag = true;
								}
							}
						}
						bool flag15 = !flag && thingWithComps != null && AdvancedAI.PrimaryVerb(pawn) != null;
						if (flag15)
						{
							bool flag16 = thingWithComps.def.IsMeleeWeapon && !thingWithComps.def.weaponTags.NullOrEmpty<string>() && !thingWithComps.def.weaponTags.Contains("CE_OneHandedWeapon");
							if (flag16)
							{
								ThingWithComps thingWithComps3 = AdvancedAI.ShieldEquiped(pawn);
								bool flag17 = thingWithComps3 != null;
								if (flag17)
								{
									ThingWithComps t;
									bool flag18 = pawn.equipment.TryDropEquipment(thingWithComps3, out t, pawn.Position, true);
									if (flag18)
									{
										Job job = JobMaker.MakeJob(JobDefOf.DropEquipment, t, 30, true);
										bool flag19 = !AdvancedAI.AlreadyHasSameJob(pawn, job.def);
										if (flag19)
										{
											pawn.jobs.EndCurrentJob(JobCondition.None, true, true);
											pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
										}
									}
								}
							}
							bool flag20 = AdvancedAI.PrimaryIsGrenade(pawn) && AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, enemyTarget, 5f, 0f);
							bool flag21 = num > AdvancedAI.PrimaryVerb(pawn).verbProps.range;
							bool flag22 = flag20 || flag21;
							if (flag22)
							{
								ThingWithComps thingWithComps4 = AdvancedAI.HasWeaponWithBetterRange(pawn, compInventory);
								bool flag23 = thingWithComps4 != null;
								if (flag23)
								{
									bool flag24 = compInventory != null;
									if (flag24)
									{
										bool flag25 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag25)
										{
											Log.Message(string.Format("{0} {1}: Switch to better ranged weapon! Target: {2} {3} Dist: {4}", new object[]
											{
												pawn,
												pawn.Position,
												enemyTarget,
												enemyTarget.Position,
												pawn.Position.DistanceTo(enemyTarget.Position)
											}));
										}
										compInventory.TrySwitchToWeapon(thingWithComps4);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: ScanInventoryForBetterWeapon exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0001AB68 File Offset: 0x00018D68
		public static void DangerousCellsDetection(Pawn pawn)
		{
			try
			{
				MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
				bool flag = mapComponent_SkyAI != null && !mapComponent_SkyAI.dangerousCells.NullOrEmpty<IntVec3>();
				if (flag)
				{
					IntVec3 invalid = IntVec3.Invalid;
					foreach (IntVec3 intVec in mapComponent_SkyAI.dangerousCells)
					{
						LocalTargetInfo a = AdvancedAI.CurrentLocalTargetInfo(pawn);
						bool flag2 = a != null && a.Cell == intVec;
						if (flag2)
						{
							bool flag3 = a.Thing is Pawn;
							if (flag3)
							{
								pawn.jobs.StopAll(false, true);
							}
							bool flag4 = a.Thing is Building;
							if (flag4)
							{
								try
								{
									Building building = AdvancedAI.MeleeTrashBuilding(intVec, pawn, 20f, 3f);
									bool flag5 = building != null;
									if (flag5)
									{
										using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, building.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false), PathEndMode.OnCell, null))
										{
											IntVec3 cellBeforeBlocker;
											Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
											bool flag6 = thing != null;
											if (flag6)
											{
												Job coverOrMeleeJob = AdvancedAI_Jobs.GetCoverOrMeleeJob(pawn, AdvancedAI.PrimaryVerb(pawn), thing, cellBeforeBlocker);
												bool flag7 = coverOrMeleeJob != null;
												if (flag7)
												{
													pawn.jobs.StartJob(coverOrMeleeJob, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
												}
											}
										}
									}
								}
								catch (Exception arg)
								{
									Log.Error(string.Format("{0} {1}: dangerousCells: target.Thing is Building part exception: {2}", pawn, pawn.Position, arg));
								}
							}
						}
						else
						{
							bool flag8 = pawn.Position == intVec;
							if (flag8)
							{
								IntVec3 c;
								bool flag9 = AdvancedAI.TryFindDirectFleeDestination(intVec, 6f, pawn, out c);
								if (flag9)
								{
									LocomotionUrgency locomotionUrgency = SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;
									pawn.jobs.StopAll(false, true);
									Job job = JobMaker.MakeJob(JobDefOf.Goto, c);
									job.locomotionUrgency = locomotionUrgency;
								}
							}
						}
					}
				}
			}
			catch (Exception arg2)
			{
				Log.Error(string.Format("{0} {1}: DangerousCellsDetection part exception: {2}", pawn, pawn.Position, arg2));
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0001AE28 File Offset: 0x00019028
		public static void EvacuateSpecialPawn(Pawn pawn, RaidData raidData)
		{
			bool flag = raidData == null;
			if (!flag)
			{
				try
				{
					List<Pawn> list = new List<Pawn>();
					bool flag2 = raidData.raidLeader != null;
					if (flag2)
					{
						list.Add(raidData.raidLeader);
					}
					foreach (Pawn pawn2 in raidData.squadCommanders)
					{
						bool flag3 = pawn2 != null;
						if (flag3)
						{
							list.Add(pawn2);
						}
					}
					Pawn pawn3;
					(from p in list
					where p != null && !AdvancedAI.IsActivePawn(p) && pawn.Position.DistanceTo(p.Position) <= 15f
					select p).TryMinBy((Pawn d) => pawn.Position.DistanceTo(d.Position), out pawn3);
					bool flag4 = pawn3 != null;
					if (flag4)
					{
						Job job = AdvancedAI_Jobs.EvacuateSpecialPawnJob(pawn, pawn3);
						bool flag5 = job != null;
						if (flag5)
						{
							pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error(string.Format("{0} {1}: EvacuateSpecialPawn exception: {2}", pawn, pawn.Position, arg));
				}
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0001AF80 File Offset: 0x00019180
		public static void UseSnipersKiteMechanic(Pawn pawn)
		{
			try
			{
				Thing enemyTarget = pawn.mindState.enemyTarget;
				Pawn pawn2 = enemyTarget as Pawn;
				bool flag = pawn2 != null;
				if (flag)
				{
					Job job = AdvancedAI_Jobs.KiteMechanic(pawn, pawn2);
					bool flag2 = job != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1}: Start Kite Job.", pawn, pawn.Position));
						}
						pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
					}
					else
					{
						bool flag4 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: Kite Job failed.", pawn, pawn.Position));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: SnipersKiteMechanic exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x0001B090 File Offset: 0x00019290
		public static void EscapeWithoutWeapon(Pawn pawn)
		{
			try
			{
				bool enemiesEscapeChanceNoWeapon = SkyAiCore.Settings.enemiesEscapeChanceNoWeapon;
				if (enemiesEscapeChanceNoWeapon)
				{
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
					bool flag = mapComponent_SkyAI == null && mapComponent_SkyAI.exitCounter == null;
					if (!flag)
					{
						bool flag2 = !mapComponent_SkyAI.exitCounter.ContainsKey(pawn);
						if (flag2)
						{
							bool flag3 = !AdvancedAI.HasExitJob(pawn) && !AdvancedAI.ShouldIgnoreLeaveDesire(pawn) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn) * (SkyAiCore.Settings.fleeChanceMultiplier * 2f));
							if (flag3)
							{
								bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag4)
								{
									Log.Message(string.Format("{0} {1}: EscapeNoWeapon. Added to exitList list.", pawn, pawn.Position));
								}
								mapComponent_SkyAI.exitCounter.Add(pawn, 0);
							}
						}
						else
						{
							bool flag5 = false;
							int num = mapComponent_SkyAI.exitCounter.TryGetValue(pawn, 0);
							RaidData raidData = AdvancedAI.PawnRaidData(pawn);
							int num2 = (raidData != null && AdvancedAI.RaidLeaderIsActive(raidData)) ? 6000 : 3500;
							bool flag6 = (float)num >= (float)num2 * Mathf.Clamp(1f - AdvancedAI_TraitUtility.FleeChance(pawn), 0.3f, 1f);
							if (flag6)
							{
								flag5 = true;
								mapComponent_SkyAI.exitCounter.Remove(pawn);
							}
							bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: EscapeNoWeapon. ExitListCounter timer: {2}. shouldLeave: {3}", new object[]
								{
									pawn,
									pawn.Position,
									num,
									flag5
								}));
							}
							bool flag8 = flag5;
							if (flag8)
							{
								Job exitJob = AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, true, true);
								bool flag9 = exitJob != null;
								if (flag9)
								{
									bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag10)
									{
										Log.Message(string.Format("{0} {1}: EscapeNoWeapon. start GetExitJob bcs of shouldLeave timer.", pawn, pawn.Position));
									}
									pawn.jobs.StartJob(exitJob, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
								}
								else
								{
									bool flag11 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag11)
									{
										Log.Message(string.Format("{0} {1}: EscapeNoWeapon. GetExitJob failed.", pawn, pawn.Position));
									}
								}
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: EscapeNoWeapon exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0001B34C File Offset: 0x0001954C
		public static void EscapeDueToTemperatureInjuries(Pawn pawn)
		{
			try
			{
				bool flag = Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn) * SkyAiCore.Settings.fleeChanceMultiplier);
				if (flag)
				{
					Job exitJob = AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
					bool flag2 = exitJob != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1} GetExitJob bcs of EscapeDueToTemperatureInjuries", pawn, pawn.Position));
						}
						pawn.jobs.StartJob(exitJob, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
					}
					else
					{
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: GetExitJob bcs of EscapeDueToTemperatureInjuries failed", pawn, pawn.Position));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: EscapeTemperatureInjuries exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x0001B460 File Offset: 0x00019660
		public static void EscapeChanceExhausted(Pawn pawn)
		{
			try
			{
				bool flag = Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn) * SkyAiCore.Settings.fleeChanceMultiplier);
				if (flag)
				{
					Job exitJob = AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
					bool flag2 = exitJob != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1}: GetExitJob bcs of EscapeExhausted", pawn, pawn.Position));
						}
						pawn.jobs.StartJob(exitJob, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
					}
					else
					{
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: GetExitJob: EscapeExhausted failed", pawn, pawn.Position));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: EscapeExhausted exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0001B574 File Offset: 0x00019774
		public static void ThingOwnerData(Pawn pawn)
		{
			try
			{
				bool flag = AdvancedAI.HasFleeingDuty(pawn) || AdvancedAI.HasExitJob(pawn) || AdvancedAI_TendUtility.HasAlreadyStartTendJob(pawn, true);
				if (!flag)
				{
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
					Dictionary<ThingCountClass, Faction> lostThings = mapComponent_SkyAI.lostThings;
					bool flag2 = !lostThings.EnumerableNullOrEmpty<KeyValuePair<ThingCountClass, Faction>>();
					if (flag2)
					{
						ThingCountClass thingCountClass;
						(from f in lostThings
						where f.Value == pawn.Faction && f.Key.thing != null && AdvancedAI_CaravanUtility.CurrentOwnerOf(f.Key.thing) == null && pawn.CanReserveAndReach(f.Key.thing, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false)
						select f into f3
						select f3.Key).TryMinBy((ThingCountClass f2) => f2.thing.Position.DistanceTo(pawn.Position), out thingCountClass);
						bool flag3 = thingCountClass != null;
						if (flag3)
						{
							CompInventory compInventory = pawn.TryGetComp<CompInventory>();
							int num = 0;
							bool flag4 = compInventory != null && compInventory.CanFitInInventory(thingCountClass.thing, ref num, false, false);
							if (flag4)
							{
								Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thingCountClass.thing);
								job.expiryInterval = 300;
								job.checkOverrideOnExpire = true;
								job.count = Mathf.Min(thingCountClass.thing.stackCount, num);
								bool flag5 = job != null;
								if (flag5)
								{
									pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
									ThingCountClass thingCountClass2 = new ThingCountClass(thingCountClass.thing, num);
									mapComponent_SkyAI.lostThings.Remove(thingCountClass2);
									AdvancedAI_CaravanUtility.AddItemToPawnThingOwnerList(pawn, thingCountClass2);
									bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
									if (debugPawnThingsOwner)
									{
										Log.Message(string.Format("{0} {1}: Item {2} removed from lostThings and added to PawnThingsOwner: {3} count: {4}", new object[]
										{
											pawn,
											pawn.Position,
											thingCountClass2,
											thingCountClass2.thing,
											thingCountClass2.Count
										}));
									}
								}
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: ThingOwnerData exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0001B7D0 File Offset: 0x000199D0
		public static void DistanceBreak(Pawn pawn)
		{
			try
			{
				bool flag = Find.TickManager.TicksGame - pawn.mindState.lastMeleeThreatHarmTick > 300;
				if (!flag)
				{
					Verb verb = AdvancedAI.PrimaryVerb(pawn);
					bool flag2 = verb != null && (verb == null || verb.verbProps.range < 7f);
					if (!flag2)
					{
						Thing enemyTarget = pawn.mindState.enemyTarget;
						bool flag3 = enemyTarget != null && AdvancedAI.IsInCloseWithTarget(pawn, enemyTarget) && pawn.GetStatValue(StatDefOf.MoveSpeed, true) > enemyTarget.GetStatValue(StatDefOf.MoveSpeed, true);
						if (flag3)
						{
							bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
							foreach (IntVec3 intVec in GenRadial.RadialCellsAround(pawn.Position, 2f, 6f))
							{
								bool flag4 = !intVec.InBounds(pawn.Map) || !intVec.Standable(pawn.Map) || !AdvancedAI.IsFreeCell(intVec, pawn.Map);
								if (!flag4)
								{
									string text;
									bool flag5 = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec, enemyTarget.Position, true, false, true, false, useFriendlyFire, true, true, out text) > 0f;
									if (flag5)
									{
										Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
										job.locomotionUrgency = LocomotionUrgency.Sprint;
										job.checkOverrideOnExpire = true;
										job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
										bool flag6 = job != null;
										if (flag6)
										{
											bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag7)
											{
												Log.Message(string.Format("{0} {1}: Trying to break distance. Im with ranged weapon. No close combat! Moving to position {2} focuscell: {3} my distance to new position: {4} Distance from pos to focus cell: {5}", new object[]
												{
													pawn,
													pawn.Position,
													intVec,
													enemyTarget.Position,
													pawn.Position.DistanceTo(intVec),
													intVec.DistanceTo(enemyTarget.Position)
												}));
											}
											pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
										}
										break;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: DistanceBreak exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0001BA70 File Offset: 0x00019C70
		public static void DutyAttackSubNodes(Pawn pawn, Lord lord, RaidData raidData)
		{
			try
			{
				bool flag = lord == null || AdvancedAI.DutyHasAttackSubNodes(pawn, true) || AdvancedAI.HasFleeingDuty(pawn) || AdvancedAI.HasExitJob(pawn);
				if (!flag)
				{
					bool flag2 = SkyAiCore.Settings.enableDoctorRole && AdvancedAI.PawnIsDoctor(pawn) && !AdvancedAI_TendUtility.HasAlreadyStartTendJob(pawn, true);
					if (flag2)
					{
						bool flag3 = false;
						Pawn pawn2 = TraderCaravanUtility.FindTrader(lord);
						bool flag4 = pawn2 != null;
						if (flag4)
						{
							flag3 = (pawn.Position.DistanceTo(pawn2.Position) > 35f);
							bool flag5 = AdvancedAI.HasExitJob(pawn2);
							if (flag5)
							{
								flag3 = true;
							}
						}
						bool flag6 = raidData != null && raidData.raidLeader != null;
						if (flag6)
						{
							flag3 = (pawn.Position.DistanceTo(raidData.raidLeader.Position) > 60f);
							bool flag7 = AdvancedAI.HasExitJob(raidData.raidLeader);
							if (flag7)
							{
								flag3 = true;
							}
						}
						bool flag8 = !flag3;
						if (flag8)
						{
							IntVec3 intVec = (pawn.mindState.enemyTarget != null) ? pawn.mindState.enemyTarget.Position : IntVec3.Invalid;
							bool takePosition = AdvancedAI.IsValidLoc(intVec);
							Job job = AdvancedAI_Roles.DoctorRole(pawn, intVec, takePosition);
							bool flag9 = job != null;
							if (flag9)
							{
								bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag10)
								{
									bool flag11 = pawn.mindState.enemyTarget != null;
									if (flag11)
									{
										Log.Message(string.Format("{0} {1}: Non-Combat duty: {2}. Doctor role! enemyTarget: {3} on pos: {4} distance: {5} EFF: {6}", new object[]
										{
											pawn,
											pawn.Position,
											pawn.mindState.duty,
											pawn.mindState.enemyTarget,
											intVec,
											pawn.Position.DistanceTo(intVec),
											AdvancedAI.EffectiveRange(pawn)
										}));
									}
									else
									{
										Log.Message(string.Format("{0} {1}: Non-Combat duty: {2}. Doctor role! EnemyTarget null.", pawn, pawn.Position, pawn.mindState.duty));
									}
								}
								pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: DutyAttackSubNodes part exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0001BCE8 File Offset: 0x00019EE8
		public static void DebugSetDutyAttackColony(Pawn pawn)
		{
			try
			{
				PawnDuty duty = pawn.mindState.duty;
				bool flag = duty != null && duty.def == DutyDefOf.Defend;
				if (flag)
				{
					AdvancedAI_LordUtility.PawnAddAssaultColonyLord(pawn);
					bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						Log.Message(string.Format("{0} {1}: Changed mindstate duty from Defend to Assault Colony using enabled debug setting.", pawn, pawn.Position));
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: DebugSetDutyAttackColony exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0001BD90 File Offset: 0x00019F90
		public static void ReactToThreats(Pawn pawn, Lord lord, RaidData raidData)
		{
			try
			{
				MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
				bool isInTarget = true;
				bool flag = raidData != null && raidData.squadsFormed && (raidData.raidStage == RaidData.RaidStage.gathering || raidData.raidStage == RaidData.RaidStage.start);
				bool flag2 = flag;
				if (flag2)
				{
					Thing thing;
					List<Thing> list;
					bool flag3 = AdvancedAI.ActiveThreat(pawn, SkyAiCore.Settings.nonCombatActiveThreatRange, false, isInTarget, true, true, true, false, false, out thing, out list) && !AdvancedAI.PawnIsGuard(pawn);
					if (flag3)
					{
						bool debugLog = SkyAiCore.Settings.debugLog;
						if (debugLog)
						{
							Log.Message(string.Format("{0} {1}: Set squad defend statue to true.", pawn, pawn.Position));
						}
						AdvancedAI_SquadUtility.ChangeSquadDefenceStatus(pawn.Position, lord, raidData, true);
					}
					else
					{
						IntVec3 center;
						bool flag4 = AdvancedAI_SquadUtility.IsSquadInDefence(lord, raidData, out center);
						if (flag4)
						{
							bool flag5 = !AdvancedAI.ActiveThreatInArea(pawn, center, pawn.Map, SkyAiCore.Settings.nonCombatActiveThreatRange, 12, out thing);
							if (flag5)
							{
								bool debugLog2 = SkyAiCore.Settings.debugLog;
								if (debugLog2)
								{
									Log.Message(string.Format("{0} {1}: Set squad defend status to false.", pawn, pawn.Position));
								}
								AdvancedAI_SquadUtility.ChangeSquadDefenceStatus(pawn.Position, lord, raidData, false);
							}
						}
					}
				}
				bool flag6 = AdvancedAI.HasExitOrNonCombatJob(pawn);
				if (flag6)
				{
					bool flag7 = raidData != null && raidData.raidStage == RaidData.RaidStage.fleeing;
					if (flag7)
					{
						bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag8)
						{
							Log.Message(string.Format("{0} {1}: Can't react to threats, bcs raidStage marked as panic flee.", pawn, pawn.Position));
						}
						return;
					}
					bool flag9 = AdvancedAI.HasKidnapOrStealJob(pawn) && AdvancedAI.HarmedRecently(pawn);
					bool flag10 = AdvancedAI.IsHumanlikeOnly(pawn) && !flag9 && !AdvancedAI_TendUtility.HasAlreadyStartTendJob(pawn, true);
					if (flag10)
					{
						bool flag11 = AdvancedAI.HasKidnapOrStealJob(pawn) && AdvancedAI.HasCarryThing(pawn);
						bool flag12 = !AdvancedAI.HasExitJob(pawn);
						if (flag12)
						{
							LocalTargetInfo enemyTarget = AdvancedAI.GetEnemyTarget(pawn, false, true);
							IntVec3 focusCell = (enemyTarget == null) ? new IntVec3(-1000, -1000, -1000) : enemyTarget.Cell;
							Job job = AdvancedAI_Jobs.SurvivalDecisions(pawn, focusCell);
							bool flag13 = job != null;
							if (flag13)
							{
								bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag14)
								{
									Log.Message(string.Format("{0} {1}: HasExitOrNonCombatJob. Start survivalDecisions job.", pawn, pawn.Position));
								}
								pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
							}
						}
						else
						{
							bool flag15 = !flag11;
							if (flag15)
							{
								Job job2 = AdvancedAI_Jobs.EvacuateNearestAllyJob(pawn, 10f);
								bool flag16 = job2 != null;
								if (flag16)
								{
									bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag17)
									{
										Log.Message(string.Format("{0} {1} HasExitOrNonCombatJob. Start evacuateNearest job.", pawn, pawn.Position));
									}
									pawn.jobs.StartJob(job2, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
								}
							}
						}
					}
					List<Pawn> list2 = new List<Pawn>();
					List<Thing> list;
					Thing thing2;
					bool flag18 = AdvancedAI.ActiveThreat(pawn, SkyAiCore.Settings.nonCombatActiveThreatRange, true, isInTarget, true, true, true, false, false, out thing2, out list) && !AdvancedAI.PawnIsGuard(pawn);
					if (flag18)
					{
						AdvancedAI_Classes.<>c__DisplayClass15_1 CS$<>8__locals2;
						AdvancedAI_TendUtility.RequireTreatment(pawn, out CS$<>8__locals2.pawnInjurySeverity);
						bool flag19 = !AdvancedAI.PawnShouldEscapeCombat(pawn) && AdvancedAI_Classes.<ReactToThreats>g__activePawn|15_0(pawn, ref CS$<>8__locals2) && AdvancedAI.PrimaryWeapon(pawn) != null;
						if (flag19)
						{
							bool flag20 = pawn.mindState != null && pawn.mindState.mentalStateHandler.InMentalState;
							if (flag20)
							{
								pawn.mindState.mentalStateHandler.Reset();
							}
							bool debugLog3 = SkyAiCore.Settings.debugLog;
							if (debugLog3)
							{
								Log.Message(string.Format("{0} {1}: ActiveThreat. Added to helperAlliesList1.", pawn, pawn.Position));
							}
							list2.Add(pawn);
							AdvancedAI_SquadUtility.ChangeSquadDefenceStatus(pawn.Position, lord, raidData, true);
							bool flag21 = mapComponent_SkyAI != null && mapComponent_SkyAI.lordtoils != null && !mapComponent_SkyAI.lordtoils.ContainsKey(pawn) && pawn.GetLord().CurLordToil != null;
							if (flag21)
							{
								mapComponent_SkyAI.lordtoils.Add(pawn, pawn.GetLord().CurLordToil);
							}
						}
						else
						{
							AdvancedAI_LordUtility.PawnAddExitLord(pawn, true);
							bool debugLog4 = SkyAiCore.Settings.debugLog;
							if (debugLog4)
							{
								Log.Message(string.Format("{0} {1}: ActiveThreat. Decided to panic flee1.", pawn, pawn.Position));
							}
							bool flag22 = pawn.jobs.curJob != null;
							if (flag22)
							{
								pawn.jobs.curJob.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
							}
						}
						bool flag23 = !pawn.Downed && lord == null;
						if (flag23)
						{
							IEnumerable<Pawn> enumerable = AdvancedAI.PawnsOfFactionOnMap(pawn);
							bool flag24 = !enumerable.EnumerableNullOrEmpty<Pawn>();
							if (flag24)
							{
								foreach (Pawn pawn2 in from p in enumerable
								where !AdvancedAI.PawnIsGuard(p)
								select p)
								{
									bool flag25 = !list2.Contains(pawn2) && pawn2.GetLord() == null;
									if (flag25)
									{
										AdvancedAI_Classes.<>c__DisplayClass15_2 CS$<>8__locals3;
										AdvancedAI_TendUtility.RequireTreatment(pawn2, out CS$<>8__locals3.allyInjurySeverity);
										bool flag26 = !AdvancedAI.PawnShouldEscapeCombat(pawn2) && pawn.Position.DistanceTo(pawn2.Position) < SkyAiCore.Settings.areaToReactAllies && AdvancedAI_Classes.<ReactToThreats>g__activePawn2|15_2(pawn2, ref CS$<>8__locals3) && AdvancedAI.PrimaryWeapon(pawn2) != null;
										if (flag26)
										{
											bool flag27 = pawn2.mindState != null && pawn2.mindState.mentalStateHandler.InMentalState;
											if (flag27)
											{
												pawn2.mindState.mentalStateHandler.Reset();
											}
											bool debugLog5 = SkyAiCore.Settings.debugLog;
											if (debugLog5)
											{
												Log.Message(string.Format("{0} {1}: ActiveThreat. Added to helperAlliesList2.", pawn2, pawn2.Position));
											}
											list2.Add(pawn2);
										}
										else
										{
											AdvancedAI_LordUtility.PawnAddExitLord(pawn, true);
											bool debugLog6 = SkyAiCore.Settings.debugLog;
											if (debugLog6)
											{
												Log.Message(string.Format("{0} {1}: ActiveThreat. Decided to panic flee2.", pawn2, pawn2.Position));
											}
											bool flag28 = pawn2.jobs != null && pawn2.jobs.curJob != null;
											if (flag28)
											{
												pawn2.jobs.curJob.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
											}
										}
									}
								}
							}
							bool flag29 = lord != null && lord.ownedPawns.Count > 0;
							if (flag29)
							{
								List<Pawn> list3 = new List<Pawn>();
								for (int i = 0; i < lord.ownedPawns.Count; i++)
								{
									Pawn pawn3 = lord.ownedPawns[i];
									bool flag30 = pawn3 != pawn && AdvancedAI_Classes.<ReactToThreats>g__shouldDefend|15_3(pawn3);
									if (flag30)
									{
										bool flag31 = pawn3.mindState != null && pawn3.mindState.mentalStateHandler.InMentalState;
										if (flag31)
										{
											pawn3.mindState.mentalStateHandler.Reset();
										}
										MapComponent_SkyAI mapComponent_SkyAI2 = AdvancedAI_Classes.MapComp(pawn3);
										bool flag32 = mapComponent_SkyAI2 != null && mapComponent_SkyAI2.lordtoils != null && !mapComponent_SkyAI2.lordtoils.ContainsKey(pawn3) && pawn3.GetLord().CurLordToil != null;
										if (flag32)
										{
											mapComponent_SkyAI2.lordtoils.Add(pawn3, pawn3.GetLord().CurLordToil);
											list3.Add(pawn3);
										}
									}
								}
								for (int j = 0; j < list3.Count; j++)
								{
									Pawn pawn4 = list3[j];
									AdvancedAI_LordUtility.PawnAddDefendLordToil(pawn4, thing2.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
									pawn4.mindState.duty = new PawnDuty(DutyDefOf.Defend, pawn.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
									pawn4.mindState.enemyTarget = thing2;
									bool flag33 = pawn4.jobs != null && pawn4.jobs.curJob != null;
									if (flag33)
									{
										bool flag34 = (SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn)) || SkyAiCore.SelectedPawnDebug(pawn4);
										if (flag34)
										{
											Log.Message(string.Format("{0} {1}: End curJob: {2}", pawn4, pawn4.Position, pawn4.jobs.curJob));
										}
										pawn4.jobs.StopAll(false, true);
									}
								}
							}
							bool debugLog7 = SkyAiCore.Settings.debugLog;
							if (debugLog7)
							{
								Log.Message(string.Format("ActiveThreat: helperAlliesList1: {0}", list2.Count<Pawn>()));
							}
						}
						bool flag35 = lord != null;
						if (flag35)
						{
							List<Pawn> list4 = new List<Pawn>();
							for (int k = 0; k < lord.ownedPawns.Count; k++)
							{
								Pawn pawn5 = lord.ownedPawns[k];
								bool flag36 = pawn5 != pawn && !AdvancedAI.PawnIsGuard(pawn5);
								if (flag36)
								{
									AdvancedAI_Classes.<>c__DisplayClass15_3 CS$<>8__locals4;
									AdvancedAI_TendUtility.RequireTreatment(pawn5, out CS$<>8__locals4.allyInjurySeverity);
									bool flag37 = !AdvancedAI.PawnShouldEscapeCombat(pawn5) && pawn.Position.DistanceTo(pawn5.Position) < SkyAiCore.Settings.areaToReactAllies && AdvancedAI_Classes.<ReactToThreats>g__activePawn3|15_5(pawn5, ref CS$<>8__locals4) && AdvancedAI.PrimaryWeapon(pawn5) != null;
									if (flag37)
									{
										bool flag38 = pawn5.mindState != null && pawn5.mindState.mentalStateHandler.InMentalState;
										if (flag38)
										{
											pawn5.mindState.mentalStateHandler.Reset();
										}
										bool flag39 = pawn5.mindState.duty != null && AdvancedAI_Classes.<ReactToThreats>g__shouldDefend|15_4(pawn5);
										if (flag39)
										{
											MapComponent_SkyAI mapComponent_SkyAI3 = AdvancedAI_Classes.MapComp(pawn5);
											bool flag40 = mapComponent_SkyAI3 != null && mapComponent_SkyAI3.lordtoils != null && !mapComponent_SkyAI3.lordtoils.ContainsKey(pawn5) && pawn5.GetLord().CurLordToil != null;
											if (flag40)
											{
												mapComponent_SkyAI3.lordtoils.Add(pawn5, pawn5.GetLord().CurLordToil);
												list4.Add(pawn5);
											}
										}
									}
									else
									{
										AdvancedAI_LordUtility.PawnAddExitLord(pawn, true);
										bool debugLog8 = SkyAiCore.Settings.debugLog;
										if (debugLog8)
										{
											Log.Message(string.Format("{0} {1}: ActiveThreat. Decided to panic flee3.", pawn5, pawn5.Position));
										}
										bool flag41 = pawn5.jobs != null && pawn5.jobs.curJob != null;
										if (flag41)
										{
											pawn5.jobs.curJob.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
										}
									}
								}
							}
							for (int l = 0; l < list4.Count; l++)
							{
								Pawn pawn6 = list4[l];
								AdvancedAI_LordUtility.PawnAddDefendLordToil(pawn6, thing2.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
								pawn6.mindState.duty = new PawnDuty(DutyDefOf.Defend, pawn.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
								pawn6.mindState.enemyTarget = thing2;
								bool flag42 = pawn6.jobs != null && pawn6.jobs.curJob != null;
								if (flag42)
								{
									bool flag43 = (SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn)) || SkyAiCore.SelectedPawnDebug(pawn6);
									if (flag43)
									{
										Log.Message(string.Format("{0} {1}: End curJob: {2}", pawn6, pawn6.Position, pawn6.jobs.curJob));
									}
									pawn6.jobs.StopAll(false, true);
								}
								bool flag44 = !list2.Contains(pawn6);
								if (flag44)
								{
									bool debugLog9 = SkyAiCore.Settings.debugLog;
									if (debugLog9)
									{
										Log.Message(string.Format("{0} {1}: ActiveThreat. Added to helperAlliesList3.", pawn6, pawn6.Position));
									}
									list2.Add(pawn6);
								}
							}
						}
					}
					bool flag45 = !list2.NullOrEmpty<Pawn>() && list2.Contains(pawn);
					if (flag45)
					{
						List<Pawn> list5 = new List<Pawn>();
						foreach (Pawn listPawn in list2)
						{
							AdvancedAI_Classes.<>c__DisplayClass15_4 CS$<>8__locals5;
							CS$<>8__locals5.listPawn = listPawn;
							bool flag46 = CS$<>8__locals5.listPawn.mindState.duty != null && AdvancedAI_Classes.<ReactToThreats>g__shouldDefend|15_6(CS$<>8__locals5.listPawn, ref CS$<>8__locals5);
							if (flag46)
							{
								bool flag47 = CS$<>8__locals5.listPawn.mindState != null && CS$<>8__locals5.listPawn.mindState.mentalStateHandler.InMentalState;
								if (flag47)
								{
									CS$<>8__locals5.listPawn.mindState.mentalStateHandler.Reset();
								}
								MapComponent_SkyAI mapComponent_SkyAI4 = AdvancedAI_Classes.MapComp(CS$<>8__locals5.listPawn);
								bool flag48 = mapComponent_SkyAI4 != null && mapComponent_SkyAI4.lordtoils != null && !mapComponent_SkyAI4.lordtoils.ContainsKey(CS$<>8__locals5.listPawn) && CS$<>8__locals5.listPawn.GetLord().CurLordToil != null;
								if (flag48)
								{
									mapComponent_SkyAI4.lordtoils.Add(CS$<>8__locals5.listPawn, CS$<>8__locals5.listPawn.GetLord().CurLordToil);
									list5.Add(CS$<>8__locals5.listPawn);
								}
							}
						}
						foreach (Pawn pawn7 in list5)
						{
							AdvancedAI_LordUtility.PawnAddDefendLordToil(pawn7, thing2.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
							pawn7.mindState.duty = new PawnDuty(DutyDefOf.Defend, pawn.Position, SkyAiCore.Settings.nonCombatActiveThreatRange);
							pawn7.mindState.enemyTarget = thing2;
							bool flag49 = pawn7.jobs != null && pawn7.jobs.curJob != null;
							if (flag49)
							{
								bool flag50 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn7));
								if (flag50)
								{
									Log.Message(string.Format("{0} {1}: End curJob: {2}", pawn7, pawn7.Position, pawn7.jobs.curJob));
								}
								pawn7.jobs.StopAll(false, true);
							}
						}
					}
				}
				bool flag51 = mapComponent_SkyAI != null && !mapComponent_SkyAI.lordtoils.EnumerableNullOrEmpty<KeyValuePair<Pawn, LordToil>>() && mapComponent_SkyAI.lordtoils.ContainsKey(pawn);
				if (flag51)
				{
					Thing thing;
					List<Thing> list;
					bool flag52 = (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.Defend) ? (!AdvancedAI.ActiveThreatInArea(pawn, pawn.mindState.duty.focus.Cell, pawn.Map, SkyAiCore.Settings.nonCombatActiveThreatRange, 12, out thing)) : (!AdvancedAI.ActiveThreat(pawn, SkyAiCore.Settings.nonCombatActiveThreatRange, false, false, true, true, true, false, false, out thing, out list));
					if (flag52)
					{
						KeyValuePair<Pawn, LordToil> keyValuePair = (from pValue in mapComponent_SkyAI.lordtoils
						where pawn == pValue.Key
						select pValue).FirstOrDefault<KeyValuePair<Pawn, LordToil>>();
						LordToil value = keyValuePair.Value;
						bool flag53 = value != null && lord.CurLordToil != value;
						if (flag53)
						{
							bool debugLog10 = SkyAiCore.Settings.debugLog;
							if (debugLog10)
							{
								Log.Message(string.Format("{0} {1}: Return back saved previous lordToil: {2}", pawn, pawn.Position, value));
							}
							AdvancedAI_LordUtility.PawnRemoveDefendLordToil(pawn, value);
						}
						mapComponent_SkyAI.lordtoils.Remove(keyValuePair.Key);
						IntVec3 point;
						bool flag54 = AdvancedAI_SquadUtility.IsSquadInDefence(lord, raidData, out point);
						if (flag54)
						{
							AdvancedAI_SquadUtility.ChangeSquadDefenceStatus(point, lord, raidData, false);
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("{0} {1}: ReactToThreats exception: {2}", pawn, pawn.Position, arg));
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0001CFE8 File Offset: 0x0001B1E8
		public static void AdvancedExplosionDetection(Pawn pawn)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = AdvancedAI.IsSuitablePawn(pawn) && AdvancedAI.IsHumanlikeOnly(pawn) && pawn.GetStatValue(StatDefOf.MoveSpeed, true) >= 2.6f && pawn.RaceProps.baseHealthScale < 5f && AdvancedAI.PlayerPawnNotDraftedOrEnemy(pawn);
				if (flag)
				{
					try
					{
						IEnumerable<IntVec3> enumerable = from c in GenRadial.RadialCellsAround(pawn.Position, SkyAiCore.Settings.explosionRadiusDetection, true)
						where c.InBounds(pawn.Map)
						select c;
						foreach (IntVec3 c2 in enumerable)
						{
							List<Thing> thingList = c2.GetThingList(pawn.Map);
							bool flag2 = !thingList.NullOrEmpty<Thing>();
							if (flag2)
							{
								for (int i = 0; i < thingList.Count<Thing>(); i++)
								{
									bool flag3 = AdvancedAI.IsDangerousCell(thingList[i].Position, pawn.Map);
									if (flag3)
									{
										AdvancedAI.Notify_DangerousExploderAboutToExplode(pawn, thingList[i]);
										Job job = AdvancedAI_Jobs.EscapeExplosionJob(pawn);
										bool flag4 = job != null;
										if (flag4)
										{
											bool flag5 = SkyAiCore.Settings.debugFleeExplosion && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag5)
											{
												Log.Message(string.Format("{0} {1}: Detected dangerous exploder: {2}. Run run run!", pawn, pawn.Position, thingList[i]));
												pawn.Map.debugDrawer.FlashCell(thingList[i].Position, 0.9f, "EXP", SkyAiCore.Settings.flashCellDelay);
											}
											pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
										}
										break;
									}
									bool flag6 = pawn.IsHashIntervalTick(60) && SkyAiCore.Settings.debugFleeExplosion && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag6)
									{
										Log.Message(string.Format("{0} {1}: Exploder null", pawn, pawn.Position));
									}
									pawn.mindState.knownExploder = null;
								}
							}
						}
					}
					catch (Exception arg)
					{
						Log.Error(string.Format("{0} {1} : AdvancedExplosionDetection exception: {2}", pawn, pawn.Position, arg));
					}
				}
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0001D304 File Offset: 0x0001B504
		public static void AdvancedFriendlyFireUtility(Pawn pawn)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = pawn.IsHashIntervalTick(32) && SkyAiCore.Settings.enableAdvancedFriendlyFireUtility;
				if (flag)
				{
					bool flag2 = pawn.CurJobDef == JobDefOf.UseVerbOnThing || pawn.CurJobDef == JobDefOf.UseVerbOnThingStatic;
					if (flag2)
					{
						LocalTargetInfo targetA = pawn.CurJob.targetA;
						bool flag3 = targetA != null;
						if (flag3)
						{
							bool flag4 = AdvancedAI.IsSuitablePawn(pawn) && pawn.RaceProps.intelligence >= Intelligence.ToolUser && pawn.Faction != null && !pawn.Faction.IsPlayer;
							if (flag4)
							{
								Verb verb = AdvancedAI.PrimaryVerb(pawn);
								bool flag5 = AdvancedAI.PrimaryWeapon(pawn) != null && verb != null;
								if (flag5)
								{
									try
									{
										bool flag6 = (AdvancedAI_TraitUtility.HasDumbTrait(pawn) && Rand.Chance(SkyAiCore.Settings.chanceDumbIgnore)) || !AdvancedAI_TraitUtility.HasDumbTrait(pawn);
										bool flag7 = (!SkyAiCore.Settings.dumbIgnore || (SkyAiCore.Settings.dumbIgnore && flag6)) && verb.verbProps.ai_AvoidFriendlyFireRadius > 0f && verb.WarmupTicksLeft <= 90 && verb.WarmupTicksLeft != 0;
										if (flag7)
										{
											bool flag8 = targetA.Thing is Building;
											if (flag8)
											{
												bool flag9 = AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, targetA.Thing, AdvancedAI.MinDistance(pawn, null), 0f);
												if (flag9)
												{
													bool flag10 = (SkyAiCore.Settings.debugLog || SkyAiCore.Settings.debugTargets) && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag10)
													{
														Log.Message(string.Format("{0} {1}: FriendlyFire Detected. Stopped all jobs.", pawn, pawn.Position));
														pawn.Map.debugDrawer.FlashCell(targetA.Cell, 0.4f, "FF3", SkyAiCore.Settings.flashCellDelay);
													}
													pawn.jobs.StopAll(false, true);
												}
											}
											else
											{
												bool flag11 = AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, targetA.Thing, AdvancedAI.MinDistance(pawn, null), 0f);
												if (flag11)
												{
													bool flag12 = (SkyAiCore.Settings.debugLog || SkyAiCore.Settings.debugTargets) && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag12)
													{
														Log.Message(string.Format("{0} {1}: FriendlyFire Detected. Stopped all jobs.", pawn, pawn.Position));
														pawn.Map.debugDrawer.FlashCell(targetA.Cell, 0.4f, "FF3", SkyAiCore.Settings.flashCellDelay);
													}
													pawn.jobs.StopAll(false, true);
												}
											}
										}
									}
									catch (Exception arg)
									{
										Log.Error(string.Format("{0} {1} : AdvancedFriendlyFireUtility exception: {2}", pawn, pawn.Position, arg));
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0001D620 File Offset: 0x0001B820
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__activePawn|15_0(Pawn p, ref AdvancedAI_Classes.<>c__DisplayClass15_1 A_1)
		{
			return A_1.pawnInjurySeverity < AdvancedAI_TendUtility.InjurySeverity.extreme || Rand.Chance(0.2f);
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0001D638 File Offset: 0x0001B838
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__activePawn2|15_2(Pawn p, ref AdvancedAI_Classes.<>c__DisplayClass15_2 A_1)
		{
			return A_1.allyInjurySeverity < AdvancedAI_TendUtility.InjurySeverity.extreme || Rand.Chance(0.2f);
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0001D650 File Offset: 0x0001B850
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__shouldDefend|15_3(Pawn p)
		{
			return p.jobs != null && p.jobs.curJob != null && p.jobs.curJob.exitMapOnArrival;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0001D650 File Offset: 0x0001B850
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__shouldDefend|15_4(Pawn p)
		{
			return p.jobs != null && p.jobs.curJob != null && p.jobs.curJob.exitMapOnArrival;
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0001D67A File Offset: 0x0001B87A
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__activePawn3|15_5(Pawn p, ref AdvancedAI_Classes.<>c__DisplayClass15_3 A_1)
		{
			return A_1.allyInjurySeverity < AdvancedAI_TendUtility.InjurySeverity.extreme || Rand.Chance(0.2f);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0001D692 File Offset: 0x0001B892
		[CompilerGenerated]
		internal static bool <ReactToThreats>g__shouldDefend|15_6(Pawn p, ref AdvancedAI_Classes.<>c__DisplayClass15_4 A_1)
		{
			return A_1.listPawn.jobs != null && A_1.listPawn.jobs.curJob != null && A_1.listPawn.jobs.curJob.exitMapOnArrival;
		}
	}
}
