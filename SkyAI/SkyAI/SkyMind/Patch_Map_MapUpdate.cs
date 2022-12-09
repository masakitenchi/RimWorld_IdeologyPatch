using System;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace SkyMind
{
	// Token: 0x0200004B RID: 75
	[HarmonyPatch(typeof(Map))]
	[HarmonyPatch("MapUpdate")]
	public class Patch_Map_MapUpdate
	{
		// Token: 0x060001ED RID: 493 RVA: 0x0002BAFC File Offset: 0x00029CFC
		private static void Postfix(ref Map __instance)
		{
			bool flag = !WorldRendererUtility.WorldRenderedNow && Find.CurrentMap == __instance;
			if (flag)
			{
				SquadAttackGridDebug.DebugDrawAllOnMap(__instance);
			}
		}
	}
}
