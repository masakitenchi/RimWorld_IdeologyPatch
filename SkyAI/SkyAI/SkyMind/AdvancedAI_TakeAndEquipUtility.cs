using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000035 RID: 53
	public static class AdvancedAI_TakeAndEquipUtility
	{
		// Token: 0x060001A6 RID: 422 RVA: 0x00025458 File Offset: 0x00023658
		public static List<ThingWithComps> AmmoListInPawnTargetInventory(Thing weapon, Pawn moveFrom, bool lookForGrenades)
		{
			AdvancedAI_TakeAndEquipUtility.<>c__DisplayClass0_0 CS$<>8__locals1;
			CS$<>8__locals1.lookForGrenades = lookForGrenades;
			List<ThingWithComps> list = new List<ThingWithComps>();
			CompAmmoUser compAmmoUser = weapon.TryGetComp<CompAmmoUser>();
			bool flag = compAmmoUser != null;
			if (flag)
			{
				AdvancedAI_TakeAndEquipUtility.<>c__DisplayClass0_1 CS$<>8__locals2;
				CS$<>8__locals2.ammoUserList = (from AmmoLink g in compAmmoUser.Props.ammoSet.ammoTypes
				select g.ammo).ToList<ThingDef>();
				bool flag2 = !CS$<>8__locals2.ammoUserList.NullOrEmpty<ThingDef>();
				if (flag2)
				{
					CompInventory comp = moveFrom.GetComp<CompInventory>();
					bool flag3 = comp != null && !comp.container.NullOrEmpty<Thing>();
					if (flag3)
					{
						for (int i = 0; i < comp.container.Count<Thing>(); i++)
						{
							ThingWithComps thingWithComps = comp.container[i] as ThingWithComps;
							bool flag4 = thingWithComps != null && AdvancedAI_TakeAndEquipUtility.<AmmoListInPawnTargetInventory>g__x|0_3(thingWithComps, ref CS$<>8__locals1, ref CS$<>8__locals2);
							if (flag4)
							{
								list.Add(thingWithComps);
							}
						}
					}
					bool flag5 = moveFrom.equipment != null;
					if (flag5)
					{
						List<ThingWithComps> allEquipmentListForReading = moveFrom.equipment.AllEquipmentListForReading;
						for (int j = 0; j < allEquipmentListForReading.Count<ThingWithComps>(); j++)
						{
							ThingWithComps thingWithComps2 = allEquipmentListForReading[j];
							bool flag6 = thingWithComps2 != null && AdvancedAI_TakeAndEquipUtility.<AmmoListInPawnTargetInventory>g__x|0_3(thingWithComps2, ref CS$<>8__locals1, ref CS$<>8__locals2);
							if (flag6)
							{
								list.Add(thingWithComps2);
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000255DC File Offset: 0x000237DC
		public static bool ShouldTryToSearchNearForBetterWeapon(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps != null;
			if (flag)
			{
				bool flag2 = AdvancedAI.DesireToGetRangedWeapon(pawn, thingWithComps);
				if (flag2)
				{
					bool flag3 = !AdvancedAI.EngagedEnemyRecently(pawn) && pawn.mindState.lastAttackedTarget != LocalTargetInfo.Invalid;
					if (flag3)
					{
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: Has desire to get new ranged weapon.", pawn, pawn.Position));
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00025674 File Offset: 0x00023874
		public static List<ThingWithComps> RangedWeaponInInventory(Pawn pawn)
		{
			return (from thing in pawn.TryGetComp<CompInventory>().rangedWeaponList
			where thing.def.IsRangedWeapon
			select thing).ToList<ThingWithComps>();
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x000256BC File Offset: 0x000238BC
		public static bool HasRangedWeaponInInventory(Pawn pawn, bool checkAmmo)
		{
			bool flag = !checkAmmo;
			bool result;
			if (flag)
			{
				result = !AdvancedAI_TakeAndEquipUtility.RangedWeaponInInventory(pawn).NullOrEmpty<ThingWithComps>();
			}
			else
			{
				CompInventory compInventory = pawn.TryGetComp<CompInventory>();
				bool flag2 = compInventory == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					foreach (ThingWithComps thingWithComps in compInventory.rangedWeaponList)
					{
						bool flag3 = thingWithComps.TryGetComp<CompAmmoUser>() != null;
						if (!flag3)
						{
							return true;
						}
						bool flag4 = AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
						if (flag4)
						{
							return true;
						}
					}
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060001AA RID: 426 RVA: 0x00025770 File Offset: 0x00023970
		public static CompAmmoUser AmmoUserInInventory(Pawn pawn)
		{
			List<ThingWithComps> list = AdvancedAI_TakeAndEquipUtility.RangedWeaponInInventory(pawn);
			bool flag = list.NullOrEmpty<ThingWithComps>();
			CompAmmoUser result;
			if (flag)
			{
				result = null;
			}
			else
			{
				foreach (ThingWithComps thing in list)
				{
					CompAmmoUser compAmmoUser = thing.TryGetComp<CompAmmoUser>();
					bool flag2 = compAmmoUser != null;
					if (flag2)
					{
						return compAmmoUser;
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x060001AB RID: 427 RVA: 0x000257F4 File Offset: 0x000239F4
		public static bool Unload(Pawn pawn)
		{
			CompInventory compInventory = pawn.TryGetComp<CompInventory>();
			return compInventory != null && !pawn.Faction.IsPlayer && pawn.CurJobDef != JobDefOf.Steal && (compInventory.capacityWeight - compInventory.currentWeight < 3f || compInventory.capacityBulk - compInventory.currentBulk < 4f);
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00025858 File Offset: 0x00023A58
		public static Job TakeOrEquip(ThingWithComps eq, bool hasPrimary)
		{
			Job result;
			if (hasPrimary)
			{
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, eq);
				job.count = eq.stackCount;
				result = job;
			}
			else
			{
				result = JobMaker.MakeJob(JobDefOf.Equip, eq);
			}
			return result;
		}

		// Token: 0x060001AD RID: 429 RVA: 0x000258A4 File Offset: 0x00023AA4
		public static bool EnemyThreatOnCell(Thing searcher, IntVec3 scannedCell)
		{
			bool flag = !searcher.Spawned || !AdvancedAI.IsValidLoc(scannedCell);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Pawn pawn = searcher as Pawn;
				bool flag2 = pawn != null && pawn.IsColonist;
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = searcher.Position.DistanceTo(scannedCell) <= 2f;
					if (flag3)
					{
						result = false;
					}
					else
					{
						MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp((Pawn)searcher);
						bool flag4 = mapComponent_SkyAI == null;
						if (flag4)
						{
							result = false;
						}
						else
						{
							IEnumerable<Thing> source = from t in AdvancedAI.PotencialTargets(searcher)
							where !t.Position.Fogged(searcher.Map)
							select t;
							for (int i = 0; i < source.Count<Thing>(); i++)
							{
								Thing thing = source.ElementAt(i);
								bool flag5 = thing.Position.DistanceTo(scannedCell) <= 14f;
								if (flag5)
								{
									bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug((Pawn)searcher);
									if (flag6)
									{
										Log.Message(string.Format("{0} {1}: EnemyThreatOnCell. Added to dangerousCells: {2} bcs of too close to {3} on {4}", new object[]
										{
											searcher,
											searcher.Position,
											scannedCell,
											thing,
											thing.Position
										}));
									}
									mapComponent_SkyAI.dangerousCells.Add(scannedCell);
									return true;
								}
								bool flag7 = scannedCell.DistanceTo(thing.Position) < AdvancedAI.EffectiveRange(thing) && GenSight.LineOfSight(thing.Position, scannedCell, searcher.Map, true, null, 0, 0);
								if (flag7)
								{
									bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug((Pawn)searcher);
									if (flag8)
									{
										Log.Message(string.Format("{0} {1}: EnemyThreatOnCell. Added to dangerousCells: {2}, bcs of {3} in line of sight on {4}", new object[]
										{
											searcher,
											searcher.Position,
											scannedCell,
											thing,
											thing.Position
										}));
									}
									mapComponent_SkyAI.dangerousCells.Add(scannedCell);
									return true;
								}
							}
							result = false;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0001F105 File Offset: 0x0001D305
		[CompilerGenerated]
		internal static bool <AmmoListInPawnTargetInventory>g__ammoValidator|0_1(Thing t)
		{
			return t.def.category == ThingCategory.Item && t is AmmoThing;
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00025B20 File Offset: 0x00023D20
		[CompilerGenerated]
		internal static bool <AmmoListInPawnTargetInventory>g__grenadeValidator|0_2(Thing t)
		{
			return AdvancedAI.IsGrenade(t);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00025B28 File Offset: 0x00023D28
		[CompilerGenerated]
		internal static bool <AmmoListInPawnTargetInventory>g__x|0_3(Thing t, ref AdvancedAI_TakeAndEquipUtility.<>c__DisplayClass0_0 A_1, ref AdvancedAI_TakeAndEquipUtility.<>c__DisplayClass0_1 A_2)
		{
			return (A_1.lookForGrenades && AdvancedAI_TakeAndEquipUtility.<AmmoListInPawnTargetInventory>g__grenadeValidator|0_2(t)) || (AdvancedAI_TakeAndEquipUtility.<AmmoListInPawnTargetInventory>g__ammoValidator|0_1(t) && A_2.ammoUserList.Contains(t.def));
		}
	}
}
