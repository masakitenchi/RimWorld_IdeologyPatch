using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x02000056 RID: 86
	[HarmonyPatch(typeof(MassUtility))]
	[HarmonyPatch("Capacity")]
	public static class Patch_MassUtility_Capacity
	{
		// Token: 0x06000207 RID: 519 RVA: 0x0002D600 File Offset: 0x0002B800
		public static bool Prefix(ref float __result, ref Pawn p, ref StringBuilder explanation)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = !MassUtility.CanEverCarryAnything(p);
				if (flag)
				{
					__result = 0f;
				}
				else
				{
					float num = Mathf.Max(p.GetStatValue(StatDefOf.CarryingCapacity, true), StatDefOf.CarryingCapacity.defaultBaseValue * p.BodySize * Current.Game.Scenario.GetStatFactor(StatDefOf.CarryingCapacity)) * 0.5f;
					bool flag2 = explanation != null;
					if (flag2)
					{
						bool flag3 = explanation.Length > 0;
						if (flag3)
						{
							explanation.AppendLine();
						}
						explanation.Append("  - " + p.LabelShortCap + ": " + num.ToStringMassOffset());
					}
					__result = num;
				}
				result = false;
			}
			return result;
		}
	}
}
