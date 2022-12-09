using System;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000041 RID: 65
	[HarmonyPatch(typeof(AttackTargetFinder), "BestShootTargetFromCurrentPosition")]
	public class Patch_AttackTargetFinder_BestShootTargetFromCurrentPosition
	{
		// Token: 0x060001D8 RID: 472 RVA: 0x00029D6C File Offset: 0x00027F6C
		private static bool Prefix(ref IAttackTarget __result, IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				Verb currentEffectiveVerb = searcher.CurrentEffectiveVerb;
				bool flag = currentEffectiveVerb == null;
				if (flag)
				{
					Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.");
					__result = null;
					result = false;
				}
				else
				{
					__result = AttackTargetFinder.BestAttackTarget(searcher, flags, (Thing x) => AdvancedAI.ExtraTargetValidator((Thing)searcher, x), AdvancedAI.MinDistance((Thing)searcher, null), Mathf.Min(maxDistance, AdvancedAI.PrimaryWeaponRange((Thing)searcher)), default(IntVec3), float.MaxValue, false, false, false);
					result = false;
				}
			}
			return result;
		}
	}
}
