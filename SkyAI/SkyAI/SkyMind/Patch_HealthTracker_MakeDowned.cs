using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000051 RID: 81
	[HarmonyPatch(typeof(Pawn_HealthTracker))]
	[HarmonyPatch("MakeDowned")]
	public class Patch_HealthTracker_MakeDowned
	{
		// Token: 0x060001FA RID: 506 RVA: 0x0002C108 File Offset: 0x0002A308
		public static void Postfix(ref Pawn_HealthTracker __instance, Pawn ___pawn)
		{
			bool flag = __instance == null || ___pawn == null;
			if (!flag)
			{
				bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
				if (!debugDisableSkyAI)
				{
					Pawn pawn = ___pawn;
					TraderCaravanRole traderCaravanRole = pawn.GetTraderCaravanRole();
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
					bool flag2 = (traderCaravanRole == TraderCaravanRole.Chattel || traderCaravanRole == TraderCaravanRole.Carrier) && mapComponent_SkyAI != null && !mapComponent_SkyAI.pawnThings.NullOrEmpty<PawnThingsOwner>();
					if (flag2)
					{
						bool flag3 = false;
						List<ThingCountClass> list = (from pto in mapComponent_SkyAI.pawnThings
						where pto != null && pto.owner == pawn
						select pto into t
						select t.thingCount).FirstOrDefault<List<ThingCountClass>>();
						bool flag4 = !list.NullOrEmpty<ThingCountClass>();
						if (flag4)
						{
							int num = list.Count - 1;
							while (num >= 0 && !list.NullOrEmpty<ThingCountClass>())
							{
								bool flag5 = !mapComponent_SkyAI.lostThings.ContainsKey(list[num]);
								if (flag5)
								{
									mapComponent_SkyAI.lostThings.Add(list[num], pawn.Faction);
									bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
									if (debugPawnThingsOwner)
									{
										Log.Message(string.Format("{0} {1}: HealthTracker_MakeDowned patch. Downed caravan carrier animal add item to lostThing dictionary: {2} count: {3}. Then item should be removed from pawnThingOwner.", new object[]
										{
											pawn,
											pawn.Position,
											list[num].thing,
											list[num].Count
										}));
									}
									bool flag6 = num <= 0;
									if (flag6)
									{
										flag3 = true;
									}
								}
								num--;
							}
						}
						bool flag7 = flag3;
						if (flag7)
						{
							PawnThingsOwner pawnThingsOwner = (from r in mapComponent_SkyAI.pawnThings
							where r.owner == pawn
							select r).FirstOrDefault<PawnThingsOwner>();
							bool flag8 = pawnThingsOwner != null;
							if (flag8)
							{
								bool debugPawnThingsOwner2 = SkyAiCore.Settings.debugPawnThingsOwner;
								if (debugPawnThingsOwner2)
								{
									List<ThingCountClass> thingCount = pawnThingsOwner.thingCount;
									bool flag9 = thingCount.NullOrEmpty<ThingCountClass>();
									if (flag9)
									{
										Log.Message(string.Format("{0} {1}: HealthTracker_MakeDowned patch. Removed {2} with items: {3}", new object[]
										{
											pawn,
											pawn.Position,
											pawnThingsOwner,
											GeneralExtensions.Join<ThingCountClass>(thingCount, null, ", ")
										}));
									}
									else
									{
										Log.Message(string.Format("{0} {1}: HealthTracker_MakeDowned patch. Removed {2}. ThingCountClass list null or empty.", pawn, pawn.Position, pawnThingsOwner));
									}
								}
								mapComponent_SkyAI.pawnThings.Remove(pawnThingsOwner);
							}
						}
					}
				}
			}
		}
	}
}
