using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200005B RID: 91
	public class JobGiver_SiegeAI : ThinkNode_JobGiver
	{
		// Token: 0x060002D8 RID: 728 RVA: 0x000385CC File Offset: 0x000367CC
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			return (JobGiver_SiegeAI)base.DeepCopy(resolve);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x000385EC File Offset: 0x000367EC
		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 intVec = pawn.mindState.duty.focus.Cell;
			bool flag = SkyAiCore.Settings.debugDisableSkyAI || pawn.RaceProps.Animal || pawn.RaceProps.intelligence == Intelligence.Animal || (pawn.Faction != null && pawn.Faction.def.techLevel == TechLevel.Animal);
			Job result;
			if (flag)
			{
				bool flag2 = intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom(RegionType.Set_All) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable);
				if (flag2)
				{
					pawn.GetLord().Notify_ReachedDutyLocation(pawn);
					result = null;
				}
				else
				{
					bool flag3 = !intVec.IsValid;
					if (flag3)
					{
						IAttackTarget attackTarget;
						bool flag4 = !(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
						where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer && pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings)
						select x).TryRandomElement(out attackTarget);
						if (flag4)
						{
							return null;
						}
						intVec = attackTarget.Thing.Position;
					}
					bool flag5 = !pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings);
					if (flag5)
					{
						result = null;
					}
					else
					{
						using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false), PathEndMode.OnCell, null))
						{
							IntVec3 cellBeforeBlocker;
							Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
							bool flag6 = thing != null;
							if (flag6)
							{
								Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, SkyAiCore.Settings.canMineMineables, SkyAiCore.Settings.canMineNonMineables);
								bool flag7 = job != null;
								if (flag7)
								{
									return job;
								}
							}
						}
						result = JobMaker.MakeJob(JobDefOf.Goto, intVec, 500, true);
					}
				}
			}
			else
			{
				bool flag8 = AdvancedAI.HasDefendBaseDuty(pawn);
				if (flag8)
				{
					bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag9)
					{
						Log.Message(string.Format("{0} {1}: SiegeAI. Pawn has defend base duty. Goes to null.", pawn, pawn.Position));
					}
					result = null;
				}
				else
				{
					bool flag10 = intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom(RegionType.Set_Passable) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable);
					if (flag10)
					{
						pawn.GetLord().Notify_ReachedDutyLocation(pawn);
						bool flag11 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag11)
						{
							Log.Message(string.Format("{0} {1}: SiegeAI reached duty. Goes to null.", pawn, pawn.Position));
						}
						result = null;
					}
					else
					{
						bool flag12 = !intVec.IsValid;
						if (flag12)
						{
							IAttackTarget attackTarget2 = AdvancedAI.AttackTarget(pawn);
							bool flag13 = attackTarget2 == null;
							if (flag13)
							{
								bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag14)
								{
									Log.Message(string.Format("{0} {1} No any enemy target found on map. JobGiver goes to null.", pawn, pawn.Position));
								}
								bool flag15 = pawn.CurJob != null && !pawn.CurJob.exitMapOnArrival;
								bool flag16 = AdvancedAI.IsHumanlikeOnly(pawn);
								if (flag16)
								{
									bool flag17 = pawn.mindState.duty != null && (pawn.mindState.duty.def == DutyDefOf.AssaultColony || pawn.mindState.duty.def == DutyDefOf.Sapper);
									bool flag18 = (pawn.mindState.duty == null || flag17) && flag15;
									if (flag18)
									{
										Job job2 = AdvancedAI_Jobs.StealDecisions(pawn, true);
										bool flag19 = job2 != null;
										if (flag19)
										{
											return job2;
										}
									}
								}
								else
								{
									bool flag20 = pawn.mindState.duty != null && (pawn.mindState.duty.def == DutyDefOf.AssaultColony || pawn.mindState.duty.def == DutyDefOf.Sapper);
									bool flag21 = (pawn.mindState.duty == null || flag20) && flag15;
									if (flag21)
									{
										Job exitJob = AdvancedAI_Jobs.GetExitJob(pawn, true, true, false);
										bool flag22 = exitJob != null;
										if (flag22)
										{
											return exitJob;
										}
									}
								}
								return null;
							}
							intVec = attackTarget2.Thing.Position;
						}
						bool flag23 = !pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings);
						if (flag23)
						{
							bool flag24 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag24)
							{
								Log.Message(string.Format("{0} {1}: SiegeAI. CanReach maintarget failed. JobGiver goes to null.", pawn, pawn.Position));
							}
							result = null;
						}
						else
						{
							Job job3 = AdvancedAI_Jobs.SurvivalDecisions(pawn, intVec);
							bool flag25 = job3 != null;
							if (flag25)
							{
								result = job3;
							}
							else
							{
								RaidData raidData = AdvancedAI.PawnRaidData(pawn);
								bool flag26 = raidData != null;
								if (flag26)
								{
									Job job4 = AdvancedAI_Jobs.WaitSquadCoverJob(pawn, intVec, raidData);
									bool flag27 = job4 != null;
									if (flag27)
									{
										return job4;
									}
								}
								bool flag28 = pawn.IsHashIntervalTick(10);
								if (flag28)
								{
									Job job5 = AdvancedAI_Jobs.StealDecisions(pawn, false);
									bool flag29 = job5 != null;
									if (flag29)
									{
										return job5;
									}
								}
								using (PawnPath pawnPath2 = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false), PathEndMode.OnCell, null))
								{
									IntVec3 intVec2;
									Thing thing2 = pawnPath2.FirstBlockingBuilding(out intVec2, pawn);
									Verb verb = AdvancedAI.PrimaryVerb(pawn);
									bool flag30 = SkyAiCore.Settings.enableDoctorRole && AdvancedAI.PawnIsDoctor(pawn);
									if (flag30)
									{
										Job job6 = AdvancedAI_Roles.DoctorRole(pawn, intVec2, true);
										bool flag31 = job6 != null;
										if (flag31)
										{
											bool flag32 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag32)
											{
												Log.Message(string.Format("{0} {1}: SiegeAI. Doctor role! CellBlocker focusCell: {2}", pawn, pawn.Position, intVec2));
											}
											return job6;
										}
									}
									bool enableLeaderRole = SkyAiCore.Settings.enableLeaderRole;
									if (enableLeaderRole)
									{
										bool flag33 = raidData != null;
										if (flag33)
										{
											bool flag34 = raidData.raidLeader != null && raidData.raidLeader.Equals(pawn);
											if (flag34)
											{
												bool debugSquadAttackData = SkyAiCore.Settings.debugSquadAttackData;
												if (debugSquadAttackData)
												{
													SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
													bool flag35 = squadData != null;
													if (flag35)
													{
														DebugViewSettings.drawBreachingGrid = true;
														List<Thing> list = squadData.FindBuildingsToAttack(pawn.Faction, 100);
														bool flag36 = !list.NullOrEmpty<Thing>();
														if (flag36)
														{
															foreach (Thing thing3 in list)
															{
																pawn.Map.debugDrawer.FlashCell(thing3.Position, 0f, "X", 150);
															}
															Log.Message(string.Format("{0} {1}: SquadAttackData. building to attack count: {2}", pawn, pawn.Position, list.Count<Thing>()));
														}
													}
												}
												bool flag37 = false;
												CompLeaderRole comp = pawn.GetComp<CompLeaderRole>();
												bool flag38 = comp != null;
												if (flag38)
												{
													flag37 = comp.generatedStage;
												}
												bool flag39 = flag37 && raidData != null && !raidData.squadsFormed && !comp.skipStage;
												if (flag39)
												{
													bool debugLog = SkyAiCore.Settings.debugLog;
													if (debugLog)
													{
														Log.Message(string.Format("{0} {1}: Siege AI. Stage generated. Start forming squads: {2}. skipStage: {3}", new object[]
														{
															pawn,
															pawn.Position,
															SkyAiCore.Settings.enableRaidLeaderGathersTroopsNearColony,
															comp.skipStage
														}));
													}
													bool enableRaidLeaderGathersTroopsNearColony = SkyAiCore.Settings.enableRaidLeaderGathersTroopsNearColony;
													if (enableRaidLeaderGathersTroopsNearColony)
													{
														AdvancedAI_SquadUtility.FormingSquadsAndGatherPoints(pawn, intVec, raidData);
														foreach (SquadData squadData2 in raidData.squads)
														{
															Pawn squadCommander = squadData2.squadCommander;
															LordJob_StageAttack lordJob_StageAttack = new LordJob_StageAttack(squadCommander, pawn.Faction, squadData2.gatherSpot, new IntRange(8000, 12000), comp.skipStage, 1);
															Lord lord = LordMaker.MakeNewLord(pawn.Faction, lordJob_StageAttack, pawn.Map, squadData2.squadPawns);
															squadData2.id = lord.loadID;
															bool debugLog2 = SkyAiCore.Settings.debugLog;
															if (debugLog2)
															{
																Log.Message(string.Format("{0} {1}: Siege AI. Make new lord: {2}. LordJob: {3} with spot: {4}", new object[]
																{
																	pawn,
																	pawn.Position,
																	lord,
																	lordJob_StageAttack,
																	squadData2.gatherSpot
																}));
															}
														}
													}
												}
												bool flag40 = SkyAiCore.Settings.enableMainBlowSiegeTactic && AdvancedAI_Classes.MapComp(pawn).mainBlowCells.NullOrEmpty<IntVec3>();
												if (flag40)
												{
													int num = AdvancedAI_Classes.MapComp(pawn).mainBlowDelay;
													bool flag41 = comp != null && num <= 0 && pawn.GetLord().ownedPawns.Count >= 25;
													if (flag41)
													{
														IntVec3 blockerCell = comp.blockerCell;
														bool flag42 = !comp.blockerCell.IsValid || comp.blockerCell == IntVec3.Invalid || comp.blockerCell == new IntVec3(0, 0, 0);
														if (flag42)
														{
															Building building12 = null;
															Building building2 = thing2 as Building;
															bool flag43 = building2 != null;
															if (flag43)
															{
																List<Building> list2 = AdvancedAI.ConnectedClosestBuildings(pawn, building2, 35, true);
																foreach (Building building3 in list2)
																{
																	bool flag44 = false;
																	foreach (IntVec3 c in GenRadial.RadialCellsAround(building3.Position, 2f, true))
																	{
																		Pawn firstPawn = c.GetFirstPawn(pawn.Map);
																		bool flag45 = firstPawn != null && firstPawn.Faction != null && !firstPawn.HostileTo(pawn);
																		if (flag45)
																		{
																			flag44 = true;
																		}
																	}
																	bool flag46 = !flag44;
																	if (flag46)
																	{
																		building12 = building3;
																	}
																}
															}
															bool flag47 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
															if (flag47)
															{
																Log.Message(string.Format("{0} {1}: SiegeAI. Pawn is Leader. Added mainBLow tactic data to comps: focus cell: {2} enemy target: {3}", new object[]
																{
																	pawn,
																	pawn.Position,
																	intVec2,
																	intVec
																}));
															}
															num = Rand.RangeInclusive(100, 300);
															bool flag48 = building12 != null;
															if (flag48)
															{
																comp.blockerCell = building12.Position;
																comp.leaderEnemyTarget = intVec;
															}
															else
															{
																comp.blockerCell = thing2.Position;
																comp.leaderEnemyTarget = intVec;
															}
														}
													}
												}
												Job job7 = AdvancedAI_Roles.LeaderRole(pawn, intVec2);
												bool flag49 = job7 != null;
												if (flag49)
												{
													return job7;
												}
											}
											bool flag50 = raidData.squadCommanders.Contains(pawn);
											if (flag50)
											{
												Job job8 = AdvancedAI_Roles.SquadCommanderRole(pawn, intVec2);
												bool flag51 = job8 != null;
												if (flag51)
												{
													return job8;
												}
											}
										}
									}
									bool flag52 = SkyAiCore.Settings.enableSniperRole && AdvancedAI.PawnIsSniper(pawn);
									if (flag52)
									{
										Job job9 = AdvancedAI_Roles.SniperRole(pawn, intVec2);
										bool flag53 = job9 != null;
										if (flag53)
										{
											return job9;
										}
									}
									bool flag54 = AdvancedAI.TryToSwitchToSiegeWeapon(pawn);
									if (flag54)
									{
										verb = AdvancedAI.PrimaryVerb(pawn);
									}
									bool flag55 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag55)
									{
										bool flag56 = AdvancedAI.PawnInExclusionList(pawn);
										if (flag56)
										{
											Log.Message(string.Format("{0} {1}: SiegeAI. Notice! Pawn in destroyer's exclusion list.", pawn, pawn.Position));
										}
									}
									bool flag57 = pawn.IsHashIntervalTick(SkyAiCore.Settings.destroyersExclusionDelay);
									if (flag57)
									{
										bool flag58 = AdvancedAI_Classes.MapComp(pawn).destroyersExclusions.Contains(pawn);
										if (flag58)
										{
											AdvancedAI_Classes.MapComp(pawn).destroyersExclusions.Remove(pawn);
										}
									}
									bool flag59 = false;
									bool flag60 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag60)
									{
										IntVec3 intVec3 = (thing2 != null) ? thing2.PositionHeld : IntVec3.Invalid;
										Log.Message(string.Format("{0} {1}: Check destroyer role. Role enabled: {2} With siege weapon: {3} Is not in exclusion list: {4} Sufficient range : {5} mainTarget: {6} {7} cellBeforeBlocker: {8} Closer to goal position: {9}", new object[]
										{
											pawn,
											pawn.Position,
											SkyAiCore.Settings.enableDestroyerRole,
											AdvancedAI.PrimaryIsSiegeWeapon(pawn),
											!AdvancedAI.PawnInExclusionList(pawn),
											verb.verbProps.range,
											thing2,
											intVec3,
											intVec2,
											AdvancedAI.PawnCloserToGoalPosition(pawn, intVec2)
										}));
									}
									bool flag61 = SkyAiCore.Settings.enableDestroyerRole && AdvancedAI.PawnCloserToGoalPosition(pawn, intVec2) && AdvancedAI.PrimaryIsSiegeWeapon(pawn) && !AdvancedAI.PawnInExclusionList(pawn) && verb.verbProps.range > 6f;
									if (flag61)
									{
										flag59 = true;
										verb = AdvancedAI.PrimaryVerb(pawn);
										bool flag62 = pawn.Position.DistanceTo(intVec) < Mathf.Max(verb.verbProps.range, 60f);
										if (flag62)
										{
											int num2 = SkyAiCore.Settings.scaleRaidWidthByEnemyRaidCount ? AdvancedAI.RaidWidth(pawn.GetLord().ownedPawns.Count) : 25;
											List<Building> list3 = AdvancedAI.PotencialBuildingTrashList(intVec2, pawn, verb, (float)num2, 10);
											bool flag63 = !list3.NullOrEmpty<Building>();
											if (flag63)
											{
												bool grenadeWeapon = AdvancedAI.PrimaryIsGrenade(pawn);
												Building building4 = null;
												foreach (Building building5 in list3)
												{
													bool flag64 = AdvancedAI.IsGoodTarget(pawn, pawn.Position, building5, verb, grenadeWeapon);
													if (flag64)
													{
														building4 = building5;
														break;
													}
												}
												bool flag65 = building4 != null;
												if (flag65)
												{
													bool flag66 = SkyAiCore.Settings.enableMainBlowSiegeTactic && !AdvancedAI.CellReservedForMainBlow(pawn).NullOrEmpty<IntVec3>() && AdvancedAI.CellReservedForMainBlow(pawn).Contains(pawn.Position);
													if (flag66)
													{
														IntVec3 intVec4 = AdvancedAI.NewCellPositionNearBuilding(pawn, building4, verb, AdvancedAI.PrimaryEffectiveWeaponRange(pawn, 0f, 1f, false, 25), false);
														bool flag67 = intVec4 != pawn.Position;
														if (flag67)
														{
															bool flag68 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
															if (flag68)
															{
																Log.Message(string.Format("{0} {1}: SiegeAI. Pawn in mainblow area! goto5 {2} near building : {3} {4}", new object[]
																{
																	pawn,
																	pawn.Position,
																	intVec4,
																	building4,
																	building4.Position
																}));
																pawn.Map.debugDrawer.FlashCell(intVec4, 0.5f, "GT5", SkyAiCore.Settings.flashCellDelay);
															}
															Job job10 = JobMaker.MakeJob(JobDefOf.Goto, intVec4);
															job10.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
															job10.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
															job10.checkOverrideOnExpire = true;
															return job10;
														}
													}
													bool flag69 = !AdvancedAI.AlreadyHasSameJob(pawn, JobDefOf.UseVerbOnThing);
													if (flag69)
													{
														bool flag70 = !verb.IsMeleeAttack && verb.Available();
														if (flag70)
														{
															bool flag71 = SkyAiCore.SelectedPawnDebug(pawn);
															if (flag71)
															{
																bool debugPath = SkyAiCore.Settings.debugPath;
																if (debugPath)
																{
																	Log.Message(string.Format("{0} {1}: SiegeAI. Current position fine. Near building : {2} {3} Distance to building: {4}", new object[]
																	{
																		pawn,
																		pawn.Position,
																		building4,
																		building4.Position,
																		pawn.Position.DistanceTo(building4.Position)
																	}));
																	pawn.Map.debugDrawer.FlashCell(pawn.Position, 0.5f, "GT6", SkyAiCore.Settings.flashCellDelay);
																}
																bool debugTargets = SkyAiCore.Settings.debugTargets;
																if (debugTargets)
																{
																	Log.Message(string.Format("{0} {1}: SiegeAI. attack building: {2} {3} Shootline: {4}", new object[]
																	{
																		pawn,
																		pawn.Position,
																		building4,
																		building4.Position,
																		AdvancedAI.TryFindShootlineFromTo(pawn.Position, building4, verb)
																	}));
																	pawn.Map.debugDrawer.FlashCell(building4.Position, 0.7f, "SB1", SkyAiCore.Settings.flashCellDelay);
																}
															}
															bool ai_IsWeapon = verb.verbProps.ai_IsWeapon;
															Job job11 = JobMaker.MakeJob(ai_IsWeapon ? JobDefOf.AttackStatic : JobDefOf.UseVerbOnThing, building4);
															job11.verbToUse = verb;
															bool flag72 = ai_IsWeapon;
															if (flag72)
															{
																job11.maxNumStaticAttacks = 1;
																job11.endIfCantShootTargetFromCurPos = true;
															}
															return job11;
														}
														Job result2 = JobMaker.MakeJob(JobDefOf.AttackMelee, building4);
														bool flag73 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag73)
														{
															Log.Message(string.Format("{0} {1}: SiegeAI. attack building melee: {2} {3} Shootline: {4}", new object[]
															{
																pawn,
																pawn.Position,
																building4,
																building4.Position,
																AdvancedAI.TryFindShootlineFromTo(pawn.Position, building4, verb)
															}));
															pawn.Map.debugDrawer.FlashCell(building4.Position, 0.7f, "SB2", SkyAiCore.Settings.flashCellDelay);
														}
														return result2;
													}
													else
													{
														bool flag74 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag74)
														{
															Log.Message(string.Format("{0} {1}: SiegeAI. Failed to attack with verb building job. Check next.", pawn, pawn.Position));
														}
													}
												}
												bool flag75 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag75)
												{
													bool flag76 = building4 == null;
													Log.Message(string.Format("{0} {1}: SiegeAI. Failed to find any potencial building for attack. Building null: {2}. Check next.", pawn, pawn.Position, flag76));
												}
											}
											bool flag77 = list3.NullOrEmpty<Building>();
											if (flag77)
											{
												bool flag78 = thing2 != null && thing2 is Building;
												if (flag78)
												{
													bool flag79 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag79)
													{
														Log.Message(string.Format("{0} {1}: SiegeAI. Potencial trashList is null. Add maintarget building: {2} {3}", new object[]
														{
															pawn,
															pawn.Position,
															thing2,
															thing2.Position
														}));
													}
													list3.Add(thing2 as Building);
												}
											}
											Building building6 = (from b in list3
											where pawn.CanReach(b, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn)
											select b).FirstOrDefault<Building>();
											List<Building> list4 = null;
											list4 = (from b in AdvancedAI.ConnectedClosestBuildings(pawn, building6, 35, true)
											where b != null && !b.Destroyed
											select b).ToList<Building>();
											bool flag80 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag80)
											{
												Log.Message(string.Format("{0} {1}: SiegeAI. Use ConnectedClosestBuildings. Found building count: {2} near: {3} {4} ", new object[]
												{
													pawn,
													pawn.Position,
													list4.Count<Building>(),
													building6,
													building6.Position
												}));
											}
											bool flag81 = SkyAiCore.Settings.debugConnectedCells && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag81)
											{
												bool flag82 = list4 != null;
												if (flag82)
												{
													foreach (Building building7 in list4)
													{
														pawn.Map.debugDrawer.FlashCell(building7.Position, 0.9f, "CC", SkyAiCore.Settings.flashCellDelay);
													}
												}
											}
											Building building8 = null;
											try
											{
												bool flag83 = !list4.NullOrEmpty<Building>();
												if (flag83)
												{
													IEnumerable<Building> enumerable = from building in list4
													where building != null && AdvancedAI.SaveDistanceToBuilding(pawn, pawn.Position, building) && AdvancedAI.CanHitBuilding(pawn, building.Position, building, 1f) && pawn.CanReserve(building, 1, -1, null, false)
													select building;
													bool flag84 = !enumerable.EnumerableNullOrEmpty<Building>();
													if (flag84)
													{
														building8 = enumerable.MinBy((Building dist) => pawn.Position.DistanceTo(dist.Position));
													}
													else
													{
														bool flag85 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag85)
														{
															Log.Message(string.Format("{0} {1}: SiegeAI. New building not found :(", pawn, pawn.Position));
														}
													}
												}
												bool flag86 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn) && building8 != null;
												if (flag86)
												{
													Log.Message(string.Format("{0} {1}: SiegeAI. Try to move for new building: {2} {3} Pawn distance to building: {4} ", new object[]
													{
														pawn,
														pawn.Position,
														building8,
														building8.Position,
														pawn.Position.DistanceTo(building8.Position)
													}));
												}
											}
											catch (Exception arg)
											{
												Log.Error(string.Format("{0} {1}: SiegeAI. connectedBuildings new building selection exception: {2}", pawn, pawn.Position, arg));
											}
											bool flag87 = building8 != null;
											if (flag87)
											{
												IntVec3 invalid = IntVec3.Invalid;
												bool flag88 = AdvancedAI.TryFindShootingPosition(pawn, building8, verb, out invalid);
												if (flag88)
												{
													bool flag89 = invalid != pawn.Position;
													if (flag89)
													{
														bool flag90 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag90)
														{
															Log.Message(string.Format("{0} {1}: SiegeAI. goto8 for loc: {2} near building : {3} {4} distance to target: {5} weapon range: {6}", new object[]
															{
																pawn,
																pawn.Position,
																invalid,
																building8,
																building8.Position,
																invalid.DistanceTo(building8.Position),
																AdvancedAI.PrimaryEffectiveWeaponRange(pawn, 0f, 1f, false, 25)
															}));
															pawn.Map.debugDrawer.FlashCell(invalid, 0.5f, "GT8", SkyAiCore.Settings.flashCellDelay);
														}
														Job job12 = JobMaker.MakeJob(JobDefOf.Goto, invalid);
														job12.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
														job12.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
														job12.checkOverrideOnExpire = true;
														return job12;
													}
												}
												IntRange distance = new IntRange(Mathf.RoundToInt(AdvancedAI.MinDistance(pawn, verb)), Mathf.Max(25, Mathf.RoundToInt(AdvancedAI.EffectiveRange(pawn))));
												IntVec3 intVec5 = AdvancedAI.NewCellPositionNearBuilding(pawn, building8, verb, distance, false);
												bool flag91 = intVec5.IsValid && intVec5 != pawn.Position;
												if (flag91)
												{
													bool flag92 = SkyAiCore.SelectedPawnDebug(pawn);
													if (flag92)
													{
														bool debugTargets2 = SkyAiCore.Settings.debugTargets;
														if (debugTargets2)
														{
															pawn.Map.debugDrawer.FlashCell(building8.Position, 0.7f, null, SkyAiCore.Settings.flashCellDelay);
														}
														bool debugPath2 = SkyAiCore.Settings.debugPath;
														if (debugPath2)
														{
															Log.Message(string.Format("{0} {1}: SiegeAI. goto3 for loc: {2} near building : {3} {4} distance to target: {5} weapon range: {6}", new object[]
															{
																pawn,
																pawn.Position,
																intVec5,
																building8,
																building8.Position,
																intVec5.DistanceTo(building8.Position),
																AdvancedAI.PrimaryEffectiveWeaponRange(pawn, 0f, 1f, false, 25)
															}));
															pawn.Map.debugDrawer.FlashCell(intVec5, 0.5f, "GT3", SkyAiCore.Settings.flashCellDelay);
														}
													}
													Job job13 = JobMaker.MakeJob(JobDefOf.Goto, intVec5);
													job13.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
													job13.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
													job13.checkOverrideOnExpire = true;
													return job13;
												}
											}
											bool flag93 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag93)
											{
												Log.Message(string.Format("{0} {1}: SiegeAI. Failed to found new CellPositionNearBuilding. Check next.", pawn, pawn.Position));
											}
											bool enableMainBlowSiegeTactic = SkyAiCore.Settings.enableMainBlowSiegeTactic;
											if (enableMainBlowSiegeTactic)
											{
												bool flag94 = !AdvancedAI.CellReservedForMainBlow(pawn).NullOrEmpty<IntVec3>() && AdvancedAI.CellReservedForMainBlow(pawn).Contains(intVec2) && Rand.Chance(0.5f);
												if (flag94)
												{
													bool flag95 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag95)
													{
														Log.Message(string.Format("{0} {1}: SiegeAI. Chilling on position...", pawn, pawn.Position));
													}
													Job job14 = Rand.Chance(0.5f) ? JobMaker.MakeJob(JobDefOf.Wait_Wander, pawn.Position) : JobMaker.MakeJob(JobDefOf.Wait, pawn.Position);
													job14.checkOverrideOnExpire = true;
													job14.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.veryslow);
													job14.canUseRangedWeapon = true;
													return job14;
												}
											}
											bool flag96 = Rand.Chance(0.8f) && !AdvancedAI.InDangerousCombat(pawn, 35f);
											if (flag96)
											{
												bool flag97 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag97)
												{
													Log.Message(string.Format("{0} {1}: SiegeAI. Chilling on position again. My data: {2} Weapon: {3} List count: {4} pawns", new object[]
													{
														pawn,
														pawn.Position,
														pawn.Position,
														AdvancedAI.PrimaryWeapon(pawn),
														AdvancedAI_Classes.MapComp(pawn).destroyersExclusions.Count
													}));
												}
												Job job15 = Rand.Chance(0.5f) ? JobMaker.MakeJob(JobDefOf.Wait_Wander, pawn.Position) : JobMaker.MakeJob(JobDefOf.Wait, pawn.Position);
												job15.checkOverrideOnExpire = true;
												job15.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.veryslow);
												job15.canUseRangedWeapon = true;
												return job15;
											}
											Job coverOrMeleeJob = AdvancedAI_Jobs.GetCoverOrMeleeJob(pawn, verb, thing2, intVec2);
											bool flag98 = coverOrMeleeJob != null;
											if (flag98)
											{
												bool flag99 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag99)
												{
													Log.Message(string.Format("{0} {1}: SiegeAI. start using GetCoverOrMeleeJobForDestroyersJob.", pawn, pawn.Position));
												}
												return coverOrMeleeJob;
											}
											bool flag100 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag100)
											{
												Log.Message(string.Format("{0} {1}: SiegeAI. Still not found a job!", pawn, pawn.Position));
											}
										}
										else
										{
											Job job16 = AdvancedAI_Jobs.MeleeOrWaitJob(pawn, thing2, intVec2);
											bool flag101 = job16 != null;
											if (flag101)
											{
												bool flag102 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag102)
												{
													Log.Message(string.Format("{0} {1}: SiegeAI. Too far from the target moving towards the target: {2}", pawn, pawn.Position, thing2));
													pawn.Map.debugDrawer.FlashCell(intVec2, 0.5f, "M9", SkyAiCore.Settings.flashCellDelay);
												}
												return job16;
											}
										}
									}
									bool flag103 = flag59;
									if (flag103)
									{
										AdvancedAI_Classes.MapComp(pawn).destroyersExclusions.Add(pawn);
										bool flag104 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag104)
										{
											Log.Message(string.Format("{0} {1}: SiegeAI. Pawn added to exclusion list for melee. Destroyer job became null. My data: {2} Weapon: {3}", new object[]
											{
												pawn,
												pawn.Position,
												pawn.Position,
												AdvancedAI.PrimaryWeapon(pawn)
											}));
										}
										return null;
									}
									bool flag105 = SkyAiCore.Settings.enableSupporterRole && AdvancedAI.PawnCloserToGoalPosition(pawn, intVec2) && verb.verbProps.range > 14f;
									if (flag105)
									{
										bool flag106 = pawn.Faction == null || pawn.Faction != Faction.OfMechanoids || SkyAiCore.Settings.mechanoidUseSupporterRole;
										bool flag107 = pawn.IsHashIntervalTick(5) && flag106 && !AdvancedAI_Classes.MapComp(pawn).coverJobs.ContainsKey(pawn) && AdvancedAI_CoverUtility.CoverRequired(pawn, intVec2, 2f, 3);
										if (flag107)
										{
											bool flag108 = AdvancedAI_Classes.MapComp(pawn) != null;
											if (flag108)
											{
												bool flag109 = !AdvancedAI_Classes.MapComp(pawn).coverJobs.ContainsKey(pawn);
												if (flag109)
												{
													AdvancedAI_Classes.MapComp(pawn).coverJobs.Add(pawn, intVec2);
												}
											}
											IntVec3 position;
											bool coverPositionFrom = AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, intVec2, (float)Mathf.RoundToInt(verb.verbProps.range * 0.5f), 4f, true, false, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out position);
											if (coverPositionFrom)
											{
												return AdvancedAI_Jobs.GetCoverJob(pawn, position, intVec2, AdvancedAI.ExpireInterval.fast, false, false, true);
											}
										}
										bool flag110 = AdvancedAI_Classes.MapComp(pawn).coverJobs.ContainsKey(pawn);
										if (flag110)
										{
											IntVec3 intVec6 = IntVec3.Invalid;
											bool flag111 = AdvancedAI_Classes.MapComp(pawn).coverJobs.ContainsKey(pawn);
											if (flag111)
											{
												AdvancedAI_Classes.MapComp(pawn).coverJobs.TryGetValue(pawn, out intVec6);
											}
											bool flag112 = intVec6 == IntVec3.Invalid || pawn.IsHashIntervalTick(15);
											if (flag112)
											{
												intVec6 = intVec2;
											}
											bool flag113 = !AdvancedAI_CoverUtility.CoverRequired(pawn, intVec6, 2f, 3);
											if (flag113)
											{
												AdvancedAI_Classes.MapComp(pawn).coverJobs.Remove(pawn);
											}
											else
											{
												IntVec3 position2;
												bool coverPositionFrom2 = AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, intVec6, (float)Mathf.RoundToInt(verb.verbProps.range * 0.5f), 4f, true, false, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out position2);
												if (coverPositionFrom2)
												{
													return AdvancedAI_Jobs.GetCoverJob(pawn, position2, intVec6, AdvancedAI.ExpireInterval.fast, false, false, true);
												}
											}
										}
									}
									bool flag114 = intVec2.InBounds(pawn.Map) && AdvancedAI.IsFreeCell((from t in pawn.Map.thingGrid.ThingsListAtFast(intVec2)
									where t != null && t.Position.InBounds(pawn.Map)
									select t).ToList<Thing>());
									if (flag114)
									{
										bool flag115 = thing2 != null && !AdvancedAI.IsDangerousTarget(thing2, pawn.Map);
										if (flag115)
										{
											bool enableMainBlowSiegeTactic2 = SkyAiCore.Settings.enableMainBlowSiegeTactic;
											if (enableMainBlowSiegeTactic2)
											{
												List<IntVec3> list5 = AdvancedAI.CellReservedForMainBlow(pawn);
												bool flag116 = !list5.NullOrEmpty<IntVec3>() && list5.Count > 0;
												if (flag116)
												{
													bool flag117 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag117)
													{
														Log.Message(string.Format("{0} {1}: SiegeAI. reservedCells not null. Count: {2}", pawn, pawn.Position, list5.Count));
													}
													bool flag118 = list5.Contains(thing2.Position);
													if (flag118)
													{
														Building building9 = AdvancedAI.MeleeTrashBuilding(thing2.Position, pawn, 20f, 3f);
														bool flag119 = building9 != null;
														if (flag119)
														{
															bool flag120 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
															if (flag120)
															{
																Log.Message(string.Format("{0} {1}: SiegeAI. newMainTarget: {2} {3}", new object[]
																{
																	pawn,
																	pawn.Position,
																	building9,
																	building9.Position
																}));
															}
															Job job17 = TrashUtility.TrashJob(pawn, building9, true, false);
															bool flag121 = job17 != null;
															if (flag121)
															{
																bool flag122 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
																if (flag122)
																{
																	Log.Message(string.Format("{0} {1}: SiegeAI. attack building4: {2} : {3}", new object[]
																	{
																		pawn,
																		pawn.Position,
																		building9,
																		building9.Position
																	}));
																	pawn.Map.debugDrawer.FlashCell(building9.Position, 0.5f, "M5", SkyAiCore.Settings.flashCellDelay);
																}
																return job17;
															}
														}
													}
												}
											}
											Job job18 = AdvancedAI_Jobs.MainPassBlockerJob(pawn, thing2, intVec2, verb);
											bool flag123 = job18 != null;
											if (flag123)
											{
												return job18;
											}
										}
										bool flag124 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag124)
										{
											Log.Message(string.Format("{0} {1}: SiegeAI. goto1 on {2}", pawn, pawn.Position, intVec));
											pawn.Map.debugDrawer.FlashCell(intVec, 0.5f, "GT1", SkyAiCore.Settings.flashCellDelay);
										}
										Job job19 = JobMaker.MakeJob(JobDefOf.Goto, intVec);
										job19.checkOverrideOnExpire = true;
										job19.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
										job19.collideWithPawns = true;
										return job19;
									}
									List<Building> list6 = AdvancedAI.PlayerBuildingsCanReachListForEnemyPawn(pawn, 40f, 30);
									bool flag125 = list6.Count <= 2;
									if (flag125)
									{
										list6 = pawn.Map.listerBuildings.allBuildingsColonist;
										bool flag126 = list6.Count == 0;
										if (flag126)
										{
											return null;
										}
									}
									List<Building> list7 = new List<Building>();
									int num3 = Mathf.Min(20, list6.Count<Building>());
									for (int i = 0; i < num3; i++)
									{
										Building building10 = list6.ElementAt(i);
										bool flag127 = building10 != null && pawn.CanReach(building10, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
										if (flag127)
										{
											list7.Add(building10);
										}
									}
									bool flag128 = !list7.NullOrEmpty<Building>();
									if (flag128)
									{
										IOrderedEnumerable<Building> source = from item in list7
										where !AdvancedAI.IsStealableItem(pawn, item, 100f, 0.7f) && !AdvancedAI.IsDangerousTarget(item, pawn.Map)
										select item into nearest
										orderby pawn.Position.DistanceTo(nearest.Position)
										select nearest;
										Func<Thing, bool> <>9__8;
										foreach (Building building11 in source.InRandomOrder(null))
										{
											using (PawnPath pawnPath3 = pawn.Map.pathFinder.FindPath(pawn.Position, building11.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false), PathEndMode.OnCell, null))
											{
												IntVec3 c2;
												pawnPath3.FirstBlockingBuilding(out c2, pawn);
												bool flag129;
												if (c2.InBounds(pawn.Map))
												{
													IEnumerable<Thing> source2 = pawn.Map.thingGrid.ThingsListAtFast(c2);
													Func<Thing, bool> predicate;
													if ((predicate = <>9__8) == null)
													{
														predicate = (<>9__8 = ((Thing t) => t != null && t.Position.InBounds(pawn.Map)));
													}
													flag129 = AdvancedAI.IsFreeCell(source2.Where(predicate).ToList<Thing>());
												}
												else
												{
													flag129 = false;
												}
												bool flag130 = flag129;
												if (flag130)
												{
													bool flag131 = TrashUtility.ShouldTrashBuilding(pawn, building11, true);
													if (flag131)
													{
														Job job20 = TrashUtility.TrashJob(pawn, building11, true, false);
														bool flag132 = job20 != null;
														if (flag132)
														{
															bool flag133 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
															if (flag133)
															{
																Log.Message(string.Format("{0} {1}: SiegeAI. attack building3: {2} : {3}", new object[]
																{
																	pawn,
																	pawn.Position,
																	building11,
																	building11.Position
																}));
																pawn.Map.debugDrawer.FlashCell(intVec, 0.5f, "M2", SkyAiCore.Settings.flashCellDelay);
															}
															return job20;
														}
													}
												}
											}
										}
									}
								}
								result = null;
							}
						}
					}
				}
			}
			return result;
		}
	}
}
