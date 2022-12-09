using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000045 RID: 69
	[HarmonyPatch(typeof(JobGiver_Wander))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_WanderNearDutyLocation_TryGiveJob
	{
		// Token: 0x060001E0 RID: 480 RVA: 0x0002A240 File Offset: 0x00028440
		private static void Postfix(ref JobGiver_WanderNearDutyLocation __instance, ref Pawn pawn, ref Job __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = __instance != null;
				if (flag)
				{
					bool flag2 = pawn.RaceProps.Animal || pawn.RaceProps.intelligence == Intelligence.Animal || (pawn.Faction != null && pawn.Faction.def.techLevel == TechLevel.Animal);
					if (!flag2)
					{
						Lord lord = pawn.GetLord();
						bool flag3 = lord != null && lord.LordJob is LordJob_StageAttack;
						if (!flag3)
						{
							LocalTargetInfo enemyTarget = AdvancedAI.GetEnemyTarget(pawn, false, false);
							PawnDuty duty = pawn.mindState.duty;
							bool flag4 = pawn.IsPrisoner || pawn.IsSlave || (pawn.Faction != null && !pawn.Faction.HostileTo(Faction.OfPlayer));
							if (!flag4)
							{
								bool flag5 = pawn.RaceProps.intelligence == Intelligence.Humanlike && duty != null && duty.def == DutyDefOf.Defend;
								if (flag5)
								{
									IntVec3 intVec = (enemyTarget == null) ? new IntVec3(Mathf.RoundToInt((float)(pawn.Map.Size.x / 2)), 0, Mathf.RoundToInt((float)(pawn.Map.Size.z / 2))) : enemyTarget.Cell;
									bool isValid = intVec.IsValid;
									if (isValid)
									{
										bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
										IntVec3 intVec2;
										bool flag6 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, intVec, 20f, 0f, true, false, true, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.DutyFrontCellsOnly, out intVec2);
										if (flag6)
										{
											bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag7)
											{
												Log.Message(string.Format("{0} {1}: DefendAI. Cover position not found. Cover on current position: {2} from position: {3}", new object[]
												{
													pawn,
													pawn.Position,
													intVec2,
													intVec
												}));
											}
											__result = AdvancedAI_Jobs.GetCoverJob(pawn, pawn.Position, intVec, AdvancedAI.ExpireInterval.normal, false, false, true);
										}
										else
										{
											string text;
											bool flag8 = intVec2.IsValid && AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, intVec, true, false, false, false, useFriendlyFire, false, false, out text) > 0f;
											if (flag8)
											{
												bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag9)
												{
													Log.Message(string.Format("{0} {1}: DefendAI. Found cover position: {2} from position: {3}", new object[]
													{
														pawn,
														pawn.Position,
														intVec2,
														intVec
													}));
												}
												__result = AdvancedAI_Jobs.GetCoverJob(pawn, intVec2, intVec, AdvancedAI.ExpireInterval.normal, false, false, true);
											}
										}
									}
									else
									{
										bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag10)
										{
											Log.Message(string.Format("{0} {1}: DefendAI. focusCell is not valid. Ignore covering.", pawn, pawn.Position));
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
