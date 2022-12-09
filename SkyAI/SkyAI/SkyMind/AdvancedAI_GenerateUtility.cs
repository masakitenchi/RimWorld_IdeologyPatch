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
	// Token: 0x02000024 RID: 36
	public static class AdvancedAI_GenerateUtility
	{
		// Token: 0x0600013C RID: 316 RVA: 0x0001E0AC File Offset: 0x0001C2AC
		public static void GenerateWeaponLists(Pawn pawn, IEnumerable<Thing> things, int maxIteration, bool checkForPlayerFaction)
		{
			AdvancedAI_GenerateUtility.<>c__DisplayClass4_0 CS$<>8__locals1;
			CS$<>8__locals1.pawn = pawn;
			CS$<>8__locals1.checkForPlayerFaction = checkForPlayerFaction;
			AdvancedAI_GenerateUtility.allDroppedWeapons.Clear();
			AdvancedAI_GenerateUtility.allCorpseWeapons.Clear();
			AdvancedAI_GenerateUtility.allDownedPawnWeapons.Clear();
			AdvancedAI_GenerateUtility.allCaravanPackWeapons.Clear();
			MapComponent_SkyAI mapComp = AdvancedAI_Classes.MapComp(CS$<>8__locals1.pawn);
			int num = 0;
			for (int i = 0; i < things.Count<Thing>(); i++)
			{
				bool flag = num >= maxIteration;
				if (flag)
				{
					break;
				}
				Thing thing = things.ElementAt(i);
				bool flag2 = thing != null;
				if (flag2)
				{
					Pawn pawn2 = thing as Pawn;
					bool flag3 = pawn2 != null;
					if (flag3)
					{
						bool animal = pawn2.RaceProps.Animal;
						if (animal)
						{
							object obj;
							if (pawn2 == null)
							{
								obj = null;
							}
							else
							{
								Pawn_InventoryTracker inventory = pawn2.inventory;
								obj = ((inventory != null) ? inventory.innerContainer : null);
							}
							bool flag4 = obj != null;
							if (flag4)
							{
								List<Thing> innerListForReading = pawn2.inventory.innerContainer.InnerListForReading;
								for (int j = 0; j < innerListForReading.Count<Thing>(); j++)
								{
									ThingWithComps thingWithComps = (ThingWithComps)innerListForReading[j];
									bool flag5 = thingWithComps != null && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(thingWithComps) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag5)
									{
										bool flag6 = pawn2.Downed && !AdvancedAI_GenerateUtility.allDownedPawnWeapons.ContainsKey(thingWithComps);
										if (flag6)
										{
											AdvancedAI_GenerateUtility.allDownedPawnWeapons.Add(thingWithComps, pawn2);
											num++;
										}
										else
										{
											bool flag7 = !AdvancedAI_GenerateUtility.allCaravanPackWeapons.ContainsKey(thingWithComps);
											if (flag7)
											{
												AdvancedAI_GenerateUtility.allCaravanPackWeapons.Add(thingWithComps, pawn2);
												num++;
											}
										}
									}
								}
							}
						}
						else
						{
							bool flag8 = pawn2.equipment != null;
							if (flag8)
							{
								List<ThingWithComps> allEquipmentListForReading = pawn2.equipment.AllEquipmentListForReading;
								for (int k = 0; k < allEquipmentListForReading.Count<ThingWithComps>(); k++)
								{
									ThingWithComps thingWithComps2 = allEquipmentListForReading[k];
									bool flag9 = thingWithComps2 != null && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(thingWithComps2) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag9)
									{
										bool flag10 = pawn2.Downed && !AdvancedAI_GenerateUtility.allDownedPawnWeapons.ContainsKey(thingWithComps2);
										if (flag10)
										{
											AdvancedAI_GenerateUtility.allDownedPawnWeapons.Add(thingWithComps2, pawn2);
											num++;
										}
									}
								}
							}
							CompInventory comp = pawn2.GetComp<CompInventory>();
							bool flag11 = comp != null && !comp.container.NullOrEmpty<Thing>();
							if (flag11)
							{
								for (int l = 0; l < comp.container.Count<Thing>(); l++)
								{
									ThingWithComps thingWithComps3 = comp.container[l] as ThingWithComps;
									bool flag12 = thingWithComps3 != null && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(thingWithComps3) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag12)
									{
										bool flag13 = pawn2.Downed && !AdvancedAI_GenerateUtility.allDownedPawnWeapons.ContainsKey(thingWithComps3);
										if (flag13)
										{
											AdvancedAI_GenerateUtility.allDownedPawnWeapons.Add(thingWithComps3, pawn2);
											num++;
										}
									}
								}
							}
						}
					}
					Corpse corpse = thing as Corpse;
					bool flag14 = corpse != null;
					if (flag14)
					{
						CompInventory comp2 = corpse.InnerPawn.GetComp<CompInventory>();
						bool flag15 = comp2 != null && !comp2.container.NullOrEmpty<Thing>();
						if (flag15)
						{
							for (int m = 0; m < comp2.container.Count<Thing>(); m++)
							{
								ThingWithComps thingWithComps4 = comp2.container[m] as ThingWithComps;
								bool flag16 = thingWithComps4 != null && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(thingWithComps4) && !AdvancedAI_GenerateUtility.allCorpseWeapons.ContainsKey(thingWithComps4) && AdvancedAI_GenerateUtility.FactionAllowed(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ThingisForbidden(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, corpse) && CS$<>8__locals1.pawn.CanReserve(corpse, 1, -1, null, false) && CS$<>8__locals1.pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
								if (flag16)
								{
									AdvancedAI_GenerateUtility.allCorpseWeapons.Add(thingWithComps4, corpse);
									num++;
								}
							}
						}
						bool flag17 = corpse.InnerPawn.equipment != null;
						if (flag17)
						{
							List<ThingWithComps> allEquipmentListForReading2 = corpse.InnerPawn.equipment.AllEquipmentListForReading;
							for (int n = 0; n < allEquipmentListForReading2.Count<ThingWithComps>(); n++)
							{
								ThingWithComps thingWithComps5 = allEquipmentListForReading2[n];
								bool flag18 = thingWithComps5 != null && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(thingWithComps5) && !AdvancedAI_GenerateUtility.allCorpseWeapons.ContainsKey(thingWithComps5) && AdvancedAI_GenerateUtility.FactionAllowed(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ThingisForbidden(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, corpse) && CS$<>8__locals1.pawn.CanReserve(corpse, 1, -1, null, false) && CS$<>8__locals1.pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
								if (flag18)
								{
									AdvancedAI_GenerateUtility.allCorpseWeapons.Add(thingWithComps5, corpse);
									num++;
								}
							}
						}
					}
					else
					{
						bool flag19 = AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__weaponValidator|4_3(thing, ref CS$<>8__locals1) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, thing) && !AdvancedAI_GenerateUtility.ItemInExclusionList(CS$<>8__locals1.pawn, mapComp, thing);
						if (flag19)
						{
							AdvancedAI_GenerateUtility.allDroppedWeapons.Add(thing);
							num++;
						}
					}
				}
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x0001E740 File Offset: 0x0001C940
		public static void GenerateAmmoLists(Pawn pawn, IEnumerable<Thing> things, int maxIteration, bool checkForPlayerFaction)
		{
			AdvancedAI_GenerateUtility.<>c__DisplayClass9_0 CS$<>8__locals1;
			CS$<>8__locals1.pawn = pawn;
			CS$<>8__locals1.checkForPlayerFaction = checkForPlayerFaction;
			AdvancedAI_GenerateUtility.allDroppedAmmo.Clear();
			AdvancedAI_GenerateUtility.allCorpseAmmo.Clear();
			AdvancedAI_GenerateUtility.allDownedPawnAmmo.Clear();
			AdvancedAI_GenerateUtility.allCaravanPackAmmo.Clear();
			MapComponent_SkyAI mapComp = AdvancedAI_Classes.MapComp(CS$<>8__locals1.pawn);
			int num = 0;
			for (int i = 0; i < things.Count<Thing>(); i++)
			{
				bool flag = num >= maxIteration;
				if (flag)
				{
					break;
				}
				Thing thing = things.ElementAt(i);
				bool flag2 = thing != null;
				if (flag2)
				{
					Pawn pawn2 = thing as Pawn;
					bool flag3 = pawn2 != null;
					if (flag3)
					{
						bool animal = pawn2.RaceProps.Animal;
						if (animal)
						{
							object obj;
							if (pawn2 == null)
							{
								obj = null;
							}
							else
							{
								Pawn_InventoryTracker inventory = pawn2.inventory;
								obj = ((inventory != null) ? inventory.innerContainer : null);
							}
							bool flag4 = obj != null;
							if (flag4)
							{
								List<Thing> innerListForReading = pawn2.inventory.innerContainer.InnerListForReading;
								for (int j = 0; j < innerListForReading.Count<Thing>(); j++)
								{
									ThingWithComps thingWithComps = (ThingWithComps)innerListForReading[j];
									bool flag5 = thingWithComps != null && AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(thingWithComps) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag5)
									{
										bool flag6 = pawn2.Downed && !AdvancedAI_GenerateUtility.allDownedPawnAmmo.ContainsKey(thingWithComps);
										if (flag6)
										{
											AdvancedAI_GenerateUtility.allDownedPawnAmmo.Add(thingWithComps, pawn2);
											num++;
										}
										else
										{
											bool flag7 = !AdvancedAI_GenerateUtility.allCaravanPackAmmo.ContainsKey(thingWithComps);
											if (flag7)
											{
												AdvancedAI_GenerateUtility.allCaravanPackAmmo.Add(thingWithComps, pawn2);
												num++;
											}
										}
									}
								}
							}
						}
						else
						{
							bool flag8 = pawn2.equipment != null;
							if (flag8)
							{
								List<ThingWithComps> allEquipmentListForReading = pawn2.equipment.AllEquipmentListForReading;
								for (int k = 0; k < allEquipmentListForReading.Count<ThingWithComps>(); k++)
								{
									ThingWithComps thingWithComps2 = allEquipmentListForReading[k];
									bool flag9 = thingWithComps2 != null && AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(thingWithComps2) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag9)
									{
										bool flag10 = pawn2.Downed && !AdvancedAI_GenerateUtility.allDownedPawnAmmo.ContainsKey(thingWithComps2);
										if (flag10)
										{
											AdvancedAI_GenerateUtility.allDownedPawnAmmo.Add(thingWithComps2, pawn2);
											num++;
										}
									}
								}
							}
							CompInventory comp = pawn2.GetComp<CompInventory>();
							bool flag11 = comp != null && !comp.container.NullOrEmpty<Thing>();
							if (flag11)
							{
								for (int l = 0; l < comp.container.Count<Thing>(); l++)
								{
									ThingWithComps thingWithComps3 = comp.container[l] as ThingWithComps;
									bool flag12 = thingWithComps3 != null && AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(thingWithComps3) && AdvancedAI_GenerateUtility.FactionAllowed(pawn2, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(pawn2, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, pawn2) && CS$<>8__locals1.pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
									if (flag12)
									{
										bool flag13 = !AdvancedAI_GenerateUtility.allDownedPawnAmmo.ContainsKey(thingWithComps3);
										if (flag13)
										{
											AdvancedAI_GenerateUtility.allDownedPawnAmmo.Add(thingWithComps3, pawn2);
											num++;
										}
									}
								}
							}
						}
					}
					Corpse corpse = thing as Corpse;
					bool flag14 = corpse != null;
					if (flag14)
					{
						CompInventory comp2 = corpse.InnerPawn.GetComp<CompInventory>();
						bool flag15 = comp2 != null && !comp2.container.NullOrEmpty<Thing>();
						if (flag15)
						{
							for (int m = 0; m < comp2.container.Count<Thing>(); m++)
							{
								ThingWithComps thingWithComps4 = comp2.container[m] as ThingWithComps;
								bool flag16 = thingWithComps4 != null && AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(thingWithComps4) && !AdvancedAI_GenerateUtility.allCorpseAmmo.ContainsKey(thingWithComps4) && AdvancedAI_GenerateUtility.FactionAllowed(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ThingisForbidden(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(corpse, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, corpse) && CS$<>8__locals1.pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
								if (flag16)
								{
									AdvancedAI_GenerateUtility.allCorpseAmmo.Add(thingWithComps4, corpse);
									num++;
								}
							}
						}
						bool flag17 = corpse.InnerPawn.equipment != null;
						if (flag17)
						{
							List<ThingWithComps> allEquipmentListForReading2 = corpse.InnerPawn.equipment.AllEquipmentListForReading;
							for (int n = 0; n < allEquipmentListForReading2.Count<ThingWithComps>(); n++)
							{
								ThingWithComps thingWithComps5 = allEquipmentListForReading2[n];
								bool flag18 = thingWithComps5 != null && AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(thingWithComps5) && !AdvancedAI_GenerateUtility.allCorpseAmmo.ContainsKey(thingWithComps5) && AdvancedAI_GenerateUtility.FactionAllowed(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ThingisForbidden(corpse, CS$<>8__locals1.pawn, CS$<>8__locals1.checkForPlayerFaction) && CS$<>8__locals1.pawn.CanReserve(corpse, 1, -1, null, false) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, corpse) && CS$<>8__locals1.pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
								if (flag18)
								{
									AdvancedAI_GenerateUtility.allCorpseAmmo.Add(thingWithComps5, corpse);
									num++;
								}
							}
						}
					}
					else
					{
						bool flag19 = AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__ammoValidator|9_1(thing, ref CS$<>8__locals1) && !AdvancedAI_GenerateUtility.ItemInDangerousList(CS$<>8__locals1.pawn, mapComp, thing) && !AdvancedAI_GenerateUtility.ItemInExclusionList(CS$<>8__locals1.pawn, mapComp, thing);
						if (flag19)
						{
							AdvancedAI_GenerateUtility.allDroppedAmmo.Add(thing);
							num++;
						}
					}
				}
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0001EDC4 File Offset: 0x0001CFC4
		public static bool ItemInDangerousList(Pawn p, MapComponent_SkyAI mapComp, Thing item)
		{
			bool flag = p.IsColonist || p.Position.DistanceTo(item.Position) <= 2f;
			return !flag && mapComp.dangerousCells.Contains(item.Position);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0001EE18 File Offset: 0x0001D018
		public static bool ItemInExclusionList(Pawn p, MapComponent_SkyAI mapComp, Thing item)
		{
			bool isColonist = p.IsColonist;
			bool result;
			if (isColonist)
			{
				result = false;
			}
			else
			{
				bool flag = mapComp.boughtThings.ContainsKey(item);
				if (flag)
				{
					Faction faction;
					mapComp.boughtThings.TryGetValue(item, out faction);
					bool flag2 = faction != null && p.Faction != null && p.Faction == faction;
					if (flag2)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0001EE80 File Offset: 0x0001D080
		public static bool FactionAllowed(Thing t, Pawn pawn, bool checkForPlayerFaction)
		{
			bool flag = pawn.Faction == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = checkForPlayerFaction && pawn.Faction == Faction.OfPlayer;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = pawn.Faction.HostileTo(Faction.OfPlayer);
					if (flag3)
					{
						result = true;
					}
					else
					{
						bool flag4 = pawn.Map != null && !pawn.Map.areaManager.Home[t.PositionHeld];
						if (flag4)
						{
							result = true;
						}
						else
						{
							Pawn pawn2 = t as Pawn;
							bool flag5 = pawn2 != null && pawn2 != null && AdvancedAI.IsAlly(pawn2, pawn, false);
							if (flag5)
							{
								result = true;
							}
							else
							{
								Corpse corpse = t as Corpse;
								bool flag6 = corpse != null && corpse != null && AdvancedAI.IsAlly(corpse.InnerPawn, pawn, false);
								result = flag6;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000141 RID: 321 RVA: 0x0001EF68 File Offset: 0x0001D168
		public static bool ThingisForbidden(Thing t, Pawn pawn, bool checkForPlayerFaction)
		{
			bool flag = !checkForPlayerFaction || pawn.Faction == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.Faction != null && (!pawn.Faction.IsPlayer || !t.IsForbidden(pawn));
				result = !flag2;
			}
			return result;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x0001F024 File Offset: 0x0001D224
		[CompilerGenerated]
		internal static bool <GenerateWeaponLists>g__biocoded|4_0(Thing t)
		{
			CompBiocodable compBiocodable = t.TryGetComp<CompBiocodable>();
			return compBiocodable != null && compBiocodable.Biocoded;
		}

		// Token: 0x06000144 RID: 324 RVA: 0x0001F044 File Offset: 0x0001D244
		[CompilerGenerated]
		internal static float <GenerateWeaponLists>g__durability|4_1(Thing t)
		{
			return (float)t.HitPoints / (float)t.MaxHitPoints;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0001F055 File Offset: 0x0001D255
		[CompilerGenerated]
		internal static bool <GenerateWeaponLists>g__isWeapon|4_2(Thing t)
		{
			return t.def.IsWeapon && !AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__biocoded|4_0(t) && AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__durability|4_1(t) >= 0.3f;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0001F080 File Offset: 0x0001D280
		[CompilerGenerated]
		internal static bool <GenerateWeaponLists>g__weaponValidator|4_3(Thing t, ref AdvancedAI_GenerateUtility.<>c__DisplayClass4_0 A_1)
		{
			return AdvancedAI_GenerateUtility.<GenerateWeaponLists>g__isWeapon|4_2(t) && t.Position.Standable(A_1.pawn.Map) && AdvancedAI_GenerateUtility.FactionAllowed(t, A_1.pawn, A_1.checkForPlayerFaction) && !AdvancedAI_GenerateUtility.ThingisForbidden(t, A_1.pawn, A_1.checkForPlayerFaction) && A_1.pawn.CanReserve(t, 1, -1, null, false) && A_1.pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true, false, TraverseMode.ByPawn);
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0001F105 File Offset: 0x0001D305
		[CompilerGenerated]
		internal static bool <GenerateAmmoLists>g__isAmmo|9_0(Thing t)
		{
			return t.def.category == ThingCategory.Item && t is AmmoThing;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0001F124 File Offset: 0x0001D324
		[CompilerGenerated]
		internal static bool <GenerateAmmoLists>g__ammoValidator|9_1(Thing t, ref AdvancedAI_GenerateUtility.<>c__DisplayClass9_0 A_1)
		{
			return AdvancedAI_GenerateUtility.<GenerateAmmoLists>g__isAmmo|9_0(t) && t.Position.Standable(A_1.pawn.Map) && !AdvancedAI_GenerateUtility.ThingisForbidden(t, A_1.pawn, A_1.checkForPlayerFaction) && AdvancedAI_GenerateUtility.FactionAllowed(t, A_1.pawn, A_1.checkForPlayerFaction) && A_1.pawn.CanReserve(t, 1, -1, null, false) && A_1.pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true, false, TraverseMode.ByPawn);
		}

		// Token: 0x0400005D RID: 93
		public static List<Thing> allDroppedWeapons = new List<Thing>();

		// Token: 0x0400005E RID: 94
		public static IDictionary<ThingWithComps, Corpse> allCorpseWeapons = new Dictionary<ThingWithComps, Corpse>();

		// Token: 0x0400005F RID: 95
		public static IDictionary<ThingWithComps, Pawn> allDownedPawnWeapons = new Dictionary<ThingWithComps, Pawn>();

		// Token: 0x04000060 RID: 96
		public static IDictionary<ThingWithComps, Pawn> allCaravanPackWeapons = new Dictionary<ThingWithComps, Pawn>();

		// Token: 0x04000061 RID: 97
		public static List<Thing> allDroppedAmmo = new List<Thing>();

		// Token: 0x04000062 RID: 98
		public static IDictionary<ThingWithComps, Corpse> allCorpseAmmo = new Dictionary<ThingWithComps, Corpse>();

		// Token: 0x04000063 RID: 99
		public static IDictionary<ThingWithComps, Pawn> allDownedPawnAmmo = new Dictionary<ThingWithComps, Pawn>();

		// Token: 0x04000064 RID: 100
		public static IDictionary<ThingWithComps, Pawn> allCaravanPackAmmo = new Dictionary<ThingWithComps, Pawn>();
	}
}
