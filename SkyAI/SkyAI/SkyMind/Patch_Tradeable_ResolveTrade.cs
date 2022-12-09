using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000054 RID: 84
	[HarmonyPatch(typeof(Tradeable))]
	[HarmonyPatch("ResolveTrade")]
	public class Patch_Tradeable_ResolveTrade
	{
		// Token: 0x06000201 RID: 513 RVA: 0x0002CC14 File Offset: 0x0002AE14
		private static void UpdatePawn(Pawn trader)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(trader);
			Lord lord = trader.GetLord();
			bool flag = lord != null && mapComponent_SkyAI != null;
			if (flag)
			{
				foreach (Pawn pawn in lord.ownedPawns)
				{
					int num = mapComponent_SkyAI.pawnThings.Count - 1;
					while (num >= 0 && !mapComponent_SkyAI.pawnThings.NullOrEmpty<PawnThingsOwner>())
					{
						PawnThingsOwner pawnThingsOwner = mapComponent_SkyAI.pawnThings[num];
						bool flag2 = pawnThingsOwner != null & pawnThingsOwner.owner == pawn;
						if (flag2)
						{
							mapComponent_SkyAI.pawnThings.Remove(pawnThingsOwner);
							break;
						}
						num--;
					}
					PawnThingsOwner item = AdvancedAI_CaravanUtility.PawnOwnerThingsList(pawn);
					bool flag3 = !mapComponent_SkyAI.pawnThings.Contains(item);
					if (flag3)
					{
						mapComponent_SkyAI.pawnThings.Add(item);
					}
				}
			}
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0002CD28 File Offset: 0x0002AF28
		public static bool Prefix(ref Tradeable __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = __instance.ActionToDo == TradeAction.PlayerBuys;
				if (flag)
				{
					bool flag2 = TradeSession.trader == null;
					if (flag2)
					{
						return true;
					}
					TraderKindDef traderKind = TradeSession.trader.TraderKind;
					bool flag3 = traderKind == null;
					if (flag3)
					{
						return true;
					}
					bool orbital = traderKind.orbital;
					if (orbital)
					{
						return true;
					}
					Func<Pawn, bool> <>9__0;
					foreach (Map map in Find.Maps)
					{
						IEnumerable<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
						Func<Pawn, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Pawn tr) => traderKind != null && tr.TraderKind != null && tr.TraderKind == traderKind));
						}
						IEnumerable<Pawn> enumerable = allPawnsSpawned.Where(predicate);
						bool flag4 = !enumerable.EnumerableNullOrEmpty<Pawn>();
						if (flag4)
						{
							foreach (Pawn pawn in enumerable)
							{
								MapComponent_SkyAI component = map.GetComponent<MapComponent_SkyAI>();
								List<Thing> thingsTrader = __instance.thingsTrader;
								bool flag5 = thingsTrader.NullOrEmpty<Thing>();
								if (flag5)
								{
									Log.Message("Tradeable_ResolveTrade patch. BoughtThings null or empty1.");
								}
								for (int i = 0; i < thingsTrader.Count<Thing>(); i++)
								{
									Thing thing = thingsTrader[i];
									bool flag6 = !component.boughtThings.ContainsKey(thing);
									if (flag6)
									{
										bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
										if (debugPawnThingsOwner)
										{
											Log.Message(string.Format("Tradeable_ResolveTrade patch1. Added boughtThings: {0} with trader faction: {1}.", thing, pawn.Faction));
										}
										component.boughtThings.Add(thing, pawn.Faction);
									}
								}
							}
						}
						else
						{
							Log.Error("Tradeable_ResolveTrade: traders null or empty.");
						}
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0002CF6C File Offset: 0x0002B16C
		public static void Postfix(ref Tradeable __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				try
				{
					bool flag = TradeSession.trader == null;
					if (!flag)
					{
						TraderKindDef traderKind = TradeSession.trader.TraderKind;
						bool flag2 = traderKind == null;
						if (flag2)
						{
							bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
							if (debugPawnThingsOwner)
							{
								Log.Message("Tradeable_ResolveTrade patch. Return, bcs of traderKind is null.");
							}
						}
						else
						{
							bool orbital = traderKind.orbital;
							if (orbital)
							{
								bool debugPawnThingsOwner2 = SkyAiCore.Settings.debugPawnThingsOwner;
								if (debugPawnThingsOwner2)
								{
									Log.Message("Tradeable_ResolveTrade patch. Return, bcs of trader is orbital.");
								}
							}
							else
							{
								IEnumerable<Pawn> enumerable = from tr in Find.WorldPawns.AllPawnsAlive
								where tr.TraderKind != null && tr.TraderKind == traderKind
								select tr;
								bool flag3 = !enumerable.EnumerableNullOrEmpty<Pawn>();
								if (flag3)
								{
									foreach (Pawn trader in enumerable)
									{
										Patch_Tradeable_ResolveTrade.UpdatePawn(trader);
									}
								}
								Func<Pawn, bool> <>9__1;
								foreach (Map map in Find.Maps)
								{
									IEnumerable<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
									Func<Pawn, bool> predicate;
									if ((predicate = <>9__1) == null)
									{
										predicate = (<>9__1 = ((Pawn tr) => traderKind != null && tr.TraderKind != null && tr.TraderKind == traderKind));
									}
									IEnumerable<Pawn> enumerable2 = allPawnsSpawned.Where(predicate);
									bool flag4 = !enumerable2.EnumerableNullOrEmpty<Pawn>();
									if (flag4)
									{
										foreach (Pawn trader2 in enumerable2)
										{
											Patch_Tradeable_ResolveTrade.UpdatePawn(trader2);
										}
									}
									else
									{
										Log.Error("Tradeable_ResolveTrade: traders null or empty.");
									}
								}
							}
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error(string.Format("Tradeable_ResolveTrade postfix patch exception: {0}", arg));
				}
			}
		}
	}
}
