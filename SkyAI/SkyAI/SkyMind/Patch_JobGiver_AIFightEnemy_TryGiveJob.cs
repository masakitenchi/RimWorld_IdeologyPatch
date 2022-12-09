using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000036 RID: 54
	[HarmonyPatch(typeof(JobGiver_AIFightEnemy))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_AIFightEnemy_TryGiveJob
	{
		// Token: 0x060001B1 RID: 433 RVA: 0x00025B59 File Offset: 0x00023D59
		protected static float GetFlagRadius(Pawn pawn)
		{
			return 999999f;
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00025B60 File Offset: 0x00023D60
		protected static IntVec3 GetFlagPosition(Pawn pawn)
		{
			return IntVec3.Invalid;
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00025B68 File Offset: 0x00023D68
		private static bool Prefix(ref JobGiver_AIFightEnemies __instance, Pawn pawn, ref Job __result)
		{
			Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass7_0 CS$<>8__locals1 = new Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass7_0();
			CS$<>8__locals1.pawn = pawn;
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = CS$<>8__locals1.pawn.RaceProps.Animal || CS$<>8__locals1.pawn.RaceProps.intelligence == Intelligence.Animal || (CS$<>8__locals1.pawn.Faction != null && CS$<>8__locals1.pawn.Faction.def.techLevel == TechLevel.Animal);
				if (flag)
				{
					result = true;
				}
				else
				{
					Lord lord = CS$<>8__locals1.pawn.GetLord();
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(CS$<>8__locals1.pawn);
					bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
					if (flag2)
					{
						LordJob lordJob = lord.LordJob;
						bool flag3 = lord.LordJob is LordJob_StageAttack;
						LordToil curLordToil = lord.CurLordToil;
						bool flag4 = lord.CurLordToil is LordToil_Stage;
						Log.Message(string.Format("{0} {1}: FightEnemyAI. lord data: lordJob: {2} isStageAttack: {3} curLordToil: {4} isStagetToil: {5} ticksInToil: {6}", new object[]
						{
							CS$<>8__locals1.pawn,
							CS$<>8__locals1.pawn.Position,
							lordJob,
							flag3,
							curLordToil,
							flag4,
							lord.ticksInToil
						}));
					}
					Patch_JobGiver_AIFightEnemy_TryGiveJob.UpdateEnemyTarget(CS$<>8__locals1.pawn);
					Thing enemyTarget = CS$<>8__locals1.pawn.mindState.enemyTarget;
					bool allowManualCastWeapons = !CS$<>8__locals1.pawn.IsColonist;
					Verb verb = CS$<>8__locals1.pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
					bool flag5 = verb == null;
					if (flag5)
					{
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1}: FightEnemyAI. verb null! Return false!", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
						}
						__result = null;
						result = false;
					}
					else
					{
						RaidData raidData = AdvancedAI.PawnRaidData(CS$<>8__locals1.pawn);
						bool isMeleeAttack = verb.verbProps.IsMeleeAttack;
						ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(CS$<>8__locals1.pawn);
						bool flag7 = enemyTarget == null;
						if (flag7)
						{
							bool flag8 = raidData != null;
							if (flag8)
							{
								PawnDuty duty = CS$<>8__locals1.pawn.mindState.duty;
								bool flag9 = duty != null && duty.focus != null;
								if (flag9)
								{
									Job job = AdvancedAI_Jobs.WaitSquadCoverJob(CS$<>8__locals1.pawn, duty.focus.Cell, raidData);
									bool flag10 = job != null;
									if (flag10)
									{
										__result = job;
										return false;
									}
								}
							}
							bool enableRaidLeaderGathersTroopsNearColony = SkyAiCore.Settings.enableRaidLeaderGathersTroopsNearColony;
							if (enableRaidLeaderGathersTroopsNearColony)
							{
								bool flag11 = AdvancedAI_LordUtility.InStageAttack(CS$<>8__locals1.pawn, lord);
								if (flag11)
								{
									bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
									if (flag12)
									{
										Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy null. Currently in LordToil_Stage and enemyTarget null. Return.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
									}
									__result = null;
									return false;
								}
							}
							bool flag13 = !AdvancedAI.HasEscortDuty(CS$<>8__locals1.pawn) && (!AdvancedAI.PawnIsDoctor(CS$<>8__locals1.pawn) & !AdvancedAI.PawnIsLeader(CS$<>8__locals1.pawn));
							if (flag13)
							{
								bool flag14 = SkyAiCore.Settings.enableSuppressionFireMode && !Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.EnumerableNullOrEmpty<Thing>() && !AdvancedAI.HasDefendDuty(CS$<>8__locals1.pawn);
								if (flag14)
								{
									bool flag15 = mapComponent_SkyAI.activeCover.ContainsKey(CS$<>8__locals1.pawn);
									float num;
									bool flag16 = mapComponent_SkyAI != null && CS$<>8__locals1.<Prefix>g__isGunner|0(CS$<>8__locals1.pawn) && (AdvancedAI.CanActiveCover(CS$<>8__locals1.pawn, out num, 0.2f) || flag15);
									if (flag16)
									{
										Building building = null;
										Thing thing = null;
										bool flag17 = !AdvancedAI.ActiveCoverEnemyIsActive(CS$<>8__locals1.pawn, verb);
										if (flag17)
										{
											thing = AdvancedAI.ClosestEnemyCamper(CS$<>8__locals1.pawn, verb, 3f, Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList, out building, true);
											bool flag18 = thing != null && !flag15;
											if (flag18)
											{
												mapComponent_SkyAI.activeCover.Add(CS$<>8__locals1.pawn, thing);
												flag15 = true;
											}
										}
										bool flag19 = false;
										bool flag20 = num >= SkyAiCore.Settings.advancedSuppresionSquadRatio + 0.1f;
										if (flag20)
										{
											for (int i = 0; i < mapComponent_SkyAI.activeCover.Count; i++)
											{
												bool flag21 = mapComponent_SkyAI.activeCover.ElementAt(i).Key == CS$<>8__locals1.pawn;
												if (flag21)
												{
													mapComponent_SkyAI.activeCover.Remove(CS$<>8__locals1.pawn);
													flag19 = true;
												}
											}
										}
										IntVec3 intVec = (building != null) ? building.Position : IntVec3.Invalid;
										bool flag22 = !flag19 && flag15 && AdvancedAI.IsValidLoc(intVec);
										if (flag22)
										{
											IntVec3 intVec2;
											AdvancedAI_CoverUtility.GetCoverPositionFrom(CS$<>8__locals1.pawn, intVec, (float)AdvancedAI.CalculateDistance(CS$<>8__locals1.pawn, thing, 4, 19), 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out intVec2);
											bool isValid = intVec2.IsValid;
											if (isValid)
											{
												bool enableAdvancedSuppressionFireMode = SkyAiCore.Settings.enableAdvancedSuppressionFireMode;
												if (enableAdvancedSuppressionFireMode)
												{
													bool flag23 = intVec2 == CS$<>8__locals1.pawn.Position && (AdvancedAI.PrimaryIsMachineGun(CS$<>8__locals1.pawn) || AdvancedAI.PrimaryIsSiegeWeapon(CS$<>8__locals1.pawn));
													if (flag23)
													{
														Verb primaryVerb = CS$<>8__locals1.pawn.equipment.PrimaryEq.PrimaryVerb;
														bool flag24 = CS$<>8__locals1.pawn.Position.DistanceTo(building.Position) > primaryVerb.verbProps.range;
														bool flag25 = AdvancedAI.CanSetFireOnTarget(CS$<>8__locals1.pawn, building);
														if (flag25)
														{
															Job job2 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing);
															job2.targetA = building;
															job2.verbToUse = primaryVerb;
															bool flag26 = flag24;
															if (flag26)
															{
																job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
																job2.checkOverrideOnExpire = true;
															}
															else
															{
																job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
																job2.checkOverrideOnExpire = true;
															}
															bool flag27 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
															if (flag27)
															{
																Log.Message(string.Format("{0} {1}: FightEnemyAI. SuppressionFireMode. No any enemy in LOS, but I feel the enemy is in cover near it defensive position: {2} on pos: {3}. Destroy it!", new object[]
																{
																	CS$<>8__locals1.pawn,
																	CS$<>8__locals1.pawn.Position,
																	building,
																	building.Position
																}));
															}
															__result = job2;
															return false;
														}
													}
												}
												bool debugLog = SkyAiCore.Settings.debugLog;
												if (debugLog)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. EnemyCamper. Found camper: {2} FocusCell: {3} Moving to cover cell: {4}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														thing,
														intVec,
														intVec2
													}));
												}
												__result = AdvancedAI_Jobs.GetCoverJob(CS$<>8__locals1.pawn, intVec2, intVec, AdvancedAI.ExpireInterval.normal, false, false, true);
												return false;
											}
										}
										bool flag28 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
										if (flag28)
										{
											string text = (mapComponent_SkyAI.activeCover.Count > 0) ? GeneralExtensions.Join<KeyValuePair<Pawn, Thing>>(mapComponent_SkyAI.activeCover, null, ", ").ToString() : "null";
											Log.Message(string.Format("{0} {1}: FightEnemyAI. Active cover count: {2} ActiveCoverList: {3}", new object[]
											{
												CS$<>8__locals1.pawn,
												CS$<>8__locals1.pawn.Position,
												mapComponent_SkyAI.activeCover.Count,
												text
											}));
										}
									}
								}
								bool flag29 = thingWithComps != null && !AdvancedAI.PawnIsSniper(CS$<>8__locals1.pawn);
								if (flag29)
								{
									bool flag30 = !isMeleeAttack && !AdvancedAI.PrimaryIsGrenade(CS$<>8__locals1.pawn) && !Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.NullOrEmpty<Thing>();
									if (flag30)
									{
										int num2 = 0;
										IEnumerable<Thing> source = Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList;
										Func<Thing, float> keySelector;
										if ((keySelector = CS$<>8__locals1.<>9__1) == null)
										{
											keySelector = (CS$<>8__locals1.<>9__1 = ((Thing closestTarget) => CS$<>8__locals1.pawn.Position.DistanceTo(closestTarget.Position)));
										}
										foreach (Thing thing2 in source.OrderBy(keySelector))
										{
											num2++;
											float num3 = (float)Mathf.Min(AdvancedAI.CalculateDistance(CS$<>8__locals1.pawn, thing2, 4, 19), 15);
											bool flag31 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag31)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. Check potencial target: {2} on pos: {3} distance: {4} with coverDistance: {5}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													thing2,
													thing2.Position,
													CS$<>8__locals1.pawn.Position.DistanceTo(thing2.Position),
													num3
												}));
											}
											IntVec3 intVec3;
											bool coverPositionFrom = AdvancedAI_CoverUtility.GetCoverPositionFrom(CS$<>8__locals1.pawn, thing2.Position, num3, 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out intVec3);
											if (coverPositionFrom)
											{
												bool flag32 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag32)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. Get cover position on: {2} for potencial target: {3} on pos: {4} distance: {5} with coverDistance: {6}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														intVec3,
														thing2,
														thing2.Position,
														CS$<>8__locals1.pawn.Position.DistanceTo(thing2.Position),
														num3
													}));
												}
												__result = AdvancedAI_Jobs.GetCoverJob(CS$<>8__locals1.pawn, intVec3, thing2.Position, AdvancedAI.ExpireInterval.fast, false, false, true);
												return false;
											}
											bool flag33 = num2 >= 5;
											if (flag33)
											{
												break;
											}
										}
									}
									IntVec3 intVec4;
									Thing thing3 = AdvancedAI.ClosestLordAllyEnemy(CS$<>8__locals1.pawn, verb, 5, out intVec4);
									bool flag34 = thing3 != null;
									if (flag34)
									{
										float num4 = CS$<>8__locals1.pawn.Position.DistanceTo(thing3.Position);
										Job job3 = JobMaker.MakeJob(JobDefOf.Goto, intVec4);
										job3.expiryInterval = AdvancedAI.CombatInterval(CS$<>8__locals1.pawn, thing3.Position, num4);
										job3.checkOverrideOnExpire = true;
										job3.locomotionUrgency = AdvancedAI.ResolveLocomotion(CS$<>8__locals1.pawn);
										bool flag35 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
										if (flag35)
										{
											Log.Message(string.Format("{0} {1}: FightEnemyAI. enemyTarget is null, but found ally in combat. GoTo: {2} distantEnemy: {3} on pos: {4} distance: {5} EFF: {6} job interval: {7}", new object[]
											{
												CS$<>8__locals1.pawn,
												CS$<>8__locals1.pawn.Position,
												intVec4,
												thing3,
												thing3.Position,
												num4,
												AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn),
												job3.expiryInterval
											}));
										}
										__result = job3;
										return false;
									}
								}
								bool flag36 = thingWithComps == null;
								if (flag36)
								{
									bool flag37 = AdvancedAI.IsBioHumanlikeOnly(CS$<>8__locals1.pawn) && !AdvancedAI.HasDefendBaseDuty(CS$<>8__locals1.pawn);
									if (flag37)
									{
										IEnumerable<Pawn> enumerable = AdvancedAI.ArmedLordPawns(CS$<>8__locals1.pawn);
										bool flag38 = !enumerable.EnumerableNullOrEmpty<Pawn>();
										if (flag38)
										{
											Pawn pawn2;
											(from p1 in enumerable
											where p1 != CS$<>8__locals1.pawn
											select p1).TryMinBy((Pawn p2) => p2.Position.DistanceTo(CS$<>8__locals1.pawn.Position), out pawn2);
											bool flag39 = pawn2 != null;
											if (flag39)
											{
												bool flag40 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag40)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. enemyTarget is null. Primary weapon null. I am not able to fight. Start to follow closest armed ally.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
												}
												__result = AdvancedAI_Jobs.LordFollowJob(CS$<>8__locals1.pawn, 8f, 250, false, false, false);
												return false;
											}
										}
									}
								}
							}
							bool flag41 = enemyTarget == null;
							if (flag41)
							{
								bool flag42 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
								if (flag42)
								{
									Log.Message(string.Format("{0} {1}: FightEnemyAI. enemyTarget is null. Return null.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
								}
								__result = null;
								return false;
							}
						}
						Pawn pawn3 = enemyTarget as Pawn;
						bool flag43 = pawn3 != null && pawn3.IsInvisible();
						if (flag43)
						{
							bool flag44 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
							if (flag44)
							{
								Log.Message(string.Format("{0} {1}: FightEnemyAI. enemyTarget is invisible. Can't see it! Switch to Siege AI.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
							}
							__result = null;
							result = false;
						}
						else
						{
							float num5 = AdvancedAI.EffectiveRange(pawn3);
							bool flag45 = AdvancedAI_LordUtility.InStageAttack(CS$<>8__locals1.pawn, lord) && pawn3 != null && CS$<>8__locals1.pawn.Position.DistanceTo(pawn3.Position) >= num5;
							if (flag45)
							{
								bool flag46 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
								if (flag46)
								{
									Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy not null, but currently in LordToil_Stage and enemyTarget null. Return.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
								}
								__result = null;
								result = false;
							}
							else
							{
								bool flag47 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
								if (flag47)
								{
									Log.Message(string.Format("{0} {1}: FightEnemyAI. EnemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
									{
										CS$<>8__locals1.pawn,
										CS$<>8__locals1.pawn.Position,
										enemyTarget,
										enemyTarget.Position,
										CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
										AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
									}));
									CS$<>8__locals1.pawn.Map.debugDrawer.FlashCell(enemyTarget.Position, 0.27f, "ENM2", SkyAiCore.Settings.flashCellDelay);
								}
								Job job4 = AdvancedAI_Jobs.SurvivalDecisions(CS$<>8__locals1.pawn, enemyTarget.Position);
								bool flag48 = job4 != null;
								if (flag48)
								{
									bool flag49 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
									if (flag49)
									{
										Log.Message(string.Format("{0} {1}: FightEnemyAI. Start check survival decisions.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
									}
									__result = job4;
									result = false;
								}
								else
								{
									bool flag50 = SkyAiCore.Settings.enableDoctorRole && AdvancedAI.PawnIsDoctor(CS$<>8__locals1.pawn);
									if (flag50)
									{
										Job job5 = AdvancedAI_Roles.DoctorRole(CS$<>8__locals1.pawn, enemyTarget.Position, enemyTarget != null);
										bool flag51 = job5 != null;
										if (flag51)
										{
											bool flag52 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag52)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. Doctor role! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													enemyTarget,
													enemyTarget.Position,
													CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
													AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
												}));
											}
											__result = job5;
											return false;
										}
									}
									bool enableLeaderRole = SkyAiCore.Settings.enableLeaderRole;
									if (enableLeaderRole)
									{
										bool flag53 = raidData != null;
										if (flag53)
										{
											bool flag54 = raidData.raidLeader != null && raidData.raidLeader.Equals(CS$<>8__locals1.pawn);
											if (flag54)
											{
												Job job6 = AdvancedAI_Roles.LeaderRole(CS$<>8__locals1.pawn, enemyTarget.Position);
												bool flag55 = job6 != null;
												if (flag55)
												{
													bool flag56 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
													if (flag56)
													{
														Log.Message(string.Format("{0} {1}: FightEnemyAI. Leader role! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
														{
															CS$<>8__locals1.pawn,
															CS$<>8__locals1.pawn.Position,
															enemyTarget,
															enemyTarget.Position,
															CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
															AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
														}));
													}
													__result = job6;
													return false;
												}
											}
											bool flag57 = raidData.squadCommanders.Contains(CS$<>8__locals1.pawn);
											if (flag57)
											{
												Job job7 = AdvancedAI_Roles.SquadCommanderRole(CS$<>8__locals1.pawn, enemyTarget.Position);
												bool flag58 = job7 != null;
												if (flag58)
												{
													bool flag59 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
													if (flag59)
													{
														Log.Message(string.Format("{0} {1}: FightEnemyAI. Squad commander role! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
														{
															CS$<>8__locals1.pawn,
															CS$<>8__locals1.pawn.Position,
															enemyTarget,
															enemyTarget.Position,
															CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
															AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
														}));
													}
													__result = job7;
													return false;
												}
											}
										}
									}
									bool flag60 = SkyAiCore.Settings.enableSniperRole && AdvancedAI.PawnIsSniper(CS$<>8__locals1.pawn);
									if (flag60)
									{
										Job job8 = AdvancedAI_Roles.SniperRole(CS$<>8__locals1.pawn, enemyTarget.Position);
										bool flag61 = job8 != null;
										if (flag61)
										{
											bool flag62 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag62)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. Sniper role! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													enemyTarget,
													enemyTarget.Position,
													CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
													AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
												}));
											}
											__result = job8;
											return false;
										}
									}
									float num6 = CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position);
									bool flag63 = isMeleeAttack || AdvancedAI.PrimaryIsGrenade(CS$<>8__locals1.pawn);
									if (flag63)
									{
										IEnumerable<Pawn> enumerable2 = AdvancedAI.RangedLordPawns(CS$<>8__locals1.pawn, 0.3f);
										bool flag64 = !enumerable2.EnumerableNullOrEmpty<Pawn>();
										if (flag64)
										{
											bool flag65 = thingWithComps == null && AdvancedAI.IsBioHumanlikeOnly(CS$<>8__locals1.pawn) && !AdvancedAI.IsInCloseWithTarget(CS$<>8__locals1.pawn, pawn3);
											if (flag65)
											{
												bool flag66 = !AdvancedAI.EnemyInCloseBattle(CS$<>8__locals1.pawn, enemyTarget);
												if (flag66)
												{
													IntVec3 intVec5;
													Pawn pawn4;
													AdvancedAI_CoverUtility.GetCoverNearAllyFrom(CS$<>8__locals1.pawn, enemyTarget.Position, enumerable2.ToList<Pawn>(), 16f, true, false, false, false, false, true, out intVec5, out pawn4);
													bool flag67 = AdvancedAI.IsValidLoc(intVec5);
													if (flag67)
													{
														bool flag68 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
														if (flag68)
														{
															Log.Message(string.Format("{0} {1}: FightEnemyAI. Primary weapon null. I am not able to fight. Enemy: {2} on pos: {3} Start to cover on: {4}.", new object[]
															{
																CS$<>8__locals1.pawn,
																CS$<>8__locals1.pawn.Position,
																enemyTarget,
																enemyTarget.Position,
																intVec5
															}));
														}
														__result = AdvancedAI_Jobs.GetCoverJob(CS$<>8__locals1.pawn, intVec5, enemyTarget.Position, AdvancedAI.ExpireInterval.fast, true, false, true);
														return false;
													}
													bool flag69 = pawn4 != null;
													if (flag69)
													{
														bool flag70 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
														if (flag70)
														{
															Log.Message(string.Format("{0} {1}: FightEnemyAI. Primary weapon null. I am not able to fight. Enemy: {2} on pos: {3} Start to follow armed ranged ally: {4} on pos: {5}.", new object[]
															{
																CS$<>8__locals1.pawn,
																CS$<>8__locals1.pawn.Position,
																enemyTarget,
																enemyTarget.Position,
																pawn4,
																pawn4.Position
															}));
														}
														int num7 = AdvancedAI.CalculateDistance(CS$<>8__locals1.pawn, enemyTarget, 2, 9);
														__result = AdvancedAI_Jobs.LordFollowJob(pawn4, (float)num7, 250, false, false, false);
														return false;
													}
												}
											}
											Pawn pawn5;
											(from p1 in enumerable2
											where p1 != CS$<>8__locals1.pawn
											select p1).TryMinBy((Pawn p2) => p2.Position.DistanceTo(CS$<>8__locals1.pawn.Position), out pawn5);
											bool flag71 = pawn5 != null && pawn5.mindState != null && pawn5.mindState.enemyTarget == null;
											if (flag71)
											{
												bool flag72 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag72)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. I have a melee weapon or grenade and must follow near combat ranged ally now.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
												}
												__result = AdvancedAI_Jobs.LordFollowJob(CS$<>8__locals1.pawn, 8f, 180, true, true, false);
												return false;
											}
										}
										bool flag73 = isMeleeAttack;
										if (flag73)
										{
											bool flag74 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag74)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. MeleeAttack! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													enemyTarget,
													enemyTarget.Position,
													CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
													AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
												}));
											}
											__result = Patch_JobGiver_AIFightEnemy_TryGiveJob.MeleeAttackJob(CS$<>8__locals1.pawn, enemyTarget);
											return false;
										}
									}
									float num8 = AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn);
									bool flag75 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
									if (flag75)
									{
										bool grenadeWeapon = AdvancedAI.PrimaryIsGrenade(CS$<>8__locals1.pawn);
										Log.Message(string.Format("{0} {1}: FightEnemyAI. Stat. Keep range enemies: {2} EnemyTarget: {3} on pos: {4} CHTF: {5} SHT: {6} distance: {7} MyEff: {8} EnemyEff: {9}", new object[]
										{
											CS$<>8__locals1.pawn,
											CS$<>8__locals1.pawn.Position,
											Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.Count<Thing>(),
											enemyTarget,
											enemyTarget.Position,
											AdvancedAI.CanHitTargetFrom(verb, CS$<>8__locals1.pawn.Position, enemyTarget, grenadeWeapon),
											AdvancedAI.TryFindShootlineFromTo(CS$<>8__locals1.pawn.Position, enemyTarget, verb),
											CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
											num8,
											num5
										}));
									}
									bool flag76 = AdvancedAI_CoverUtility.IsCovered(CS$<>8__locals1.pawn, enemyTarget.Position);
									bool flag77 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
									if (flag77)
									{
										Log.Message(string.Format("{0} {1}: FightEnemyAI. Check the need for cover first. isCovered: {2}.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, flag76));
									}
									IntRange intRange = AdvancedAI.PrimaryEffectiveWeaponRange(pawn3, 1f, 1f, false, 25);
									bool flag78 = intRange.max < 8 && num8 > 8f;
									bool flag79 = !flag78;
									if (flag79)
									{
										float num9 = Mathf.Pow(Mathf.Clamp01(num6 / num5), 2f);
										float num10 = Mathf.Lerp(4f, 19f, num9);
										bool flag80 = Mathf.RoundToInt(num6) <= intRange.max;
										bool flag81 = (float)Mathf.RoundToInt(num6) <= AdvancedAI.PrimaryWeaponRange(CS$<>8__locals1.pawn);
										bool flag82 = !flag76 && flag80 && flag81;
										if (flag82)
										{
											bool flag83 = num8 > num5;
											if (flag83)
											{
												IntVec3 intVec6;
												bool coverPositionFrom2 = AdvancedAI_CoverUtility.GetCoverPositionFrom(CS$<>8__locals1.pawn, enemyTarget.Position, num10, 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out intVec6);
												if (coverPositionFrom2)
												{
													bool flag84 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
													if (flag84)
													{
														Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy in dangerous distance. I'm not covered! Passive coverPositionFrom on pos: {2} EnemyTarget: {3} on pos: {4} distance: {5} MyEff: {6} EnemyEff: {7} CoverDistMulti: {8} CoverDistance: {9} NoCoverRequired? {10}", new object[]
														{
															CS$<>8__locals1.pawn,
															CS$<>8__locals1.pawn.Position,
															intVec6,
															enemyTarget,
															enemyTarget.Position,
															CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
															num8,
															num5,
															num9,
															num10,
															flag78
														}));
													}
													__result = AdvancedAI_Jobs.GetCoverJob(CS$<>8__locals1.pawn, intVec6, enemyTarget.Position, AdvancedAI.ExpireInterval.fast, false, false, true);
													bool flag85 = __result != null;
													if (flag85)
													{
														return false;
													}
												}
												else
												{
													bool flag86 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
													if (flag86)
													{
														Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy in dangerous distance. I'm not covered! Failed to find passive coverPosition. EnemyTarget: {2} on pos: {3} distance: {4} MyEff: {5} EnemyEff: {6} CoverDistMulti: {7} CoverDistance: {8} NoCoverRequired? {9}", new object[]
														{
															CS$<>8__locals1.pawn,
															CS$<>8__locals1.pawn.Position,
															enemyTarget,
															enemyTarget.Position,
															CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
															num8,
															num5,
															num9,
															num10,
															flag78
														}));
													}
												}
											}
											IntVec3 intVec7;
											bool coverPositionFrom3 = AdvancedAI_CoverUtility.GetCoverPositionFrom(CS$<>8__locals1.pawn, enemyTarget.Position, num10, 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out intVec7);
											if (coverPositionFrom3)
											{
												bool flag87 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag87)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy in dangerous distance. I'm not covered! Active coverPositionFrom on pos: {2} EnemyTarget: {3} on pos: {4} distance: {5} MyEff: {6} EnemyEff: {7} CoverDistMulti: {8} CoverDistance: {9} NoCoverRequired? {10}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														intVec7,
														enemyTarget,
														enemyTarget.Position,
														CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
														num8,
														num5,
														num9,
														num10,
														flag78
													}));
												}
												__result = AdvancedAI_Jobs.GetCoverJob(CS$<>8__locals1.pawn, intVec7, enemyTarget.Position, AdvancedAI.ExpireInterval.fast, false, false, true);
												bool flag88 = __result != null;
												if (flag88)
												{
													return false;
												}
											}
											else
											{
												bool flag89 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag89)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy in dangerous distance. I'm not covered! Failed to find active coverPosition. EnemyTarget: {2} on pos: {3} distance: {4} MyEff: {5} EnemyEff: {6} CoverDistMulti: {7} CoverDistance: {8} NoCoverRequired? {9}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														enemyTarget,
														enemyTarget.Position,
														CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
														num8,
														num5,
														num9,
														num10,
														flag78
													}));
												}
											}
										}
										else
										{
											bool flag90 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag90)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy in dangerous distance. I'm already covered! enemyTarget: {2} on pos: {3} distance: {4} MyEff: {5} EnemyEff: {6} CoverDistMulti: {7} CoverDistance: {8} NoCoverRequired? {9}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													enemyTarget,
													enemyTarget.Position,
													num6,
													num8,
													num5,
													num9,
													num10,
													flag78
												}));
											}
										}
									}
									else
									{
										bool flag91 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
										if (flag91)
										{
											Log.Message(string.Format("{0} {1}: FightEnemyAI. Enemy is not dangerous. enemyTarget: {2} on pos: {3} distance: {4} MyEff: {5} EnemyEff: {6}", new object[]
											{
												CS$<>8__locals1.pawn,
												CS$<>8__locals1.pawn.Position,
												enemyTarget,
												enemyTarget.Position,
												num6,
												num8,
												num5
											}));
										}
									}
									bool flag92 = CS$<>8__locals1.pawn.IsHashIntervalTick(5);
									if (flag92)
									{
										Job job9 = AdvancedAI_Jobs.StealDecisions(CS$<>8__locals1.pawn, false);
										bool flag93 = job9 != null;
										if (flag93)
										{
											bool flag94 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag94)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. StealDecisions! enemyTarget: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
												{
													CS$<>8__locals1.pawn,
													CS$<>8__locals1.pawn.Position,
													enemyTarget,
													enemyTarget.Position,
													CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
													AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
												}));
											}
											__result = job9;
											return false;
										}
									}
									IntVec3 intVec8;
									bool flag96;
									bool flag95 = !AdvancedAI.TryFindShootingPosition(CS$<>8__locals1.pawn, verb, flag76, num8, out intVec8, out flag96);
									if (flag95)
									{
										__result = null;
										result = false;
									}
									else
									{
										bool flag97 = CS$<>8__locals1.pawn.CanReach(enemyTarget, PathEndMode.OnCell, Danger.Some, false, false, TraverseMode.ByPawn);
										bool flag98 = intVec8.Equals(CS$<>8__locals1.pawn.Position);
										if (flag98)
										{
											bool flag99 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
											if (flag99)
											{
												Log.Message(string.Format("{0} {1}: FightEnemyAI. TryFindShootingPosition dest is equals pawn position.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
											}
											bool flag100 = flag97;
											if (flag100)
											{
												bool flag101 = AdvancedAI.DutyHasSiegeSubNode(CS$<>8__locals1.pawn) && AdvancedAI.PositionUnderCrossfire(CS$<>8__locals1.pawn, enemyTarget.Position, enemyTarget, true, true);
												bool flag102 = !flag76 && !flag96 && !flag101;
												if (flag102)
												{
													Job job10 = JobMaker.MakeJob(JobDefOf.Goto, enemyTarget);
													job10.checkOverrideOnExpire = true;
													job10.expiryInterval = AdvancedAI.CombatInterval(CS$<>8__locals1.pawn, enemyTarget.Position, num6);
													bool flag103 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
													if (flag103)
													{
														Log.Message(string.Format("{0} {1}: FightEnemyAI. AIGotoNearestHostile. Job target: {2} Goto: {3}", new object[]
														{
															CS$<>8__locals1.pawn,
															CS$<>8__locals1.pawn.Position,
															enemyTarget,
															enemyTarget.Position
														}));
													}
													__result = job10;
													return false;
												}
											}
											bool flag104 = CS$<>8__locals1.pawn.Position.Standable(CS$<>8__locals1.pawn.Map) && CS$<>8__locals1.pawn.Map.pawnDestinationReservationManager.CanReserve(CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Drafted);
											if (flag104)
											{
												bool flag105 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag105)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. WaitCombat on position! enemyTarget: {2} on pos: {3} TFSP Dest: {4} distance: {5} MyEff: {6}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														enemyTarget,
														enemyTarget.Position,
														intVec8,
														CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
														AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn)
													}));
												}
												Job job11 = JobMaker.MakeJob(JobDefOf.Wait_Combat, intVec8);
												job11.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
												job11.checkOverrideOnExpire = true;
												__result = job11;
												result = false;
											}
											else
											{
												__result = null;
												result = false;
											}
										}
										else
										{
											bool flag106 = (enemyTarget.Position == intVec8 && flag97) || CS$<>8__locals1.pawn.CanReach(intVec8, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
											if (flag106)
											{
												Job job12 = JobMaker.MakeJob(JobDefOf.Goto, intVec8);
												job12.expiryInterval = AdvancedAI.CombatInterval(CS$<>8__locals1.pawn, enemyTarget.Position, num6);
												job12.checkOverrideOnExpire = true;
												job12.collideWithPawns = true;
												job12.locomotionUrgency = AdvancedAI.ResolveCombatLocomotion(CS$<>8__locals1.pawn, enemyTarget, num6);
												bool flag107 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag107)
												{
													Log.Message(string.Format("{0} {1}: FightEnemyAI. GoTo: {2} enemyTarget: {3} on pos: {4} distance: {5} EFF: {6} job interval: {7}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														intVec8,
														enemyTarget,
														enemyTarget.Position,
														CS$<>8__locals1.pawn.Position.DistanceTo(enemyTarget.Position),
														AdvancedAI.EffectiveRange(CS$<>8__locals1.pawn),
														job12.expiryInterval
													}));
												}
												__result = job12;
												result = false;
											}
											else
											{
												__result = null;
												result = false;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00027DE4 File Offset: 0x00025FE4
		public static void UpdateEnemyTarget(Pawn pawn)
		{
			Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass8_0 CS$<>8__locals1 = new Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass8_0();
			CS$<>8__locals1.pawn = pawn;
			MethodInfo methodInfo = AccessTools.Method(typeof(Pawn_MindState), "Notify_EngagedTarget", null, null);
			Thing thing = CS$<>8__locals1.pawn.mindState.enemyTarget;
			bool flag = thing != null && (thing.Destroyed || Find.TickManager.TicksGame - CS$<>8__locals1.pawn.mindState.lastEngageTargetTick > 400 || !CS$<>8__locals1.pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn) || (float)(CS$<>8__locals1.pawn.Position - thing.Position).LengthHorizontalSquared > Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRadius * Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRadius || ((IAttackTarget)thing).ThreatDisabled(CS$<>8__locals1.pawn));
			if (flag)
			{
				thing = null;
			}
			bool flag2 = thing == null;
			if (flag2)
			{
				thing = Patch_JobGiver_AIFightEnemy_TryGiveJob.FindAttackTargetIfPossible(CS$<>8__locals1.pawn);
				bool flag3 = thing != null;
				if (flag3)
				{
					methodInfo.Invoke(CS$<>8__locals1.pawn.mindState, null);
					Lord lord = CS$<>8__locals1.pawn.GetLord();
					if (lord != null)
					{
						lord.Notify_PawnAcquiredTarget(CS$<>8__locals1.pawn, thing);
					}
				}
			}
			else
			{
				Thing thing2 = Patch_JobGiver_AIFightEnemy_TryGiveJob.FindAttackTargetIfPossible(CS$<>8__locals1.pawn);
				bool flag4 = thing2 == null && !Patch_JobGiver_AIFightEnemy_TryGiveJob.chaseTarget;
				if (flag4)
				{
					thing = null;
				}
				else
				{
					bool flag5 = thing2 != null && thing2 != thing;
					if (flag5)
					{
						methodInfo.Invoke(CS$<>8__locals1.pawn.mindState, null);
						thing = thing2;
					}
				}
			}
			bool flag6 = CS$<>8__locals1.pawn.mindState.duty != null && CS$<>8__locals1.pawn.mindState.duty.def == DutyDefOf.AssaultColony;
			if (flag6)
			{
				bool flag7 = thing == null && !Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.NullOrEmpty<Thing>() && Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.Count > 0;
				if (flag7)
				{
					Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass8_1 CS$<>8__locals2 = new Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass8_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					Patch_JobGiver_AIFightEnemy_TryGiveJob.<>c__DisplayClass8_1 CS$<>8__locals3 = CS$<>8__locals2;
					Verb verb = AdvancedAI.PrimaryVerb(CS$<>8__locals2.CS$<>8__locals1.pawn);
					CS$<>8__locals3.isMelee = (verb != null && verb != null && verb.verbProps != null && verb.verbProps.IsMeleeAttack);
					IEnumerable<Thing> enumerable = from t in Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList
					where t != null && AdvancedAI.IsActiveTarget(CS$<>8__locals2.CS$<>8__locals1.pawn, t, true, false) && CS$<>8__locals2.CS$<>8__locals1.<UpdateEnemyTarget>g__colonyPawnOutside|0(t) && base.<UpdateEnemyTarget>g__checkMelee|1(t)
					select t;
					bool flag8 = !enumerable.EnumerableNullOrEmpty<Thing>();
					if (flag8)
					{
						enumerable.TryMinBy((Thing closestEnemy) => CS$<>8__locals2.CS$<>8__locals1.pawn.Position.DistanceTo(closestEnemy.Position), out thing);
						bool flag9 = CS$<>8__locals2.CS$<>8__locals1.pawn.Position.DistanceTo(thing.Position) < 55f;
						if (flag9)
						{
							bool flag10 = AdvancedAI.PathNodesCount(CS$<>8__locals2.CS$<>8__locals1.pawn, thing.Position) < 75;
							if (flag10)
							{
								methodInfo.Invoke(CS$<>8__locals2.CS$<>8__locals1.pawn.mindState, null);
								Lord lord2 = CS$<>8__locals2.CS$<>8__locals1.pawn.GetLord();
								if (lord2 != null)
								{
									lord2.Notify_PawnAcquiredTarget(CS$<>8__locals2.CS$<>8__locals1.pawn, thing);
								}
								bool flag11 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals2.CS$<>8__locals1.pawn);
								if (flag11)
								{
									Log.Message(string.Format("{0} {1}: FightEnemyAI. Found outside home area enemy {2} on {3}. Take it faster!", new object[]
									{
										CS$<>8__locals2.CS$<>8__locals1.pawn,
										CS$<>8__locals2.CS$<>8__locals1.pawn.Position,
										thing,
										thing.Position
									}));
								}
							}
						}
					}
				}
			}
			CS$<>8__locals1.pawn.mindState.enemyTarget = thing;
			bool flag12 = thing is Pawn && thing.Faction == Faction.OfPlayer && CS$<>8__locals1.pawn.Position.InHorDistOf(thing.Position, 55f);
			if (flag12)
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000281D4 File Offset: 0x000263D4
		public static Thing FindAttackTargetIfPossible(Pawn pawn)
		{
			bool flag = pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null;
			Thing result;
			if (flag)
			{
				result = null;
			}
			else
			{
				float maxDistance = Mathf.Max(50f, AdvancedAI.PrimaryWeaponRange(pawn));
				Thing thing;
				AdvancedAI.ActiveThreat(pawn, maxDistance, true, false, true, true, true, false, false, out thing, out Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList);
				bool flag2 = thing != null;
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: FightEnemyAI. EnemyTarget found with activeThreat: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
						{
							pawn,
							pawn.Position,
							thing,
							thing.Position,
							pawn.Position.DistanceTo(thing.Position),
							AdvancedAI.EffectiveRange(pawn)
						}));
					}
					result = thing;
				}
				else
				{
					bool flag4 = !Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.NullOrEmpty<Thing>() && thing == null;
					bool flag5 = !AdvancedAI.DutyHasSiegeSubNode(pawn) && (flag4 || Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList.NullOrEmpty<Thing>());
					if (flag5)
					{
						bool flag6 = flag4 && SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1}: FightEnemyAI. enemy null, but targetKeepRangeList is not empty: {2}", pawn, pawn.Position, GeneralExtensions.Join<Thing>(Patch_JobGiver_AIFightEnemy_TryGiveJob.targetKeepRangeList, null, ", ")));
						}
						Thing thing2 = Patch_JobGiver_AIFightEnemy_TryGiveJob.FindAttackTarget(pawn);
						bool flag7 = thing2 != null && AdvancedAI.IsActiveTarget(pawn, thing2, false, false);
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: FightEnemyAI. EnemyTarget found with vanilla targetFinder: {2} on pos: {3} distance: {4} EFF: {5}", new object[]
								{
									pawn,
									pawn.Position,
									thing2,
									thing2.Position,
									pawn.Position.DistanceTo(thing2.Position),
									AdvancedAI.EffectiveRange(pawn)
								}));
							}
						}
						result = thing2;
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x000283F4 File Offset: 0x000265F4
		public static Thing FindAttackTarget(Pawn pawn)
		{
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			bool flag = Patch_JobGiver_AIFightEnemy_TryGiveJob.needLOSToAcquireNonPawnTargets;
			if (flag)
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
			}
			bool flag3;
			bool flag2 = AdvancedAI.PrimaryVerbIsIncendiary(pawn, out flag3);
			if (flag2)
			{
				targetScanFlags |= TargetScanFlags.NeedNonBurning;
			}
			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => AdvancedAI.ExtraTargetValidator(pawn, x), 0f, Patch_JobGiver_AIFightEnemy_TryGiveJob.targetAcquireRadius, Patch_JobGiver_AIFightEnemy_TryGiveJob.GetFlagPosition(pawn), Patch_JobGiver_AIFightEnemy_TryGiveJob.GetFlagRadius(pawn), false, true, false);
			bool flag4 = thing != null;
			Thing result;
			if (flag4)
			{
				result = thing;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00028498 File Offset: 0x00026698
		public static Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
		{
			return new Job(JobDefOf.AttackMelee, enemyTarget)
			{
				expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal),
				locomotionUrgency = AdvancedAI.ResolveCombatLocomotion(pawn, enemyTarget, 0f),
				checkOverrideOnExpire = true,
				expireRequiresEnemiesNearby = true
			};
		}

		// Token: 0x0400010D RID: 269
		public static float targetAcquireRadius = 75f;

		// Token: 0x0400010E RID: 270
		public static float targetKeepRadius = 85f;

		// Token: 0x0400010F RID: 271
		public static bool needLOSToAcquireNonPawnTargets = false;

		// Token: 0x04000110 RID: 272
		public static bool chaseTarget;

		// Token: 0x04000111 RID: 273
		public static List<Thing> targetKeepRangeList;
	}
}
