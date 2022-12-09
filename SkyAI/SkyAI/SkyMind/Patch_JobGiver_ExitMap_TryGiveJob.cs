using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200003E RID: 62
	[HarmonyPatch(typeof(JobGiver_ExitMap))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_ExitMap_TryGiveJob
	{
		// Token: 0x060001D2 RID: 466 RVA: 0x000290D8 File Offset: 0x000272D8
		private static void Postfix(ref JobGiver_ExitMap __instance, ref Pawn pawn, ref Job __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = !AdvancedAI.IsHumanlikeOnly(pawn);
				if (!flag)
				{
					IntVec3 intVec;
					AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(pawn, out intVec, true, false);
					bool flag2 = AdvancedAI.IsValidLoc(intVec);
					if (flag2)
					{
						bool debugLeaveCells = SkyAiCore.Settings.debugLeaveCells;
						if (debugLeaveCells)
						{
							Log.Message(string.Format("{0} {1}: JobGiver_ExitMap.TryGiveJob using TryPerfectExitSpot, found cell to leave map: {2} from cur. position: {3}", new object[]
							{
								pawn,
								pawn.Position,
								intVec,
								pawn.Position
							}));
							pawn.Map.debugDrawer.FlashCell(intVec, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
						}
					}
					bool isValid = intVec.IsValid;
					if (isValid)
					{
						try
						{
							Job evacuateLeavingJob = AdvancedAI_Jobs.GetEvacuateLeavingJob(pawn, intVec, false, true, true);
							bool flag3 = evacuateLeavingJob != null;
							if (flag3)
							{
								bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag4)
								{
									Log.Message(string.Format("{0} from using JobGiver_ExitMap : GetEvacuateLeavingJob going to spot: {1}", pawn, intVec));
									pawn.Map.debugDrawer.FlashCell(intVec, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
								}
								__result = evacuateLeavingJob;
								return;
							}
						}
						catch (Exception arg)
						{
							Log.Error(string.Format("{0} {1} : JobGiver_ExitMap: GetEvacuateLeavingJob part exception: {2}", pawn, pawn.Position, arg));
						}
						bool flag5 = SkyAiCore.Settings.enableStealingMode && pawn.Faction != null && !pawn.Faction.Hidden && pawn.Faction.def.humanlikeFaction && pawn.RaceProps.intelligence >= Intelligence.Humanlike && pawn.Faction.HostileTo(Faction.OfPlayer);
						if (flag5)
						{
							Lord lord = pawn.GetLord();
							bool flag6 = lord != null && lord.LordJob is LordJob_AssaultColony;
							if (flag6)
							{
								try
								{
									Job job = AdvancedAI_Jobs.StealToInventoryJob(pawn);
									bool flag7 = job != null;
									if (flag7)
									{
										__result = job;
										return;
									}
								}
								catch (Exception arg2)
								{
									Log.Error(string.Format("{0} {1} : JobGiver_ExitMap: Steal to inventory part exception: {2}", pawn, pawn.Position, arg2));
								}
								bool flag8 = AdvancedAI.InDangerousCombat(pawn, 30f);
								Thing thing;
								bool flag9 = AdvancedAI.TryFindBestItemToSteal(pawn.Position, pawn.Map, flag8 ? 18f : 4f, out thing, pawn, null);
								if (flag9)
								{
									bool flag10 = thing != null;
									if (flag10)
									{
										bool flag11 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag11)
										{
											Log.Message(string.Format("{0} from using TryFindBestItemToSteal going to thing on: {1}", pawn, thing.Position));
											pawn.Map.debugDrawer.FlashCell(thing.Position, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
										}
										Job job2 = JobMaker.MakeJob(JobDefOf.Steal);
										job2.targetA = thing;
										job2.targetB = intVec;
										job2.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
										job2.count = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit));
										bool flag12 = job2 != null;
										if (flag12)
										{
											__result = job2;
											return;
										}
									}
								}
							}
						}
						Job job3 = JobMaker.MakeJob(JobDefOf.Goto, intVec);
						job3.exitMapOnArrival = true;
						job3.failIfCantJoinOrCreateCaravan = false;
						job3.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
						job3.expiryInterval = 999999;
						job3.canBashDoors = true;
						job3.canBashFences = true;
						__result = job3;
					}
				}
			}
		}
	}
}
