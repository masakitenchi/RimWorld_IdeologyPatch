using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000040 RID: 64
	[HarmonyPatch(typeof(JobGiver_Kidnap))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_Kidnap_TryGiveJob
	{
		// Token: 0x060001D6 RID: 470 RVA: 0x00029850 File Offset: 0x00027A50
		private static bool Prefix(ref JobGiver_Kidnap __instance, Pawn pawn, ref Job __result)
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
								Log.Message(string.Format("{0} from using JobGiver_Kidnap: GetEvacuateLeavingJob going to spot: {1}", pawn, intVec));
								pawn.Map.debugDrawer.FlashCell(intVec, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
							}
							__result = evacuateLeavingJob;
							return false;
						}
					}
					catch (Exception arg)
					{
						Log.Error(string.Format("{0} {1} : JobGiver_Kidnap: GetEvacuateLeavingJob part exception: {2}", pawn, pawn.Position, arg));
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
					bool flag6 = AdvancedAI.InDangerousCombat(pawn, 30f);
					try
					{
						Pawn pawn2;
						bool flag7 = AdvancedAI.TryFindGoodKidnapVictim(pawn, flag6 ? 25f : 10f, out pawn2, null) && pawn2 != null && pawn.CanReserveAndReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false);
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} from using TryFindGoodKidnapVictim going to victim on: {1}", pawn, pawn2.Position));
								pawn.Map.debugDrawer.FlashCell(pawn2.Position, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
							}
							Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
							job.targetA = pawn2;
							job.targetB = intVec;
							job.count = 1;
							job.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
							bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag9)
							{
								Log.Message(string.Format("{0}: KidnapJobGiver: Kidnaping: {1}", pawn, pawn2));
							}
							bool flag10 = job != null;
							if (flag10)
							{
								__result = job;
								return false;
							}
						}
					}
					catch (Exception arg2)
					{
						Log.Error(string.Format("{0} {1} : Kidnap: TryFindGoodKidnapVictim: Kidnaping part exception: {2}", pawn, pawn.Position, arg2));
					}
					Job job2 = AdvancedAI_Jobs.StealToInventoryJob(pawn);
					bool flag11 = job2 != null;
					if (flag11)
					{
						__result = job2;
						result = false;
					}
					else
					{
						try
						{
							Thing thing;
							bool flag12 = pawn.Faction != null && !pawn.Faction.Hidden && pawn.Faction.def.humanlikeFaction && pawn.Faction.HostileTo(Faction.OfPlayer) && AdvancedAI.TryFindBestItemToSteal(pawn.Position, pawn.Map, flag6 ? 24f : 6f, out thing, pawn, null);
							if (flag12)
							{
								bool flag13 = thing != null;
								if (flag13)
								{
									bool flag14 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag14)
									{
										Log.Message(string.Format("{0} from using TryFindBestItemToSteal2 going to thing on: {1}", pawn, thing.Position));
										pawn.Map.debugDrawer.FlashCell(thing.Position, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
									}
									Job job3 = JobMaker.MakeJob(JobDefOf.Steal);
									job3.targetA = thing;
									job3.targetB = intVec;
									job3.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
									job3.count = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit));
									bool flag15 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag15)
									{
										Log.Message(string.Format("{0}: KidnapJobGiver: Steal: {1}", pawn, thing));
									}
									bool flag16 = job3 != null;
									if (flag16)
									{
										__result = job3;
										return false;
									}
								}
							}
						}
						catch (Exception arg3)
						{
							Log.Error(string.Format("{0} {1} : JobGiver_Kidnap: KidnapJobGiver: Steal part exception: {2}", pawn, pawn.Position, arg3));
						}
						__result = null;
						result = false;
					}
				}
			}
			return result;
		}
	}
}
