using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace ColonistGroupFix
{
    public class HarmonyPatch
    {
        private static readonly Type patchtype;
        static HarmonyPatch()
        {
            patchtype = typeof(HarmonyPatch);
            Harmony harmony = new Harmony("com.reggex.CGPatch");
            Log.Message("<color=red>ColonistGroupFix Loaded</color>");
            harmony.Patch(AccessTools.Method(typeof(TacticalGroups.TacticalColonistBar), "GetNonHiddenPawns"), null, new HarmonyMethod(patchtype, "HidePawnsInCryoCasket"));
        }


    }
}
