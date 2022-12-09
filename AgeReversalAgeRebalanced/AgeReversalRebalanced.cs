using System;
using RimWorld;
using HarmonyLib;
using Verse;



namespace AgeReversalAgeRebalanced
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public const float AgeReversalLifeStage = 25f / 80f;
        private static readonly Type patchType;
        static HarmonyPatches()
        {
            Log.Message("AgeReversalRebanlanced Loaded");
            patchType = typeof(HarmonyPatches);
            Harmony harmony= new Harmony("regex.ARAR");
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_AgeReversalDemanded), "CanHaveThoughtImpl"), null, new HarmonyMethod(patchType, "CanHaveThoughtPostfix"));
        }
        #if DEBUG
        [HarmonyDebug]
        #endif
        public static void CanHaveThoughtPostfix(ref bool __result, Pawn pawn)
        {
            __result = (float)pawn.ageTracker.AgeBiologicalYears / (float)pawn.RaceProps.lifeExpectancy < AgeReversalLifeStage ? false : __result;
        }
    }
}
