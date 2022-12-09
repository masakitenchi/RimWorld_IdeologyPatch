using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000043 RID: 67
	[HarmonyPatch(typeof(JobGiver_AITrashColonyClose))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_AITrashColonyClose_TryGiveJob
	{
		// Token: 0x060001DC RID: 476 RVA: 0x00029EB8 File Offset: 0x000280B8
		private static void Postfix(ref JobGiver_AITrashColonyClose __instance, ref Pawn pawn, ref Job __result)
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
						Log.Message(string.Format("{0} {1}: AITrashColonyClose. Job target: {2}", pawn, pawn.Position, __result.targetA));
					}
				}
			}
		}
	}
}
