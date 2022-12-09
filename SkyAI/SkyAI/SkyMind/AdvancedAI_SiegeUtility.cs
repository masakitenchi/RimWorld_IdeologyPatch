using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200001A RID: 26
	public static class AdvancedAI_SiegeUtility
	{
		// Token: 0x060000C4 RID: 196 RVA: 0x0000F528 File Offset: 0x0000D728
		public static TechLevel TurretTech(Faction faction)
		{
			TechLevel result;
			switch (faction.def.techLevel)
			{
			case TechLevel.Animal:
				result = TechLevel.Medieval;
				break;
			case TechLevel.Neolithic:
				result = TechLevel.Medieval;
				break;
			case TechLevel.Medieval:
				result = TechLevel.Medieval;
				break;
			case TechLevel.Industrial:
				result = TechLevel.Industrial;
				break;
			case TechLevel.Spacer:
				result = TechLevel.Spacer;
				break;
			case TechLevel.Ultra:
				result = TechLevel.Spacer;
				break;
			case TechLevel.Archotech:
				result = TechLevel.Spacer;
				break;
			default:
				result = TechLevel.Industrial;
				break;
			}
			return result;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x0000F58C File Offset: 0x0000D78C
		public static bool CanPlaceBlueprintAt(IntVec3 root, Rot4 rot, ThingDef buildingDef, Map map, ThingDef stuffDef)
		{
			return GenConstruct.CanPlaceBlueprintAt(buildingDef, root, rot, map, false, null, null, stuffDef).Accepted;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x0000F5B4 File Offset: 0x0000D7B4
		public static Pawn FirstPawnOfFaction(IntVec3 intVec3, Faction faction, Map map)
		{
			IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(intVec3, 25f, true);
			Func<IntVec3, bool> <>9__0;
			Func<IntVec3, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(map)));
			}
			foreach (IntVec3 c2 in source.Where(predicate))
			{
				Pawn firstPawn = c2.GetFirstPawn(map);
				bool flag = firstPawn != null && firstPawn.Faction == faction && firstPawn.GetLord() != null;
				if (flag)
				{
					return firstPawn;
				}
			}
			return null;
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0000F674 File Offset: 0x0000D874
		public static bool ProjectileFlyOverhead(ThingDef turret)
		{
			ThingDef turretGunDef = turret.building.turretGunDef;
			ThingDef thingDef = (from verb in turretGunDef.Verbs
			where verb.defaultProjectile != null
			select verb into proj
			select proj.defaultProjectile).FirstOrDefault<ThingDef>();
			return thingDef != null && thingDef.projectile != null && thingDef.projectile.flyOverhead;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000F700 File Offset: 0x0000D900
		public static IntVec3 FindArtySpot(ThingDef artyDef, Rot4 rot, Map map, IntVec3 ___center, ThingDef stuff, bool flyOverhead)
		{
			CellRect cellRect = CellRect.CenteredOn(___center, 8);
			cellRect.ClipInsideMap(map);
			int num = 0;
			IntVec3 end = IntVec3.Invalid;
			for (int i = 0; i < 50; i++)
			{
				IntVec3 intVec;
				RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(___center, map, (float)Rand.RangeInclusive(10, 30), out intVec);
				bool flag = GenSight.LineOfSight(___center, intVec, map, false, null, 0, 0);
				if (flag)
				{
					end = intVec;
					break;
				}
			}
			bool flag2 = !end.IsValid;
			if (flag2)
			{
				RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(___center, map, 10f, out end);
			}
			IntVec3 randomCell;
			for (;;)
			{
				num++;
				bool flag3 = num > 250;
				if (flag3)
				{
					break;
				}
				randomCell = cellRect.RandomCell;
				bool flag4 = flyOverhead || GenSight.LineOfSight(randomCell, end, map, false, null, 0, 0);
				bool flag5 = flag4 && map.reachability.CanReach(randomCell, ___center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) && !randomCell.Roofed(map) && AdvancedAI_SiegeUtility.CanPlaceBlueprintAt(randomCell, rot, artyDef, map, stuff);
				if (flag5)
				{
					goto Block_8;
				}
			}
			return IntVec3.Invalid;
			Block_8:
			return randomCell;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0000F814 File Offset: 0x0000DA14
		public static IEnumerable<ThingDef> ArtyDefs()
		{
			return from def in DefDatabase<ThingDef>.AllDefs
			where def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer")
			select def;
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0000F850 File Offset: 0x0000DA50
		public static bool IsBarricade(Building building)
		{
			return building.def.fillPercent > 0.6f && building.def.fillPercent < 0.99f && building.def.building != null && building.def.building.isInert && !building.def.building.ai_chillDestination && building.def.placingDraggableDimensions == 1;
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0000F8C8 File Offset: 0x0000DAC8
		public static IntVec3 FixedTurretPosition(IntVec3 center, IntVec3 dutyCenter, Map map)
		{
			List<IntVec3> list = AdvancedAI.CellsBetweenPositions(center, dutyCenter, map, false, 0, 0, 1, 0f);
			IntVec3 result = IntVec3.Invalid;
			bool flag = false;
			foreach (IntVec3 intVec in list)
			{
				List<IntVec3> list2 = GenAdjFast.AdjacentCells8Way(intVec);
				foreach (IntVec3 c in list2)
				{
					bool flag2 = AdvancedAI_SiegeUtility.IsBarricade(c.GetFirstBuilding(map));
					if (flag2)
					{
						flag = true;
						break;
					}
				}
				bool flag3 = !flag;
				if (flag3)
				{
					result = intVec;
				}
			}
			return result;
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000F9A4 File Offset: 0x0000DBA4
		public static IntVec3 FirePointNearBarricade(IntVec3 dutyCenter, IntVec3 target, Map map)
		{
			IEnumerable<IntVec3> enumerable = from c in GenRadial.RadialCellsAround(dutyCenter, 21f, true)
			where c.InBounds(map)
			select c;
			IOrderedEnumerable<Building> orderedEnumerable = from c in enumerable
			where c.DistanceTo(target) < dutyCenter.DistanceTo(target)
			select c into ex
			select ex.GetFirstBuilding(map) into b
			where b != null && (AdvancedAI_SiegeUtility.IsBarricade(b) || AdvancedAI_SiegeUtility.<FirePointNearBarricade>g__isBarricadeFrame|8_1(b))
			select b into dist
			orderby dist.Position.DistanceTo(dutyCenter)
			select dist;
			Func<IntVec3, Building> <>9__7;
			List<Building> list = orderedEnumerable.Where(delegate(Building c)
			{
				IEnumerable<IntVec3> source = GenAdjFast.AdjacentCells8Way(c);
				Func<IntVec3, Building> selector;
				if ((selector = <>9__7) == null)
				{
					selector = (<>9__7 = ((IntVec3 b) => b.GetFirstBuilding(map)));
				}
				return source.Select(selector).Count<Building>() >= 2;
			}).ToList<Building>();
			bool debugPath = SkyAiCore.Settings.debugPath;
			if (debugPath)
			{
				foreach (IntVec3 c2 in enumerable)
				{
					map.debugDrawer.FlashCell(c2, 0.43f, null, 5000);
				}
				foreach (Building building in orderedEnumerable)
				{
					map.debugDrawer.FlashCell(building.Position, 0.72f, "B", 5000);
				}
				foreach (Building building2 in list)
				{
					Log.Message(string.Format("Siege. Found barricades: {0}", building2));
					map.debugDrawer.FlashCell(building2.Position, 0.47f, "A_A", 5000);
				}
			}
			IntVec3 intVec = AdvancedAI_SiegeUtility.BestCellNearBarricade(list, target, map);
			bool flag = intVec.IsValid && intVec != IntVec3.Invalid;
			IntVec3 result;
			if (flag)
			{
				bool debugPath2 = SkyAiCore.Settings.debugPath;
				if (debugPath2)
				{
					map.debugDrawer.FlashCell(intVec, 0.78f, "XXX", 5000);
					Log.Message(string.Format("Siege. Result turret cell: {0}", intVec));
				}
				result = intVec;
			}
			else
			{
				result = IntVec3.Invalid;
			}
			return result;
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000FC38 File Offset: 0x0000DE38
		public static IntVec3 BestCellNearBarricade(List<Building> barricades, IntVec3 target, Map map)
		{
			Func<IntVec3, bool> <>9__2;
			for (int i = 0; i < barricades.Count<Building>(); i++)
			{
				List<IntVec3> list = GenAdjFast.AdjacentCells8Way(barricades[i].Position);
				for (int j = 3; j > 1; j--)
				{
					int num = 0;
					IEnumerable<IntVec3> source = list;
					Func<IntVec3, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((IntVec3 c) => c.InBounds(map) && c.Standable(map) && base.<BestCellNearBarricade>g__isInsideCell|1(c)));
					}
					List<IntVec3> list2 = source.Where(predicate).ToList<IntVec3>();
					for (int k = 0; k < list2.Count<IntVec3>(); k++)
					{
						List<IntVec3> list3 = GenAdjFast.AdjacentCells8Way(list2[k]);
						for (int l = 0; l < list3.Count<IntVec3>(); l++)
						{
							Building firstBuilding = list3[l].GetFirstBuilding(map);
							bool flag = firstBuilding != null && (AdvancedAI_SiegeUtility.IsBarricade(firstBuilding) || AdvancedAI_SiegeUtility.<BestCellNearBarricade>g__isBarricadeFrame|9_0(firstBuilding));
							if (flag)
							{
								num++;
							}
							bool flag2 = num >= j;
							if (flag2)
							{
								return list2[k];
							}
						}
						num = 0;
					}
				}
			}
			return IntVec3.Invalid;
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000FD94 File Offset: 0x0000DF94
		public static ThingDef GetStuff(BuildableDef t, Faction faction)
		{
			bool flag = !t.MadeFromStuff;
			ThingDef steelBar;
			if (flag)
			{
				steelBar = ThingDefOfAI.SteelBar;
			}
			else
			{
				IEnumerable<ThingDef> source = from material in DefDatabase<ThingDef>.AllDefs
				where material.stuffProps != null && !material.stuffProps.categories.NullOrEmpty<StuffCategoryDef>()
				select material;
				int j;
				int i;
				for (i = 0; i < t.stuffCategories.Count; i = j + 1)
				{
					IEnumerable<ThingDef> enumerable = from material in source
					where material.stuffProps.categories.Contains(t.stuffCategories[i]) && material.techLevel <= faction.def.techLevel
					select material;
					bool flag2 = enumerable != null;
					if (flag2)
					{
						return enumerable.RandomElementByWeight((ThingDef w) => 9999f - w.GetStatValueAbstract(StatDefOf.MarketValue, null));
					}
					j = i;
				}
				steelBar = ThingDefOfAI.SteelBar;
			}
			return steelBar;
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000FEA8 File Offset: 0x0000E0A8
		public static Rot4 GetRot(IntVec3 loc, Map map)
		{
			bool flag = loc.x > loc.z;
			Rot4 result;
			if (flag)
			{
				result = ((loc.x < map.Size.x / 2) ? Rot4.East : Rot4.West);
			}
			else
			{
				result = ((loc.z < map.Size.z / 2) ? Rot4.North : Rot4.South);
			}
			return result;
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000FF14 File Offset: 0x0000E114
		public static IDictionary<Pawn, int> RaidConstuctionLevelSkills(Lord lord, bool cheatLevel = false, int minLevel = 14)
		{
			Dictionary<Pawn, int> dictionary = new Dictionary<Pawn, int>();
			bool flag = lord != null;
			if (flag)
			{
				foreach (Pawn pawn in lord.ownedPawns)
				{
					bool flag2 = pawn != null && pawn.skills != null && pawn.skills.skills != null;
					if (flag2)
					{
						SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Construction);
						bool flag3 = !skill.TotallyDisabled;
						if (flag3)
						{
							dictionary.Add(pawn, pawn.skills.GetSkill(SkillDefOf.Construction).Level);
						}
					}
				}
			}
			if (cheatLevel)
			{
				int num = (from t in dictionary
				where t.Value >= minLevel
				select t).Count<KeyValuePair<Pawn, int>>();
				int num2 = 0;
				bool flag4 = num <= 2;
				if (flag4)
				{
					foreach (KeyValuePair<Pawn, int> keyValuePair in dictionary)
					{
						bool flag5 = keyValuePair.Value < 14 && !AdvancedAI.PawnIsLeader(keyValuePair.Key) && !AdvancedAI.PawnIsDoctor(keyValuePair.Key);
						if (flag5)
						{
							num2++;
							Pawn key = keyValuePair.Key;
							key.skills.GetSkill(SkillDefOf.Construction).Level = Rand.RangeInclusive(minLevel, minLevel + 2);
						}
						bool flag6 = num2 >= 2;
						if (flag6)
						{
							break;
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000100F4 File Offset: 0x0000E2F4
		[CompilerGenerated]
		internal static bool <FirePointNearBarricade>g__isBarricadeFrame|8_1(Building f)
		{
			return f is Frame && AdvancedAI_SiegeUtility.IsBarricade((Frame)f);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x000100F4 File Offset: 0x0000E2F4
		[CompilerGenerated]
		internal static bool <BestCellNearBarricade>g__isBarricadeFrame|9_0(Building f)
		{
			return f is Frame && AdvancedAI_SiegeUtility.IsBarricade((Frame)f);
		}
	}
}
