using RimWorld;
using Verse;
using Androids;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace AndroidsIdeologyPatch
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
    {
		private static readonly Type patchType;
        public static bool IsDroid(this Pawn p)
        {
#if DEBUG
            Log.Message("Name:"+p.Name);
            Log.Message("FleshType:"+p.RaceProps.FleshType.ToString());
            Log.Message("IsDroid?:" +( p.RaceProps.FleshType.ToString() == "ChJDroid"));
#endif
            return p.RaceProps.FleshType.ToString() == "ChJDroid";
            //return p.def.defName == "ChjDroid" || p.def.defName == "ChjBattleDroid";
        }
        public static bool IsSkynet(this Pawn p)
        {
            return p.health.hediffSet.hediffs.Exists(x => x.def.defName == "AndroidPassive");
        }
		static HarmonyPatches()
        {
            Log.Message("AndroidsIdeologyPatch Loaded");
			patchType = typeof(HarmonyPatches);
			Harmony harmony = new Harmony("com.reggex.AndroidIdeologyPatch");
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_HasNoProsthetic), "ShouldHaveThought"),null, new HarmonyMethod(patchType,"ProstheticShouldHaveThoughtPostfix"));
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_HasNoProsthetic_Social), "ShouldHaveThought"), null, new HarmonyMethod(patchType, "ProstheticShouldHaveThoughtSocialPostfix"));
			harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_IdeoDiversity), "ShouldHaveThought"), null,null,new HarmonyMethod(patchType, "DiversityTranspiler"));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_IdeoDiversity_Social),"ShouldHaveThought"),null,new HarmonyMethod(patchType,"SocialPostfix"));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform), "ShouldHaveThought"), null,null, new HarmonyMethod(patchType, "UniformTranspiler"));
		}
		public static void ProstheticShouldHaveThoughtPostfix(ref ThoughtState __result, Pawn p)
        {
            __result = p.IsAndroid() || p.IsSkynet() ? false : __result;
			return;
        }
		public static void ProstheticShouldHaveThoughtSocialPostfix(ref ThoughtState __result, Pawn otherPawn)
        {
            __result = otherPawn.IsAndroid() || otherPawn.IsSkynet() ? false : __result;
			return;
        }
        /*public static bool IdeoShouldHaveThoughtPrefix(ref ThoughtState __result, Pawn p, ThoughtDef ___def)
        {
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
                //Log.Message("List[" + i + "] is :" + list[i].Name);
                //Log.Message("IsQuestLodger?:" + list[i].IsQuestLodger() + "\n Is Humanlike?:" + list[i].RaceProps.Humanlike + "\n Is Slave? :" + list[i].IsSlave + "\n Is Prisoner? :" + list[i].IsPrisoner);
                //Log.Message("defname:" + list[i].def.defName);
                if (!list[i].IsQuestLodger() && list[i].RaceProps.Humanlike && !list[i].IsSlave && !list[i].IsPrisoner && !list[i].IsDroid())
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
        }*/
#if DEBUG
        [HarmonyDebug]
#endif
        public static IEnumerable<CodeInstruction> DiversityTranspiler(IEnumerable<CodeInstruction> instructions,ILGenerator generator)
        {
            int index=0;
            bool foundif = false, foundnextloop = false;
            List<CodeInstruction> codes = instructions.ToList();
            Label nextloop = generator.DefineLabel();
            for(var i=0;i<codes.Count;i++)
            {
                if(!foundif && codes[i].opcode==OpCodes.Ldloc_2 && codes[i+1].opcode==OpCodes.Ldloc_3)
                {
                    foundif = true;
                    index = i+5;
                }
                if(!foundnextloop && codes[i].opcode==OpCodes.Ldloc_3 && codes[i+1].opcode==OpCodes.Ldc_I4_1)
                {
                    codes[i].labels.Add(nextloop);
                    foundnextloop = true;
                }
            }
            if(!foundif || !foundnextloop)
            {
                Log.Message("Error: No Such Code Found");
                return instructions;
            }
            List<CodeInstruction> newinstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Callvirt,AccessTools.Method(typeof(System.Collections.Generic.List<Verse.Pawn>),"get_Item")),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(HarmonyPatches),"IsDroid")),
                new CodeInstruction(OpCodes.Brtrue_S,nextloop),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Callvirt,AccessTools.Method(typeof(System.Collections.Generic.List<Verse.Pawn>),"get_Item")),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(HarmonyPatches),"IsSkynet")),
                new CodeInstruction(OpCodes.Brtrue_S,nextloop),
            };
            codes.InsertRange(index, newinstructions);
            return codes;
        }
        public static void SocialPostfix(ref ThoughtState __result, Pawn otherPawn)
        {
            __result = otherPawn.IsDroid() || otherPawn.IsSkynet() ? false : __result;
        }
        public static IEnumerable<CodeInstruction> UniformTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int index=0;
            bool foundif = false, foundnextloop = false;
            List<CodeInstruction> codes = instructions.ToList();
            Label nextloop = generator.DefineLabel();
            Label starthere = generator.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                if (!foundif && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldloc_2)
                {
                    foundif = true;
                    index = i+5;
                }
                if (!foundnextloop && codes[i].opcode == OpCodes.Ldloc_2 && codes[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    codes[i].labels.Add(nextloop);
                    foundnextloop = true;
                }
            }
            if (!foundif || !foundnextloop)
            {
                Log.Message("Error: Cannot patch IdeoUniformity method.");
                return instructions;
            }
            List<CodeInstruction> newinstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Callvirt,AccessTools.Method(typeof(System.Collections.Generic.List<Verse.Pawn>),"get_Item")),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(HarmonyPatches),"IsDroid")),
                new CodeInstruction(OpCodes.Brtrue_S,nextloop),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Callvirt,AccessTools.Method(typeof(System.Collections.Generic.List<Verse.Pawn>),"get_Item")),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(HarmonyPatches),"IsSkynet")),
                new CodeInstruction(OpCodes.Brtrue_S,nextloop),
            };
            newinstructions[0].labels.Add(starthere);
            codes.InsertRange(index, newinstructions);
            return codes;
        }
        /*public static bool IdeoShouldHaveThoughtUniformPrefix(ref ThoughtState __result,Pawn p)
        {
            if (p.Faction == null || !p.IsColonist)
            {
                __result = false;
                return false;
            }
            List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != p && list[i].RaceProps.Humanlike && !list[i].IsSlave && !list[i].IsQuestLodger() && !list[i].IsDroid())
                {
                    if (list[i].Ideo != p.Ideo)
                    {
                        __result = false;
                        return false;
                    }
                    num++;
                }
            }
            __result=num > 0;
            return false;
        }*/
    }
}