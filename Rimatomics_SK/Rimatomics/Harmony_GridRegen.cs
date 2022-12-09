using HarmonyLib;
using Verse;

namespace Rimatomics
{
	[HarmonyPatch(typeof(Map), "MapUpdate")]
	public static class Harmony_GridRegen
	{
		public static void Prefix(Map __instance)
		{
			__instance.Rimatomics().RegenGrids();
		}
	}
}
