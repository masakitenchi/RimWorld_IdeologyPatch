using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000048 RID: 72
	[HarmonyPatch(typeof(LordToil_Siege))]
	[HarmonyPatch("UpdateAllDuties")]
	public class Patch_LordToil_Siege_UpdateAllDuties_PostFix
	{
		// Token: 0x060001E6 RID: 486 RVA: 0x0002AB60 File Offset: 0x00028D60
		public static void Postfix(ref LordToil_Siege __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				Lord lord = __instance.lord;
				Dictionary<Pawn, DutyDef> duties = __instance.rememberedDuties;
				bool flag = lord == null || duties == null;
				if (!flag)
				{
					IEnumerable<KeyValuePair<Pawn, int>> enumerable = from p in AdvancedAI_SiegeUtility.RaidConstuctionLevelSkills(lord, false, 14)
					where p.Key.skills.GetSkill(SkillDefOf.Construction).Level >= 11 && !duties.ContainsKey(p.Key)
					select p;
					bool flag2 = !enumerable.EnumerableNullOrEmpty<KeyValuePair<Pawn, int>>();
					if (flag2)
					{
						foreach (KeyValuePair<Pawn, int> keyValuePair in enumerable)
						{
							bool flag3 = keyValuePair.Key != null;
							if (flag3)
							{
								bool debugLog = SkyAiCore.Settings.debugLog;
								if (debugLog)
								{
									Log.Message(string.Format("LordToil_Siege. UpdateAllDuties. Set as Builder: {0}", keyValuePair.Key));
								}
								__instance.rememberedDuties.Add(keyValuePair.Key, DutyDefOf.Build);
								Traverse.Create(__instance).Method("SetAsBuilder", new Type[]
								{
									typeof(Pawn)
								}, null).GetValue(new object[]
								{
									keyValuePair.Key
								});
							}
						}
					}
				}
			}
		}
	}
}
