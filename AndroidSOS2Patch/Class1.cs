using Androids;
using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AndroidSOS2Patch
{
    [StaticConstructorOnStartup]
    public static class AndroidSOS2Patch
    {
        private static bool IsDroid(this Pawn p)
        {
            if (!(p.def.defName == "ChjDroid"))
            {
                return p.def.defName == "ChjBattleDroid";
            }
            return true;
        }
        private static readonly Type patchType;
        static AndroidSOS2Patch()
        {
            Log.Message("AndroidSOS2Patch Enabled");
            patchType = typeof(AndroidSOS2Patch);
            Harmony harmony = new Harmony("com.reggex.AndroidsSOS2Patch");
            if(ModLister.HasActiveModWithName("Save Our Ship 2"))
            {
                harmony.Patch(AccessTools.Method(typeof(RimworldMod.VacuumIsNotFun.VacuumExtensions), "ExtraDangerFor"), null, null, new HarmonyMethod(patchType, "DroidsDontFearVaccum"));
                harmony.Patch(AccessTools.Method(typeof(WeatherEvent_VacuumDamage), "FireEvent"), null, null, new HarmonyMethod(patchType, "DroidsAreImmuneToVaccum"));
                harmony.Patch(AccessTools.Method(typeof(RimworldMod.VacuumIsNotFun.H_Vacuum_PathFinder), "AdditionalPathCost"), null, new HarmonyMethod(patchType, "AdditionalPathCostPostfix"));
            }
        }
        public static IEnumerable<CodeInstruction> DroidsAreImmuneToVaccum(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int i,ifindex=0;
            bool foundif = false, foundloop = false;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label continueloop = generator.DefineLabel();
            for(i=0;i<codes.Count;i++)
            {
                if(!foundif && codes[i].opcode==OpCodes.Callvirt && codes[i+1].opcode==OpCodes.Callvirt && codes[i+2].opcode==OpCodes.Brfalse)
                {
                    foundif = true;
                    ifindex = i + 3; 
                }
                if(!foundloop && codes[i].opcode==OpCodes.Ldloca_S && codes[i+1].opcode==OpCodes.Call && codes[i+2].opcode==OpCodes.Brtrue)
                {
                    codes[i].labels.Add(continueloop);
                    foundloop = true;
                }
            }
            if(!foundif && !foundloop)
            {
                FileLog.Log("Error : no such code found");
                return codes;
            }
            List<CodeInstruction> newInstructions = new List<CodeInstruction> { 
                new CodeInstruction(OpCodes.Ldloc_3), 
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AndroidSOS2Patch), "IsDroid")),
                new CodeInstruction(OpCodes.Brtrue, continueloop)
            };
            codes.InsertRange(ifindex, newInstructions);
            return codes;
        }
        public static void AdditionalPathCostPostfix (ref int __result, TraverseParms parms)
        {
            __result = parms.pawn.IsDroid() ? 0 : __result;
            return;
        }
        public static IEnumerable<CodeInstruction> DroidsDontFearVaccum(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Label someDanger = generator.DefineLabel();
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            codes.Find(x=>x.opcode==OpCodes.Ldc_I4_2).labels.Add(someDanger);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AndroidSOS2Patch), "IsDroid"));
            yield return new CodeInstruction(OpCodes.Brtrue_S, someDanger);
            foreach (CodeInstruction instruction in instructions)
                yield return instruction;
        }
    }
}
