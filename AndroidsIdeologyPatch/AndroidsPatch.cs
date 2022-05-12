using RimWorld;
using Verse;
using Androids;
using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AndroidsIdeologyPatch
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
    {
		private static readonly Type patchType;
		static HarmonyPatches()
        {
			Log.Message("AndroidsIdeologyPatch Loaded");
			patchType = typeof(HarmonyPatches);
			Harmony harmony = new Harmony("com.reggex.AndroidIedologyPatch");
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_HasNoProsthetic), "ShouldHaveThought"),null, new HarmonyMethod(patchType,"ShouldHaveThoughtPostfix"));
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_HasNoProsthetic_Social), "ShouldHaveThought"), null, new HarmonyMethod(patchType, "ShouldHaveThoughtSocialPostfix"));
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_IdeoDiversity), "ShouldHaveThought"), new HarmonyMethod(patchType, "ShouldHaveThoughtPrefix"));
		}
		public static void ShouldHaveThoughtPostfix(ref ThoughtState __result, Pawn p)
        {
			//Log.Message("Pawn name:" + p.Name);
			//Log.Message("Pawn Race:" + p.RaceProps);
			//Log.Message(p.Name + "is Android:" + p.IsAndroid());
			__result = p.IsAndroid() ? false : __result;
			return;
        }
		public static void ShouldHaveThoughtSocialPostfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
			//Log.Message("Pawn name:" + p.Name);
			//Log.Message("Other Pawn name:" + otherPawn.Name);
			//Log.Message(otherPawn.Name + "is Android:" + otherPawn.IsAndroid());
			__result = otherPawn.IsAndroid() ? false : __result;
			return;
        }
        public static bool ShouldHaveThoughtPrefix(ref ThoughtState __result, Pawn p, ThoughtDef ___def)
        {
            //Log.Message("Prefix Enabled.");
            if (p.Faction == null || !p.IsColonist)
            {
                __result = false;
                return false;
            }
            int num = 0;
            int num2 = 0;
            List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
            for (int i = 0; i < list.Count; i++)
            {
                Log.Message("List[" + i + "] is :" + list[i].Name);
                Log.Message("IsQuestLodger?:" + list[i].IsQuestLodger() + "\n Is Humanlike?:" + list[i].RaceProps.Humanlike + "\n Is Slave? :" + list[i].IsSlave + "\n Is Prisoner? :" + list[i].IsPrisoner);
                Log.Message("defname:" + list[i].def.defName);
                if (!list[i].IsQuestLodger() && list[i].RaceProps.Humanlike && !list[i].IsSlave && !list[i].IsPrisoner && list[i].def.defName != "ChjDroid" && list[i].def.defName!="ChjBattleDroid")
                {
                    num2++;
                    if (list[i] != p && list[i].Ideo != p.Ideo)
                    {
                        num++;
                    }
                }
            }
            if (num == 0)
            {
                __result = ThoughtState.Inactive;
                return false;
            }
            __result = ThoughtState.ActiveAtStage(Mathf.RoundToInt((float)num / (float)(num2 - 1) * (float)(___def.stages.Count - 1)));
            return false;
        }
    }
}