using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000026 RID: 38
	public static class AdvancedAI_BreachingUtility
	{
		// Token: 0x0600014D RID: 333 RVA: 0x0001F2BC File Offset: 0x0001D4BC
		public static List<PawnKindDef> GetSiegePawnKindsForFaction(Faction faction)
		{
			bool flag = faction == null;
			List<PawnKindDef> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				IEnumerable<PawnKindDef> source = from kind in DefDatabase<PawnKindDef>.AllDefsListForReading
				where kind.defaultFactionType == faction.def
				select kind;
				List<PawnKindDef> list = (from kind in source
				where kind != null && kind.weaponTags != null && (kind.weaponTags.ContainsString("Grenade") || kind.weaponTags.ContainsString("Norbhammar"))
				select kind).ToList<PawnKindDef>();
				result = list;
			}
			return result;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0001F334 File Offset: 0x0001D534
		public static bool ContainsString(this List<string> strTags, string tag)
		{
			bool flag = strTags.NullOrEmpty<string>();
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				foreach (string text in strTags)
				{
					bool flag2 = text.Equals(tag, StringComparison.CurrentCultureIgnoreCase);
					if (flag2)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0001F3A8 File Offset: 0x0001D5A8
		public static List<string> GetWeaponTagsFromPawnKindList(List<PawnKindDef> list)
		{
			bool flag = list.NullOrEmpty<PawnKindDef>();
			List<string> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				List<string> list2 = new List<string>();
				foreach (PawnKindDef pawnKindDef in list)
				{
					foreach (string item in pawnKindDef.weaponTags)
					{
						bool flag2 = !list2.Contains(item);
						if (flag2)
						{
							list2.Add(item);
						}
					}
				}
				result = list2;
			}
			return result;
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0001F470 File Offset: 0x0001D670
		public static bool TryDistributeAdditionalSiegeWeaponForRaidPawns(List<Pawn> pawns)
		{
			bool flag = false;
			bool flag2 = pawns.Count <= 0;
			bool result;
			if (flag2)
			{
				result = flag;
			}
			else
			{
				Pawn pawn = pawns.FirstOrDefault<Pawn>();
				List<PawnKindDef> siegePawnKindsForFaction = AdvancedAI_BreachingUtility.GetSiegePawnKindsForFaction(pawn.Faction);
				List<string> factionGrenadeTags = AdvancedAI_BreachingUtility.GetWeaponTagsFromPawnKindList(siegePawnKindsForFaction);
				Predicate<string> <>9__3;
				Func<ThingDef, bool> <>9__2;
				foreach (Pawn pawn2 in from x in pawns
				where !AdvancedAI_BreachingUtility.<TryDistributeAdditionalSiegeWeaponForRaidPawns>g__PawnHasImportantRole|3_0(x)
				select x)
				{
					CompInventory comp = pawn2.GetComp<CompInventory>();
					bool flag3 = comp != null && AdvancedAI.InventorySiegeWeaponList(comp).NullOrEmpty<ThingWithComps>() && !factionGrenadeTags.EnumerableNullOrEmpty<string>();
					if (flag3)
					{
						IEnumerable<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
						Func<ThingDef, bool> predicate;
						if ((predicate = <>9__2) == null)
						{
							predicate = (<>9__2 = delegate(ThingDef t)
							{
								if ((double)t.generateAllowChance > 0.4 && t.weaponTags != null && !t.weaponTags.NullOrEmpty<string>())
								{
									List<string> weaponTags = t.weaponTags;
									Predicate<string> predicate2;
									if ((predicate2 = <>9__3) == null)
									{
										predicate2 = (<>9__3 = ((string tag) => factionGrenadeTags.Contains(tag)));
									}
									if (weaponTags.Any(predicate2))
									{
										return t.weaponTags.Contains("CE_AI_Grenade");
									}
								}
								return false;
							});
						}
						IEnumerable<ThingDef> enumerable = allDefsListForReading.Where(predicate);
						bool flag4 = !enumerable.EnumerableNullOrEmpty<ThingDef>();
						if (flag4)
						{
							ThingDef thingDef = enumerable.RandomElementByWeight((ThingDef x) => x.generateAllowChance);
							ThingDef stuff = thingDef.MadeFromStuff ? thingDef.defaultStuff : null;
							Thing thing = ThingMaker.MakeThing(thingDef, stuff);
							int num = Rand.RangeInclusive(5, 10);
							comp.container.TryAdd(thing, num, true);
							flag = true;
							bool debugLog = SkyAiCore.Settings.debugLog;
							if (debugLog)
							{
								Log.Message(string.Format("{0} {1}: BreachingAI. Add equpment: {2} with count: {3}", new object[]
								{
									pawn2,
									pawn2.Position,
									thing,
									num
								}));
							}
						}
					}
				}
				result = flag;
			}
			return result;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0001F668 File Offset: 0x0001D868
		[CompilerGenerated]
		internal static bool <TryDistributeAdditionalSiegeWeaponForRaidPawns>g__PawnHasImportantRole|3_0(Pawn p)
		{
			return AdvancedAI.PawnIsDoctor(p) || AdvancedAI.IsRaidLeaderOrSquadCommander(p) || AdvancedAI.PawnIsSniper(p);
		}
	}
}
