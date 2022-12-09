using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000018 RID: 24
	public static class AdvancedAI_CaravanUtility
	{
		// Token: 0x060000B9 RID: 185 RVA: 0x0000DEF8 File Offset: 0x0000C0F8
		public static Pawn CurrentOwnerOf(Thing t)
		{
			IThingHolder parentHolder = t.ParentHolder;
			bool flag = parentHolder is Pawn_EquipmentTracker || parentHolder is Pawn_ApparelTracker || parentHolder is Pawn_InventoryTracker;
			Pawn result;
			if (flag)
			{
				result = (Pawn)parentHolder.ParentHolder;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000DF44 File Offset: 0x0000C144
		public static void AddItemToPawnThingOwnerList(Pawn pawn, ThingCountClass thingCountClass)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			bool flag = mapComponent_SkyAI != null;
			if (flag)
			{
				List<PawnThingsOwner> pawnThings = mapComponent_SkyAI.pawnThings;
				bool flag2 = !pawnThings.NullOrEmpty<PawnThingsOwner>();
				if (flag2)
				{
					bool flag3 = !pawnThings.Any((PawnThingsOwner p) => p.owner == pawn);
					if (flag3)
					{
						List<ThingCountClass> list = new List<ThingCountClass>();
						list.Add(thingCountClass);
						pawnThings.Add(new PawnThingsOwner(pawn, list));
						bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
						if (debugPawnThingsOwner)
						{
							Log.Message(string.Format("{0} {1}: PawnThingOwner. AddItemToPawnThingOwnerList. Added: {2} as new pawnThingOwner list.", pawn, pawn.Position, thingCountClass));
						}
					}
					else
					{
						Predicate<ThingCountClass> <>9__1;
						foreach (PawnThingsOwner pawnThingsOwner in pawnThings)
						{
							bool flag4;
							if (pawnThingsOwner != null && pawnThingsOwner.owner != null && pawnThingsOwner.owner == pawn)
							{
								List<ThingCountClass> thingCount = pawnThingsOwner.thingCount;
								Predicate<ThingCountClass> predicate;
								if ((predicate = <>9__1) == null)
								{
									predicate = (<>9__1 = ((ThingCountClass tcc) => tcc.thing == thingCountClass.thing));
								}
								flag4 = !thingCount.Any(predicate);
							}
							else
							{
								flag4 = false;
							}
							bool flag5 = flag4;
							if (flag5)
							{
								pawnThingsOwner.thingCount.Add(thingCountClass);
								bool debugPawnThingsOwner2 = SkyAiCore.Settings.debugPawnThingsOwner;
								if (debugPawnThingsOwner2)
								{
									Log.Message(string.Format("{0} {1}: PawnThingOwner. AddItemToPawnThingOwnerList. Added: {2} to existing pawnThingOwner list.", pawn, pawn.Position, thingCountClass));
								}
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000E114 File Offset: 0x0000C314
		public static bool ThingOwner(Pawn pawn, Thing thing)
		{
			bool result = false;
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			bool flag = mapComponent_SkyAI != null;
			if (flag)
			{
				List<PawnThingsOwner> pawnThings = mapComponent_SkyAI.pawnThings;
				bool flag2 = !pawnThings.NullOrEmpty<PawnThingsOwner>();
				if (flag2)
				{
					foreach (PawnThingsOwner pawnThingsOwner in pawnThings)
					{
						bool flag3 = pawnThingsOwner != null && pawnThingsOwner.owner == pawn;
						if (flag3)
						{
							foreach (ThingCountClass thingCountClass in pawnThingsOwner.thingCount)
							{
								bool flag4 = thingCountClass.thing == thing;
								if (flag4)
								{
									result = true;
									break;
								}
							}
							break;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0000E20C File Offset: 0x0000C40C
		public static PawnThingsOwner PawnOwnerThingsList(Pawn pawn)
		{
			List<ThingCountClass> list = new List<ThingCountClass>();
			bool flag = pawn.inventory != null;
			if (flag)
			{
				using (List<Thing>.Enumerator enumerator = pawn.inventory.innerContainer.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Thing t = enumerator.Current;
						bool flag2 = !list.Any((ThingCountClass c) => c.thing == t);
						if (flag2)
						{
							ThingCountClass item = new ThingCountClass(t, t.stackCount);
							list.Add(item);
						}
					}
				}
			}
			bool flag3 = pawn.apparel != null;
			if (flag3)
			{
				using (List<Apparel>.Enumerator enumerator2 = pawn.apparel.WornApparel.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Apparel t = enumerator2.Current;
						bool flag4 = !list.Any((ThingCountClass c) => c.thing == t);
						if (flag4)
						{
							ThingCount t2 = new ThingCount(t, t.stackCount);
							list.Add(t2);
						}
					}
				}
			}
			bool flag5 = pawn.equipment != null && pawn.equipment.Primary != null;
			if (flag5)
			{
				ThingWithComps primary = AdvancedAI.PrimaryWeapon(pawn);
				bool flag6 = primary != null && !list.Any((ThingCountClass c) => c.thing == primary);
				if (flag6)
				{
					list.Add(new ThingCount(primary, 1));
				}
			}
			return new PawnThingsOwner(pawn, list);
		}
	}
}
