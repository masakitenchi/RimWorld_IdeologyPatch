using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace BedUtility
{
    [StaticConstructorOnStartup]
    public static class BedOwned
    {
        private static readonly Type patchType;
        static BedOwned()
        {
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Log.Message("BedUtility Enabled");
            patchType = typeof(BedOwned);
            Harmony harmony = new Harmony("com.reggex.BedUtility");
            harmony.Patch(AccessTools.Method(typeof(Pawn_Ownership), "OwnedBed"),new HarmonyMethod(patchType,"OwnedBedList"));
            harmony.Patch(AccessTools.Method(typeof(Pawn_RoyaltyTracker), "HasPersonalBedroom"), null,new HarmonyMethod(patchType, "HasPersonalBedroomPostfix"));
        }

        public static void HasPersonalBedroomPostfix(ref bool __result, Pawn p)
        {

        }
    }
}
    