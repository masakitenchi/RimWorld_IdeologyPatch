using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200000B RID: 11
	public class JobGiver_TakeAndEquip : ThinkNode_JobGiver
	{
		private const float ammoFractionOfNonAmmoInventory = 0.666f;

		// Token: 0x02000069 RID: 105
		private enum WorkPriority
		{
			// Token: 0x0400018C RID: 396
			None,
			// Token: 0x0400018D RID: 397
			Unloading,
			// Token: 0x0400018E RID: 398
			LowAmmo,
			// Token: 0x0400018F RID: 399
			Weapon,
			// Token: 0x04000190 RID: 400
			Ammo
		}
		// Token: 0x0600002B RID: 43 RVA: 0x000030FB File Offset: 0x000012FB
		public static bool ShouldTryToSearchNearForBetterWeapon(Pawn pawn)
		{
			return AdvancedAI_TakeAndEquipUtility.ShouldTryToSearchNearForBetterWeapon(pawn);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003104 File Offset: 0x00001304
		private WorkPriority GetPriorityWork(Pawn pawn)
		{
			bool trader = pawn.kindDef.trader;
			WorkPriority result;
			if (trader)
			{
				result = WorkPriority.None;
			}
			else
			{
				bool flag = AdvancedAI.PrimaryWeapon(pawn) != null;
				CompAmmoUser compAmmoUser = flag ? pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : (AdvancedAI_TakeAndEquipUtility.HasRangedWeaponInInventory(pawn, false) ? AdvancedAI_TakeAndEquipUtility.AmmoUserInInventory(pawn) : null);
				bool flag2 = pawn.Faction.IsPlayer && compAmmoUser != null;
				if (flag2)
				{
					Loadout loadout = Utility_Loadouts.GetLoadout(pawn);
					bool flag3 = loadout != null && loadout.SlotCount > 0;
					if (flag3)
					{
						return WorkPriority.None;
					}
				}
				bool flag4 = !flag;
				if (flag4)
				{
					bool flag5 = AdvancedAI_TakeAndEquipUtility.Unload(pawn);
					if (flag5)
					{
						return WorkPriority.Unloading;
					}
					bool flag6 = !AdvancedAI_TakeAndEquipUtility.HasRangedWeaponInInventory(pawn, true);
					if (flag6)
					{
						return WorkPriority.Weapon;
					}
				}
				CompInventory compInventory = pawn.TryGetComp<CompInventory>();
				bool flag7 = compAmmoUser != null && compAmmoUser.UseAmmo;
				if (flag7)
				{
					FloatRange floatRange = new FloatRange(1f, 2f);
					List<DefModExtension> modExtensions = pawn.kindDef.modExtensions;
					object obj;
					if (modExtensions == null)
					{
						obj = null;
					}
					else
					{
						obj = modExtensions.FirstOrDefault((DefModExtension x) => x is LoadoutPropertiesExtension);
					}
					LoadoutPropertiesExtension loadoutPropertiesExtension = (LoadoutPropertiesExtension)obj;
					List<string> weaponTags = pawn.kindDef.weaponTags;
					bool flag8 = weaponTags != null && weaponTags.Any<string>();
					bool flag9 = flag8 && compAmmoUser.parent.def.weaponTags.Any(new Predicate<string>(pawn.kindDef.weaponTags.Contains)) && loadoutPropertiesExtension != null && loadoutPropertiesExtension.primaryMagazineCount != FloatRange.Zero;
					if (flag9)
					{
						floatRange.min = loadoutPropertiesExtension.primaryMagazineCount.min;
						floatRange.max = loadoutPropertiesExtension.primaryMagazineCount.max;
					}
					floatRange.min *= (float)compAmmoUser.Props.magazineSize;
					floatRange.max *= (float)compAmmoUser.Props.magazineSize;
					int num = 0;
					float num2 = 0f;
					foreach (AmmoLink ammoLink in compAmmoUser.Props.ammoSet.ammoTypes)
					{
						int num3 = compInventory.AmmoCountOfDef(ammoLink.ammo);
						num += num3;
						num2 += (float)num3 * ammoLink.ammo.GetStatValueAbstract(CE_StatDefOf.Bulk, null);
					}
					float num4 = 0.666f * (compInventory.capacityBulk - compInventory.currentBulk + num2);
					bool flag10 = num2 < num4;
					if (flag10)
					{
						bool flag11 = compAmmoUser.Props.magazineSize == 0 || (float)num < floatRange.min;
						if (flag11)
						{
							return AdvancedAI_TakeAndEquipUtility.Unload(pawn) ? WorkPriority.Unloading : WorkPriority.LowAmmo;
						}
						bool flag12 = (float)num < floatRange.max && !AdvancedAI.InDangerousCombat(pawn, 35f);
						if (flag12)
						{
							return AdvancedAI_TakeAndEquipUtility.Unload(pawn) ? WorkPriority.Unloading : WorkPriority.Ammo;
						}
					}
					bool flag13 = JobGiver_TakeAndEquip.ShouldTryToSearchNearForBetterWeapon(pawn);
					if (flag13)
					{
						return WorkPriority.Weapon;
					}
				}
				result = WorkPriority.None;
			}
			return result;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003438 File Offset: 0x00001638
		public override float GetPriority(Pawn pawn)
		{
			bool flag = (!Controller.settings.AutoTakeAmmo && pawn.IsColonist) || !Controller.settings.EnableAmmoSystem;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = pawn.Faction == null;
				if (flag2)
				{
					result = 0f;
				}
				else
				{
					WorkPriority priorityWork = this.GetPriorityWork(pawn);
					bool flag3 = priorityWork == WorkPriority.Unloading;
					if (flag3)
					{
						result = 9.2f;
					}
					else
					{
						bool flag4 = priorityWork == WorkPriority.LowAmmo;
						if (flag4)
						{
							result = 9f;
						}
						else
						{
							bool flag5 = priorityWork == WorkPriority.Weapon;
							if (flag5)
							{
								result = 8f;
							}
							else
							{
								bool flag6 = priorityWork == WorkPriority.Ammo;
								if (flag6)
								{
									result = 6f;
								}
								else
								{
									bool flag7 = priorityWork == WorkPriority.None;
									if (flag7)
									{
										result = 0f;
									}
									else
									{
										TimeAssignmentDef timeAssignmentDef = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
										bool flag8 = timeAssignmentDef == TimeAssignmentDefOf.Sleep;
										if (flag8)
										{
											result = 0f;
										}
										else
										{
											bool flag9 = pawn.health == null || pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
											if (flag9)
											{
												result = 0f;
											}
											else
											{
												result = 0f;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003578 File Offset: 0x00001778
		protected override Job TryGiveJob(Pawn pawn)
		{
			JobGiver_TakeAndEquip.<>c__DisplayClass5_0 CS$<>8__locals1 = new JobGiver_TakeAndEquip.<>c__DisplayClass5_0();
			CS$<>8__locals1.pawn = pawn;
			bool flag = !Controller.settings.EnableAmmoSystem || !Controller.settings.AutoTakeAmmo;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = CS$<>8__locals1.pawn.Faction == null;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = !CS$<>8__locals1.pawn.RaceProps.Humanlike || (CS$<>8__locals1.pawn.story != null && CS$<>8__locals1.pawn.WorkTagIsDisabled(WorkTags.Violent));
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = CS$<>8__locals1.pawn.Faction.IsPlayer && CS$<>8__locals1.pawn.Drafted;
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = !CS$<>8__locals1.pawn.IsHashIntervalTick(120);
							if (flag5)
							{
								result = null;
							}
							else
							{
								MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(CS$<>8__locals1.pawn);
								bool flag6 = !CS$<>8__locals1.pawn.Faction.IsPlayer && mapComponent_SkyAI != null && mapComponent_SkyAI.mapPawnCount > 60;
								if (flag6)
								{
									result = null;
								}
								else
								{
									bool flag7 = CS$<>8__locals1.pawn.IsPrisoner && (CS$<>8__locals1.pawn.HostFaction != Faction.OfPlayer || CS$<>8__locals1.pawn.guest.interactionMode == PrisonerInteractionModeDefOf.Release);
									if (flag7)
									{
										result = null;
									}
									else
									{
										bool isSlave = CS$<>8__locals1.pawn.IsSlave;
										if (isSlave)
										{
											result = null;
										}
										else
										{
											RaidData raidData = AdvancedAI.PawnRaidData(CS$<>8__locals1.pawn);
											bool flag8 = raidData != null && raidData.raidStage == RaidData.RaidStage.fleeing;
											if (flag8)
											{
												result = null;
											}
											else
											{
												bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
												if (flag9)
												{
													Log.Message(string.Format("{0} {1}: TakeWeapon job. priority: {2} capacityWeight: {3} + currentWeight: {4} capacityBulk: {5} currentBulk: {6}", new object[]
													{
														CS$<>8__locals1.pawn,
														CS$<>8__locals1.pawn.Position,
														this.GetPriorityWork(CS$<>8__locals1.pawn),
														CS$<>8__locals1.pawn.TryGetComp<CompInventory>().capacityWeight,
														CS$<>8__locals1.pawn.TryGetComp<CompInventory>().currentWeight,
														CS$<>8__locals1.pawn.TryGetComp<CompInventory>().capacityBulk,
														CS$<>8__locals1.pawn.TryGetComp<CompInventory>().currentBulk
													}));
												}
												bool flag10 = CS$<>8__locals1.pawn.story != null && CS$<>8__locals1.pawn.story.traits != null && CS$<>8__locals1.pawn.story.traits.HasTrait(TraitDefOf.Brawler);
												CompInventory compInventory = CS$<>8__locals1.pawn.TryGetComp<CompInventory>();
												CS$<>8__locals1.hasPrimary = (CS$<>8__locals1.pawn.equipment != null && CS$<>8__locals1.pawn.equipment.Primary != null);
												bool flag11 = JobGiver_TakeAndEquip.ShouldTryToSearchNearForBetterWeapon(CS$<>8__locals1.pawn);
												CS$<>8__locals1.primaryAmmoUser = (CS$<>8__locals1.hasPrimary ? CS$<>8__locals1.pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : null);
												CompAmmoUser compAmmoUser = CS$<>8__locals1.hasPrimary ? CS$<>8__locals1.pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : (AdvancedAI_TakeAndEquipUtility.HasRangedWeaponInInventory(CS$<>8__locals1.pawn, false) ? AdvancedAI_TakeAndEquipUtility.AmmoUserInInventory(CS$<>8__locals1.pawn) : null);
												bool flag12 = compInventory == null;
												if (flag12)
												{
													result = null;
												}
												else
												{
													bool flag13 = (!CS$<>8__locals1.pawn.Faction.IsPlayer & CS$<>8__locals1.hasPrimary) && CS$<>8__locals1.pawn.equipment.Primary.def.IsMeleeWeapon && !flag10;
													if (flag13)
													{
														bool flag14 = CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Melee).Level || CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 4;
														if (flag14)
														{
															ThingWithComps thingWithComps = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine);
															bool flag15 = thingWithComps != null;
															if (flag15)
															{
																compInventory.TrySwitchToWeapon(thingWithComps);
															}
														}
													}
													bool flag16 = !CS$<>8__locals1.pawn.Faction.IsPlayer && !CS$<>8__locals1.hasPrimary;
													if (flag16)
													{
														bool flag17 = (CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Melee).Level || CS$<>8__locals1.pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6) && !flag10;
														if (flag17)
														{
															ThingWithComps thingWithComps2 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine);
															bool flag18 = thingWithComps2 != null;
															if (flag18)
															{
																compInventory.TrySwitchToWeapon(thingWithComps2);
															}
														}
														else
														{
															ThingWithComps thingWithComps3 = compInventory.meleeWeaponList.Find((ThingWithComps thing) => thing.def.IsMeleeWeapon);
															bool flag19 = thingWithComps3 != null;
															if (flag19)
															{
																compInventory.TrySwitchToWeapon(thingWithComps3);
															}
														}
													}
													WorkPriority priorityWork = this.GetPriorityWork(CS$<>8__locals1.pawn);
													bool flag20 = !CS$<>8__locals1.pawn.Faction.IsPlayer && CS$<>8__locals1.primaryAmmoUser != null && priorityWork == WorkPriority.Unloading && compInventory.rangedWeaponList.Count >= 1;
													if (flag20)
													{
														Thing thing16 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.def != CS$<>8__locals1.pawn.equipment.Primary.def);
														bool flag21 = thing16 != null;
														if (flag21)
														{
															Thing thing2 = null;
															bool flag22 = !thing16.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine;
															if (flag22)
															{
																using (List<AmmoLink>.Enumerator enumerator = thing16.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
																{
																	while (enumerator.MoveNext())
																	{
																		AmmoLink link = enumerator.Current;
																		bool flag23 = compInventory.ammoList.Find((Thing thing) => thing.def == link.ammo) == null;
																		if (flag23)
																		{
																			thing2 = thing16;
																			break;
																		}
																	}
																}
															}
															bool flag24 = thing2 != null;
															if (flag24)
															{
																Thing t;
																bool flag25 = compInventory.container.TryDrop(thing16, CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, ThingPlaceMode.Near, thing16.stackCount, out t, null, null);
																if (flag25)
																{
																	CS$<>8__locals1.pawn.jobs.EndCurrentJob(JobCondition.None, true, true);
																	CS$<>8__locals1.pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, t, 30, true), new JobTag?(JobTag.Misc), false);
																}
															}
														}
													}
													bool flag26 = (!CS$<>8__locals1.pawn.Faction.IsPlayer & CS$<>8__locals1.hasPrimary) && compInventory.ammoList.Count > 1 && priorityWork == WorkPriority.Unloading;
													if (flag26)
													{
														Thing thing3 = null;
														thing3 = ((CS$<>8__locals1.primaryAmmoUser != null) ? compInventory.ammoList.Find((Thing thing) => !CS$<>8__locals1.primaryAmmoUser.Props.ammoSet.ammoTypes.Any((AmmoLink a) => a.ammo == thing.def)) : compInventory.ammoList.RandomElement<Thing>());
														bool flag27 = thing3 != null;
														if (flag27)
														{
															Thing thing4 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => CS$<>8__locals1.hasPrimary && thing.TryGetComp<CompAmmoUser>() != null && thing.def != CS$<>8__locals1.pawn.equipment.Primary.def);
															bool flag28 = thing4 != null;
															if (flag28)
															{
																Thing thing5 = null;
																using (List<AmmoLink>.Enumerator enumerator2 = thing4.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
																{
																	if (enumerator2.MoveNext())
																	{
																		AmmoLink link = enumerator2.Current;
																		thing5 = compInventory.ammoList.Find((Thing thing) => thing.def == link.ammo);
																	}
																}
																bool flag29 = thing5 != null && thing5 != thing3;
																if (flag29)
																{
																	Thing thing6;
																	bool flag30 = compInventory.container.TryDrop(thing5, CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, ThingPlaceMode.Near, thing5.stackCount, out thing6, null, null);
																	if (flag30)
																	{
																		CS$<>8__locals1.pawn.jobs.EndCurrentJob(JobCondition.None, true, true);
																		CS$<>8__locals1.pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, 30, true), new JobTag?(JobTag.Misc), false);
																	}
																}
															}
															else
															{
																Thing thing7;
																bool flag31 = compInventory.container.TryDrop(thing3, CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, ThingPlaceMode.Near, thing3.stackCount, out thing7, null, null);
																if (flag31)
																{
																	CS$<>8__locals1.pawn.jobs.EndCurrentJob(JobCondition.None, true, true);
																	CS$<>8__locals1.pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, 30, true), new JobTag?(JobTag.Misc), false);
																}
															}
														}
													}
													bool flag32 = (priorityWork == WorkPriority.Weapon && !CS$<>8__locals1.hasPrimary) || flag11;
													if (flag32)
													{
														bool flag33 = !CS$<>8__locals1.hasPrimary;
														if (flag33)
														{
															ThingWithComps thingWithComps4 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null);
															bool flag34 = thingWithComps4 != null;
															if (flag34)
															{
																Thing thing8 = null;
																using (List<AmmoLink>.Enumerator enumerator3 = thingWithComps4.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
																{
																	if (enumerator3.MoveNext())
																	{
																		AmmoLink link = enumerator3.Current;
																		thing8 = compInventory.ammoList.Find((Thing thing) => thing.def == link.ammo);
																	}
																}
																bool flag35 = thing8 != null;
																if (flag35)
																{
																	compInventory.TrySwitchToWeapon(thingWithComps4);
																}
															}
														}
														bool flag36 = !CS$<>8__locals1.pawn.Faction.IsPlayer;
														if (flag36)
														{
															float radius = flag11 ? 7f : 20f;
															List<Thing> things = AdvancedAI.HaulableThingsAround(CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, radius, false);
															AdvancedAI_GenerateUtility.GenerateWeaponLists(CS$<>8__locals1.pawn, things, 10, false);
															List<Thing> allDroppedWeapons = AdvancedAI_GenerateUtility.allDroppedWeapons;
															IDictionary<ThingWithComps, Corpse> allCorpseWeapons = AdvancedAI_GenerateUtility.allCorpseWeapons;
															IDictionary<ThingWithComps, Pawn> allDownedPawnWeapons = AdvancedAI_GenerateUtility.allDownedPawnWeapons;
															IDictionary<ThingWithComps, Pawn> allCaravanPackWeapons = AdvancedAI_GenerateUtility.allCaravanPackWeapons;
															List<Thing> list = new List<Thing>();
															list.AddRange(allDroppedWeapons);
															list.AddRange(allCorpseWeapons.Keys);
															list.AddRange(allDownedPawnWeapons.Keys);
															list.AddRange(allCaravanPackWeapons.Keys);
															list = (from w in list
															orderby base.<TryGiveJob>g__weight|12(w) descending
															select w).ToList<Thing>();
															bool flag37 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
															if (flag37)
															{
																foreach (Thing thing9 in list)
																{
																	Log.Message(string.Format("{0} {1}: TakeAndEquipDebug1. Weapon: {2} value: {3} w.Pos: {4} dist: {5} market: {6} isRanged: {7}", new object[]
																	{
																		CS$<>8__locals1.pawn,
																		CS$<>8__locals1.pawn.Position,
																		thing9,
																		CS$<>8__locals1.<TryGiveJob>g__weight|12(thing9),
																		thing9.PositionHeld,
																		CS$<>8__locals1.pawn.Position.DistanceTo(thing9.Position),
																		thing9.MarketValue,
																		thing9.def.IsRangedWeapon
																	}));
																}
															}
															List<Thing> list2 = (from w in list
															where w.def.IsRangedWeapon
															select w).ToList<Thing>();
															bool flag38 = !list2.NullOrEmpty<Thing>();
															if (flag38)
															{
																foreach (Thing thing10 in list2)
																{
																	bool flag39 = AdvancedAI_TakeAndEquipUtility.EnemyThreatOnCell(CS$<>8__locals1.pawn, thing10.PositionHeld);
																	if (flag39)
																	{
																		bool flag40 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																		if (flag40)
																		{
																			Log.Message(string.Format("{0} {1}: TakeAndEquipDebug1.1. Weapon: {2}. Passing bcs of enemy threat on pos: {3}", new object[]
																			{
																				CS$<>8__locals1.pawn,
																				CS$<>8__locals1.pawn.Position,
																				thing10,
																				thing10.PositionHeld
																			}));
																		}
																	}
																	else
																	{
																		bool flag41 = thing10.TryGetComp<CompAmmoUser>() == null;
																		if (flag41)
																		{
																			ThingWithComps thingWithComps5 = thing10 as ThingWithComps;
																			bool flag42 = thingWithComps5 != null;
																			if (flag42)
																			{
																				bool flag43 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																				if (flag43)
																				{
																					Log.Message(string.Format("{0} {1}: TakeAndEquipDebug1.2. Weapon: {2}. Try to interact with non-AmmoUser selected weapon.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, thingWithComps5));
																				}
																				bool flag44 = allCorpseWeapons.ContainsKey(thingWithComps5);
																				if (flag44)
																				{
																					int num;
																					bool flag45 = compInventory.CanFitInInventory(thingWithComps5, ref num, false, false);
																					if (flag45)
																					{
																						Corpse corpse;
																						allCorpseWeapons.TryGetValue(thingWithComps5, out corpse);
																						bool flag46 = corpse != null;
																						if (flag46)
																						{
																							bool flag47 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																							if (flag47)
																							{
																								Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting corpse: {2} on pos {3} for {4}", new object[]
																								{
																									CS$<>8__locals1.pawn,
																									CS$<>8__locals1.pawn.Position,
																									corpse,
																									corpse.Position,
																									thingWithComps5
																								}));
																							}
																							return JobMaker.MakeJob(CE_JobDefOf.LootItem, corpse, thingWithComps5);
																						}
																					}
																				}
																				else
																				{
																					bool flag48 = allDownedPawnWeapons.ContainsKey(thingWithComps5);
																					if (flag48)
																					{
																						int num2;
																						bool flag49 = compInventory.CanFitInInventory(thingWithComps5, ref num2, false, false);
																						if (flag49)
																						{
																							Pawn pawn2;
																							allDownedPawnWeapons.TryGetValue(thingWithComps5, out pawn2);
																							bool flag50 = pawn2 != null;
																							if (flag50)
																							{
																								bool flag51 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																								if (flag51)
																								{
																									Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4}", new object[]
																									{
																										CS$<>8__locals1.pawn,
																										CS$<>8__locals1.pawn.Position,
																										pawn2,
																										pawn2.Position,
																										thingWithComps5
																									}));
																								}
																								return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn2, thingWithComps5);
																							}
																						}
																					}
																					else
																					{
																						bool flag52 = allCaravanPackWeapons.ContainsKey(thingWithComps5);
																						if (flag52)
																						{
																							int num3;
																							bool flag53 = compInventory.CanFitInInventory(thingWithComps5, ref num3, false, false);
																							if (flag53)
																							{
																								Pawn pawn3;
																								allCaravanPackWeapons.TryGetValue(thingWithComps5, out pawn3);
																								bool flag54 = pawn3 != null;
																								if (flag54)
																								{
																									bool flag55 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																									if (flag55)
																									{
																										Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting pack animal: {2} on pos {3} for {4}", new object[]
																										{
																											CS$<>8__locals1.pawn,
																											CS$<>8__locals1.pawn.Position,
																											pawn3,
																											pawn3.Position,
																											thingWithComps5
																										}));
																									}
																									return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn3, thingWithComps5);
																								}
																							}
																						}
																						else
																						{
																							int num4;
																							bool flag56 = compInventory.CanFitInInventory(thingWithComps5, ref num4, false, false);
																							if (flag56)
																							{
																								bool flag57 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																								if (flag57)
																								{
																									Log.Message(string.Format("{0} {1}: TakeAndEquip. TakeInventory {2} on pos: {3}", new object[]
																									{
																										CS$<>8__locals1.pawn,
																										CS$<>8__locals1.pawn.Position,
																										thingWithComps5,
																										thingWithComps5.Position
																									}));
																								}
																								return AdvancedAI_TakeAndEquipUtility.TakeOrEquip(thingWithComps5, CS$<>8__locals1.hasPrimary);
																							}
																						}
																					}
																				}
																			}
																		}
																		else
																		{
																			AdvancedAI_GenerateUtility.GenerateAmmoLists(CS$<>8__locals1.pawn, things, 20, false);
																			List<Thing> allDroppedAmmo = AdvancedAI_GenerateUtility.allDroppedAmmo;
																			IDictionary<ThingWithComps, Corpse> allCorpseAmmo = AdvancedAI_GenerateUtility.allCorpseAmmo;
																			IDictionary<ThingWithComps, Pawn> allDownedPawnAmmo = AdvancedAI_GenerateUtility.allDownedPawnAmmo;
																			IDictionary<ThingWithComps, Pawn> allCaravanPackAmmo = AdvancedAI_GenerateUtility.allCaravanPackAmmo;
																			List<Thing> list3 = new List<Thing>();
																			list3.AddRange(allDroppedAmmo);
																			list3.AddRange(allCorpseAmmo.Keys);
																			list3.AddRange(allDownedPawnAmmo.Keys);
																			list3.AddRange(allCaravanPackAmmo.Keys);
																			bool flag58 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																			if (flag58)
																			{
																				bool flag59 = list3.NullOrEmpty<Thing>();
																				if (flag59)
																				{
																					Log.Message(string.Format("{0} {1}: TakeAndEquipDebug2. Any ammo for weapon: {2} not found.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, thing10));
																				}
																				else
																				{
																					foreach (Thing thing11 in list3)
																					{
																						Log.Message(string.Format("{0} {1}: TakeAndEquipDebug2. Ammo: {2} w.Pos: {3} dist: {4}", new object[]
																						{
																							CS$<>8__locals1.pawn,
																							CS$<>8__locals1.pawn.Position,
																							thing11,
																							thing11.PositionHeld,
																							CS$<>8__locals1.pawn.Position.DistanceTo(thing11.Position)
																						}));
																					}
																				}
																			}
																			List<ThingDef> thingDefAmmoList = (from g in thing10.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes
																			select g.ammo).ToList<ThingDef>();
																			bool flag60 = !list3.NullOrEmpty<Thing>() && !thingDefAmmoList.NullOrEmpty<ThingDef>();
																			if (flag60)
																			{
																				int desiredStackSize = thing10.TryGetComp<CompAmmoUser>().Props.magazineSize * 2;
																				Thing thing12 = list3.FirstOrDefault((Thing x) => thingDefAmmoList.Contains(x.def) && x.stackCount > desiredStackSize && !AdvancedAI_TakeAndEquipUtility.EnemyThreatOnCell(CS$<>8__locals1.pawn, x.PositionHeld));
																				bool flag61 = thing12 != null;
																				if (flag61)
																				{
																					int num5;
																					bool flag62 = compInventory.CanFitInInventory(thing10, ref num5, false, false);
																					bool flag63 = flag62;
																					if (flag63)
																					{
																						ThingWithComps thingWithComps6 = thing10 as ThingWithComps;
																						bool flag64 = thingWithComps6 != null;
																						if (flag64)
																						{
																							bool flag65 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																							if (flag65)
																							{
																								Log.Message(string.Format("{0} {1}: TakeAndEquipDebug2.1. Weapon: {2}. Try to interact with selected weapon.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, thingWithComps6));
																							}
																							bool flag66 = allCorpseWeapons.ContainsKey(thingWithComps6);
																							if (flag66)
																							{
																								Corpse corpse2;
																								allCorpseWeapons.TryGetValue(thingWithComps6, out corpse2);
																								bool flag67 = corpse2 != null;
																								if (flag67)
																								{
																									bool flag68 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																									if (flag68)
																									{
																										Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting corpse: {2} on pos {3} for {4}", new object[]
																										{
																											CS$<>8__locals1.pawn,
																											CS$<>8__locals1.pawn.Position,
																											corpse2,
																											corpse2.Position,
																											thingWithComps6
																										}));
																									}
																									return JobMaker.MakeJob(CE_JobDefOf.LootItem, corpse2, thingWithComps6);
																								}
																							}
																							else
																							{
																								bool flag69 = allDownedPawnWeapons.ContainsKey(thingWithComps6);
																								if (flag69)
																								{
																									Pawn pawn4;
																									allDownedPawnWeapons.TryGetValue(thingWithComps6, out pawn4);
																									bool flag70 = pawn4 != null;
																									if (flag70)
																									{
																										bool flag71 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag71)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4}", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												pawn4,
																												pawn4.Position,
																												thingWithComps6
																											}));
																										}
																										return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn4, thingWithComps6);
																									}
																								}
																								else
																								{
																									bool flag72 = allCaravanPackWeapons.ContainsKey(thingWithComps6);
																									if (!flag72)
																									{
																										bool flag73 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag73)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. TakeInventory {2} on pos: {3}", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												thingWithComps6,
																												thingWithComps6.Position
																											}));
																										}
																										return AdvancedAI_TakeAndEquipUtility.TakeOrEquip(thingWithComps6, CS$<>8__locals1.hasPrimary);
																									}
																									Pawn pawn5;
																									allCaravanPackWeapons.TryGetValue(thingWithComps6, out pawn5);
																									bool flag74 = pawn5 != null;
																									if (flag74)
																									{
																										bool flag75 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag75)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting pack animal: {2} on pos {3} for {4}", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												pawn5,
																												pawn5.Position,
																												thingWithComps6
																											}));
																										}
																										return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn5, thingWithComps6);
																									}
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
															else
															{
																bool flag76 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																if (flag76)
																{
																	Log.Message(string.Format("{0} {1}: TakeAndEquipDebug1.1. rangedWeapons list null or empty.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position));
																}
															}
															bool flag77 = !list.NullOrEmpty<Thing>() && !CS$<>8__locals1.hasPrimary;
															if (flag77)
															{
																Thing thing13;
																(from w in list
																where !w.def.IsRangedWeapon && w.def.IsMeleeWeapon && !AdvancedAI_TakeAndEquipUtility.EnemyThreatOnCell(CS$<>8__locals1.pawn, w.PositionHeld)
																select w).TryMaxBy((Thing w2) => w2.MarketValue - (float)JobGiver_TakeAndEquip.<TryGiveJob>g__position|5_11(w2).DistanceToSquared(CS$<>8__locals1.pawn.Position) * 2f, out thing13);
																bool flag78 = thing13 != null;
																if (flag78)
																{
																	ThingWithComps thingWithComps7 = thing13 as ThingWithComps;
																	bool flag79 = thingWithComps7 != null;
																	if (flag79)
																	{
																		bool flag80 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																		if (flag80)
																		{
																			Log.Message(string.Format("{0} {1}: TakeAndEquipDebug4. Weapon: {2}. Try to interact with selected melee weapon.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, thingWithComps7));
																		}
																		bool flag81 = allCorpseWeapons.ContainsKey(thingWithComps7);
																		if (flag81)
																		{
																			Corpse corpse3;
																			allCorpseWeapons.TryGetValue(thingWithComps7, out corpse3);
																			bool flag82 = corpse3 != null;
																			if (flag82)
																			{
																				bool flag83 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																				if (flag83)
																				{
																					Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting corpse: {2} on pos {3} for {4}", new object[]
																					{
																						CS$<>8__locals1.pawn,
																						CS$<>8__locals1.pawn.Position,
																						corpse3,
																						corpse3.Position,
																						thingWithComps7
																					}));
																				}
																				return JobMaker.MakeJob(CE_JobDefOf.LootItem, corpse3, thingWithComps7);
																			}
																		}
																		else
																		{
																			bool flag84 = allDownedPawnWeapons.ContainsKey(thingWithComps7);
																			if (flag84)
																			{
																				Pawn pawn6;
																				allDownedPawnWeapons.TryGetValue(thingWithComps7, out pawn6);
																				bool flag85 = pawn6 != null;
																				if (flag85)
																				{
																					bool flag86 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																					if (flag86)
																					{
																						Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4}", new object[]
																						{
																							CS$<>8__locals1.pawn,
																							CS$<>8__locals1.pawn.Position,
																							pawn6,
																							pawn6.Position,
																							thingWithComps7
																						}));
																					}
																					return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn6, thingWithComps7);
																				}
																			}
																			else
																			{
																				bool flag87 = allCaravanPackWeapons.ContainsKey(thingWithComps7);
																				if (!flag87)
																				{
																					bool flag88 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																					if (flag88)
																					{
																						Log.Message(string.Format("{0} {1}: TakeAndEquip. TakeInventory {2} on pos: {3}", new object[]
																						{
																							CS$<>8__locals1.pawn,
																							CS$<>8__locals1.pawn.Position,
																							thing13,
																							thing13.Position
																						}));
																					}
																					return AdvancedAI_TakeAndEquipUtility.TakeOrEquip((ThingWithComps)thing13, CS$<>8__locals1.hasPrimary);
																				}
																				Pawn pawn7;
																				allCaravanPackWeapons.TryGetValue(thingWithComps7, out pawn7);
																				bool flag89 = pawn7 != null;
																				if (flag89)
																				{
																					bool flag90 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																					if (flag90)
																					{
																						Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4}", new object[]
																						{
																							CS$<>8__locals1.pawn,
																							CS$<>8__locals1.pawn.Position,
																							pawn7,
																							pawn7.Position,
																							thingWithComps7
																						}));
																					}
																					return JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn7, thingWithComps7);
																				}
																			}
																		}
																	}
																}
															}
														}
													}
													bool flag91 = (priorityWork == WorkPriority.Ammo || priorityWork == WorkPriority.LowAmmo) && compAmmoUser != null;
													if (flag91)
													{
														List<ThingDef> list4 = (from AmmoLink g in compAmmoUser.Props.ammoSet.ammoTypes
														select g.ammo).ToList<ThingDef>();
														bool flag92 = !list4.NullOrEmpty<ThingDef>();
														if (flag92)
														{
															List<Thing> things2 = AdvancedAI.HaulableThingsAround(CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, 20f, false);
															AdvancedAI_GenerateUtility.GenerateAmmoLists(CS$<>8__locals1.pawn, things2, 20, true);
															List<Thing> allDroppedAmmo2 = AdvancedAI_GenerateUtility.allDroppedAmmo;
															IDictionary<ThingWithComps, Corpse> allCorpseAmmo2 = AdvancedAI_GenerateUtility.allCorpseAmmo;
															IDictionary<ThingWithComps, Pawn> allDownedPawnAmmo2 = AdvancedAI_GenerateUtility.allDownedPawnAmmo;
															IDictionary<ThingWithComps, Pawn> allCaravanPackAmmo2 = AdvancedAI_GenerateUtility.allCaravanPackAmmo;
															List<Thing> list5 = new List<Thing>();
															list5.AddRange(allDroppedAmmo2);
															list5.AddRange(allCorpseAmmo2.Keys);
															list5.AddRange(allDownedPawnAmmo2.Keys);
															list5.AddRange(allCaravanPackAmmo2.Keys);
															bool flag93 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
															if (flag93)
															{
																foreach (Thing thing14 in list5)
																{
																	Log.Message(string.Format("{0} {1}: TakeAndEquipDebug3. Ammo: {2} w.Pos: {3} w.PosHeld: {4} dist: {5}", new object[]
																	{
																		CS$<>8__locals1.pawn,
																		CS$<>8__locals1.pawn.Position,
																		thing14,
																		thing14.Position,
																		thing14.PositionHeld,
																		CS$<>8__locals1.pawn.Position.DistanceTo(thing14.Position)
																	}));
																}
															}
															bool flag94 = !list5.NullOrEmpty<Thing>();
															if (flag94)
															{
																foreach (Thing thing15 in list5)
																{
																	bool flag95 = AdvancedAI_TakeAndEquipUtility.EnemyThreatOnCell(CS$<>8__locals1.pawn, thing15.PositionHeld);
																	if (flag95)
																	{
																		bool flag96 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																		if (flag96)
																		{
																			Log.Message(string.Format("{0} {1}: TakeAndEquipDebug3.1. Ammo: {2}. Passing bcs of enemy threat on pos: {3}", new object[]
																			{
																				CS$<>8__locals1.pawn,
																				CS$<>8__locals1.pawn.Position,
																				thing15,
																				thing15.PositionHeld
																			}));
																		}
																	}
																	else
																	{
																		foreach (ThingDef thingDef in list4)
																		{
																			bool flag97 = thingDef == thing15.def;
																			if (flag97)
																			{
																				float num6 = thing15.GetStatValue(CE_StatDefOf.Bulk, true) * (float)thing15.stackCount;
																				bool flag98 = num6 > 0.5f;
																				if (flag98)
																				{
																					int num7;
																					bool flag99 = compInventory.CanFitInInventory(thing15, ref num7, false, false);
																					if (flag99)
																					{
																						ThingWithComps thingWithComps8 = thing15 as ThingWithComps;
																						bool flag100 = thingWithComps8 != null;
																						if (flag100)
																						{
																							bool flag101 = SkyAiCore.Settings.debugTakeAndEquip && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																							if (flag101)
																							{
																								Log.Message(string.Format("{0} {1}: TakeAndEquipDebug3.2. Ammo: {2}. Try to interact with selected ammo.", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, thingWithComps8));
																							}
																							bool flag102 = allCorpseAmmo2.ContainsKey(thingWithComps8);
																							if (flag102)
																							{
																								Corpse corpse4;
																								allCorpseAmmo2.TryGetValue(thingWithComps8, out corpse4);
																								bool flag103 = corpse4 != null;
																								if (flag103)
																								{
																									Job job = JobMaker.MakeJob(CE_JobDefOf.LootItem, corpse4, thingWithComps8);
																									job.count = Mathf.RoundToInt((float)num7);
																									bool flag104 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																									if (flag104)
																									{
																										Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting corpse: {2} on pos {3} for {4} count: {5}", new object[]
																										{
																											CS$<>8__locals1.pawn,
																											CS$<>8__locals1.pawn.Position,
																											corpse4,
																											corpse4.Position,
																											thingWithComps8,
																											num7
																										}));
																									}
																									return job;
																								}
																							}
																							else
																							{
																								bool flag105 = allDownedPawnAmmo2.ContainsKey(thingWithComps8);
																								if (flag105)
																								{
																									Pawn pawn8;
																									allDownedPawnAmmo2.TryGetValue(thingWithComps8, out pawn8);
																									bool flag106 = pawn8 != null;
																									if (flag106)
																									{
																										Job job2 = JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn8, thingWithComps8);
																										job2.count = Mathf.RoundToInt((float)num7);
																										bool flag107 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag107)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4} count: {5}", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												pawn8,
																												pawn8.Position,
																												thingWithComps8,
																												num7
																											}));
																										}
																										return job2;
																									}
																								}
																								else
																								{
																									bool flag108 = allCaravanPackAmmo2.ContainsKey(thingWithComps8);
																									if (!flag108)
																									{
																										Job job3 = JobMaker.MakeJob(JobDefOf.TakeInventory, thing15);
																										job3.count = Mathf.RoundToInt((float)num7);
																										bool flag109 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag109)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. TakeInventory {2} count: {3} on pos: {4} ", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												thing15,
																												num7,
																												thing15.Position
																											}));
																										}
																										return job3;
																									}
																									Pawn pawn9;
																									allCaravanPackAmmo2.TryGetValue(thingWithComps8, out pawn9);
																									bool flag110 = pawn9 != null;
																									if (flag110)
																									{
																										Job job4 = JobMaker.MakeJob(CE_JobDefOf.LootItem, pawn9, thingWithComps8);
																										job4.count = Mathf.RoundToInt((float)num7);
																										bool flag111 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
																										if (flag111)
																										{
																											Log.Message(string.Format("{0} {1}: TakeAndEquip. Looting downed: {2} on pos {3} for {4} count: {5}", new object[]
																											{
																												CS$<>8__locals1.pawn,
																												CS$<>8__locals1.pawn.Position,
																												pawn9,
																												pawn9.Position,
																												thingWithComps8,
																												num7
																											}));
																										}
																										return job4;
																									}
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
													result = null;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000057C8 File Offset: 0x000039C8
		[CompilerGenerated]
        internal static IntVec3<TryGiveJob> g__position|5_11(Thing w)
		{
			return w.Position.IsValid ? w.Position : w.PositionHeld;
		}

		// Token: 0x04000013 RID: 19
		
	}
}
