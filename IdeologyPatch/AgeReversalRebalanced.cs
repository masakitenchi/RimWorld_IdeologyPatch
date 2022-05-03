using System;
using RimWorld;
using HarmonyLib;
using Verse;



namespace IdeologyPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public const float AgeReversalLifeStage = 25f / 80f;
        private static readonly Type patchType;
        static HarmonyPatches()
        {
            Log.Message("IdeologyPatch Loaded");
            patchType = typeof(HarmonyPatches);
            Harmony harmony= new Harmony("regex.IP");
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_AgeReversalDemanded), "CanHaveThought"), null, new HarmonyMethod(patchType, "CanHaveThoughtPostfix"));
        }
        public static bool CanHaveThoughtPostfix(bool result, Pawn pawn)
        {
            /*Log.Message("PostfixCalled");
            Harmony.DEBUG = true;
            Log.Message("prior to postfix:" + result);
            Log.Message("CurrentAgeis:" + pawn.ageTracker.AgeBiologicalYears);
            Log.Message("ExpectedLifeis:" + pawn.RaceProps.lifeExpectancy);
            Log.Message("AgeReversalLifeStage=" + AgeReversalLifeStage);
            Log.Message("(float) pawn.ageTracker.AgeBiologicalYears /(float)  pawn.RaceProps.lifeExpectancy=" + (float)pawn.ageTracker.AgeBiologicalYears / (float)pawn.RaceProps.lifeExpectancy);*/
            if ((float) pawn.ageTracker.AgeBiologicalYears /(float)  pawn.RaceProps.lifeExpectancy < AgeReversalLifeStage)
                return false;
            //Log.Message("after postfix:" + result);
            return result;
        }
    }
}
