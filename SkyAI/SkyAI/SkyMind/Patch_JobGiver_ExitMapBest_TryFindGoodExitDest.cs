using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200003D RID: 61
	[HarmonyPatch(typeof(JobGiver_ExitMapBest))]
	[HarmonyPatch("TryFindGoodExitDest")]
	public class Patch_JobGiver_ExitMapBest_TryFindGoodExitDest
	{
		// Token: 0x060001D0 RID: 464 RVA: 0x00028FDC File Offset: 0x000271DC
		private static bool Prefix(ref JobGiver_ExitMapBest __instance, ref Pawn pawn, bool canDig, ref IntVec3 spot, ref bool __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = !AdvancedAI.IsHumanlikeOnly(pawn);
				if (flag)
				{
					result = true;
				}
				else
				{
					bool flag2 = !AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(pawn, out spot, true, false);
					if (flag2)
					{
						__result = false;
						result = false;
					}
					else
					{
						bool flag3 = AdvancedAI.IsValidLoc(spot);
						if (flag3)
						{
							bool debugLeaveCells = SkyAiCore.Settings.debugLeaveCells;
							if (debugLeaveCells)
							{
								Log.Message(string.Format("{0} {1}: JobGiver_ExitMapBest.TryFindGoodExitDest using TryPerfectExitSpot, found cell to leave map: {2} from cur. position: {3}", new object[]
								{
									pawn,
									pawn.Position,
									spot,
									pawn.Position
								}));
								pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
							}
						}
						__result = true;
						result = false;
					}
				}
			}
			return result;
		}
	}
}
