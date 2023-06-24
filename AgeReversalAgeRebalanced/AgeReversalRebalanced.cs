using System;
using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.IO;
using System.Linq;

namespace AgeReversalAgeRebalanced
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        public static float AgeReversalLifeStage = 25f / 80f;
        static HarmonyPatches()
        {
            Log.Message("AgeReversalRebanlanced Loaded");
            Harmony harmony = new Harmony("regex.ARAR");
            harmony.PatchAll();
        }
        public static void CanHaveThoughtPostfix(ref ThoughtState __result, Pawn p)
        {
            __result = (float)p.ageTracker.AgeBiologicalYears / (float)p.RaceProps.lifeExpectancy < AgeReversalLifeStage ? ThoughtState.Inactive : __result;
        }

        // if (p.ageTracker.AgeBiologicalYears < 25)
        /*IL_002f: ldarg.1
		IL_0030: ldfld class Verse.Pawn_AgeTracker Verse.Pawn::ageTracker

        IL_0035: callvirt instance int32 Verse.Pawn_AgeTracker::get_AgeBiologicalYears()
        IL_003a: ldc.i4.s 25
        IL_003c: bge.s IL_0044

        IL_003c: bge.s IL_0044*/

        /* if(p.ageTracker.AgeBiologicalYears / p.RaceProps.lifeExpectancy < AgeReversalLifeStage)
         *  ldarg.1
         *  ldfld class Verse.Pawn_AgeTracker Verse.Pawn::ageTracker
         *  callvirt instance int32 Verse.Pawn_AgeTracker::get_AgeBiologicalYears()
         *  ldarg.1
         *  ldfld class Verse.RaceProperties Verse.Pawn::RaceProps
         *  ldfld float32 Verse.RaceProperties::lifeExpectancy
         *  div
         *  ldsfld float32 AgeReversalAgeRebalanced.HarmonyPatches::AgeReversalLifeStage
         *  bge.s 
         * 
         */

        [HarmonyPatch(typeof(ThoughtWorker_AgeReversalDemanded), "ShouldHaveThought")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CanHaveThoughtTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> inst = instructions.ToList();
            int AgeIndex = inst.FindIndex(x => x.OperandIs(AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeBiologicalYears))));
            if(AgeIndex < 0)
            {
                Log.Error("[AgeReversalAgeRebalanced] Old Transpiler. Please Contact the mod author");
                return instructions;
            }
            //remove idc.i4.s 25
            inst.RemoveAt(++AgeIndex);
            //insert new code
            inst.InsertRange(AgeIndex, new[]
            {
                //AgeBiologicalYears is int32, need to convert it to float32 first
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.RaceProps))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RaceProperties), nameof(RaceProperties.lifeExpectancy))),
                //new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Div),
                CodeInstruction.LoadField(typeof(HarmonyPatches), nameof(AgeReversalLifeStage))
            });
            /*File.WriteAllLines("E:\\before.txt", instructions.Select(x => x.ToString()));
            File.WriteAllLines("E:\\after.txt",inst.Select(x => x.ToString()));*/
            return inst;
        }
    }
}
