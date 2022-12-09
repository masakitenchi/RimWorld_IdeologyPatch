using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000053 RID: 83
	[HarmonyPatch(typeof(VisitorGiftForPlayerUtility))]
	[HarmonyPatch("GiveGift")]
	public class Patch_VisitorGiftForPlayerUtility_GiveGift
	{
		// Token: 0x060001FE RID: 510 RVA: 0x0002CA28 File Offset: 0x0002AC28
		private static void UpdateGiftGivers(List<Pawn> possibleGivers)
		{
			foreach (Pawn pawn in possibleGivers)
			{
				MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
				bool flag = mapComponent_SkyAI == null;
				if (!flag)
				{
					List<Pawn> list = new List<Pawn>();
					int num = mapComponent_SkyAI.pawnThings.Count - 1;
					while (num >= 0 && !mapComponent_SkyAI.pawnThings.NullOrEmpty<PawnThingsOwner>())
					{
						PawnThingsOwner pawnThingsOwner = mapComponent_SkyAI.pawnThings[num];
						bool flag2 = (pawnThingsOwner != null & pawnThingsOwner.owner != null) && pawnThingsOwner.owner == pawn;
						if (flag2)
						{
							bool flag3 = !list.Contains(pawnThingsOwner.owner);
							if (flag3)
							{
								list.Add(pawnThingsOwner.owner);
								mapComponent_SkyAI.pawnThings.Remove(pawnThingsOwner);
								break;
							}
						}
						num--;
					}
					foreach (Pawn pawn2 in list)
					{
						PawnThingsOwner item = AdvancedAI_CaravanUtility.PawnOwnerThingsList(pawn2);
						bool flag4 = !mapComponent_SkyAI.pawnThings.Contains(item);
						if (flag4)
						{
							mapComponent_SkyAI.pawnThings.Add(item);
						}
					}
				}
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0002CBC0 File Offset: 0x0002ADC0
		public static void Postfix(List<Pawn> possibleGivers)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				try
				{
					Patch_VisitorGiftForPlayerUtility_GiveGift.UpdateGiftGivers(possibleGivers);
				}
				catch (Exception arg)
				{
					Log.Error(string.Format("VisitorGiftForPlayerUtility_GiveGift postfix patch exception: {0}", arg));
				}
			}
		}
	}
}
