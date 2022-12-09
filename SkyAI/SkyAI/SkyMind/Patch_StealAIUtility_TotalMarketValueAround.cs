using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x0200004F RID: 79
	[HarmonyPatch(typeof(StealAIUtility))]
	[HarmonyPatch("TotalMarketValueAround")]
	public class Patch_StealAIUtility_TotalMarketValueAround
	{
		// Token: 0x060001F5 RID: 501 RVA: 0x0002BF40 File Offset: 0x0002A140
		private static bool Prefix(ref float __result, ref List<Pawn> pawns)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				float num = 0f;
				Patch_StealAIUtility_TotalMarketValueAround.tmpToSteal.Clear();
				for (int i = 0; i < pawns.Count; i++)
				{
					bool spawned = pawns[i].Spawned;
					if (spawned)
					{
						Thing thing;
						bool flag = StealAIUtility.TryFindBestItemToSteal(pawns[i].Position, pawns[i].Map, 7f, out thing, pawns[i], Patch_StealAIUtility_TotalMarketValueAround.tmpToSteal);
						if (flag)
						{
							bool flag2 = pawns[i].Map.areaManager.Home[thing.Position];
							if (flag2)
							{
								num += StealAIUtility.GetValue(thing);
								Patch_StealAIUtility_TotalMarketValueAround.tmpToSteal.Add(thing);
							}
						}
					}
				}
				Patch_StealAIUtility_TotalMarketValueAround.tmpToSteal.Clear();
				__result = num;
				result = false;
			}
			return result;
		}

		// Token: 0x0400011F RID: 287
		private static List<Thing> tmpToSteal = new List<Thing>();
	}
}
