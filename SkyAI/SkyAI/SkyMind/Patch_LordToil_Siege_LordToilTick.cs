using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000049 RID: 73
	[HarmonyPatch(typeof(LordToil_Siege), "LordToilTick")]
	internal static class Patch_LordToil_Siege_LordToilTick
	{
		// Token: 0x060001E8 RID: 488 RVA: 0x0002ACBC File Offset: 0x00028EBC
		private static bool Prefix(ref LordToil_Siege __instance)
		{
			Patch_LordToil_Siege_LordToilTick.<>c__DisplayClass2_0 CS$<>8__locals1 = new Patch_LordToil_Siege_LordToilTick.<>c__DisplayClass2_0();
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				CS$<>8__locals1.lord = __instance.lord;
				CS$<>8__locals1.map = __instance.Map;
				CS$<>8__locals1.f = CS$<>8__locals1.lord.faction;
				bool flag = CS$<>8__locals1.lord == null || CS$<>8__locals1.map == null || CS$<>8__locals1.f == null;
				if (flag)
				{
					result = true;
				}
				else
				{
					LordToilData_Siege lordToilData_Siege = Patch_LordToil_Siege_LordToilTick.DataGetter.Invoke(__instance);
					IEnumerable<Frame> source = Patch_LordToil_Siege_LordToilTick.FramesGetter.Invoke(__instance);
					bool flag2 = CS$<>8__locals1.lord.ticksInToil == 450;
					if (flag2)
					{
						CS$<>8__locals1.lord.CurLordToil.UpdateAllDuties();
					}
					bool flag3 = CS$<>8__locals1.lord.ticksInToil > 450 && CS$<>8__locals1.lord.ticksInToil % 500 == 0;
					if (flag3)
					{
						__instance.UpdateAllDuties();
					}
					bool flag4 = CS$<>8__locals1.lord.ticksInToil == 5000;
					if (flag4)
					{
						List<Blueprint_Build> list = new List<Blueprint_Build>();
						IntVec3 invalid = IntVec3.Invalid;
						bool flag5 = !AdvancedAI.TryFindEnemyBuildingLOSCellNearTheCenterOfTheMap(CS$<>8__locals1.f, lordToilData_Siege.siegeCenter, CS$<>8__locals1.map, out invalid);
						if (flag5)
						{
							RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(lordToilData_Siege.siegeCenter, CS$<>8__locals1.map, 10f, out invalid);
						}
						IntVec3 intVec = AdvancedAI_SiegeUtility.FirePointNearBarricade(lordToilData_Siege.siegeCenter, invalid, CS$<>8__locals1.map);
						bool isValid = intVec.IsValid;
						if (isValid)
						{
							Dictionary<Pawn, DutyDef> rememberedDuties = __instance.rememberedDuties;
							IEnumerable<ThingDef> enumerable = (from def in DefDatabase<ThingDef>.AllDefs
							where def.building != null && def.building.buildingTags.Contains("SiegeGun")
							select def).Where(delegate(ThingDef th)
							{
								bool result2;
								if (CS$<>8__locals1.<Prefix>g__techLevel|1(th.techLevel))
								{
									result2 = (th.constructionSkillPrerequisite <= rememberedDuties.Max((KeyValuePair<Pawn, DutyDef> p) => p.Key.skills.GetSkill(SkillDefOf.Construction).Level));
								}
								else
								{
									result2 = false;
								}
								return result2;
							});
							bool flag6 = !enumerable.EnumerableNullOrEmpty<ThingDef>() && Rand.Chance(0.25f);
							if (flag6)
							{
								ThingDef thingDef = enumerable.RandomElementByWeight((ThingDef tch) => (float)tch.techLevel);
								ThingDef stuff = AdvancedAI_SiegeUtility.GetStuff(thingDef, CS$<>8__locals1.f);
								IntVec3 intVec2 = (thingDef.Size.x > 1) ? AdvancedAI_SiegeUtility.FixedTurretPosition(intVec, lordToilData_Siege.siegeCenter, CS$<>8__locals1.map) : intVec;
								Blueprint_Build item = GenConstruct.PlaceBlueprintForBuild(thingDef, intVec2, CS$<>8__locals1.map, AdvancedAI_SiegeUtility.GetRot(intVec2, CS$<>8__locals1.map), CS$<>8__locals1.f, stuff);
								list.Add(item);
							}
							else
							{
								IEnumerable<ThingDef> enumerable2 = (from def in DefDatabase<ThingDef>.AllDefs
								where def.building != null && def.building.buildingTags.Contains("MachineGun")
								select def).Where(delegate(ThingDef th)
								{
									bool result2;
									if (th.techLevel == AdvancedAI_SiegeUtility.TurretTech(CS$<>8__locals1.f))
									{
										result2 = (th.constructionSkillPrerequisite <= rememberedDuties.Max((KeyValuePair<Pawn, DutyDef> p) => p.Key.skills.GetSkill(SkillDefOf.Construction).Level));
									}
									else
									{
										result2 = false;
									}
									return result2;
								});
								bool flag7 = !enumerable2.EnumerableNullOrEmpty<ThingDef>();
								if (flag7)
								{
									ThingDef thingDef2 = enumerable2.RandomElementByWeight((ThingDef tch) => (float)tch.techLevel);
									ThingDef stuff2 = AdvancedAI_SiegeUtility.GetStuff(thingDef2, CS$<>8__locals1.f);
									Blueprint_Build item2 = GenConstruct.PlaceBlueprintForBuild(thingDef2, intVec, CS$<>8__locals1.map, AdvancedAI_SiegeUtility.GetRot(intVec, CS$<>8__locals1.map), CS$<>8__locals1.f, stuff2);
									list.Add(item2);
								}
							}
							List<Thing> list2 = new List<Thing>();
							foreach (Blueprint_Build blueprint_Build in list)
							{
								lordToilData_Siege.blueprints.Add(blueprint_Build);
								using (List<ThingDefCountClass>.Enumerator enumerator2 = blueprint_Build.MaterialsNeeded().GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										ThingDefCountClass cost = enumerator2.Current;
										Thing thing = list2.FirstOrDefault((Thing t) => t.def == cost.thingDef);
										bool flag8 = thing != null;
										if (flag8)
										{
											thing.stackCount += cost.count;
										}
										else
										{
											Thing thing2 = ThingMaker.MakeThing(cost.thingDef, null);
											thing2.stackCount = cost.count;
											list2.Add(thing2);
										}
									}
								}
							}
							for (int i = 0; i < list2.Count; i++)
							{
								list2[i].stackCount = Mathf.CeilToInt((float)list2[i].stackCount * Rand.Range(1f, 1.2f));
							}
							List<List<Thing>> list3 = new List<List<Thing>>();
							for (int j = 0; j < list2.Count; j++)
							{
								while (list2[j].stackCount > list2[j].def.stackLimit)
								{
									int num = Mathf.CeilToInt((float)list2[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
									Thing thing3 = ThingMaker.MakeThing(list2[j].def, null);
									thing3.stackCount = num;
									list2[j].stackCount -= num;
									list2.Add(thing3);
								}
							}
							List<Thing> list4 = new List<Thing>();
							for (int k = 0; k < list2.Count; k++)
							{
								list4.Add(list2[k]);
								bool flag9 = k % 2 == 1 || k == list2.Count - 1;
								if (flag9)
								{
									list3.Add(list4);
									list4 = new List<Thing>();
								}
							}
							DropPodUtility.DropThingGroupsNear(lordToilData_Siege.siegeCenter, CS$<>8__locals1.map, list3, 110, false, false, true, true, true, false);
							lordToilData_Siege.desiredBuilderFraction = new FloatRange(0.25f, 0.4f).RandomInRange;
						}
					}
					bool flag10 = CS$<>8__locals1.lord.ticksInToil == 11000;
					if (flag10)
					{
						IEnumerable<IntVec3> source2 = GenRadial.RadialCellsAround(lordToilData_Siege.siegeCenter, 25f, true);
						Func<IntVec3, bool> predicate;
						if ((predicate = CS$<>8__locals1.<>9__11) == null)
						{
							predicate = (CS$<>8__locals1.<>9__11 = ((IntVec3 c) => c.InBounds(CS$<>8__locals1.map)));
						}
						foreach (IntVec3 c4 in source2.Where(predicate))
						{
							List<Thing> list5 = CS$<>8__locals1.map.thingGrid.ThingsListAtFast(c4);
							for (int l = 0; l < list5.Count<Thing>(); l++)
							{
								bool flag11;
								if (list5[l] != null && !list5[l].def.thingCategories.NullOrEmpty<ThingCategoryDef>())
								{
									flag11 = list5[l].def.thingCategories.Any((ThingCategoryDef tcd) => tcd == DefDatabase<ThingCategoryDef>.GetNamed("WeaponCrate", false));
								}
								else
								{
									flag11 = false;
								}
								bool flag12 = flag11;
								if (flag12)
								{
									list5[l].Destroy(DestroyMode.Vanish);
								}
							}
						}
					}
					bool flag13 = Find.TickManager.TicksGame % 500 != 0;
					if (flag13)
					{
						result = false;
					}
					else
					{
						bool flag14;
						if (!(from frame in source
						where !frame.Destroyed
						select frame).Any<Frame>())
						{
							if (!(from blue in lordToilData_Siege.blueprints
							where !blue.Destroyed
							select blue).Any<Blueprint>())
							{
								flag14 = !__instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == CS$<>8__locals1.lord.faction && b.def.building.buildingTags.Contains("Artillery_BaseDestroyer"));
								goto IL_81F;
							}
						}
						flag14 = false;
						IL_81F:
						bool flag15 = flag14;
						if (flag15)
						{
							CS$<>8__locals1.lord.ReceiveMemo("NoArtillery");
							result = false;
						}
						else
						{
							bool flag16;
							if (!(from frame in source
							where !frame.Destroyed
							select frame).Any<Frame>())
							{
								if (!(from blue in lordToilData_Siege.blueprints
								where !blue.Destroyed
								select blue).Any<Blueprint>())
								{
									flag16 = !__instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == CS$<>8__locals1.f);
									goto IL_8C6;
								}
							}
							flag16 = false;
							IL_8C6:
							bool flag17 = flag16;
							if (flag17)
							{
								Log.Message("No blueprints found");
							}
							IEnumerable<IntVec3> enumerable3 = from c in GenRadial.RadialCellsAround(lordToilData_Siege.siegeCenter, 20f, true)
							where c.InBounds(CS$<>8__locals1.map)
							select c;
							int num2 = 0;
							List<Thing> list6 = new List<Thing>();
							for (int m = 0; m < enumerable3.Count<IntVec3>(); m++)
							{
								IntVec3 c2 = enumerable3.ElementAt(m);
								List<Thing> thingList = c2.GetThingList(CS$<>8__locals1.map);
								for (int n = 0; n < thingList.Count; n++)
								{
									Thing thing4 = thingList[n];
									bool flag18 = thing4.def.building != null && thing4 is Building_TurretGunCE && !list6.Contains(thing4);
									if (flag18)
									{
										ThingDef ammo = Patch_LordToil_Siege_LordToilTick.GetAmmo(thing4.def);
										bool flag19 = ammo != null;
										if (flag19)
										{
											list6.Add(thing4);
											int num3 = 0;
											foreach (IntVec3 c3 in enumerable3)
											{
												Thing firstItem = c3.GetFirstItem(CS$<>8__locals1.map);
												bool flag20 = firstItem != null && firstItem.def == ammo;
												if (flag20)
												{
													num3 += firstItem.stackCount;
												}
											}
											bool flag21 = num3 < Mathf.RoundToInt((float)ammo.stackLimit * 0.3f);
											if (flag21)
											{
												int num4 = Mathf.RoundToInt((float)ammo.stackLimit * 0.4f - (float)num3);
												Traverse.Create(__instance).Method("DropSupplies", new Type[]
												{
													typeof(ThingDef),
													typeof(int)
												}, null).GetValue(new object[]
												{
													ammo,
													num4
												});
											}
										}
									}
									else
									{
										bool flag22 = thing4.def.building != null && thing4 is Building_TurretGun;
										if (flag22)
										{
											ThingDef thingDef3 = TurretGunUtility.TryFindRandomShellDef(thing4.def, false, true, CS$<>8__locals1.f.def.techLevel, false, 450f);
											bool flag23 = thingDef3 != null;
											if (flag23)
											{
												Traverse.Create(__instance).Method("DropSupplies", new Type[]
												{
													typeof(ThingDef),
													typeof(int)
												}, null).GetValue(new object[]
												{
													thingDef3,
													6
												});
											}
										}
									}
									bool flag24 = thing4.def == ThingDefOf.MealSurvivalPack;
									if (flag24)
									{
										num2 += thing4.stackCount;
									}
								}
							}
							bool flag25 = num2 < 5;
							if (flag25)
							{
								Traverse.Create(__instance).Method("DropSupplies", new Type[]
								{
									typeof(ThingDef),
									typeof(int)
								}, null).GetValue(new object[]
								{
									ThingDefOf.MealSurvivalPack,
									12
								});
							}
							result = false;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0002B8E4 File Offset: 0x00029AE4
		public static ThingDef GetAmmo(ThingDef def)
		{
			CompProperties_AmmoUser compProperties = def.building.turretGunDef.GetCompProperties<CompProperties_AmmoUser>();
			ThingDef result;
			if (compProperties == null)
			{
				result = null;
			}
			else
			{
				result = (from a in compProperties.ammoSet.ammoTypes
				select a.ammo).FirstOrDefault<ThingDef>();
			}
			return result;
		}

		// Token: 0x0400011C RID: 284
		private static readonly GetterHandler<LordToil_Siege, LordToilData_Siege> DataGetter = FastAccess.CreateFieldGetter<LordToil_Siege, LordToilData_Siege>(new string[]
		{
			"Data"
		});

		// Token: 0x0400011D RID: 285
		private static readonly GetterHandler<LordToil_Siege, IEnumerable<Frame>> FramesGetter = FastAccess.CreateFieldGetter<LordToil_Siege, IEnumerable<Frame>>(new string[]
		{
			"Frames"
		});
	}
}
