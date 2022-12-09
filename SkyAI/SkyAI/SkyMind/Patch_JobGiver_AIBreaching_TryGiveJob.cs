using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(JobGiver_AIBreaching))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_AIBreaching_TryGiveJob
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00002AA4 File Offset: 0x00000CA4
		private static bool Prefix(ref JobGiver_AIBreaching __instance, Pawn pawn, ref Job __result)
		{
			bool flag = SkyAiCore.Settings.debugDisableSkyAI || pawn.RaceProps.Animal || pawn.RaceProps.intelligence == Intelligence.Animal || (pawn.Faction != null && pawn.Faction.def.techLevel == TechLevel.Animal);
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				IntVec3 cell = pawn.mindState.duty.focus.Cell;
				bool flag2 = cell.IsValid && (float)cell.DistanceToSquared(pawn.Position) < 25f && cell.GetRoom(pawn.Map) == pawn.GetRoom(RegionType.Set_All) && cell.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable);
				if (flag2)
				{
					pawn.GetLord().Notify_ReachedDutyLocation(pawn);
					__result = null;
					result = false;
				}
				else
				{
					Verb verb = BreachingUtility.FindVerbToUseForBreaching(pawn);
					bool flag3 = verb == null;
					if (flag3)
					{
						__result = null;
						result = false;
					}
					else
					{
						MethodInfo methodInfo = AccessTools.Method(typeof(JobGiver_AIBreaching), "UpdateBreachingTarget", null, null);
						methodInfo.Invoke(__instance, new object[]
						{
							pawn,
							verb
						});
						BreachingTargetData breachingTarget = pawn.mindState.breachingTarget;
						Lord lord = pawn.GetLord();
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							LordJob lordJob = lord.LordJob;
							bool flag5 = lord.LordJob is LordJob_StageAttack;
							LordToil curLordToil = lord.CurLordToil;
							bool flag6 = lord.CurLordToil is LordToil_Stage;
							Log.Message(string.Format("{0} {1}: BreachingAI. lord data: lordJob: {2} isStageAttack: {3} curLordToil: {4} isStagetToil: {5} ticksInToil: {6}", new object[]
							{
								pawn,
								pawn.Position,
								lordJob,
								flag5,
								curLordToil,
								flag6,
								lord.ticksInToil
							}));
						}
						Job job = AdvancedAI_Jobs.SurvivalDecisions(pawn, cell);
						bool flag7 = job != null;
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: BreachingAI. Start check survival decisions. Focus cell: {2}", pawn, pawn.Position, cell));
							}
							__result = job;
							result = false;
						}
						else
						{
							bool flag9 = SkyAiCore.Settings.enableDoctorRole && AdvancedAI.PawnIsDoctor(pawn);
							if (flag9)
							{
								Job job2 = AdvancedAI_Roles.DoctorRole(pawn, cell, true);
								bool flag10 = job2 != null;
								if (flag10)
								{
									bool flag11 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag11)
									{
										Log.Message(string.Format("{0} {1}: BreachingAI. Doctor role! Focus cell: {2}", pawn, pawn.Position, cell));
									}
									__result = job2;
									return false;
								}
							}
							bool flag12 = SkyAiCore.Settings.enableLeaderRole && AdvancedAI.PawnIsLeader(pawn);
							if (flag12)
							{
								Job job3 = AdvancedAI_Roles.LeaderRole(pawn, cell);
								bool flag13 = job3 != null;
								if (flag13)
								{
									bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag14)
									{
										Log.Message(string.Format("{0} {1}: BreachingAI. Leader role! Focus cell: {2}", pawn, pawn.Position, cell));
									}
									__result = job3;
									return false;
								}
							}
							bool flag15 = SkyAiCore.Settings.enableSniperRole && AdvancedAI.PawnIsSniper(pawn);
							if (flag15)
							{
								Job job4 = AdvancedAI_Roles.SniperRole(pawn, cell);
								bool flag16 = job4 != null;
								if (flag16)
								{
									bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag17)
									{
										Log.Message(string.Format("{0} {1}: BreachingAI. Sniper role! Focus cell: {2}", pawn, pawn.Position, cell));
									}
									__result = job4;
									return false;
								}
							}
							bool flag18 = AdvancedAI.TryToSwitchToSiegeWeapon(pawn);
							if (flag18)
							{
								verb = AdvancedAI.PrimaryVerb(pawn);
							}
							ThingWithComps weapon = AdvancedAI.PrimaryWeapon(pawn);
							bool flag19 = !AdvancedAI.IsSiegeWeapon(weapon);
							if (flag19)
							{
								__result = null;
								result = false;
							}
							else
							{
								bool flag20 = breachingTarget == null;
								if (flag20)
								{
									bool flag21 = cell.IsValid && pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag21)
									{
										Job job5 = JobMaker.MakeJob(JobDefOf.Goto, cell, 500, true);
										BreachingUtility.FinalizeTrashJob(job5);
										__result = job5;
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
									bool flag22 = !breachingTarget.firingPosition.IsValid;
									if (flag22)
									{
										__result = null;
										result = false;
									}
									else
									{
										Thing target = breachingTarget.target;
										IntVec3 firingPosition = breachingTarget.firingPosition;
										bool isMeleeAttack = verb.IsMeleeAttack;
										if (isMeleeAttack)
										{
											Job job6 = JobMaker.MakeJob(JobDefOf.AttackMelee, target, firingPosition);
											job6.verbToUse = verb;
											BreachingUtility.FinalizeTrashJob(job6);
											__result = job6;
											result = false;
										}
										else
										{
											bool flag23 = firingPosition.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(firingPosition, pawn, false);
											Job job7 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing, target, flag23 ? firingPosition : IntVec3.Invalid);
											job7.verbToUse = verb;
											job7.preventFriendlyFire = true;
											BreachingUtility.FinalizeTrashJob(job7);
											__result = job7;
											result = false;
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

		// Token: 0x06000027 RID: 39 RVA: 0x00002FF0 File Offset: 0x000011F0
		public static Verb FindVerbToUseForBreaching(Pawn pawn)
		{
			Pawn_EquipmentTracker equipment = pawn.equipment;
			CompEquippable compEquippable = (equipment != null) ? equipment.PrimaryEq : null;
			bool flag = compEquippable == null;
			Verb result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Verb primaryVerb = compEquippable.PrimaryVerb;
				bool flag2 = Patch_JobGiver_AIBreaching_TryGiveJob.UsableVerb(primaryVerb) && primaryVerb.verbProps.ai_IsBuildingDestroyer;
				if (flag2)
				{
					result = primaryVerb;
				}
				else
				{
					List<Verb> allVerbs = compEquippable.AllVerbs;
					for (int i = 0; i < allVerbs.Count; i++)
					{
						Verb verb = allVerbs[i];
						bool flag3 = Patch_JobGiver_AIBreaching_TryGiveJob.UsableVerb(verb) && verb.verbProps.ai_IsBuildingDestroyer;
						if (flag3)
						{
							return verb;
						}
					}
					bool flag4 = Patch_JobGiver_AIBreaching_TryGiveJob.UsableVerb(primaryVerb);
					if (flag4)
					{
						result = primaryVerb;
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000030BC File Offset: 0x000012BC
		private static bool UsableVerb(Verb verb)
		{
			return verb != null && verb.Available() && verb.HarmsHealth();
		}

		// Token: 0x0400000F RID: 15
		private const float ReachDestDist = 5f;

		// Token: 0x04000010 RID: 16
		private const int CheckOverrideInterval = 500;

		// Token: 0x04000011 RID: 17
		private const float WanderDuringBusyJobChance = 0.3f;

		// Token: 0x04000012 RID: 18
		private static IntRange WanderTicks = new IntRange(30, 80);
	}
}
