using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000042 RID: 66
	[HarmonyPatch(typeof(JobGiver_TakeCombatEnhancingDrug))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_TakeCombatEnhancingDrug_TryGiveJob
	{
		// Token: 0x060001DA RID: 474 RVA: 0x00029E34 File Offset: 0x00028034
		private static void Postfix(ref JobGiver_TakeCombatEnhancingDrug __instance, ref Pawn pawn, ref Job __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = __result != null && __result.targetA != null;
				if (flag)
				{
					bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						Log.Message(string.Format("{0} {1}: TakeCombatEnhancingDrug. Job target: {2}", pawn, pawn.Position, __result.targetA));
					}
				}
			}
		}
	}
}
