using RimWorld;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using Verse;
using HarmonyLib;
using Rimatomics;
using SaveOurShip2;

namespace SOS2RimatoHeatNetCompat
{
    public class HeatNetCompat : DefModExtension
    {
        private float _HeatStorage;
        public float HeatStorage
        {
            get => _HeatStorage;
            set => _HeatStorage = value;
        }
        public float HeatAdd
        {
            set => _HeatStorage += value;
        }
        public float HeatSub
        {
            set => _HeatStorage -= value;
        }
    }
    [StaticConstructorOnStartup]
    public class HarmonyPatch
    {
        private static readonly Type patchtype;
        static HarmonyPatch()
        {
            if (ModLister.HasActiveModWithName("Save Our Ship 2"))
            {
                Log.Message("Shipatomics Loaded");
                patchtype = typeof(HarmonyPatch);
                Harmony harmony = new Harmony("com.reggex.Shipatomics");
            }
        }
    }
}