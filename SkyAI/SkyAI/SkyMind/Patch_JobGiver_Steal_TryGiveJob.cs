using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200003F RID: 63
	[HarmonyPatch(typeof(JobGiver_Steal))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_Steal_TryGiveJob
	{
		// Token: 0x060001D4 RID: 468 RVA: 0x000294C8 File Offset: 0x000276C8
		private static bool Prefix(ref JobGiver_Steal __instance, Pawn pawn, ref Job __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				IntVec3 intVec;
				bool flag = !AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(pawn, out intVec, false, false);
				if (flag)
				{
					__result = null;
					result = false;
				}
				else
				{
					try
					{
						Job evacuateLeavingJob = AdvancedAI_Jobs.GetEvacuateLeavingJob(pawn, intVec, false, true, true);
						bool flag2 = evacuateLeavingJob != null;
						if (flag2)
						{
							bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag3)
							{
								Log.Message(string.Format("{0} from using JobGiver_Steal: GetEvacuateLeavingJob going to spot: {1}", pawn, intVec));
								pawn.Map.debugDrawer.FlashCell(intVec, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
							}
							__result = evacuateLeavingJob;
							return false;
						}
					}
					catch (Exception arg)
					{
						Log.Error(string.Format("{0} {1} : JobGiver_Steal: GetEvacuateLeavingJob part exception: {2}", pawn, pawn.Position, arg));
					}
					bool flag4 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn) && pawn.pather != null;
					if (flag4)
					{
						PawnPath curPath = pawn.pather.curPath;
						bool flag5 = curPath != null && curPath.Found;
						if (flag5)
						{
							Log.Message(string.Format("{0} path to map exit from JobGiver_Steal : {1}", pawn, intVec));
							List<IntVec3> nodesReversed = curPath.NodesReversed;
							int num = nodesReversed.Count - 1;
							for (int i = num; i > 0; i--)
							{
								pawn.Map.debugDrawer.FlashCell(nodesReversed[i], 0.23f, null, SkyAiCore.Settings.flashCellDelay);
							}
						}
					}
					try
					{
						Job job = AdvancedAI_Jobs.StealToInventoryJob(pawn);
						bool flag6 = job != null;
						if (flag6)
						{
							__result = job;
							return false;
						}
					}
					catch (Exception arg2)
					{
						Log.Error(string.Format("{0} {1} : JobGiver_Steal: Steal to inventory part exception: {2}", pawn, pawn.Position, arg2));
					}
					try
					{
						bool flag7 = AdvancedAI.InDangerousCombat(pawn, 30f);
						Thing thing;
						bool flag8 = AdvancedAI.TryFindBestItemToSteal(pawn.Position, pawn.Map, flag7 ? 20f : 8f, out thing, pawn, null);
						if (flag8)
						{
							bool flag9 = thing != null;
							if (flag9)
							{
								bool flag10 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag10)
								{
									Log.Message(string.Format("{0} from using TryFindBestItemToSteal going to thing on: {1}", pawn, thing.Position));
									pawn.Map.debugDrawer.FlashCell(thing.Position, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
								}
								Job job2 = JobMaker.MakeJob(JobDefOf.Steal);
								job2.targetA = thing;
								job2.targetB = intVec;
								job2.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
								job2.count = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit));
								bool flag11 = job2 != null;
								if (flag11)
								{
									__result = job2;
									return false;
								}
							}
						}
					}
					catch (Exception arg3)
					{
						Log.Error(string.Format("{0} {1} : JobGiver_Steal: TryFindBestItemToSteal part exception: {2}", pawn, pawn.Position, arg3));
					}
					__result = null;
					result = false;
				}
			}
			return result;
		}
	}
}
