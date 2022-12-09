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
	// Token: 0x0200002F RID: 47
	public class CompLeaderRole : ThingComp
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00022D20 File Offset: 0x00020F20
		public CompProperties_LeaderRole Props
		{
			get
			{
				return (CompProperties_LeaderRole)this.props;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600017A RID: 378 RVA: 0x00022D30 File Offset: 0x00020F30
		// (set) Token: 0x0600017B RID: 379 RVA: 0x00022D5D File Offset: 0x00020F5D
		public IntVec3 BlockerCell
		{
			get
			{
				IntVec3 intVec = this.blockerCell;
				bool flag = false;
				IntVec3 invalid;
				if (flag)
				{
					invalid = IntVec3.Invalid;
				}
				else
				{
					invalid = this.blockerCell;
				}
				return invalid;
			}
			set
			{
				this.blockerCell = value;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600017C RID: 380 RVA: 0x00022D68 File Offset: 0x00020F68
		// (set) Token: 0x0600017D RID: 381 RVA: 0x00022DD5 File Offset: 0x00020FD5
		public IntVec3 LeaderEnemyTarget
		{
			get
			{
				bool flag = !AdvancedAI.IsValidLoc(this.leaderEnemyTarget);
				IntVec3 invalid;
				if (flag)
				{
					Pawn pawn = this.parent as Pawn;
					bool flag2 = pawn != null;
					if (flag2)
					{
						IAttackTarget attackTarget = AdvancedAI.AttackTarget(pawn);
						bool flag3 = attackTarget != null;
						if (flag3)
						{
							return attackTarget.Thing.Position;
						}
					}
					invalid = IntVec3.Invalid;
				}
				else
				{
					invalid = this.leaderEnemyTarget;
				}
				return invalid;
			}
			set
			{
				this.leaderEnemyTarget = value;
			}
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00022DE0 File Offset: 0x00020FE0
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<IntVec3>(ref this.leaderEnemyTarget, "enemyTarget", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.blockerCell, "blockerCell", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.bodyguardCount, "bodyguardCount", 0, false);
			Scribe_Values.Look<bool>(ref this.raidHasSquadLeaders, "raidHasSquadLeaders", false, false);
			Scribe_Values.Look<bool>(ref this.usedReservation, "usedReservation", false, false);
			Scribe_Values.Look<int>(ref this.initReservedSquadsCount, "initReservedSquadsCount", 0, false);
			Scribe_Values.Look<string>(ref this.baseLordToil, "baseLordToil", null, false);
			Scribe_Values.Look<bool>(ref this.generatedStage, "generatedStage", false, false);
			Scribe_Values.Look<bool>(ref this.skipStage, "skipStage", false, false);
			Scribe_Defs.Look<HediffDef>(ref this.leaderAura, "leaderAura");
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00022EC4 File Offset: 0x000210C4
		public override void CompTick()
		{
			base.CompTick();
			Pawn pawn = this.parent as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = pawn.IsHashIntervalTick(250);
				if (flag2)
				{
					bool flag3 = pawn.Map == null;
					if (flag3)
					{
						pawn.AllComps.Remove(this);
					}
					else
					{
						bool flag4 = !AdvancedAI.IsActivePawn(pawn);
						if (!flag4)
						{
							bool flag5 = this.leaderAura != null && SkyAiCore.Settings.enableRaidLeaderAura;
							if (flag5)
							{
								HediffLeader hediffLeader = (HediffLeader)pawn.health.hediffSet.GetFirstHediffOfDef(this.leaderAura, false);
								bool flag6 = hediffLeader == null;
								if (flag6)
								{
									hediffLeader = (HediffLeader)HediffMaker.MakeHediff(this.leaderAura, pawn, null);
									hediffLeader.Severity = AdvancedAI_Aura.AuraLevel(pawn);
									pawn.health.AddHediff(hediffLeader, null, null, null);
									bool debugLog = SkyAiCore.Settings.debugLog;
									if (debugLog)
									{
										Log.Message(string.Format("{0} {1}: Added leader aura: {2}. Severity: {3} with stage: {4}", new object[]
										{
											pawn,
											pawn.Position,
											hediffLeader.LabelCap,
											AdvancedAI_Aura.AuraLevel(pawn),
											hediffLeader.CurStage
										}));
									}
								}
							}
							else
							{
								this.leaderAura = AdvancedAI_Aura.AuraHediffDef(pawn);
								bool debugLog2 = SkyAiCore.Settings.debugLog;
								if (debugLog2)
								{
									Log.Message(string.Format("{0} {1}: Generate leader aura: {2} Severity: {3}", new object[]
									{
										pawn,
										pawn.Position,
										this.leaderAura.LabelCap,
										AdvancedAI_Aura.AuraLevel(pawn)
									}));
								}
							}
							Lord lord = pawn.GetLord();
							RaidData raidData = AdvancedAI.PawnRaidData(pawn);
							SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
							bool flag7 = pawn.IsHashIntervalTick(500) && raidData != null && squadData != null;
							if (flag7)
							{
								AdvancedAI_SquadUtility.UpdateSquadID(pawn, squadData);
								bool flag8 = !squadData.squadEnteredSiegeCombat;
								if (flag8)
								{
									squadData.squadEnteredSiegeCombat = AdvancedAI_SquadUtility.SquadEnteredSiegeCombat(pawn, lord);
									bool squadEnteredSiegeCombat = squadData.squadEnteredSiegeCombat;
									if (squadEnteredSiegeCombat)
									{
										AdvancedAI_SquadUtility.UpdateStageForSiegeAI(pawn);
										raidData.raidStage = RaidData.RaidStage.attack;
									}
									foreach (Pawn pawn6 in AdvancedAI_SquadUtility.ActiveLeadersList(raidData))
									{
										bool flag9 = pawn6 != pawn;
										if (flag9)
										{
											CompSquadCommanderRole compSquadCommanderRole = pawn6.TryGetComp<CompSquadCommanderRole>();
											bool flag10 = compSquadCommanderRole != null && squadData.squadEnteredSiegeCombat;
											if (flag10)
											{
												AdvancedAI_SquadUtility.UpdateStageForSiegeAI(pawn6);
												raidData.raidStage = RaidData.RaidStage.attack;
											}
										}
									}
								}
							}
							bool flag11 = raidData != null;
							if (flag11)
							{
								bool flag12 = !this.generatedStage && AdvancedAI.IsValidLoc(this.LeaderEnemyTarget);
								if (flag12)
								{
									float num = pawn.Position.DistanceTo(this.LeaderEnemyTarget);
									this.skipStage = (num < 80f);
									bool debugLog3 = SkyAiCore.Settings.debugLog;
									if (debugLog3)
									{
										Log.Message(string.Format("{0} {1}: Stage generated. Leader distance: {2}  target: {3} skipStage: {4}", new object[]
										{
											pawn,
											pawn.Position,
											num,
											this.LeaderEnemyTarget,
											this.skipStage
										}));
									}
									this.generatedStage = true;
									bool flag13 = this.skipStage;
									if (flag13)
									{
										raidData.raidStage = RaidData.RaidStage.startAttacking;
									}
								}
								this.raidHasSquadLeaders = (!raidData.squadCommanders.NullOrEmpty<Pawn>() && raidData.squadCommanders.Count > 0);
								bool flag14 = AdvancedAI.IsValidLoc(this.BlockerCell);
								if (flag14)
								{
									raidData.leaderTarget = this.BlockerCell;
								}
								List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
								bool flag15 = lord != null && !list.NullOrEmpty<Lord>();
								if (flag15)
								{
									int num2 = 0;
									try
									{
										LordToil curLordToil = lord.CurLordToil;
										using (List<Lord>.Enumerator enumerator2 = list.GetEnumerator())
										{
											while (enumerator2.MoveNext())
											{
												Lord squadLord = enumerator2.Current;
												LordToil curLordToil2 = squadLord.CurLordToil;
												bool flag16 = raidData.raidStage == RaidData.RaidStage.startAttacking || raidData.raidStage == RaidData.RaidStage.attack || raidData.raidStage == RaidData.RaidStage.fleeing;
												if (flag16)
												{
													bool flag17 = this.baseLordToil == null || this.baseLordToil == "";
													if (flag17)
													{
														this.baseLordToil = curLordToil.ToString();
													}
													bool flag18 = (this.baseLordToil != null || this.baseLordToil != "") && !curLordToil.ToString().Equals(this.baseLordToil);
													if (flag18)
													{
														bool flag19 = curLordToil2.GetType() != curLordToil.GetType();
														if (flag19)
														{
															Pawn pawn2 = AdvancedAI_SquadUtility.RaidSquadCommanders(raidData, false).Where(delegate(Pawn sc)
															{
																if (sc != pawn)
																{
																	Lord lord2 = sc.GetLord();
																	if (lord2 != null && lord2 != null && lord2 == squadLord && !AdvancedAI.PawnIsLeader(sc))
																	{
																		return !AdvancedAI.PawnIsGuard(sc);
																	}
																}
																return false;
															}).FirstOrDefault<Pawn>();
															bool flag20 = pawn2 != null;
															if (flag20)
															{
																bool debugLog4 = SkyAiCore.Settings.debugLog;
																if (debugLog4)
																{
																	Log.Message(string.Format("{0} {1}: change toil to leader {2}", pawn2, pawn2, this.baseLordToil));
																}
																AdvancedAI_LordUtility.AddLordToil(pawn2, curLordToil);
															}
														}
													}
													bool flag21 = this.syncSquadLords && AdvancedAI_LordUtility.EndAssaultColonyLordToils.Contains(curLordToil2.GetType()) && !AdvancedAI_LordUtility.EndAssaultColonyLordToils.Contains(curLordToil.GetType());
													if (flag21)
													{
														Pawn pawn3 = AdvancedAI_SquadUtility.RaidSquadCommanders(raidData, false).Where(delegate(Pawn sc)
														{
															if (sc != pawn)
															{
																Lord lord2 = sc.GetLord();
																if (lord2 != null && lord2 != null && lord2 == squadLord && !AdvancedAI.PawnIsLeader(sc))
																{
																	return !AdvancedAI.PawnIsGuard(sc);
																}
															}
															return false;
														}).FirstOrDefault<Pawn>();
														bool flag22 = pawn3 != null;
														if (flag22)
														{
															AdvancedAI_LordUtility.AddLordToil(pawn3, curLordToil);
														}
													}
												}
												num2 += squadLord.ownedPawns.Count;
											}
										}
									}
									catch (Exception arg)
									{
										Log.Error(string.Format("{0} {1}: CompLeader exception part1: {2}", pawn, pawn.Position, arg));
									}
									try
									{
										List<Pawn> list2 = AdvancedAI_SquadUtility.RaidReservedSquads(raidData);
										bool flag23 = this.initReservedSquadsCount == 0;
										if (flag23)
										{
											this.initReservedSquadsCount = list2.Count;
										}
										bool flag24 = list2.Count > 0;
										if (flag24)
										{
											bool flag25 = SkyAiCore.Settings.debugRaidData && pawn.IsHashIntervalTick(1000);
											if (flag25)
											{
												Log.Message(string.Format("{0} {1}: Reserved squads count: {2}.", pawn, pawn.Position, list2.Count));
											}
											bool flag26 = AdvancedAI_SquadUtility.ShouldUseReserveSquad(list2.Count, this.initReservedSquadsCount, num2, raidData.raidCount) || pawn.Downed || pawn.Dead || raidData.raidLeader == null;
											if (flag26)
											{
												foreach (Pawn pawn4 in list2)
												{
													SquadData squadData2 = AdvancedAI_SquadUtility.PawnSquadData(pawn4);
													bool flag27 = squadData2 != null;
													if (flag27)
													{
														bool debugLog5 = SkyAiCore.Settings.debugLog;
														if (debugLog5)
														{
															Log.Message(string.Format("{0} {1}: as Leader, ordered to send reserve to attack: {2}'s squad.", pawn, pawn.Position, pawn4));
														}
														squadData2.isReserved = false;
														break;
													}
												}
											}
										}
									}
									catch (Exception arg2)
									{
										Log.Error(string.Format("{0} {1}: CompLeader exception part2: {2}", pawn, pawn.Position, arg2));
									}
								}
								bool flag28 = lord != null && !this.usedReservation;
								if (flag28)
								{
									try
									{
										AdvancedAI_SquadUtility.RaidSquadCommandersReservation(pawn, raidData, this.skipStage, 2);
									}
									catch (Exception arg3)
									{
										Log.Error(string.Format("{0} {1}: CompLeader exception part3: {2}", pawn, pawn.Position, arg3));
									}
								}
							}
							bool flag29 = lord != null;
							if (flag29)
							{
								try
								{
									bool flag30 = raidData != null && squadData != null && raidData.raidStage == RaidData.RaidStage.gathering;
									if (flag30)
									{
										IntVec3 spot = squadData.gatherSpot;
										string squadName = squadData.squadName;
										bool flag31 = squadData.isReserved;
										int num3 = (from p in lord.ownedPawns
										where p.Position.DistanceTo(spot) <= 20f
										select p).Count<Pawn>();
										int num4 = Mathf.RoundToInt((float)lord.ownedPawns.Count * 0.9f);
										bool flag32 = num3 >= num4;
										bool flag33 = flag32 && !raidData.raidOrders.ContainsKey(pawn) && !flag31;
										if (flag33)
										{
											raidData.raidOrders.Add(pawn, flag32);
										}
										bool flag34 = flag31 && !AdvancedAI.IsActivePawn(pawn);
										if (flag34)
										{
											flag31 = false;
											raidData.raidIsReady = true;
										}
										squadData.isReady = raidData.raidIsReady;
										bool flag35 = SkyAiCore.Settings.debugLog && (pawn.IsHashIntervalTick(2000) || raidData.raidIsReady);
										if (flag35)
										{
											bool flag36 = raidData.raidIsReady && !flag31;
											if (flag36)
											{
												Log.Message(string.Format("{0} {1}: StageAttack. CompSquadCommanderRole. Squad: {2} is ready! {3}/{4}. Moving forward! Data: raidIsReady: {5} squadIsReserved: {6}", new object[]
												{
													pawn,
													pawn.Position,
													squadName,
													num3,
													num4,
													raidData.raidIsReady,
													flag31
												}));
											}
											else
											{
												bool flag37 = flag31;
												if (flag37)
												{
													Log.Message(string.Format("{0} {1}: StageAttack. CompLeaderRole. Waiting on position {2}. Squad: {3} in reserve: {4}/{5}. Waiting for the right time.", new object[]
													{
														pawn,
														pawn.Position,
														spot,
														squadName,
														num3,
														num4
													}));
												}
												else
												{
													bool flag38 = flag32;
													if (flag38)
													{
														Log.Message(string.Format("{0} {1}: StageAttack. CompLeaderRole. Gathered before attack. Squad {2} is ready! {3}/{4}. Ready to attack.", new object[]
														{
															pawn,
															pawn.Position,
															squadName,
															num3,
															num4
														}));
													}
													else
													{
														Log.Message(string.Format("{0} {1}: StageAttack. CompLeaderRole. Gathering on position {2}. Squad {3} is Ready? {4}/{5}. Nope.", new object[]
														{
															pawn,
															pawn.Position,
															spot,
															squadName,
															num3,
															num4
														}));
													}
												}
											}
										}
									}
								}
								catch (Exception arg4)
								{
									Log.Error(string.Format("{0} {1}: CompLeader exception part4: {2}", pawn, pawn.Position, arg4));
								}
								try
								{
									bool flag39 = this.bodyguardCount == 0;
									if (flag39)
									{
										this.bodyguardCount = Mathf.Clamp(Mathf.RoundToInt((float)lord.ownedPawns.Count * 0.1f), 1, 3);
										bool debugLog6 = SkyAiCore.Settings.debugLog;
										if (debugLog6)
										{
											Log.Message(string.Format("{0} {1}: CompLeaderRole. Set bodyguards count: {2}", pawn, pawn.Position, this.bodyguardCount));
										}
									}
									bool flag40 = raidData != null && (raidData.leaderGuards.NullOrEmpty<Pawn>() || (!raidData.leaderGuards.NullOrEmpty<Pawn>() && raidData.leaderGuards.Count < this.bodyguardCount));
									if (flag40)
									{
										IEnumerable<Pawn> enumerable = from p in lord.ownedPawns
										where p != null && p != pawn && base.<CompTick>g__checkGuardList|3(p) && !AdvancedAI.PawnIsDoctor(p) && !AdvancedAI.PrimaryIsSiegeWeapon(pawn) && !AdvancedAI.PawnIsSniper(p) && AdvancedAI.IsActivePawn(p)
										select p;
										bool flag41 = !enumerable.EnumerableNullOrEmpty<Pawn>();
										if (flag41)
										{
											int num5 = this.bodyguardCount - raidData.leaderGuards.Count;
											Func<Pawn, bool> <>9__6;
											for (int i = 0; i < num5; i++)
											{
												IEnumerable<Pawn> source = enumerable;
												Func<Pawn, bool> predicate;
												if ((predicate = <>9__6) == null)
												{
													predicate = (<>9__6 = ((Pawn p) => p.skills != null && !p.WorkTypeIsDisabled(WorkTypeDefOf.Hunting) && base.<CompTick>g__pawnWeapon|5(p)));
												}
												Pawn pawn5;
												source.Where(predicate).TryRandomElementByWeight((Pawn w) => (float)w.skills.GetSkill(SkillDefOf.Shooting).Level, out pawn5);
												bool flag42 = pawn5 == null;
												if (flag42)
												{
													pawn5 = enumerable.RandomElement<Pawn>();
												}
												bool flag43 = pawn5 != null;
												if (flag43)
												{
													CompGuardRole compGuardRole = pawn5.TryGetComp<CompGuardRole>();
													bool flag44 = compGuardRole == null;
													if (flag44)
													{
														compGuardRole = (CompGuardRole)Activator.CreateInstance(typeof(CompGuardRole));
														compGuardRole.parent = pawn5;
														pawn5.AllComps.Add(compGuardRole);
														compGuardRole.Initialize(compGuardRole.Props);
													}
													compGuardRole.escortee = pawn;
													raidData.leaderGuards.Add(pawn5);
													SquadData squadData3 = AdvancedAI_SquadUtility.PawnSquadData(pawn5);
													bool flag45 = squadData3 != null;
													if (flag45)
													{
														squadData3.squadPawns.Remove(pawn5);
													}
													bool debugLog7 = SkyAiCore.Settings.debugLog;
													if (debugLog7)
													{
														Log.Message(string.Format("{0} {1}: become bodyGuard for Raid Leader.", pawn5, pawn5.Position));
													}
												}
											}
										}
									}
								}
								catch (Exception arg5)
								{
									Log.Error(string.Format("{0} {1}: CompLeader exception part5: {2}", pawn, pawn.Position, arg5));
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00023FEC File Offset: 0x000221EC
		public override void PostDraw()
		{
			base.PostDraw();
			bool enableRoleIcons = SkyAiCore.Settings.enableRoleIcons;
			if (enableRoleIcons)
			{
				Material material = this.raidHasSquadLeaders ? Materials.raidLeaderIconMat : Materials.leaderIconMat;
				Pawn pawn = this.parent as Pawn;
				bool flag = material != null && pawn != null && !pawn.Dead && !pawn.Downed;
				if (flag)
				{
					Vector3 position = this.parent.TrueCenter();
					position.y = AltitudeLayer.WorldClipper.AltitudeFor() + 0.28125f;
					position.z += 1.2f;
					position.x += (float)(this.parent.def.size.x / 2);
					Graphics.DrawMesh(MeshPool.plane08, position, Quaternion.identity, material, 0);
				}
			}
		}

		// Token: 0x040000F9 RID: 249
		public bool raidHasSquadLeaders = false;

		// Token: 0x040000FA RID: 250
		public int bodyguardCount = 0;

		// Token: 0x040000FB RID: 251
		public bool usedReservation = false;

		// Token: 0x040000FC RID: 252
		public int initReservedSquadsCount;

		// Token: 0x040000FD RID: 253
		public string baseLordToil;

		// Token: 0x040000FE RID: 254
		public bool generatedStage = false;

		// Token: 0x040000FF RID: 255
		public bool skipStage;

		// Token: 0x04000100 RID: 256
		public HediffDef leaderAura = null;

		// Token: 0x04000101 RID: 257
		public bool syncSquadLords = true;

		// Token: 0x04000102 RID: 258
		public IntVec3 blockerCell;

		// Token: 0x04000103 RID: 259
		public IntVec3 leaderEnemyTarget = IntVec3.Invalid;
	}
}
