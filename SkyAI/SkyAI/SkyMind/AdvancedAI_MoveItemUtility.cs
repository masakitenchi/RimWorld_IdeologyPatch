using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000025 RID: 37
	public static class AdvancedAI_MoveItemUtility
	{
		// Token: 0x06000149 RID: 329 RVA: 0x0001F1AC File Offset: 0x0001D3AC
		public static void MoveItemsToInventory(ThingWithComps weapon, Pawn MoveFrom, Pawn MoteTo)
		{
			AdvancedAI_MoveItemUtility.MoveItemToInventory(weapon, MoveFrom, MoteTo);
			List<ThingWithComps> list = AdvancedAI_TakeAndEquipUtility.AmmoListInPawnTargetInventory(weapon, MoveFrom, true);
			for (int i = 0; i < list.Count<ThingWithComps>(); i++)
			{
				ThingWithComps thingWithComps = list[i];
				bool flag = thingWithComps != null;
				if (flag)
				{
					AdvancedAI_MoveItemUtility.MoveItemToInventory(thingWithComps, MoveFrom, MoteTo);
				}
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0001F200 File Offset: 0x0001D400
		public static void MoveItemToInventory(ThingWithComps item, Pawn moveFrom, Pawn moveTo)
		{
			Pawn pawn = AdvancedAI_MoveItemUtility.MoveInventoryTo(item, moveTo, moveFrom);
			ThingOwner<Thing> thingOwner = (pawn != null) ? pawn.inventory.innerContainer : null;
			bool flag = item.holdingOwner != null;
			if (flag)
			{
				thingOwner.TryAddOrTransfer(item, true);
			}
			else
			{
				thingOwner.TryAdd(item, true);
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0001F250 File Offset: 0x0001D450
		public static Pawn MoveInventoryTo(Thing item, Pawn candidate, Pawn currentItemOwner = null)
		{
			bool flag = item is Pawn;
			Pawn result;
			if (flag)
			{
				Log.Error("Called FindPawnToMoveInventoryTo but the item is a pawn.");
				result = null;
			}
			else
			{
				bool flag2 = AdvancedAI_MoveItemUtility.CanMoveInventoryTo(candidate) && candidate != currentItemOwner && !MassUtility.IsOverEncumbered(candidate);
				if (flag2)
				{
					result = candidate;
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0001F2A4 File Offset: 0x0001D4A4
		private static bool CanMoveInventoryTo(Pawn pawn)
		{
			return MassUtility.CanEverCarryAnything(pawn);
		}
	}
}
