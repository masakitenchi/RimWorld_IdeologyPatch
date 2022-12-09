using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200001D RID: 29
	public static class AdvancedAI_Roles
	{
		// Token: 0x06000112 RID: 274 RVA: 0x00018ACC File Offset: 0x00016CCC
		private static AdvancedAI_Roles.WeaponClass WeaponType(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps == null;
			AdvancedAI_Roles.WeaponClass result;
			if (flag)
			{
				result = AdvancedAI_Roles.WeaponClass.none;
			}
			else
			{
				ThingWithComps thingWithComps2 = AdvancedAI.ShieldEquiped(pawn);
				bool flag2 = thingWithComps2 != null;
				if (flag2)
				{
					bool flag3 = !AdvancedAI.IsGrenade(thingWithComps);
					if (flag3)
					{
						result = AdvancedAI_Roles.WeaponClass.meleeShield;
					}
					else
					{
						result = AdvancedAI_Roles.WeaponClass.shieldGrenadier;
					}
				}
				else
				{
					bool flag4 = AdvancedAI.IsGrenade(thingWithComps);
					if (flag4)
					{
						result = AdvancedAI_Roles.WeaponClass.grenadier;
					}
					else
					{
						Verb verb = AdvancedAI.PrimaryVerb(pawn);
						bool flag5 = verb == null;
						if (flag5)
						{
							result = AdvancedAI_Roles.WeaponClass.none;
						}
						else
						{
							int num = Mathf.RoundToInt(verb.verbProps.range);
							IEnumerable<int> source = Enumerable.Range(58, 80);
							IEnumerable<int> source2 = Enumerable.Range(42, 57);
							IEnumerable<int> source3 = Enumerable.Range(1, 41);
							bool flag6 = verb.verbProps.ai_IsBuildingDestroyer && num >= 18;
							if (flag6)
							{
								result = AdvancedAI_Roles.WeaponClass.baseDestroyer;
							}
							else
							{
								bool flag7 = verb.verbProps.ai_AvoidFriendlyFireRadius > 0f && num >= 18;
								if (flag7)
								{
									result = AdvancedAI_Roles.WeaponClass.heavy;
								}
								else
								{
									bool flag8 = source.Contains(num);
									if (flag8)
									{
										result = AdvancedAI_Roles.WeaponClass.longRange;
									}
									else
									{
										bool flag9 = source2.Contains(num);
										if (flag9)
										{
											result = AdvancedAI_Roles.WeaponClass.middleRange;
										}
										else
										{
											bool flag10 = source3.Contains(num);
											if (flag10)
											{
												result = AdvancedAI_Roles.WeaponClass.shortRange;
											}
											else
											{
												result = AdvancedAI_Roles.WeaponClass.none;
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

		// Token: 0x06000113 RID: 275 RVA: 0x00018C14 File Offset: 0x00016E14
		public static Job DoctorRole(Pawn pawn, IntVec3 focusCell, bool takePosition)
		{
			bool flag = AdvancedAI.HasSiegeBuilderJob(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				CompDoctorRole comp = pawn.GetComp<CompDoctorRole>();
				bool flag2 = comp != null && !comp.DoctorIsBusy;
				if (flag2)
				{
					Pawn patient = comp.Patient;
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn) && patient != null;
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: DoctorRole. Start to check for patient: {2} on {3} ", new object[]
						{
							pawn,
							pawn.Position,
							patient,
							patient.Position
						}));
					}
					bool flag4 = patient != null && patient != pawn && AdvancedAI.IsInCloseWithTarget(pawn, patient);
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(patient));
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: DoctorRole. GetTendJob1 with {2}", pawn, pawn.Position, patient));
						}
						return AdvancedAI_Jobs.GetTendJob(pawn, IntVec3.Invalid, patient);
					}
					Pawn pawn2 = AdvancedAI_TendUtility.InjuriedAllyPawn(pawn);
					bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag6)
					{
						bool flag7 = pawn2 != null;
						if (flag7)
						{
							Log.Message(string.Format("{0} {1}: DoctorRole. Found accessible injuredPatient: {2}", pawn, pawn.Position, pawn2));
						}
						else
						{
							Log.Message(string.Format("{0} {1}: DoctorRole. Any accessible (w/o danger combat) injured allies not found.", pawn, pawn.Position));
						}
					}
					try
					{
						bool flag8 = pawn2 != null && pawn2 != pawn && !AdvancedAI_TendUtility.ReservedByAnotherDoctor(pawn, pawn2) && comp.DoctorHasMedicine;
						if (flag8)
						{
							bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: DoctorRole. Check for reservation... {2}", pawn, pawn.Position, pawn2));
							}
							bool flag10 = (!comp.Reserved && comp.Patient == null) || (comp.Patient != null && comp.Patient == pawn2);
							if (flag10)
							{
								bool flag11 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag11)
								{
									Log.Message(string.Format("{0} {1}: DoctorRole. Is InjuriedPatient too far? And should use standart medical treatment?: {2} TreatmentType: {3} ", new object[]
									{
										pawn,
										pawn.Position,
										AdvancedAI_TendUtility.PawnShouldUseStandartMedicalTreatment(pawn2, focusCell),
										comp.GetTreatmentType
									}));
								}
								bool flag12 = comp.GetTreatmentType == CompDoctorRole.TreatmentType.standart;
								if (flag12)
								{
									bool flag13 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag13)
									{
										Log.Message(string.Format("{0} {1}: DoctorRole. InjuredPatient found: {2} on {3} using treatmentType: {4} ", new object[]
										{
											pawn,
											pawn.Position,
											pawn2,
											pawn2.Position,
											comp.GetTreatmentType
										}));
									}
									IntVec3 intVec = IntVec3.Invalid;
									int num = Mathf.RoundToInt(pawn2.Position.DistanceTo(pawn.Position) + 1f);
									bool flag14 = !pawn2.RaceProps.Animal;
									if (flag14)
									{
										bool flag15 = !AdvancedAI_CoverUtility.GetCoverCloserToAllyFrom(pawn2, pawn2.Position, (float)num, true, false, false, false, false, true, out intVec, pawn);
										if (flag15)
										{
											bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag16)
											{
												Log.Message(string.Format("{0} {1}: DoctorRole. InjuredPatient cover position is not valid", pawn, pawn.Position));
											}
											intVec = pawn2.Position;
										}
									}
									bool flag17 = !intVec.IsValid;
									if (flag17)
									{
										bool flag18 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag18)
										{
											Log.Message(string.Format("{0} {1}: DoctorRole. InjuredPatient position is not valid", pawn, pawn.Position));
										}
										intVec = pawn2.Position;
									}
									bool flag19 = AdvancedAI.CanMove(pawn2) && (double)pawn2.GetStatValue(StatDefOf.MoveSpeed, true) >= 2.4;
									if (flag19)
									{
										bool flag20 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn2));
										if (flag20)
										{
											Log.Message(string.Format("{0} {1}: DoctorRole. InjuredPatient {2} forced start job. Moving to injuredPatientCoverPosition: {3} treatmentType: {4}", new object[]
											{
												pawn,
												pawn.Position,
												pawn2,
												intVec,
												comp.GetTreatmentType
											}));
										}
										Job coverJob = AdvancedAI_Jobs.GetCoverJob(pawn2, intVec, IntVec3.Invalid, AdvancedAI.ExpireInterval.normal, true, true, true);
										bool flag21 = coverJob != null;
										if (flag21)
										{
											pawn2.jobs.StartJob(coverJob, JobCondition.InterruptForced, null, false, true, null, null, false, false);
										}
									}
									else
									{
										intVec = pawn2.Position;
										Job coverJob2 = AdvancedAI_Jobs.GetCoverJob(pawn2, intVec, IntVec3.Invalid, AdvancedAI.ExpireInterval.normal, true, true, true);
										bool flag22 = coverJob2 != null;
										if (flag22)
										{
											pawn2.jobs.StartJob(coverJob2, JobCondition.InterruptForced, null, false, true, null, null, false, false);
										}
									}
									try
									{
										comp.patient = pawn2;
										comp.treatmentType = CompDoctorRole.TreatmentType.standart;
										bool flag23 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn2));
										if (flag23)
										{
											Log.Message(string.Format("{0} {1}: DoctorRole. GetTendJob2 with injuredPatient {2} on pos: {3}", new object[]
											{
												pawn,
												pawn.Position,
												pawn2,
												intVec
											}));
										}
										return AdvancedAI_Jobs.GetTendJob(pawn, intVec, pawn2);
									}
									catch (Exception arg)
									{
										Log.Error(string.Format("{0} {1}: DoctorRole. InjuredPatient end part exception: {2}", pawn, pawn.Position, arg));
									}
								}
							}
						}
					}
					catch (Exception arg2)
					{
						Log.Error(string.Format("{0} {1}: DoctorRole. InjuredPatient part exception: {2}", pawn, pawn.Position, arg2));
					}
					try
					{
						bool enemyDoctorWillApplyFirstAidToDownedAllies = SkyAiCore.Settings.enemyDoctorWillApplyFirstAidToDownedAllies;
						if (enemyDoctorWillApplyFirstAidToDownedAllies)
						{
							Pawn pawn3 = AdvancedAI_TendUtility.IncapableAllyPawn(pawn);
							bool flag24 = pawn3 != null && pawn3 != pawn && !AdvancedAI_TendUtility.ReservedByAnotherDoctor(pawn, pawn3);
							if (flag24)
							{
								bool flag25 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag25)
								{
									Log.Message(string.Format("{0} {1}: DoctorRole. Check for reservation... {2}", pawn, pawn.Position, pawn3));
								}
								bool flag26 = (!comp.Reserved && comp.Patient == null) || comp.Patient == pawn3;
								if (flag26)
								{
									try
									{
										bool flag27 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag27)
										{
											Log.Message(string.Format("{0} {1}: DoctorRole. IncapablePatient found: {2} on {3} ", new object[]
											{
												pawn,
												pawn.Position,
												pawn3,
												pawn3.Position
											}));
										}
										comp.patient = pawn3;
										comp.treatmentType = CompDoctorRole.TreatmentType.standart;
										bool flag28 = AdvancedAI_TendUtility.RequireConsciousnessBuff(pawn, pawn3);
										if (flag28)
										{
											Job result2 = JobMaker.MakeJob(JobDefOfAI.AddConsciousnessBuff, pawn3);
											bool flag29 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag29)
											{
												Log.Message(string.Format("{0} {1}: DoctorRole. AddConsciousnessBuff job. Moving to incapable pawn: {2} on {3} for buff incapable patient.", new object[]
												{
													pawn,
													pawn.Position,
													pawn3,
													pawn3.Position
												}));
											}
											return result2;
										}
										bool doctorHasMedicine = comp.DoctorHasMedicine;
										if (doctorHasMedicine)
										{
											bool flag30 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn3));
											if (flag30)
											{
												Log.Message(string.Format("{0} {1}: DoctorRole. GetTendJob5 with incapablePatient {2} on {3}", new object[]
												{
													pawn,
													pawn.Position,
													pawn3,
													pawn3.Position
												}));
											}
											return AdvancedAI_Jobs.GetTendJob(pawn, IntVec3.Invalid, pawn3);
										}
										bool flag31 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag31)
										{
											Log.Message(string.Format("{0} {1}: DoctorRole. Can't tend incapable patient: {2} on {3} without medicine.", new object[]
											{
												pawn,
												pawn.Position,
												pawn3,
												pawn3.Position
											}));
										}
									}
									catch (Exception arg3)
									{
										Log.Error(string.Format("{0} {1}: DoctorRole. IncapablePatient end part exception: {2}", pawn, pawn.Position, arg3));
									}
								}
							}
						}
					}
					catch (Exception arg4)
					{
						Log.Error(string.Format("{0} {1}: DoctorRole. IncapablePatient part exception: {2}", pawn, pawn.Position, arg4));
					}
					IntVec3 intVec2 = takePosition ? AdvancedAI.GetFocusCell(pawn, focusCell, takePosition) : focusCell;
					bool flag32 = takePosition && comp.Patient == null && AdvancedAI.DutyHasAttackSubNodes(pawn, false) && AdvancedAI.IsValidLoc(intVec2);
					if (flag32)
					{
						int num2 = AdvancedAI.CalculateDistance(pawn, intVec2, 2, 9);
						bool flag33 = AdvancedAI_SquadUtility.IsStartingRaidStage(pawn) || pawn.Position.DistanceTo(intVec2) > 80f;
						if (flag33)
						{
							bool flag34 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag34)
							{
								Log.Message(string.Format("{0} {1}: DoctorRole. Distant to focusCell is too long or in the starting raid stage. Doctor start to follow closest lord pawn with radius: {2}", pawn, pawn.Position, num2));
							}
							return AdvancedAI_Jobs.LordFollowJob(pawn, (float)num2, 180, true, false, true);
						}
						IntVec3 intVec3;
						bool doctorCoverPosition = AdvancedAI_CoverUtility.GetDoctorCoverPosition(pawn, intVec2, out intVec3);
						if (doctorCoverPosition)
						{
							bool flag35 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag35)
							{
								Log.Message(string.Format("{0} {1}: DoctorRole. I'am free and going to cover: {2} with focusCell: {3}", new object[]
								{
									pawn,
									pawn.Position,
									intVec3,
									intVec2
								}));
							}
							bool ignoreFocusCellMinDistance = AdvancedAI.PrimaryVerb(pawn) != null && AdvancedAI.PrimaryVerb(pawn).IsMeleeAttack;
							Job coverJob3 = AdvancedAI_Jobs.GetCoverJob(pawn, intVec3, intVec2, AdvancedAI.ExpireInterval.fast, ignoreFocusCellMinDistance, false, true);
							bool flag36 = coverJob3 != null;
							if (flag36)
							{
								return coverJob3;
							}
						}
						bool flag37 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag37)
						{
							Log.Message(string.Format("{0} {1}: DoctorRole. Start to follow closest lord pawn with radius: {2}", pawn, pawn.Position, num2));
						}
						return AdvancedAI_Jobs.LordFollowJob(pawn, (float)num2, 140, true, false, true);
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000196F8 File Offset: 0x000178F8
		public static Job LeaderRole(Pawn pawn, IntVec3 focusCell)
		{
			bool flag = AdvancedAI.HasSiegeBuilderJob(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = !AdvancedAI.HasDefendDuty(pawn);
				if (flag2)
				{
					IntVec3 focusCell2 = AdvancedAI.GetFocusCell(pawn, focusCell, true);
					bool flag3 = AdvancedAI.IsValidLoc(focusCell2);
					if (flag3)
					{
						SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
						bool flag4 = (squadData != null && squadData.squadEnteredSiegeCombat) || (squadData == null && !AdvancedAI_SquadUtility.IsStartingRaidStage(pawn));
						if (flag4)
						{
							bool ignoreFocusCellMinDistance = true;
							IntVec3 position;
							bool flag5 = !AdvancedAI_CoverUtility.GetLeaderCoverPosition(pawn, focusCell, out position);
							if (flag5)
							{
								ignoreFocusCellMinDistance = false;
								bool flag6 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, (float)AdvancedAI.CalculateDistance(pawn, focusCell, 4, 19), 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out position);
								if (flag6)
								{
									bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag7)
									{
										Log.Message(string.Format("{0} {1}: LeaderRole. GetCoverPositionFrom failed. Leader is free and start to follow closest lord pawn.", pawn, pawn.Position));
									}
									return AdvancedAI_Jobs.LordFollowJob(pawn, 4f, 140, true, false, true);
								}
							}
							Job coverJob = AdvancedAI_Jobs.GetCoverJob(pawn, position, focusCell, AdvancedAI.ExpireInterval.normal, ignoreFocusCellMinDistance, false, true);
							bool flag8 = coverJob != null;
							if (flag8)
							{
								return coverJob;
							}
						}
						int num = AdvancedAI.CalculateDistance(pawn, focusCell, 2, 9);
						bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag9)
						{
							Log.Message(string.Format("{0} {1}: LeaderRole. GetLeaderCoverPosition. Cover job not available. Leader start to follow closest lord pawn with radius: {2} EnteredSiegeCombat: {3}", new object[]
							{
								pawn,
								pawn.Position,
								num,
								squadData.squadEnteredSiegeCombat
							}));
						}
						return AdvancedAI_Jobs.LordFollowJob(pawn, (float)num, 140, true, false, true);
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000198C0 File Offset: 0x00017AC0
		public static Job SquadCommanderRole(Pawn pawn, IntVec3 focusCell)
		{
			bool flag = AdvancedAI.HasSiegeBuilderJob(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = !AdvancedAI.HasDefendDuty(pawn);
				if (flag2)
				{
					IntVec3 focusCell2 = AdvancedAI.GetFocusCell(pawn, focusCell, true);
					bool flag3 = AdvancedAI.IsValidLoc(focusCell2);
					if (flag3)
					{
						SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
						bool flag4 = (squadData != null && squadData.squadEnteredSiegeCombat) || (squadData == null && !AdvancedAI_SquadUtility.IsStartingRaidStage(pawn));
						if (flag4)
						{
							bool ignoreFocusCellMinDistance = true;
							IntVec3 position;
							bool flag5 = !AdvancedAI_CoverUtility.GetLeaderCoverPosition(pawn, focusCell, out position);
							if (flag5)
							{
								ignoreFocusCellMinDistance = false;
								bool flag6 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, (float)AdvancedAI.CalculateDistance(pawn, focusCell, 4, 19), 0f, true, true, true, true, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out position);
								if (flag6)
								{
									bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag7)
									{
										Log.Message(string.Format("{0} {1}: SquadCommanderRole. GetCoverPositionFrom failed. Squad commander is free and start to follow closest lord pawn.", pawn, pawn.Position));
									}
									return AdvancedAI_Jobs.LordFollowJob(pawn, 4f, 140, true, false, true);
								}
							}
							Job coverJob = AdvancedAI_Jobs.GetCoverJob(pawn, position, focusCell, AdvancedAI.ExpireInterval.normal, ignoreFocusCellMinDistance, false, true);
							bool flag8 = coverJob != null;
							if (flag8)
							{
								return coverJob;
							}
						}
						int num = AdvancedAI.CalculateDistance(pawn, focusCell, 2, 9);
						bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag9)
						{
							Log.Message(string.Format("{0} {1}: SquadCommanderRole. GetLeaderCoverPosition failed. Cover job not available. Squad commander start to follow closest lord pawn with radius: {2}. EnteredSiegeCombat: {3}", new object[]
							{
								pawn,
								pawn.Position,
								num,
								squadData.squadEnteredSiegeCombat
							}));
						}
						return AdvancedAI_Jobs.LordFollowJob(pawn, (float)num, 140, true, false, true);
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00019A88 File Offset: 0x00017C88
		public static Job SniperRole(Pawn pawn, IntVec3 focusCell)
		{
			bool flag = AdvancedAI.HasSiegeBuilderJob(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.IsHashIntervalTick(3) && AdvancedAI.SniperIsAlone(pawn);
				if (flag2)
				{
					result = AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, true, false);
				}
				else
				{
					bool flag3 = !AdvancedAI.HasDefendDuty(pawn);
					if (flag3)
					{
						IntVec3 focusCell2 = AdvancedAI.GetFocusCell(pawn, focusCell, true);
						bool flag4 = AdvancedAI.IsValidLoc(focusCell2);
						if (flag4)
						{
							IntRange intRange = AdvancedAI.PrimaryEffectiveWeaponRange(pawn, 0.55f, 1f, false, 25);
							bool inRandomOrder = AdvancedAI.GetEnemyTarget(pawn, true, false) != null;
							IntVec3 position;
							bool sniperCoverPosition = AdvancedAI_CoverUtility.GetSniperCoverPosition(pawn, focusCell2, intRange, true, false, true, true, false, inRandomOrder, false, out position);
							if (sniperCoverPosition)
							{
								return AdvancedAI_Jobs.GetCoverJob(pawn, position, focusCell2, AdvancedAI.ExpireInterval.normal, false, false, true);
							}
						}
					}
					result = null;
				}
			}
			return result;
		}

		// Token: 0x020000B4 RID: 180
		private enum WeaponClass
		{
			// Token: 0x04000255 RID: 597
			none,
			// Token: 0x04000256 RID: 598
			melee,
			// Token: 0x04000257 RID: 599
			meleeShield,
			// Token: 0x04000258 RID: 600
			grenadier,
			// Token: 0x04000259 RID: 601
			shieldGrenadier,
			// Token: 0x0400025A RID: 602
			baseDestroyer,
			// Token: 0x0400025B RID: 603
			heavy,
			// Token: 0x0400025C RID: 604
			shortRange,
			// Token: 0x0400025D RID: 605
			middleRange,
			// Token: 0x0400025E RID: 606
			longRange
		}
	}
}
