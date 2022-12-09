using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000057 RID: 87
	[HarmonyPatch(typeof(CastPositionFinder))]
	[HarmonyPatch("CastPositionPreference")]
	public class Patch_CastPositionFinder_CastPositionPreference
	{
		// Token: 0x06000208 RID: 520 RVA: 0x0002D6D8 File Offset: 0x0002B8D8
		private static bool Prefix(IntVec3 c, ref float __result, CastPositionRequest ___req)
		{
			bool flag = !SkyAiCore.Settings.debugDisableSkyAI && ___req.caster != null && ___req.caster.RaceProps.intelligence == Intelligence.Humanlike;
			if (flag)
			{
				bool flag2 = c.DistanceTo(___req.target.Position) <= 2f && AdvancedAI.PositionUnderCrossfire(___req.caster, c, ___req.target, true, false);
				if (flag2)
				{
					__result = -1f;
					return false;
				}
				float minRange = ___req.verb.verbProps.minRange;
				bool flag3 = minRange > 0f;
				if (flag3)
				{
					bool flag4 = c.DistanceTo(___req.target.Position) <= minRange;
					if (flag4)
					{
						__result = -1f;
						return false;
					}
				}
			}
			return true;
		}
	}
}
