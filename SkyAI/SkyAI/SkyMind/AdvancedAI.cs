using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using SK;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200005A RID: 90
	public static class AdvancedAI
	{
		// Token: 0x0600020E RID: 526 RVA: 0x0002DEA0 File Offset: 0x0002C0A0
		public static bool PawnCloserToGoalPosition(Pawn pawn, IntVec3 goalPosition)
		{
			return pawn.Position.DistanceTo(goalPosition) < SkyAiCore.Settings.minDistanceToGoalPosition;
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0002DECC File Offset: 0x0002C0CC
		public static float MinDistance(Thing thing, Verb verb = null)
		{
			Pawn pawn = thing as Pawn;
			bool flag = pawn != null;
			float result;
			if (flag)
			{
				bool flag2 = verb == null;
				if (flag2)
				{
					verb = AdvancedAI.PrimaryVerb(pawn);
				}
				bool flag3 = verb == null || verb.verbProps == null;
				if (flag3)
				{
					result = 0f;
				}
				else
				{
					float ai_AvoidFriendlyFireRadius = verb.verbProps.ai_AvoidFriendlyFireRadius;
					bool flag4 = ai_AvoidFriendlyFireRadius <= 0f;
					if (flag4)
					{
						result = 0f;
					}
					else
					{
						result = SkyAiCore.Settings.extraFriendlyFireMinDistance + Mathf.Round(Mathf.Max(verb.verbProps.minRange, ai_AvoidFriendlyFireRadius));
					}
				}
			}
			else
			{
				Building_TurretGunCE building_TurretGunCE = thing as Building_TurretGunCE;
				bool flag5 = building_TurretGunCE != null && building_TurretGunCE.CurrentEffectiveVerb != null;
				if (flag5)
				{
					result = building_TurretGunCE.CurrentEffectiveVerb.verbProps.minRange;
				}
				else
				{
					Building_TurretGun building_TurretGun = thing as Building_TurretGun;
					bool flag6 = building_TurretGun != null && building_TurretGun.CurrentEffectiveVerb != null;
					if (flag6)
					{
						result = building_TurretGun.CurrentEffectiveVerb.verbProps.minRange;
					}
					else
					{
						IAttackTargetSearcher attackTargetSearcher = thing as IAttackTargetSearcher;
						bool flag7 = attackTargetSearcher != null;
						if (flag7)
						{
							Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
							bool flag8 = currentEffectiveVerb != null;
							if (flag8)
							{
								return currentEffectiveVerb.verbProps.minRange;
							}
						}
						result = 0f;
					}
				}
			}
			return result;
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0002E020 File Offset: 0x0002C220
		public static bool TryFindShootingPosition(Pawn pawn, Building building, Verb verb, out IntVec3 dest)
		{
			bool flag = verb == null;
			bool result;
			if (flag)
			{
				dest = IntVec3.Invalid;
				result = false;
			}
			else
			{
				float maxRangeFromTarget = AdvancedAI.EffectiveRange(pawn);
				result = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
				{
					caster = pawn,
					target = building,
					verb = verb,
					maxRegions = 9,
					maxRangeFromTarget = maxRangeFromTarget,
					wantCoverFromTarget = false
				}, out dest);
			}
			return result;
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0002E094 File Offset: 0x0002C294
		public static bool TryFindShootingPosition(Pawn pawn, Verb verb, bool coverRequired, float effectiveRange, out IntVec3 dest, out bool canHit)
		{
			Thing enemyTarget = pawn.mindState.enemyTarget;
			bool flag = verb == null || enemyTarget == null;
			bool result;
			if (flag)
			{
				dest = IntVec3.Invalid;
				canHit = false;
				result = false;
			}
			else
			{
				bool flag2 = AdvancedAI.CanHitFromTargetWithVerb(pawn, enemyTarget, verb);
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: TryFindShootingPosition. CanHitTargetFrom! target: {2} {3} No moving!", new object[]
						{
							pawn,
							pawn.Position,
							enemyTarget,
							enemyTarget.Position
						}));
					}
					dest = pawn.Position;
					canHit = true;
					result = true;
				}
				else
				{
					IntVec3 intVec;
					bool flag4 = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
					{
						caster = pawn,
						target = enemyTarget,
						verb = verb,
						maxRangeFromTarget = effectiveRange,
						wantCoverFromTarget = (!verb.IsMeleeAttack && verb.verbProps.range >= 5f)
					}, out intVec);
					dest = intVec;
					canHit = false;
					result = flag4;
				}
			}
			return result;
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0002E1C0 File Offset: 0x0002C3C0
		public static bool CanHitFromTargetWithVerb(Pawn pawn, Thing enemyTarget, Verb verb)
		{
			bool grenadeWeapon = AdvancedAI.PrimaryIsGrenade(pawn);
			bool flag = SkyAiCore.Settings.debugDetailTargetLog && SkyAiCore.SelectedPawnDebug(pawn);
			bool result;
			if (flag)
			{
				bool flag2 = AdvancedAI.CanHitTargetFrom(verb, pawn.Position, enemyTarget, grenadeWeapon);
				bool flag3 = GenSight.LineOfSight(pawn.Position, enemyTarget.Position, pawn.Map, false, null, 0, 0);
				bool flag4 = pawn.Position.DistanceTo(enemyTarget.Position) <= verb.verbProps.range;
				bool flag5 = pawn.Position.DistanceTo(enemyTarget.Position) >= AdvancedAI.MinDistance(pawn, verb);
				bool flag6 = flag2 && flag3 && flag4 && flag5;
				Log.Message(string.Format("{0} {1}: FightAI. Detail target log: {2} {3} CHTF: {4} LOS: {5} maxDist: {6} minDist: {7} result: {8}", new object[]
				{
					pawn,
					pawn.Position,
					enemyTarget,
					enemyTarget.Position,
					flag2,
					flag3,
					flag4,
					flag5,
					flag6
				}));
				result = flag6;
			}
			else
			{
				bool flag7 = AdvancedAI.CanHitTargetFrom(verb, pawn.Position, enemyTarget, grenadeWeapon) && GenSight.LineOfSight(pawn.Position, enemyTarget.Position, pawn.Map, false, null, 0, 0) && pawn.Position.DistanceTo(enemyTarget.Position) <= verb.verbProps.range && pawn.Position.DistanceTo(enemyTarget.Position) >= AdvancedAI.MinDistance(pawn, verb);
				result = flag7;
			}
			return result;
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0002E368 File Offset: 0x0002C568
		public static List<Building> PlayerBuildingsCanReachListForEnemyPawn(Pawn pawn, float radius = 40f, int maxListCount = 30)
		{
			List<Building> list = new List<Building>();
			Func<IntVec3, float> <>9__0;
			foreach (Pawn pawn2 in pawn.Map.mapPawns.FreeColonists.InRandomOrder(null))
			{
				IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(pawn2.Position, radius, false);
				Func<IntVec3, float> keySelector;
				if ((keySelector = <>9__0) == null)
				{
					keySelector = (<>9__0 = ((IntVec3 nearest) => pawn.Position.DistanceTo(nearest)));
				}
				IOrderedEnumerable<IntVec3> orderedEnumerable = source.OrderByDescending(keySelector);
				foreach (IntVec3 c in orderedEnumerable)
				{
					Building firstBuilding = c.GetFirstBuilding(pawn.Map);
					bool flag = firstBuilding != null && firstBuilding.Faction != null && firstBuilding.Faction == Faction.OfPlayer && !list.Contains(firstBuilding) && pawn.CanReach(firstBuilding, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
					if (flag)
					{
						list.Add(firstBuilding);
						bool flag2 = list.Count<Building>() >= maxListCount;
						if (flag2)
						{
							break;
						}
					}
				}
				bool flag3 = list.Count<Building>() >= maxListCount;
				if (flag3)
				{
					break;
				}
			}
			return list;
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0002E504 File Offset: 0x0002C704
		public static bool LightWeapon(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = thingWithComps != null;
				if (flag2)
				{
					bool flag3 = thingWithComps.GetStatValue(StatDefOf.Mass, true) <= SkyAiCore.Settings.minWeaponWeightForLightWeapon;
					if (flag3)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0002E55C File Offset: 0x0002C75C
		public static bool IsHumanlikeOnly(Pawn pawn)
		{
			return pawn.RaceProps.intelligence == Intelligence.Humanlike;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0002E57C File Offset: 0x0002C77C
		public static bool IsBioHumanlikeOnly(Pawn pawn)
		{
			bool flag = SK_Utility.isMechanical(pawn);
			return !flag && AdvancedAI.IsHumanlikeOnly(pawn) && pawn.RaceProps.FleshType == FleshTypeDefOf.Normal;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0002E5BC File Offset: 0x0002C7BC
		public static IEnumerable<Pawn> PawnsOfFactionOnMap(Pawn pawn)
		{
			bool flag = pawn.Map != null && pawn.Faction != null;
			IEnumerable<Pawn> result;
			if (flag)
			{
				result = from p in pawn.Map.mapPawns.AllPawnsSpawned
				where p.Faction != null && p.Faction == pawn.Faction
				select p;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0002E628 File Offset: 0x0002C828
		public static ThingWithComps PrimaryWeapon(Pawn pawn)
		{
			ThingWithComps result;
			if (pawn == null)
			{
				result = null;
			}
			else
			{
				Pawn_EquipmentTracker equipment = pawn.equipment;
				result = ((equipment != null) ? equipment.Primary : null);
			}
			return result;
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0002E654 File Offset: 0x0002C854
		public static ThingWithComps ShieldEquiped(Pawn pawn)
		{
			Pawn_EquipmentTracker pawn_EquipmentTracker = (pawn != null) ? pawn.equipment : null;
			bool flag = pawn_EquipmentTracker == null;
			ThingWithComps result;
			if (flag)
			{
				result = null;
			}
			else
			{
				List<ThingWithComps> allEquipmentListForReading = pawn_EquipmentTracker.AllEquipmentListForReading;
				bool flag2 = allEquipmentListForReading.NullOrEmpty<ThingWithComps>();
				if (flag2)
				{
					result = null;
				}
				else
				{
					ThingWithComps thingWithComps = (from ap in allEquipmentListForReading
					where ap is Apparel_Shield
					select ap).FirstOrDefault<ThingWithComps>();
					bool flag3 = thingWithComps == null;
					if (flag3)
					{
						result = null;
					}
					else
					{
						result = thingWithComps;
					}
				}
			}
			return result;
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0002E6D8 File Offset: 0x0002C8D8
		public static void RemoveFromExitList(Pawn pawn, MapComponent_SkyAI mapComp)
		{
			bool flag = mapComp != null && mapComp.exitCounter.ContainsKey(pawn);
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: Removed from exitList list.", pawn, pawn.Position));
				}
				mapComp.exitCounter.Remove(pawn);
			}
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0002E744 File Offset: 0x0002C944
		public static bool HasPrimaryLoadedRangedWeapon(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps != null && thingWithComps.def.IsRangedWeapon;
			if (flag)
			{
				CompAmmoUser compAmmoUser = thingWithComps.TryGetComp<CompAmmoUser>();
				bool flag2 = compAmmoUser != null && compAmmoUser.CurMagCount > 0;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600021C RID: 540 RVA: 0x0002E798 File Offset: 0x0002C998
		public static bool HasAmmoForWeaponInInventory(ThingWithComps weapon, CompInventory compInventory)
		{
			CompAmmoUser compAmmoUser = weapon.TryGetComp<CompAmmoUser>();
			bool flag = compAmmoUser == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = compAmmoUser.CurMagCount > 0;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = !compInventory.ammoList.NullOrEmpty<Thing>();
					if (flag3)
					{
						foreach (AmmoLink ammoLink in compAmmoUser.Props.ammoSet.ammoTypes)
						{
							bool flag4 = (from t in compInventory.ammoList
							select t.def).Contains(ammoLink.ammo);
							if (flag4)
							{
								return true;
							}
						}
					}
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0002E880 File Offset: 0x0002CA80
		public static bool IsSiegeWeapon(ThingWithComps weapon)
		{
			List<Verb> list = AdvancedAI.WeaponVerbs(weapon);
			bool flag = list != null;
			if (flag)
			{
				foreach (Verb verb in list)
				{
					bool ai_IsBuildingDestroyer = verb.verbProps.ai_IsBuildingDestroyer;
					if (ai_IsBuildingDestroyer)
					{
						return true;
					}
				}
			}
			bool result;
			if (weapon != null && !weapon.def.weaponTags.NullOrEmpty<string>())
			{
				result = weapon.def.weaponTags.Any((string tag) => tag == "CE_AI_Launcher" || tag == "CE_AI_Grenade");
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0002E950 File Offset: 0x0002CB50
		public static bool PrimaryIsSiegeWeapon(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps != null && AdvancedAI.IsSiegeWeapon(thingWithComps);
			if (flag)
			{
				bool flag2 = thingWithComps.TryGetComp<CompAmmoUser>() != null;
				if (!flag2)
				{
					return true;
				}
				CompInventory compInventory = pawn.TryGetComp<CompInventory>();
				bool flag3 = compInventory != null && AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
				if (flag3)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0002E9B8 File Offset: 0x0002CBB8
		public static bool PrimaryIsMachineGun(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool result;
			if (thingWithComps != null && !thingWithComps.def.thingCategories.NullOrEmpty<ThingCategoryDef>())
			{
				result = thingWithComps.def.thingCategories.Any((ThingCategoryDef tag) => tag == ThingCategoryDefOfAI.MachineGun);
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0002EA18 File Offset: 0x0002CC18
		public static bool PrimaryIsGrenade(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			return thingWithComps != null && AdvancedAI.IsGrenade(thingWithComps);
		}

		// Token: 0x06000221 RID: 545 RVA: 0x0002EA40 File Offset: 0x0002CC40
		public static ThingWithComps HasWeaponWithBetterRange(Pawn pawn, CompInventory compInventory)
		{
			IEnumerable<ThingWithComps> enumerable = from weapon in AdvancedAI.InventoryRangedWeaponList(compInventory)
			where weapon.TryGetComp<CompAmmoUser>() != null
			select weapon;
			bool flag = AdvancedAI.PrimaryWeapon(pawn) != null && AdvancedAI.PrimaryVerb(pawn) != null && !enumerable.EnumerableNullOrEmpty<ThingWithComps>();
			if (flag)
			{
				foreach (ThingWithComps thingWithComps in enumerable)
				{
					bool flag2 = AdvancedAI.WeaponVerbs(thingWithComps) != null;
					if (flag2)
					{
						foreach (Verb verb in AdvancedAI.WeaponVerbs(thingWithComps))
						{
							bool flag3 = verb.verbProps.range > AdvancedAI.PrimaryWeaponRange(pawn);
							if (flag3)
							{
								return thingWithComps;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0002EB50 File Offset: 0x0002CD50
		public static List<ThingWithComps> InventorySiegeWeaponList(CompInventory compInventory)
		{
			List<ThingWithComps> list = new List<ThingWithComps>();
			foreach (Thing thing in ((IEnumerable<Thing>)compInventory.container))
			{
				ThingWithComps thingWithComps = thing as ThingWithComps;
				bool flag = thingWithComps == null || !thingWithComps.def.IsWeapon;
				if (!flag)
				{
					bool flag2 = AdvancedAI.WeaponVerbs(thingWithComps) != null;
					if (flag2)
					{
						foreach (Verb verb in AdvancedAI.WeaponVerbs(thingWithComps))
						{
							bool flag3;
							if (!verb.verbProps.ai_IsBuildingDestroyer)
							{
								if (!thingWithComps.def.weaponTags.NullOrEmpty<string>())
								{
									flag3 = thingWithComps.def.weaponTags.Any((string tag) => tag == "CE_AI_Launcher" || tag == "CE_AI_Grenade");
								}
								else
								{
									flag3 = false;
								}
							}
							else
							{
								flag3 = true;
							}
							bool flag4 = flag3;
							if (flag4)
							{
								bool flag5 = thingWithComps.TryGetComp<CompAmmoUser>() != null && AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
								if (flag5)
								{
									list.Add(thingWithComps);
								}
								else
								{
									list.Add(thingWithComps);
								}
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0002ECD4 File Offset: 0x0002CED4
		public static List<ThingWithComps> InventoryRangedWeaponList(CompInventory compInventory)
		{
			List<ThingWithComps> list = new List<ThingWithComps>();
			ThingWithComps thingWithComps = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null);
			bool flag = thingWithComps != null;
			if (flag)
			{
				bool flag2 = AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
				if (flag2)
				{
					list.Add(thingWithComps);
				}
			}
			return list;
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0002ED3C File Offset: 0x0002CF3C
		public static List<ThingWithComps> InventoryMeleeWeaponList(CompInventory compInventory)
		{
			List<ThingWithComps> list = new List<ThingWithComps>();
			IEnumerable<ThingWithComps> enumerable = from thing in compInventory.meleeWeaponList
			where thing.def.IsMeleeWeapon
			select thing;
			bool flag = !enumerable.EnumerableNullOrEmpty<ThingWithComps>();
			if (flag)
			{
				foreach (ThingWithComps item in enumerable)
				{
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0002EDD4 File Offset: 0x0002CFD4
		public static bool DesireToGetRangedWeapon(Pawn pawn, ThingWithComps currentWeapon)
		{
			bool flag = !AdvancedAI.IsBioHumanlikeOnly(pawn) || pawn.IsColonist;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				ThingWithComps thingWithComps = currentWeapon ?? AdvancedAI.PrimaryWeapon(pawn);
				bool flag2 = pawn.Faction != null && pawn.Faction.def.techLevel >= TechLevel.Industrial;
				if (flag2)
				{
					bool flag3 = thingWithComps != null && AdvancedAI.<DesireToGetRangedWeapon>g__isNotCombatRangedWeapon|23_0(thingWithComps) && !AdvancedAI_TakeAndEquipUtility.HasRangedWeaponInInventory(pawn, true);
					if (flag3)
					{
						bool isGoodBreacher = pawn.kindDef.isGoodBreacher;
						if (isGoodBreacher)
						{
							return false;
						}
						bool flag4 = pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler);
						return !flag4 && (pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 4);
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0002EEFC File Offset: 0x0002D0FC
		public static bool HasPrimaryWeaponOrSwitchToWeaponFromInventory(Pawn pawn, bool switchToWeapon = true)
		{
			MapComponent_SkyAI mapComp = AdvancedAI_Classes.MapComp(pawn);
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			CompInventory compInventory = pawn.TryGetComp<CompInventory>();
			bool flag = compInventory == null;
			bool result;
			if (flag)
			{
				bool flag2 = thingWithComps != null;
				if (flag2)
				{
					AdvancedAI.RemoveFromExitList(pawn, mapComp);
					result = true;
				}
				else
				{
					result = false;
				}
			}
			else
			{
				bool flag3 = thingWithComps != null && AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
				if (flag3)
				{
					AdvancedAI.RemoveFromExitList(pawn, mapComp);
					result = true;
				}
				else
				{
					ThingWithComps thingWithComps2 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null);
					bool flag4 = thingWithComps2 != null;
					if (flag4)
					{
						bool flag5 = AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps2, compInventory);
						if (flag5)
						{
							ThingWithComps thingWithComps3 = AdvancedAI.ShieldEquiped(pawn);
							bool flag6 = thingWithComps3 != null;
							if (flag6)
							{
								ThingWithComps t;
								bool flag7 = pawn.equipment.TryDropEquipment(thingWithComps3, out t, pawn.Position, true);
								if (flag7)
								{
									Job job = JobMaker.MakeJob(JobDefOf.DropEquipment, t, 30, true);
									bool flag8 = !AdvancedAI.AlreadyHasSameJob(pawn, job.def);
									if (flag8)
									{
										pawn.jobs.EndCurrentJob(JobCondition.None, true, true);
										pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
										return true;
									}
								}
							}
							AdvancedAI.RemoveFromExitList(pawn, mapComp);
							bool flag9 = switchToWeapon && pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def.isIdle;
							if (flag9)
							{
								compInventory.TrySwitchToWeapon(thingWithComps2);
							}
							return true;
						}
						IEnumerable<ThingWithComps> enumerable = from thing in compInventory.meleeWeaponList
						where thing.def.IsMeleeWeapon
						select thing;
						bool flag10 = !enumerable.EnumerableNullOrEmpty<ThingWithComps>();
						if (flag10)
						{
							AdvancedAI.RemoveFromExitList(pawn, mapComp);
							if (switchToWeapon)
							{
								pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Equip, enumerable.RandomElement<ThingWithComps>()), JobCondition.None, null, false, true, null, null, false, false);
							}
							return true;
						}
					}
					ThingWithComps thingWithComps4 = AdvancedAI.GrenadeInInventory(compInventory);
					bool flag11 = thingWithComps == null && thingWithComps4 != null;
					if (flag11)
					{
						AdvancedAI.RemoveFromExitList(pawn, mapComp);
						compInventory.TrySwitchToWeapon(thingWithComps4);
						result = true;
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0002F158 File Offset: 0x0002D358
		public static int RaidWidth(int count)
		{
			return Mathf.RoundToInt(AdvancedAI.SiegeWidthByRaidCount.Evaluate((float)count));
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0002F17C File Offset: 0x0002D37C
		public static bool IsStunned(Pawn pawn)
		{
			return pawn.stances != null && pawn.stances.stunner != null && pawn.stances.stunner.Stunned;
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0002F1B8 File Offset: 0x0002D3B8
		public static bool IsSuitablePawn(Pawn pawn)
		{
			return pawn.Map != null && AdvancedAI.IsActivePawn(pawn) && !pawn.Fogged() && !AdvancedAI.IsStunned(pawn);
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0002F1F0 File Offset: 0x0002D3F0
		public static bool IsDangerousTarget(Thing thing, Map map)
		{
			CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
			bool flag = compExplosive != null;
			if (flag)
			{
				bool flag2 = thing.Position.ContainsStaticFire(map);
				if (flag2)
				{
					return true;
				}
			}
			List<IntVec3> list = GenAdjFast.AdjacentCells8Way(thing.Position);
			for (int i = 0; i < list.Count<IntVec3>(); i++)
			{
				bool flag3 = list[i].Standable(map) && list[i].ContainsStaticFire(map);
				if (flag3)
				{
					return true;
				}
				bool flag4 = AdvancedAI.IsDangerousCell(list[i], map);
				if (flag4)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0002F29C File Offset: 0x0002D49C
		public static bool AnyAllyNearby(Thing searcher, float radius)
		{
			bool result = false;
			foreach (IntVec3 c in GenRadial.RadialCellsAround(searcher.Position, radius, true))
			{
				bool flag = !c.InBounds(searcher.Map);
				if (!flag)
				{
					Pawn firstPawn = c.GetFirstPawn(searcher.Map);
					bool flag2 = firstPawn != null && AdvancedAI.IsAlly((Pawn)searcher, firstPawn, false);
					if (flag2)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0002F33C File Offset: 0x0002D53C
		public static bool IsAlly(Thing t, Thing t2, bool checkSelf)
		{
			bool flag = checkSelf && t == t2;
			return flag || (t != t2 && t.Faction != null && t2.Faction != null && (t.Faction == t2.Faction || !t.Faction.HostileTo(t2.Faction)));
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0002F3A0 File Offset: 0x0002D5A0
		public static bool IsHostile(Thing t, Thing t2)
		{
			bool flag = t == t2;
			return !flag && (t.Faction != null && t2.Faction != null) && t.Faction.HostileTo(t2.Faction);
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0002F3E3 File Offset: 0x0002D5E3
		public static IEnumerable<Pawn> RadialDistinctPawnsAround(IntVec3 center, Map map, float radius, bool useCenter)
		{
			int numCells = GenRadial.NumCellsInRadius(radius);
			HashSet<Pawn> returnedPawns = null;
			int num;
			for (int i = (!useCenter) ? 1 : 0; i < numCells; i = num + 1)
			{
				IntVec3 c = GenRadial.RadialPattern[i] + center;
				bool flag = !c.InBounds(map);
				if (!flag)
				{
					List<Thing> thingList = c.GetThingList(map);
					int j = 0;
					while (j < thingList.Count)
					{
						Pawn p = thingList[j] as Pawn;
						bool flag2 = p != null;
						if (!flag2)
						{
							goto IL_127;
						}
						bool flag3 = returnedPawns == null;
						if (flag3)
						{
							returnedPawns = new HashSet<Pawn>();
						}
						bool flag4 = returnedPawns.Contains(p);
						if (!flag4)
						{
							returnedPawns.Add(p);
							goto IL_127;
						}
						IL_14B:
						num = j;
						j = num + 1;
						continue;
						IL_127:
						yield return p;
						p = null;
						goto IL_14B;
					}
					c = default(IntVec3);
					thingList = null;
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0002F408 File Offset: 0x0002D608
		public static List<Pawn> AllyPawnsAroundForPawn(Pawn pawn, IntVec3 center, Map map, float radius, bool useCenter)
		{
			IEnumerable<Pawn> enumerable = AdvancedAI.RadialDistinctPawnsAround(center, map, radius, useCenter);
			bool flag = enumerable.EnumerableNullOrEmpty<Pawn>();
			List<Pawn> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				result = (from t in enumerable
				where t != null && AdvancedAI.IsAlly(pawn, t, false)
				select t).ToList<Pawn>();
			}
			return result;
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0002F458 File Offset: 0x0002D658
		public static bool AnyAllyPawnsAroundForPawn(Pawn pawn, IntVec3 center, Map map, float radius, bool useCenter)
		{
			List<Pawn> list = AdvancedAI.AllyPawnsAroundForPawn(pawn, center, map, radius, useCenter);
			return list != null && list.Count<Pawn>() > 0;
		}

		// Token: 0x06000231 RID: 561 RVA: 0x0002F488 File Offset: 0x0002D688
		public static bool IsDangerousCell(IntVec3 intVec, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAtFast(intVec);
			for (int i = 0; i < list.Count<Thing>(); i++)
			{
				ProjectileProperties projectile = list[i].def.projectile;
				bool flag = projectile != null && projectile.explosionRadius > 0f && list[i].def.category == ThingCategory.Projectile;
				if (flag)
				{
					ProjectileCE projectileCE = list[i] as ProjectileCE;
					bool flag2 = projectileCE != null;
					if (flag2)
					{
						LocalTargetInfo intendedTarget = projectileCE.intendedTarget;
						bool flag3 = intendedTarget != null;
						if (!flag3)
						{
							return true;
						}
						Vector3 vector = (projectileCE.ExactPosition - intendedTarget.CenterVector3).Abs();
						bool flag4 = vector.x * vector.z < 4f;
						if (flag4)
						{
							return true;
						}
					}
					else
					{
						Projectile projectile2 = list[i] as Projectile;
						bool flag5 = projectile2 != null;
						if (!flag5)
						{
							return true;
						}
						LocalTargetInfo intendedTarget2 = projectile2.intendedTarget;
						bool flag6 = intendedTarget2 != null;
						if (!flag6)
						{
							return true;
						}
						Vector3 vector2 = (projectile2.ExactPosition - intendedTarget2.CenterVector3).Abs();
						bool flag7 = vector2.x * vector2.z <= 3f;
						if (flag7)
						{
							return true;
						}
					}
				}
				CompExplosive compExplosive = list[i].TryGetComp<CompExplosive>();
				bool flag8 = compExplosive != null;
				if (flag8)
				{
					bool flag9 = intVec.ContainsStaticFire(map);
					if (flag9)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000232 RID: 562 RVA: 0x0002F64C File Offset: 0x0002D84C
		public static bool IsDangerousCellAround(IntVec3 intVec, Map map, float distance = 3f)
		{
			IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(intVec, distance, false);
			Func<IntVec3, bool> <>9__0;
			Func<IntVec3, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(map)));
			}
			foreach (IntVec3 intVec2 in source.Where(predicate))
			{
				bool flag = AdvancedAI.IsDangerousCell(intVec2, map);
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000233 RID: 563 RVA: 0x0002F6EC File Offset: 0x0002D8EC
		public static List<Region> PawnAndNeighborRegions(Pawn pawn)
		{
			Region region = pawn.Position.GetRegion(pawn.Map, RegionType.Set_Passable);
			List<Region> list = region.Neighbors.ToList<Region>();
			bool flag = region != null;
			if (flag)
			{
				list.Add(region);
			}
			return list;
		}

		// Token: 0x06000234 RID: 564 RVA: 0x0002F734 File Offset: 0x0002D934
		public static bool SaveDistanceToBuilding(Pawn pawn, IntVec3 source, Building building)
		{
			return source.DistanceTo(building.Position) >= AdvancedAI.MinDistance(pawn, null);
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0002F760 File Offset: 0x0002D960
		public static IntVec3 CellNearPosition(Pawn pawn, IntVec3 source, IntVec3 nearPosition)
		{
			IEnumerable<IntVec3> enumerable = from pass in GenAdjFast.AdjacentCells8Way(nearPosition)
			where pass.InBounds(pawn.Map) && !pass.Impassable(pawn.Map)
			select pass;
			bool flag = !enumerable.EnumerableNullOrEmpty<IntVec3>();
			IntVec3 result;
			if (flag)
			{
				bool flag2 = source == nearPosition;
				if (flag2)
				{
					result = enumerable.MinBy((IntVec3 closer) => pawn.Position.DistanceTo(closer));
				}
				else
				{
					result = enumerable.MinBy((IntVec3 closer) => source.DistanceTo(closer));
				}
			}
			else
			{
				result = nearPosition;
			}
			return result;
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0002F7EC File Offset: 0x0002D9EC
		public static IntVec3 CellNearPosition(Pawn pawn, IntVec3 source)
		{
			IEnumerable<IntVec3> enumerable = from cell in GenAdjFast.AdjacentCellsCardinal(source)
			where cell.InBounds(pawn.Map) && !source.Equals(cell) && !cell.ContainsStaticFire(pawn.Map) && AdvancedAI.IsFreeCell(cell, pawn.Map)
			select cell;
			bool flag = !enumerable.EnumerableNullOrEmpty<IntVec3>();
			IntVec3 result;
			if (flag)
			{
				result = enumerable.MinBy((IntVec3 closer) => pawn.Position.DistanceTo(closer));
			}
			else
			{
				result = IntVec3.Invalid;
			}
			return result;
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0002F858 File Offset: 0x0002DA58
		public static bool IsAcceptableTarget(Faction faction, Building building)
		{
			return building != null && faction != null && building.Faction != null && building.Faction.HostileTo(faction) && building.def.useHitPoints && building.def.building.isEdifice;
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0002F8A8 File Offset: 0x0002DAA8
		public static bool MainBlowTacticCanExec(Pawn pawn, Building target, int mainBLowRadius)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null && lord.ownedPawns.Count > 0;
			if (flag)
			{
				int count = lord.ownedPawns.Count;
				List<Building> list = AdvancedAI.ConnectedClosestBuildings(pawn, target, count * 2, false);
				bool flag2 = !list.NullOrEmpty<Building>();
				if (flag2)
				{
					int num = Mathf.Max(0, list.Count - mainBLowRadius * 2);
					bool flag3 = num > count;
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic && !flag3;
					if (flag4)
					{
						Log.Message(string.Format("{0} {1}: Can't exec main blow tactic with near target: {2}. Data: freeTargetsCount/Pcount: {3}/{4} result: {5}", new object[]
						{
							pawn,
							pawn.Position,
							target,
							num,
							count,
							flag3
						}));
					}
					return flag3;
				}
			}
			bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
			if (flag5)
			{
				Log.Message(string.Format("{0} {1}: Can't exec main blow tactic with near target: {2}.", pawn, pawn.Position, target));
			}
			return false;
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0002F9DC File Offset: 0x0002DBDC
		public static List<Building> ConnectedClosestBuildings(Pawn pawn, Building building, int max, bool closestCells)
		{
			AdvancedAI.<>c__DisplayClass44_0 CS$<>8__locals1 = new AdvancedAI.<>c__DisplayClass44_0();
			CS$<>8__locals1.pawn = pawn;
			List<Building> list = new List<Building>();
			bool flag = list.NullOrEmpty<Building>();
			if (flag)
			{
				list.Add(building);
			}
			bool flag2 = false;
			for (int i = max; i >= list.Count<Building>(); i--)
			{
				int num = 0;
				for (int j = 0; j < list.Count<Building>(); j++)
				{
					Building building2 = list[j];
					bool flag3 = building2 == null || building2.Destroyed;
					if (!flag3)
					{
						IEnumerable<IntVec3> enumerable;
						if (!closestCells)
						{
							IEnumerable<IntVec3> source = GenAdjFast.AdjacentCells8Way(building2);
							Func<IntVec3, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__2) == null)
							{
								predicate = (CS$<>8__locals1.<>9__2 = ((IntVec3 c) => c.InBounds(CS$<>8__locals1.pawn.Map)));
							}
							enumerable = source.Where(predicate);
						}
						else
						{
							IEnumerable<IntVec3> source2 = GenAdjFast.AdjacentCells8Way(building2);
							Func<IntVec3, bool> predicate2;
							if ((predicate2 = CS$<>8__locals1.<>9__0) == null)
							{
								predicate2 = (CS$<>8__locals1.<>9__0 = ((IntVec3 c) => c.InBounds(CS$<>8__locals1.pawn.Map)));
							}
							IEnumerable<IntVec3> source3 = source2.Where(predicate2);
							Func<IntVec3, float> keySelector;
							if ((keySelector = CS$<>8__locals1.<>9__1) == null)
							{
								keySelector = (CS$<>8__locals1.<>9__1 = ((IntVec3 c2) => CS$<>8__locals1.pawn.Position.DistanceTo(c2)));
							}
							IEnumerable<IntVec3> enumerable2 = source3.OrderBy(keySelector);
							enumerable = enumerable2;
						}
						IEnumerable<IntVec3> source4 = enumerable;
						for (int k = 0; k < source4.Count<IntVec3>(); k++)
						{
							AdvancedAI.<>c__DisplayClass44_1 CS$<>8__locals2;
							CS$<>8__locals2.b = source4.ElementAt(k).GetFirstBuilding(CS$<>8__locals1.pawn.Map);
							bool flag4 = AdvancedAI.IsAcceptableTarget(CS$<>8__locals1.pawn.Faction, CS$<>8__locals2.b) && !list.Contains(CS$<>8__locals2.b) && CS$<>8__locals1.<ConnectedClosestBuildings>g__shouldUse|3(CS$<>8__locals2.b, ref CS$<>8__locals2);
							if (flag4)
							{
								bool flag5 = list.Count<Building>() >= max;
								if (flag5)
								{
									break;
								}
								list.Add(CS$<>8__locals2.b);
								num++;
							}
						}
						bool flag6 = list.Count<Building>() >= max || num == 0;
						if (flag6)
						{
							flag2 = true;
							break;
						}
					}
				}
				bool flag7 = flag2;
				if (flag7)
				{
					break;
				}
			}
			return list;
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0002FBFC File Offset: 0x0002DDFC
		public static bool TryFindShootlineFromTo(IntVec3 source, LocalTargetInfo target, Verb verb)
		{
			Verb_LaunchProjectileCE verb_LaunchProjectileCE = verb as Verb_LaunchProjectileCE;
			ShootLine shootLine;
			return (verb_LaunchProjectileCE != null) ? verb_LaunchProjectileCE.TryFindCEShootLineFromTo(source, target, ref shootLine) : verb.TryFindShootLineFromTo(source, target, out shootLine);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0002FC30 File Offset: 0x0002DE30
		public static bool CanHitTargetFrom(Verb verb, IntVec3 source, LocalTargetInfo target, bool grenadeWeapon)
		{
			bool result;
			if (grenadeWeapon)
			{
				ShootLine shootLine;
				result = verb.TryFindShootLineFromTo(source, target, out shootLine);
			}
			else
			{
				result = (AdvancedAI.TryFindShootlineFromTo(source, target, verb) && source.DistanceTo(target.Cell) <= verb.verbProps.range);
			}
			return result;
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0002FC80 File Offset: 0x0002DE80
		public static bool CanHitBuilding(Pawn pawn, IntVec3 source, Building building, float externalMinDistance)
		{
			bool flag = true;
			bool flag2 = AdvancedAI.FriendlyFireThreat(pawn, source, building, AdvancedAI.MinDistance(pawn, null), externalMinDistance);
			if (flag2)
			{
				bool flag3 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag3)
				{
					pawn.Map.debugDrawer.FlashCell(building.Position, 0.4f, "CHB", SkyAiCore.Settings.flashCellDelay);
				}
				flag = false;
			}
			bool flag4 = flag && AdvancedAI.PrimaryIsSiegeWeapon(pawn);
			if (flag4)
			{
				int num = 0;
				List<IntVec3> list = AdvancedAI.CellsBetweenPositions(source, building.Position, pawn.Map, true, 0, 0, 1, 0f);
				foreach (IntVec3 c in list)
				{
					bool flag5 = !c.InBounds(pawn.Map);
					if (!flag5)
					{
						Building firstBuilding = c.GetFirstBuilding(pawn.Map);
						bool flag6 = firstBuilding != null && firstBuilding.def.passability == Traversability.Impassable && (double)firstBuilding.def.pathCost <= 0.99 && firstBuilding.def.building.isInert && !firstBuilding.def.building.ai_chillDestination;
						if (flag6)
						{
							num++;
						}
						bool flag7 = num >= 2;
						if (flag7)
						{
							break;
						}
					}
				}
				bool flag8 = num >= 2;
				if (flag8)
				{
					flag = false;
				}
			}
			bool flag9 = flag;
			if (flag9)
			{
				bool flag10 = !pawn.CanReach(building, PathEndMode.Touch, Danger.Some, false, false, TraverseMode.ByPawn);
				if (flag10)
				{
					flag = false;
				}
			}
			return flag;
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0002FE4C File Offset: 0x0002E04C
		public static bool FriendlyFireThreat(Thing searcher, IntVec3 source, Thing target, float distance, float externalMinDistance)
		{
			bool flag = searcher.Faction == null || distance <= 0f;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				Pawn pawn = searcher as Pawn;
				bool flag3 = pawn != null;
				if (flag3)
				{
					IntVec3 b = AdvancedAI.CellNearPosition(pawn, source, target.Position);
					foreach (IntVec3 intVec in GenRadial.RadialCellsAround(target.Position, distance + externalMinDistance, true))
					{
						bool flag4 = !intVec.InBounds(pawn.Map);
						if (!flag4)
						{
							foreach (Thing thing in pawn.Map.thingGrid.ThingsListAtFast(intVec))
							{
								Pawn pawn2 = thing as Pawn;
								bool flag5 = pawn2 != null && AdvancedAI.IsAlly(pawn, pawn2, true);
								if (flag5)
								{
									bool flag6 = !intVec.WithinRegions(b, pawn.Map, 3, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), RegionType.Set_Passable);
									if (flag6)
									{
										bool flag7 = pawn2.Position.DistanceTo(target.Position) < Mathf.Round((distance + externalMinDistance) * 0.5f);
										if (flag7)
										{
											bool flag8 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag8)
											{
												pawn.Map.debugDrawer.FlashCell(pawn2.Position, 0.4f, "FF1", SkyAiCore.Settings.flashCellDelay);
											}
											flag2 = true;
										}
									}
									else
									{
										bool flag9 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag9)
										{
											pawn.Map.debugDrawer.FlashCell(pawn2.Position, 0.4f, "FF2", SkyAiCore.Settings.flashCellDelay);
										}
										flag2 = true;
									}
								}
							}
							bool flag10 = flag2;
							if (flag10)
							{
								break;
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0003009C File Offset: 0x0002E29C
		public static bool IsTurret(Building turret)
		{
			bool result;
			if (turret.def.tradeTags != null && !turret.def.tradeTags.NullOrEmpty<string>())
			{
				result = turret.def.tradeTags.Any((string str) => str == "CE_Turret");
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00030100 File Offset: 0x0002E300
		public static bool IsGrenade(Thing thing)
		{
			bool result;
			if (thing != null && thing.def.weaponTags != null && !thing.def.weaponTags.NullOrEmpty<string>())
			{
				result = thing.def.weaponTags.Any((string str) => str == "CE_AI_Grenade");
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00030168 File Offset: 0x0002E368
		public static bool CanSetFireOnTarget(Pawn pawn, Thing target)
		{
			bool flag;
			return AdvancedAI.PrimaryVerbIsIncendiary(pawn, out flag) && target.FlammableNow;
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00030190 File Offset: 0x0002E390
		public static bool PrimaryVerbIsIncendiary(Pawn pawn, out bool nonDestructiveAmmo)
		{
			nonDestructiveAmmo = false;
			bool useIncendiaryWeaponCheck = SkyAiCore.Settings.useIncendiaryWeaponCheck;
			if (useIncendiaryWeaponCheck)
			{
				ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
				bool flag = thingWithComps != null;
				if (flag)
				{
					CompAmmoUser compAmmoUser = thingWithComps.TryGetComp<CompAmmoUser>();
					bool flag2 = compAmmoUser != null;
					if (flag2)
					{
						ThingDef curAmmoProjectile = compAmmoUser.CurAmmoProjectile;
						bool flag3 = curAmmoProjectile != null && curAmmoProjectile.projectile != null;
						if (flag3)
						{
							bool ai_IsIncendiary = curAmmoProjectile.projectile.ai_IsIncendiary;
							if (ai_IsIncendiary)
							{
								return true;
							}
							bool flag4 = curAmmoProjectile.projectile.damageDef == DamageDefOf.EMP || curAmmoProjectile.projectile.damageDef == DamageDefOf.Extinguish;
							if (flag4)
							{
								nonDestructiveAmmo = true;
							}
						}
					}
					else
					{
						List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
						for (int i = 0; i < allVerbs.Count; i++)
						{
							bool isPrimary = allVerbs[i].verbProps.isPrimary;
							if (isPrimary)
							{
								return allVerbs[i].IsIncendiary();
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000242 RID: 578 RVA: 0x000302B8 File Offset: 0x0002E4B8
		public static List<Verb> WeaponVerbs(ThingWithComps thingWithComps)
		{
			return (from r in thingWithComps.GetComp<CompEquippable>().AllVerbs
			where r.verbProps != null
			select r).ToList<Verb>();
		}

		// Token: 0x06000243 RID: 579 RVA: 0x00030300 File Offset: 0x0002E500
		public static List<IntVec3> CellReservedForMainBlow(Pawn pawn)
		{
			bool flag = AdvancedAI_Classes.MapComp(pawn) == null;
			List<IntVec3> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				result = AdvancedAI_Classes.MapComp(pawn).mainBlowCells;
			}
			return result;
		}

		// Token: 0x06000244 RID: 580 RVA: 0x00030330 File Offset: 0x0002E530
		public static bool GetFriendlyFireType(Pawn pawn)
		{
			bool result;
			switch (SkyAiCore.Settings.avoidFriendlyFireType)
			{
			case Settings.FriendlyFireType.Disabled:
				result = false;
				break;
			case Settings.FriendlyFireType.SiegeWeaponOnly:
			{
				ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
				bool flag = SkyAiCore.Settings.autoSwitchToAllRangedWeapons && thingWithComps != null;
				if (flag)
				{
					MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
					bool flag2 = mapComponent_SkyAI != null && mapComponent_SkyAI.mapRangedPawnCount <= 60;
					if (flag2)
					{
						result = (thingWithComps.def.IsRangedWeapon && !AdvancedAI.IsGrenade(thingWithComps));
						break;
					}
				}
				result = (thingWithComps != null && AdvancedAI.PrimaryIsSiegeWeapon(pawn) && !AdvancedAI.IsGrenade(thingWithComps));
				break;
			}
			case Settings.FriendlyFireType.SiegeAndMachineGunsOnly:
			{
				ThingWithComps thingWithComps2 = AdvancedAI.PrimaryWeapon(pawn);
				bool flag3 = SkyAiCore.Settings.autoSwitchToAllRangedWeapons && thingWithComps2 != null;
				if (flag3)
				{
					MapComponent_SkyAI mapComponent_SkyAI2 = AdvancedAI_Classes.MapComp(pawn);
					bool flag4 = mapComponent_SkyAI2 != null && mapComponent_SkyAI2.mapRangedPawnCount <= 60;
					if (flag4)
					{
						result = (thingWithComps2.def.IsRangedWeapon && !AdvancedAI.IsGrenade(thingWithComps2));
						break;
					}
				}
				result = (thingWithComps2 != null && !AdvancedAI.IsGrenade(thingWithComps2) && (AdvancedAI.PrimaryIsSiegeWeapon(pawn) || AdvancedAI.PrimaryIsMachineGun(pawn)));
				break;
			}
			case Settings.FriendlyFireType.AllRangedWeapon:
			{
				ThingWithComps thingWithComps3 = AdvancedAI.PrimaryWeapon(pawn);
				result = (thingWithComps3 != null && thingWithComps3.def.IsRangedWeapon && !AdvancedAI.IsGrenade(thingWithComps3));
				break;
			}
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06000245 RID: 581 RVA: 0x000304B8 File Offset: 0x0002E6B8
		public static bool FriendlyFireThreatInShootline(Thing searcher, IntVec3 start, IntVec3 end, float maxObstacles, float maxDistance = 60f, float maxDistanceWithAdjacentCells = 55f, float startFromDist = 3f)
		{
			bool flag = searcher.Faction == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				Pawn pawn = searcher as Pawn;
				bool flag3 = pawn != null;
				if (flag3)
				{
					ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
					bool flag4 = thingWithComps != null && AdvancedAI.IsGrenade(thingWithComps);
					if (flag4)
					{
						return false;
					}
					float num = AdvancedAI.PrimaryWeaponRange(pawn);
					float num2 = (num < SkyAiCore.Settings.combatKeepRange) ? num : maxDistance;
					bool flag5 = AdvancedAI.GetFriendlyFireType(pawn) && start.DistanceTo(end) <= num2 && GenSight.LineOfSight(start, end, pawn.Map, false, null, 0, 0);
					if (flag5)
					{
						List<IntVec3> list = AdvancedAI.CellsBetweenPositions(start, end, pawn.Map, true, 0, 0, 1, 0f);
						int num3 = 0;
						float num4 = 0f;
						bool flag6 = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
						bool flag7 = flag6 || AdvancedAI.PrimaryIsMachineGun(pawn);
						using (List<IntVec3>.Enumerator enumerator = list.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								IntVec3 cell = enumerator.Current;
								bool flag8 = !cell.InBounds(pawn.Map);
								if (!flag8)
								{
									bool flag9 = pawn.Position.DistanceTo(cell) < startFromDist;
									if (!flag9)
									{
										num3++;
										List<Thing> thingList = cell.GetThingList(pawn.Map);
										Pawn pawn2 = null;
										bool flag10 = cell.DistanceTo(end) >= 2f;
										if (flag10)
										{
											for (int i = 0; i < thingList.Count<Thing>(); i++)
											{
												pawn2 = (thingList[i] as Pawn);
												bool flag11 = flag6;
												if (flag11)
												{
													bool flag12 = pawn2 != null && AdvancedAI.IsAlly(pawn, pawn2, false);
													if (flag12)
													{
														num4 += 1f;
														bool flag13 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag13)
														{
															Log.Message(string.Format("{0} {1}: Get obstacle: {2}. Count: {3}/{4}", new object[]
															{
																pawn,
																pawn.Position,
																thingList[i],
																num4,
																maxObstacles
															}));
														}
													}
													else
													{
														num4 = thingList[i].def.fillPercent;
														bool flag14 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag14)
														{
															Log.Message(string.Format("{0} {1}: Get obstacle: {2}. Count: {3}/{4}", new object[]
															{
																pawn,
																pawn.Position,
																thingList[i],
																num4,
																maxObstacles
															}));
														}
													}
												}
											}
										}
										bool flag15 = maxObstacles > 0f && num4 >= maxObstacles;
										if (flag15)
										{
											bool flag16 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag16)
											{
												Log.Message(string.Format("{0} {1}: FriendlyFireThreatInShootline cell: {2} - true. Failed to attack bcs max obstacles. Obstactle count: {3}/{4}", new object[]
												{
													pawn,
													pawn.Position,
													cell,
													num4,
													maxObstacles
												}));
											}
											flag2 = true;
											break;
										}
										bool flag17 = pawn2 != null && AdvancedAI.IsAlly(pawn, pawn2, false);
										if (flag17)
										{
											bool flag18 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag18)
											{
												Log.Message(string.Format("{0} {1}: FriendlyFireThreatInShootline cell: {2} - true. Failed to attack bcs of ally in shootline. Obstactle count: {3}/{4}", new object[]
												{
													pawn,
													pawn.Position,
													cell,
													num4,
													maxObstacles
												}));
											}
											flag2 = true;
											break;
										}
										bool flag19 = (float)num3 <= maxDistanceWithAdjacentCells && flag7;
										if (flag19)
										{
											List<IntVec3> list2 = GenAdjFast.AdjacentCells8Way(cell);
											IEnumerable<IntVec3> source = list2;
											Func<IntVec3, bool> predicate;
											Func<IntVec3, bool> <>9__0;
											if ((predicate = <>9__0) == null)
											{
												predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(pawn.Map) && c != cell));
											}
											foreach (IntVec3 c2 in source.Where(predicate))
											{
												Pawn firstPawn = c2.GetFirstPawn(pawn.Map);
												bool flag20 = firstPawn != null && AdvancedAI.IsAlly(pawn, firstPawn, false);
												if (flag20)
												{
													bool flag21 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag21)
													{
														Log.Message(string.Format("{0} {1}: FriendlyFireThreatInShootline cell: {2} - true. Failed to attack bcs of ally in shootline in adjacent cells. Obstactle count: {3}/{4}", new object[]
														{
															pawn,
															pawn.Position,
															cell,
															num4,
															maxObstacles
														}));
													}
													flag2 = true;
													break;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x06000246 RID: 582 RVA: 0x00030B40 File Offset: 0x0002ED40
		public static List<IntVec3> CellsBetweenPositions(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, int halfXOffset = 0, int halfZOffset = 0, int step = 1, float maxDist = 0f)
		{
			List<IntVec3> list = new List<IntVec3>();
			int num = 0;
			bool flag = !start.InBounds(map) || !end.InBounds(map);
			List<IntVec3> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = start.x == end.x;
				bool flag3;
				if (flag2)
				{
					flag3 = (start.z < end.z);
				}
				else
				{
					flag3 = (start.x < end.x);
				}
				int num2 = Mathf.Abs(end.x - start.x);
				int num3 = Mathf.Abs(end.z - start.z);
				int num4 = start.x;
				int num5 = start.z;
				int i = 1 + num2 + num3;
				int num6 = (end.x > start.x) ? 1 : -1;
				int num7 = (end.z > start.z) ? 1 : -1;
				num2 *= 4;
				num3 *= 4;
				num2 += halfXOffset * 2;
				num3 += halfZOffset * 2;
				int num8 = num2 / 2 - num3 / 2;
				IntVec3 intVec = default(IntVec3);
				while (i > 1)
				{
					intVec.x = num4;
					intVec.z = num5;
					bool flag4 = !skipFirstCell || !(intVec == start);
					if (flag4)
					{
						num++;
						bool flag5 = num == step;
						if (flag5)
						{
							bool flag6 = intVec.InBounds(map);
							if (flag6)
							{
								bool flag7 = maxDist > 0f;
								if (flag7)
								{
									bool flag8 = start.DistanceTo(intVec) > maxDist;
									if (flag8)
									{
										break;
									}
								}
								list.Add(intVec);
							}
							num = 0;
						}
					}
					bool flag9 = num8 > 0 || (num8 == 0 && flag3);
					if (flag9)
					{
						num4 += num6;
						num8 -= num3;
					}
					else
					{
						num5 += num7;
						num8 += num2;
					}
					i--;
				}
				result = list;
			}
			return result;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x00030D1C File Offset: 0x0002EF1C
		public static void AddCellsToMainBlow(Pawn pawn, IntVec3 targetPosition, IntVec3 enemyTarget, float radius, bool useCellsBetween)
		{
			Building firstBuilding = targetPosition.GetFirstBuilding(pawn.Map);
			bool flag = firstBuilding == null;
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: Mainblow tactic building target position null: {2} failed.", pawn, pawn.Position, firstBuilding));
				}
			}
			else
			{
				bool flag3 = !AdvancedAI.MainBlowTacticCanExec(pawn, firstBuilding, Mathf.RoundToInt(radius));
				if (!flag3)
				{
					bool flag4 = AdvancedAI_Classes.MapComp(pawn).mainBlowCells.NullOrEmpty<IntVec3>();
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: Added position: {2} as a mainblow tactic siege attack.", pawn, pawn.Position, firstBuilding));
						}
						List<IntVec3> list = new List<IntVec3>();
						bool isValid = enemyTarget.IsValid;
						if (isValid)
						{
							int num = 0;
							bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
							if (flag6)
							{
								Log.Message(string.Format("{0} {1}: MainBlow target duty found: {2}", pawn, pawn.Position, enemyTarget));
							}
							if (useCellsBetween)
							{
								List<IntVec3> list2 = AdvancedAI.CellsBetweenPositions(firstBuilding.Position, enemyTarget, pawn.Map, false, 0, 0, 1, 0f);
								bool flag7 = list2.Count > 17;
								if (flag7)
								{
									bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
									if (flag8)
									{
										Log.Message(string.Format("{0} {1}: MainBlow cellsBetween count: {2}. More then 17, return.", pawn, pawn.Position, list2.Count));
									}
									return;
								}
								bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
								if (flag9)
								{
									Log.Message(string.Format("{0} {1}: MainBlow cellsBetween count: {2}", pawn, pawn.Position, list2.Count<IntVec3>()));
								}
								bool flag10 = !list2.NullOrEmpty<IntVec3>();
								if (flag10)
								{
									foreach (IntVec3 item in list2)
									{
										num++;
										bool flag11 = (float)num > radius;
										if (flag11)
										{
											list.Add(item);
											num = 0;
										}
									}
								}
							}
						}
						List<IntVec3> enemyCellsInside = AdvancedAI.EnemyCellsInside(pawn);
						IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(firstBuilding.Position, radius, true);
						Func<IntVec3, bool> predicate;
						Func<IntVec3, bool> <>9__0;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((IntVec3 c) => !enemyCellsInside.Contains(c)));
						}
						foreach (IntVec3 intVec in source.Where(predicate))
						{
							bool flag12 = SkyAiCore.Settings.debugTargets && SkyAiCore.Settings.enableMainBlowSiegeTactic;
							if (flag12)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.17f, "X", 15000);
							}
							AdvancedAI_Classes.MapComp(pawn).mainBlowCells.Add(intVec);
						}
						bool flag13 = !list.NullOrEmpty<IntVec3>() && list.Count > 0;
						if (flag13)
						{
							float num2 = 0f;
							foreach (IntVec3 center in list)
							{
								num2 += 1f;
								foreach (IntVec3 intVec2 in GenRadial.RadialCellsAround(center, Mathf.Clamp(radius - num2, 2f, radius), true))
								{
									bool flag14 = !AdvancedAI_Classes.MapComp(pawn).mainBlowCells.Contains(intVec2) && !enemyCellsInside.Contains(intVec2);
									if (flag14)
									{
										bool flag15 = SkyAiCore.Settings.debugTargets && SkyAiCore.Settings.enableMainBlowSiegeTactic;
										if (flag15)
										{
											pawn.Map.debugDrawer.FlashCell(intVec2, 0.17f, "X", 15000);
										}
										AdvancedAI_Classes.MapComp(pawn).mainBlowCells.Add(intVec2);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000248 RID: 584 RVA: 0x000311C4 File Offset: 0x0002F3C4
		public static List<IntVec3> EnemyCellsInside(Pawn pawn)
		{
			List<IntVec3> list = new List<IntVec3>();
			List<Pawn> freeColonists = pawn.Map.mapPawns.FreeColonists;
			foreach (Pawn thing in freeColonists)
			{
				Region region = thing.GetRegion(RegionType.Set_Passable);
				bool flag = region != null;
				if (flag)
				{
					foreach (IntVec3 item in region.Cells)
					{
						list.Add(item);
					}
				}
				Room room = thing.GetRoom(RegionType.Set_Passable);
				bool flag2 = room != null;
				if (flag2)
				{
					foreach (IntVec3 item2 in room.Cells)
					{
						bool flag3 = !list.Contains(item2);
						if (flag3)
						{
							list.Add(item2);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000249 RID: 585 RVA: 0x00031308 File Offset: 0x0002F508
		public static Building MeleeTrashBuilding(IntVec3 fromSource, Pawn pawn, float inDistance, float minDistFromSource)
		{
			Building result = null;
			IOrderedEnumerable<Building> orderedEnumerable = from cell in GenRadial.RadialCellsAround(fromSource, inDistance, true)
			where cell.InBounds(pawn.Map) && cell.DistanceTo(fromSource) >= minDistFromSource && !AdvancedAI.CellReservedForMainBlow(pawn).Contains(cell)
			select cell into @select
			select @select.GetFirstBuilding(pawn.Map) into building
			where AdvancedAI.IsAcceptableTarget(pawn.Faction, building)
			select building into bd
			orderby pawn.Position.DistanceTo(bd.Position)
			select bd;
			bool flag = !orderedEnumerable.EnumerableNullOrEmpty<Building>();
			if (flag)
			{
				AdvancedAI.<>c__DisplayClass60_1 CS$<>8__locals2;
				CS$<>8__locals2.mapComp = AdvancedAI_Classes.MapComp(pawn);
				foreach (Building building2 in orderedEnumerable)
				{
					bool flag2 = !AdvancedAI.DangerousNonLOSTarget(pawn, building2, 8f) && !AdvancedAI.<MeleeTrashBuilding>g__inDangerousCells|60_4(building2.Position, ref CS$<>8__locals2) && pawn.CanReserveAndReach(building2, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false);
					if (flag2)
					{
						result = building2;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0003143C File Offset: 0x0002F63C
		public static bool TryFindDirectFleeDestination(IntVec3 root, float dist, Pawn pawn, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				result = root + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * dist);
				bool flag = result.Walkable(pawn.Map) && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, pawn.Map, true, null, 0, 0) && !AdvancedAI.InDangerousCellList(pawn, result);
				if (flag)
				{
					return true;
				}
			}
			Region region = pawn.GetRegion(RegionType.Set_Passable);
			Func<IntVec3, bool> <>9__0;
			for (int j = 0; j < 30; j++)
			{
				IEnumerable<IntVec3> cells = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), null, null, RegionType.Set_Passable).Cells;
				Func<IntVec3, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((IntVec3 c) => !AdvancedAI.InDangerousCellList(pawn, c)));
				}
				IEnumerable<IntVec3> enumerable = cells.Where(predicate);
				bool flag2 = !enumerable.EnumerableNullOrEmpty<IntVec3>();
				if (flag2)
				{
					IntVec3 intVec = enumerable.RandomElement<IntVec3>();
					bool flag3 = intVec.Walkable(pawn.Map) && (float)(root - intVec).LengthHorizontalSquared > dist * dist && !AdvancedAI.InDangerousCellList(pawn, root);
					if (flag3)
					{
						using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, pawn, PathEndMode.OnCell, null))
						{
							bool flag4 = PawnPathUtility.TryFindCellAtIndex(pawnPath, (int)dist + 3, out result);
							if (flag4)
							{
								return true;
							}
						}
					}
				}
			}
			result = pawn.Position;
			return false;
		}

		// Token: 0x0600024B RID: 587 RVA: 0x00031674 File Offset: 0x0002F874
		public static Verb PrimaryVerb(Thing searcher)
		{
			Pawn pawn = searcher as Pawn;
			bool flag = pawn != null;
			Verb result;
			if (flag)
			{
				Building_Turret building_Turret = pawn.MannedThing() as Building_Turret;
				bool flag2 = building_Turret != null;
				if (flag2)
				{
					result = building_Turret.AttackVerb;
				}
				else
				{
					Verb verb;
					if (pawn == null)
					{
						verb = null;
					}
					else
					{
						Pawn_EquipmentTracker equipment = pawn.equipment;
						if (equipment == null)
						{
							verb = null;
						}
						else
						{
							CompEquippable primaryEq = equipment.PrimaryEq;
							verb = ((primaryEq != null) ? primaryEq.PrimaryVerb : null);
						}
					}
					Verb verb2 = verb;
					bool flag3 = verb2 == null;
					if (flag3)
					{
						verb2 = pawn.TryGetAttackVerb(null, !pawn.IsColonist);
					}
					result = verb2;
				}
			}
			else
			{
				Building_TurretGunCE building_TurretGunCE = searcher as Building_TurretGunCE;
				bool flag4 = building_TurretGunCE != null && building_TurretGunCE.CurrentEffectiveVerb != null;
				if (flag4)
				{
					result = building_TurretGunCE.CurrentEffectiveVerb;
				}
				else
				{
					Building_TurretGun building_TurretGun = searcher as Building_TurretGun;
					bool flag5 = building_TurretGun != null && building_TurretGun.CurrentEffectiveVerb != null;
					if (flag5)
					{
						result = building_TurretGun.CurrentEffectiveVerb;
					}
					else
					{
						IAttackTargetSearcher attackTargetSearcher = searcher as IAttackTargetSearcher;
						bool flag6 = attackTargetSearcher != null;
						if (flag6)
						{
							Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
							bool flag7 = currentEffectiveVerb != null;
							if (flag7)
							{
								return currentEffectiveVerb;
							}
						}
						result = null;
					}
				}
			}
			return result;
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0003178C File Offset: 0x0002F98C
		public static bool IsGoodTarget(Pawn pawn, IntVec3 source, Building buildingTarget, Verb verb, bool grenadeWeapon)
		{
			bool flag = verb == null;
			if (flag)
			{
				verb = AdvancedAI.PrimaryVerb(pawn);
			}
			bool flag2 = SkyAiCore.Settings.debugDetailTargetLog && SkyAiCore.SelectedPawnDebug(pawn);
			bool result;
			if (flag2)
			{
				bool flag3 = AdvancedAI.SaveDistanceToBuilding(pawn, source, buildingTarget);
				bool flag4 = AdvancedAI.CanHitTargetFrom(verb, source, buildingTarget, grenadeWeapon);
				bool flag5 = AdvancedAI.CanHitBuilding(pawn, source, buildingTarget, 0f);
				bool flag6 = !AdvancedAI.FriendlyFireThreatInShootline(pawn, source, buildingTarget.Position, 2f, 60f, 55f, 3f);
				bool flag7 = pawn.CanReserve(buildingTarget, 1, -1, null, false);
				bool flag8 = source.DistanceTo(buildingTarget.Position) <= verb.verbProps.range;
				bool flag9 = AdvancedAI.TryFindShootlineFromTo(source, buildingTarget, verb);
				bool flag10 = flag8 && flag3 && flag4 && flag5 && flag6 && flag7;
				Log.Message(string.Format("{0} {1}: from pos: {2} Verb: {3} Check target: {4} {5} saveDistance: {6} canHitTargetFrom/shootline: {7}/{8} canHitBuilding: {9} friendlyFireShootline: {10} Reserve: {11} Is good target: {12}", new object[]
				{
					pawn,
					pawn.Position,
					source,
					verb,
					buildingTarget,
					buildingTarget.Position,
					flag3,
					flag4,
					flag9,
					flag5,
					flag6,
					flag7,
					flag10
				}));
				result = flag10;
			}
			else
			{
				bool flag11 = AdvancedAI.SaveDistanceToBuilding(pawn, source, buildingTarget) && source.DistanceTo(buildingTarget.Position) <= verb.verbProps.range && AdvancedAI.CanHitTargetFrom(verb, source, buildingTarget, grenadeWeapon) && !AdvancedAI.FriendlyFireThreatInShootline(pawn, source, buildingTarget.Position, 2f, 60f, 55f, 3f) && AdvancedAI.CanHitBuilding(pawn, source, buildingTarget, 0f) && pawn.CanReserve(buildingTarget, 1, -1, null, false);
				result = flag11;
			}
			return result;
		}

		// Token: 0x0600024D RID: 589 RVA: 0x0003198C File Offset: 0x0002FB8C
		public static bool ShouldInterest(Pawn source, Building target)
		{
			float num = 1f;
			bool flag = target.OccupiedRect().Count<IntVec3>() == 1 && source.Map != null;
			if (flag)
			{
				bool flag2 = source.Map.IsPlayerHome && !source.Map.areaManager.Home[target.Position];
				if (flag2)
				{
					num -= 0.5f;
				}
				IEnumerable<IntVec3> adjacentCellsCardinal = target.OccupiedRect().AdjacentCellsCardinal;
				IEnumerable<IntVec3> source2 = adjacentCellsCardinal;
				Func<IntVec3, bool> <>9__0;
				Func<IntVec3, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(source.Map)));
				}
				foreach (IntVec3 c3 in source2.Where(predicate))
				{
					Building firstBuilding = c3.GetFirstBuilding(source.Map);
					bool flag3 = (firstBuilding != null) ? (!firstBuilding.IsBarrier()) : c3.Walkable(source.Map);
					if (flag3)
					{
						num -= 0.25f;
					}
					bool flag4 = num <= 0f;
					if (flag4)
					{
						break;
					}
				}
				bool flag5 = num <= 0f;
				if (flag5)
				{
					return false;
				}
				foreach (IntVec3 c2 in GenRadial.RadialCellsAround(target.Position, 2f, false))
				{
					bool flag6 = !c2.InBounds(source.Map);
					if (!flag6)
					{
						Pawn firstPawn = c2.GetFirstPawn(source.Map);
						bool flag7 = source.Map.areaManager.Home[c2] || (firstPawn != null && AdvancedAI.IsHostile(source, firstPawn));
						if (flag7)
						{
							num += 0.1f;
							break;
						}
					}
				}
			}
			return num > 0f;
		}

		// Token: 0x0600024E RID: 590 RVA: 0x00031BE4 File Offset: 0x0002FDE4
		public static bool IsBarrier(this Building building)
		{
			return building.OccupiedRect().Cells.Count<IntVec3>() == 1 && (building.def.passability == Traversability.Impassable || building is Building_Door || building is Building_FenceDoor);
		}

		// Token: 0x0600024F RID: 591 RVA: 0x00031C34 File Offset: 0x0002FE34
		public static List<Building> CheckBuildingsThroughLists(Pawn pawn, Verb verb, IOrderedEnumerable<Building> buildings, int listMaxCount, bool incendiaryWeapon)
		{
			List<Building> list = new List<Building>();
			if (incendiaryWeapon)
			{
				buildings.ThenByDescending((Building b) => b.GetStatValue(StatDefOf.Flammability, true));
			}
			List<IntVec3> list2 = AdvancedAI.CellReservedForMainBlow(pawn);
			bool flag = SkyAiCore.Settings.enableMainBlowSiegeTactic && !list2.NullOrEmpty<IntVec3>() && list2.Count > 0;
			if (flag)
			{
				buildings = (from cell in list2
				where cell.InBounds(pawn.Map)
				select cell into @select
				select @select.GetFirstBuilding(pawn.Map) into building
				where AdvancedAI.IsAcceptableTarget(pawn.Faction, building)
				select building into nearest
				orderby pawn.Position.DistanceTo(nearest.Position)
				select nearest).ThenBy((Building prior) => prior is Building_Door || AdvancedAI.IsTurret(prior));
			}
			List<Building> list3 = new List<Building>();
			foreach (Building building4 in buildings)
			{
				bool flag2 = AdvancedAI.CanHitBuilding(pawn, pawn.Position, building4, 0f);
				if (flag2)
				{
					bool flag3 = AdvancedAI.CanHitTargetFrom(verb, pawn.Position, building4, AdvancedAI.PrimaryIsGrenade(pawn));
					if (flag3)
					{
						bool flag4 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							Log.Message(string.Format("{0} added building to attack: {1} {2} Count: {3}/{4} ", new object[]
							{
								pawn,
								building4,
								building4.Position,
								list.Count,
								listMaxCount
							}));
							pawn.Map.debugDrawer.FlashCell(building4.Position, 0.9f, "L", SkyAiCore.Settings.flashCellDelay);
						}
						list.Add(building4);
					}
					else
					{
						list3.Add(building4);
					}
				}
				bool flag5 = list.Count >= listMaxCount;
				if (flag5)
				{
					break;
				}
			}
			bool flag6 = list.Count < listMaxCount;
			if (flag6)
			{
				foreach (Building building2 in list3)
				{
					bool flag7 = list.Count >= listMaxCount;
					if (flag7)
					{
						break;
					}
					bool flag8 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag8)
					{
						Log.Message(string.Format("{0} added from non-lineofsight building to attack: {1} {2} Count: {3}/{4} ", new object[]
						{
							pawn,
							building2,
							building2.Position,
							list.Count,
							listMaxCount
						}));
						pawn.Map.debugDrawer.FlashCell(building2.Position, 0.7f, "NL", SkyAiCore.Settings.flashCellDelay);
					}
					list.Add(building2);
				}
				foreach (Building building3 in buildings)
				{
					bool flag9 = list.Count >= listMaxCount;
					if (flag9)
					{
						break;
					}
					bool flag10 = AdvancedAI.CanHitBuilding(pawn, pawn.Position, building3, 0f) && !list.Contains(building3);
					if (flag10)
					{
						bool flag11 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag11)
						{
							Log.Message(string.Format("{0} added from non-lineofsight building to attack: {1} {2} Count: {3}/{4} ", new object[]
							{
								pawn,
								building3,
								building3.Position,
								list.Count,
								listMaxCount
							}));
							pawn.Map.debugDrawer.FlashCell(building3.Position, 0.7f, "NL", SkyAiCore.Settings.flashCellDelay);
						}
						list.Add(building3);
					}
				}
			}
			return list;
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00032104 File Offset: 0x00030304
		public static IntVec3 ClosestBehindCellFromPosition(Pawn pawn, IntVec3 fromTargetPosition)
		{
			bool debugCoverCells = SkyAiCore.Settings.debugCoverCells;
			if (debugCoverCells)
			{
				IEnumerable<IntVec3> enumerable = from c in AdvancedAI.CellsBetweenPositions(pawn.Position, fromTargetPosition, pawn.Map, true, 0, 0, 1, 0f)
				where c.InBounds(pawn.Map)
				select c;
				IEnumerable<IntVec3> enumerable2 = from nearestCell in enumerable
				where nearestCell.Standable(pawn.Map) && !nearestCell.ContainsStaticFire(pawn.Map) && nearestCell.DistanceTo(fromTargetPosition) < AdvancedAI.EffectiveRange(pawn)
				select nearestCell;
				bool flag = !enumerable2.EnumerableNullOrEmpty<IntVec3>();
				if (flag)
				{
					IntVec3 intVec = enumerable2.FirstOrDefault<IntVec3>();
					bool flag2 = SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						foreach (IntVec3 c2 in enumerable)
						{
							pawn.Map.debugDrawer.FlashCell(c2, 0.43f, null, SkyAiCore.Settings.flashCellDelay);
						}
						pawn.Map.debugDrawer.FlashCell(intVec, 0.52f, "C", SkyAiCore.Settings.flashCellDelay);
					}
					return intVec;
				}
			}
			else
			{
				IEnumerable<IntVec3> source = from c in AdvancedAI.CellsBetweenPositions(pawn.Position, fromTargetPosition, pawn.Map, true, 0, 0, 1, 0f)
				where c.InBounds(pawn.Map)
				select c;
				IEnumerable<IntVec3> enumerable3 = from nearestCell in source
				where nearestCell.DistanceTo(fromTargetPosition) < AdvancedAI.EffectiveRange(pawn)
				select nearestCell;
				bool flag3 = !enumerable3.EnumerableNullOrEmpty<IntVec3>();
				if (flag3)
				{
					return enumerable3.FirstOrDefault<IntVec3>();
				}
			}
			return IntVec3.Invalid;
		}

		// Token: 0x06000251 RID: 593 RVA: 0x000322D0 File Offset: 0x000304D0
		public static bool DangerousNonLOSTargetForRanged(Pawn pawn, IntVec3 pawnNewPosition, Building building, float distance)
		{
			return AdvancedAI.DangerousNonLOSTarget(pawn, building, 8f) && pawnNewPosition.DistanceTo(building.Position) <= distance;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00032308 File Offset: 0x00030508
		public static List<Building> PotencialBuildingTrashList(IntVec3 targetPosition, Pawn pawn, Verb verb, float inDistance, int listMaxCount)
		{
			List<Building> result = new List<Building>();
			IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(targetPosition, inDistance, true);
			IntVec3 closestCell = source.MinBy((IntVec3 nearestCell) => pawn.Position.DistanceTo(nearestCell));
			IEnumerable<IntVec3> enumerable = from cell in source
			where cell.InBounds(pawn.Map) && targetPosition.DistanceTo(closestCell) * 1.25f >= cell.DistanceTo(closestCell)
			select cell;
			IEnumerable<Building> source2 = from @select in enumerable
			select @select.GetFirstBuilding(pawn.Map) into building
			where AdvancedAI.IsAcceptableTarget(pawn.Faction, building) && AdvancedAI.ShouldInterest(pawn, building)
			select building;
			bool flag = SkyAiCore.Settings.debugPotencialTargets && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag)
			{
				foreach (IntVec3 c in enumerable)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.12f, "Z", SkyAiCore.Settings.flashCellDelay);
				}
			}
			bool flag2;
			bool incendiaryWeapon = AdvancedAI.PrimaryVerbIsIncendiary(pawn, out flag2);
			bool flag3 = !flag2;
			if (flag3)
			{
				IOrderedEnumerable<Building> buildings = from building in source2
				where GenSight.LineOfSightToThing(pawn.Position, building, pawn.Map, false, null) && AdvancedAI.PawnAndNeighborRegions(pawn).Contains(AdvancedAI.CellNearPosition(pawn, pawn.Position, building.Position).GetRegion(pawn.Map, RegionType.Set_Passable))
				select building into nearest
				orderby pawn.Position.DistanceTo(nearest.Position)
				select nearest;
				List<Building> list = AdvancedAI.CheckBuildingsThroughLists(pawn, verb, buildings, listMaxCount, incendiaryWeapon);
				bool flag4 = !list.NullOrEmpty<Building>();
				if (flag4)
				{
					result = list;
					return result;
				}
				IOrderedEnumerable<Building> buildings2 = (from building in source2
				where GenSight.LineOfSightToThing(pawn.Position, building, pawn.Map, false, null) || AdvancedAI.PawnAndNeighborRegions(pawn).Contains(AdvancedAI.CellNearPosition(pawn, pawn.Position, building.Position).GetRegion(pawn.Map, RegionType.Set_Passable))
				select building into nearest
				orderby pawn.Position.DistanceTo(nearest.Position)
				select nearest).ThenBy((Building prior) => prior is Building_Door || AdvancedAI.IsTurret(prior));
				List<Building> list2 = AdvancedAI.CheckBuildingsThroughLists(pawn, verb, buildings2, listMaxCount, incendiaryWeapon);
				bool flag5 = !list2.NullOrEmpty<Building>();
				if (flag5)
				{
					result = list2;
					return result;
				}
			}
			return result;
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0003250C File Offset: 0x0003070C
		public static IntVec3 NewCellPositionNearBuilding(Pawn pawn, Building building, Verb verb, IntRange distance, bool coverRequired)
		{
			IntVec3 intVec = IntVec3.Invalid;
			bool flag = building == null;
			IntVec3 result;
			if (flag)
			{
				result = intVec;
			}
			else
			{
				IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(building.Position, (float)distance.max, true);
				IntVec3 closestCell = source.MinBy((IntVec3 nearestCell) => pawn.Position.DistanceTo(nearestCell));
				IOrderedEnumerable<IntVec3> orderedEnumerable = from distantCell in source
				where distantCell.DistanceTo(building.Position) >= AdvancedAI.EffectiveRange(pawn) * 0.6f && building.Position.DistanceTo(closestCell) * 1.5f >= distantCell.DistanceTo(closestCell)
				select distantCell into cell
				where cell.DistanceTo(building.Position) >= (float)distance.min && pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn)
				select cell into nearestCell
				orderby pawn.Position.DistanceTo(nearestCell) descending
				select nearestCell;
				bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
				bool grenadeWeapon = AdvancedAI.PrimaryIsGrenade(pawn);
				foreach (IntVec3 intVec2 in orderedEnumerable)
				{
					bool flag2 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						pawn.Map.debugDrawer.FlashCell(intVec2, 0.2f, null, SkyAiCore.Settings.flashCellDelay);
					}
					bool flag3 = !AdvancedAI.IsDangerousCellAround(intVec2, pawn.Map, 3f) && intVec2.Standable(pawn.Map) && !intVec2.ContainsStaticFire(pawn.Map);
					if (flag3)
					{
						bool flag4 = SkyAiCore.Settings.enableMainBlowSiegeTactic && !AdvancedAI.CellReservedForMainBlow(pawn).NullOrEmpty<IntVec3>() && AdvancedAI.CellReservedForMainBlow(pawn).Contains(intVec2);
						if (!flag4)
						{
							bool flag5 = AdvancedAI.DangerousNonLOSTargetForRanged(pawn, intVec2, building, 15f);
							if (!flag5)
							{
								if (coverRequired)
								{
									string text;
									bool flag6 = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, building.Position, true, false, true, true, useFriendlyFire, false, false, out text) <= 0f;
									if (flag6)
									{
										continue;
									}
								}
								bool flag7 = AdvancedAI.IsGoodTarget(pawn, intVec2, building, verb, grenadeWeapon);
								if (flag7)
								{
									bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag8)
									{
										Log.Message(string.Format("{0} {1}: IsGoodTarget. Found new cell: {2} near {3} pawn dist to cell: {4} cell dist to building: {5}", new object[]
										{
											pawn,
											pawn.Position,
											intVec2,
											building.Position,
											pawn.Position.DistanceTo(intVec2),
											intVec2.DistanceTo(building.Position)
										}));
									}
									intVec = intVec2;
									break;
								}
							}
						}
					}
				}
				result = intVec;
			}
			return result;
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0003283C File Offset: 0x00030A3C
		public static bool PawnInExclusionList(Pawn pawn)
		{
			List<Pawn> destroyersExclusions = AdvancedAI_Classes.MapComp(pawn).destroyersExclusions;
			return AdvancedAI_Classes.MapComp(pawn) != null && !destroyersExclusions.NullOrEmpty<Pawn>() && destroyersExclusions.Contains(pawn);
		}

		// Token: 0x06000255 RID: 597 RVA: 0x00032874 File Offset: 0x00030A74
		public static bool IsFreeCell(List<Thing> things)
		{
			bool flag = true;
			bool flag2 = !SkyAiCore.Settings.additionalCheckForFreeCells;
			bool result;
			if (flag2)
			{
				result = true;
			}
			else
			{
				int num = 0;
				foreach (Thing thing in things)
				{
					Pawn pawn = thing as Pawn;
					bool flag3 = pawn != null;
					if (flag3)
					{
						num++;
					}
					bool flag4 = (float)num >= 4f;
					if (flag4)
					{
						flag = false;
						break;
					}
				}
				result = flag;
			}
			return result;
		}

		// Token: 0x06000256 RID: 598 RVA: 0x00032914 File Offset: 0x00030B14
		public static bool TryToSwitchToSiegeWeapon(Pawn pawn)
		{
			bool flag = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				CompInventory compInventory = pawn.TryGetComp<CompInventory>();
				bool flag2 = compInventory != null;
				if (flag2)
				{
					ThingWithComps thingWithComps = (from thing in compInventory.rangedWeaponList
					where thing.TryGetComp<CompAmmoUser>() != null && AdvancedAI.IsSiegeWeapon(thing)
					select thing).FirstOrDefault<ThingWithComps>();
					bool flag3 = thingWithComps != null;
					if (flag3)
					{
						Thing thing2 = null;
						using (List<AmmoLink>.Enumerator enumerator = thingWithComps.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								AmmoLink link = enumerator.Current;
								thing2 = compInventory.ammoList.Find((Thing thing) => thing.def == link.ammo);
							}
						}
						bool flag4 = thing2 != null;
						if (flag4)
						{
							compInventory.TrySwitchToWeapon(thingWithComps);
							return true;
						}
					}
					ThingWithComps thingWithComps2 = AdvancedAI.GrenadeInInventory(compInventory);
					bool flag5 = thingWithComps2 != null;
					if (flag5)
					{
						compInventory.TrySwitchToWeapon(thingWithComps2);
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000257 RID: 599 RVA: 0x00032A48 File Offset: 0x00030C48
		public static ThingWithComps GrenadeInInventory(CompInventory inventory)
		{
			ThingWithComps thingWithComps = inventory.rangedWeaponList.Find((ThingWithComps thing) => AdvancedAI.IsGrenade(thing));
			bool flag = thingWithComps != null;
			ThingWithComps result;
			if (flag)
			{
				result = thingWithComps;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000258 RID: 600 RVA: 0x00032A94 File Offset: 0x00030C94
		public static int GetPawnsInRadius(Pawn pawn, IntVec3 fromLoc, float distance, out int busyPawnCount)
		{
			busyPawnCount = 0;
			int num = 0;
			foreach (IntVec3 c in GenRadial.RadialCellsAround(fromLoc, distance, true))
			{
				bool flag = !c.InBounds(pawn.Map);
				if (!flag)
				{
					foreach (Thing thing in pawn.Map.thingGrid.ThingsListAtFast(c))
					{
						Pawn pawn2 = thing as Pawn;
						bool flag2 = pawn2 != null && !pawn2.Downed && AdvancedAI.IsAlly(pawn2, pawn, false);
						if (flag2)
						{
							bool flag3 = pawn2.CurJobDef == JobDefOf.AttackMelee;
							if (flag3)
							{
								busyPawnCount++;
							}
							num++;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00032BA4 File Offset: 0x00030DA4
		public static bool IsFreeCell(IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				bool flag = pawn != null;
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600025A RID: 602 RVA: 0x00032BFC File Offset: 0x00030DFC
		public static bool CellAlreadyOccupied(Pawn pawn, IntVec3 onCell)
		{
			bool flag = AdvancedAI.PawnIsSniper(pawn);
			if (flag)
			{
				foreach (IntVec3 c in GenRadial.RadialCellsAround(onCell, 2f, true))
				{
					bool flag2 = !c.InBounds(pawn.Map);
					if (!flag2)
					{
						foreach (Thing thing in pawn.Map.thingGrid.ThingsListAtFast(c))
						{
							Pawn pawn2 = thing as Pawn;
							bool flag3 = pawn2 != null && !pawn2.Downed && pawn2 != pawn;
							if (flag3)
							{
								return true;
							}
						}
					}
				}
			}
			else
			{
				foreach (Thing thing2 in pawn.Map.thingGrid.ThingsListAtFast(onCell))
				{
					Pawn pawn3 = thing2 as Pawn;
					bool flag4 = pawn3 != null && !pawn3.Downed && pawn3 != pawn;
					if (flag4)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600025B RID: 603 RVA: 0x00032D78 File Offset: 0x00030F78
		public static bool PawnIsSniper(Pawn pawn)
		{
			ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
			bool flag = thingWithComps != null;
			if (flag)
			{
				bool flag2 = thingWithComps.def.thingCategories.Any((ThingCategoryDef cat) => cat == ThingCategoryDefOfAI.SRifles);
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600025C RID: 604 RVA: 0x00032DD4 File Offset: 0x00030FD4
		public static bool PawnIsGuard(Pawn pawn)
		{
			return pawn.GetComp<CompGuardRole>() != null;
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00032DF0 File Offset: 0x00030FF0
		public static bool EnemyTooClose(Pawn pawn, Pawn enemy, float multiplierBasedOnRange, out float rangeToEnemy)
		{
			rangeToEnemy = pawn.Position.DistanceTo(enemy.Position);
			float num = AdvancedAI.PrimaryWeaponRange(pawn);
			bool flag = num > 0f;
			return flag && rangeToEnemy < Mathf.Round(num * multiplierBasedOnRange);
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00032E3C File Offset: 0x0003103C
		public static RaidData PawnRaidData(Pawn pawn)
		{
			bool flag = pawn == null || pawn.Faction == null;
			RaidData result;
			if (flag)
			{
				result = null;
			}
			else
			{
				MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
				bool flag2 = mapComponent_SkyAI != null;
				if (flag2)
				{
					List<RaidData> raidData = mapComponent_SkyAI.raidData;
					bool flag3 = !raidData.NullOrEmpty<RaidData>();
					if (flag3)
					{
						foreach (RaidData raidData2 in raidData)
						{
							bool flag4 = !raidData2.squads.NullOrEmpty<SquadData>();
							if (flag4)
							{
								foreach (SquadData squadData in raidData2.squads)
								{
									RaidData raidData3 = squadData.PawnRaidData(pawn);
									bool flag5 = raidData3 != null;
									if (flag5)
									{
										return raidData3;
									}
								}
							}
						}
						foreach (RaidData raidData4 in raidData)
						{
							bool flag6 = raidData4 != null && !raidData4.raidPawns.NullOrEmpty<Pawn>() && raidData4.raidPawns.Contains(pawn);
							if (flag6)
							{
								return raidData4;
							}
						}
					}
				}
				result = null;
			}
			return result;
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00032FC0 File Offset: 0x000311C0
		public static int RaidCount(Pawn pawn)
		{
			bool flag = AdvancedAI.PawnRaidData(pawn) != null;
			int result;
			if (flag)
			{
				result = AdvancedAI.PawnRaidData(pawn).raidCount;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00032FF0 File Offset: 0x000311F0
		public static bool IsRaidLeaderOrSquadCommander(Pawn pawn)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null;
			bool result;
			if (flag)
			{
				Pawn raidLeader = raidData.raidLeader;
				bool flag2 = raidData.squadCommanders.Contains(pawn);
				bool flag3 = raidLeader != null && raidLeader.Equals(pawn);
				result = (flag3 || flag2);
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00033040 File Offset: 0x00031240
		public static bool HasSiegeBuilderJob(Pawn pawn)
		{
			return pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.Build;
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0003307C File Offset: 0x0003127C
		public static bool HasFleeingDuty(Pawn pawn)
		{
			return pawn.mindState.duty != null && (pawn.mindState.duty.def == DutyDefOf.ExitMapRandom || pawn.mindState.duty.def == DutyDefOf.Steal || pawn.mindState.duty.def == DutyDefOf.Kidnap);
		}

		// Token: 0x06000263 RID: 611 RVA: 0x000330E8 File Offset: 0x000312E8
		public static bool PawnShouldEscapeCombat(Pawn pawn)
		{
			switch (pawn.GetTraderCaravanRole())
			{
			case TraderCaravanRole.Trader:
				return true;
			case TraderCaravanRole.Carrier:
				return true;
			case TraderCaravanRole.Chattel:
				return true;
			}
			return false;
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0003312C File Offset: 0x0003132C
		public static bool DutyHasAttackSubNodes(Pawn pawn, bool checkEnemyTarget = true)
		{
			bool flag = pawn.mindState == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				PawnDuty duty = pawn.mindState.duty;
				bool flag3 = duty != null;
				if (flag3)
				{
					bool flag4 = duty.def.thinkNode != null && !duty.def.thinkNode.subNodes.NullOrEmpty<ThinkNode>();
					if (flag4)
					{
						foreach (ThinkNode thinkNode in duty.def.thinkNode.subNodes)
						{
							bool flag5 = thinkNode is JobGiver_AIFightEnemies || thinkNode is JobGiver_AIFightEnemy || thinkNode is JobGiver_SiegeAI || thinkNode is JobGiver_AIBreaching;
							if (flag5)
							{
								bool flag6 = checkEnemyTarget && pawn.mindState.enemyTarget != null;
								if (flag6)
								{
									flag2 = true;
								}
								else
								{
									bool flag7 = !checkEnemyTarget;
									if (flag7)
									{
										flag2 = true;
									}
								}
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x06000265 RID: 613 RVA: 0x00033254 File Offset: 0x00031454
		public static bool DutyHasSiegeSubNode(Pawn pawn)
		{
			bool flag = pawn.mindState == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				PawnDuty duty = pawn.mindState.duty;
				bool flag3 = duty != null;
				if (flag3)
				{
					bool flag4 = duty.def.thinkNode != null && !duty.def.thinkNode.subNodes.NullOrEmpty<ThinkNode>();
					if (flag4)
					{
						foreach (ThinkNode thinkNode in duty.def.thinkNode.subNodes)
						{
							bool flag5 = thinkNode is JobGiver_SiegeAI;
							if (flag5)
							{
								flag2 = true;
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0003332C File Offset: 0x0003152C
		public static bool SniperIsAlone(Pawn pawn)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null && AdvancedAI.RaidLeaderIsActive(raidData);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Lord lord = pawn.GetLord();
				bool flag2 = lord != null && lord.ticksInToil >= 10000;
				if (flag2)
				{
					int num = 0;
					int num2 = 0;
					foreach (Pawn pawn2 in lord.ownedPawns)
					{
						bool flag3 = AdvancedAI.IsActivePawn(pawn2);
						if (flag3)
						{
							bool flag4 = raidData.squadCommanders.Contains(pawn2);
							if (flag4)
							{
								bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag5)
								{
									Log.Message(string.Format("{0} {1} Check for escape. Squad commander {2} is active.", pawn, pawn.Position, pawn2));
								}
								return false;
							}
							num++;
							bool flag6 = AdvancedAI.PawnIsSniper(pawn2);
							if (flag6)
							{
								num2++;
							}
						}
					}
					float num3 = 0.25f;
					float num4 = (AdvancedAI.RaidCount(pawn) <= 0) ? num3 : ((float)(lord.ownedPawns.Count / AdvancedAI.RaidCount(pawn)));
					bool flag7 = num2 >= num && num4 <= num3;
					bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag8)
					{
						Log.Message(string.Format("{0} {1} Check for escape. {2}/{3} lord/raid count: {4}/{5} raidLordRatio: {6} result: {7}", new object[]
						{
							pawn,
							pawn.Position,
							num2,
							num,
							lord.ownedPawns.Count,
							AdvancedAI.RaidCount(pawn),
							num4,
							flag7
						}));
					}
					bool flag9 = flag7;
					if (flag9)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x00033530 File Offset: 0x00031730
		public static PawnPosture LayingOnGround(int type)
		{
			bool flag = type == 1;
			PawnPosture result;
			if (flag)
			{
				result = PawnPosture.LayingOnGroundFaceUp;
			}
			else
			{
				bool flag2 = type == 2;
				if (flag2)
				{
					result = PawnPosture.LayingOnGroundNormal;
				}
				else
				{
					result = PawnPosture.Standing;
				}
			}
			return result;
		}

		// Token: 0x06000268 RID: 616 RVA: 0x00033560 File Offset: 0x00031760
		public static bool PawnIsDoctor(Pawn pawn)
		{
			return pawn.TryGetComp<CompDoctorRole>() != null;
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0003357C File Offset: 0x0003177C
		public static bool PawnIsLeader(Pawn pawn)
		{
			return pawn.TryGetComp<CompLeaderRole>() != null;
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00033598 File Offset: 0x00031798
		public static bool PawnIsSquadLeader(Pawn pawn)
		{
			return pawn.TryGetComp<CompSquadCommanderRole>() != null;
		}

		// Token: 0x0600026B RID: 619 RVA: 0x000335B4 File Offset: 0x000317B4
		public static bool CanTakeItem(Pawn p, Thing item)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(p);
			bool flag = !AdvancedAI_GenerateUtility.FactionAllowed(item, p, false);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = mapComponent_SkyAI.boughtThings.ContainsKey(item);
				if (flag2)
				{
					Faction faction;
					mapComponent_SkyAI.boughtThings.TryGetValue(item, out faction);
					bool flag3 = faction != null && p.Faction != null && p.Faction == faction;
					if (flag3)
					{
						return false;
					}
				}
				result = !mapComponent_SkyAI.dangerousCells.Contains(item.Position);
			}
			return result;
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0003363C File Offset: 0x0003183C
		public static Thing ClosestThing(Pawn pawn, ThingRequestGroup thingRequestGroup, float radius)
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(thingRequestGroup), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), radius, (Thing x) => base.<ClosestThing>g__flag|0(thingRequestGroup) && AdvancedAI.CanTakeItem(pawn, x) && !AdvancedAI.PositionUnderCrossfire(pawn, x.Position, null, false, false) && pawn.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
			bool flag = thing != null;
			Thing result;
			if (flag)
			{
				result = thing;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x000336C0 File Offset: 0x000318C0
		public static Pawn RaidLeader(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				RaidData raidData = AdvancedAI.PawnRaidData(pawn);
				bool flag2 = raidData != null;
				if (flag2)
				{
					Pawn raidLeader = raidData.raidLeader;
					bool flag3 = AdvancedAI.IsActivePawn(raidLeader);
					if (flag3)
					{
						return raidLeader;
					}
				}
			}
			return null;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00033714 File Offset: 0x00031914
		public static Thing GetNewTargetWithCellBlocker(Pawn pawn, out IntVec3 cellBlocker)
		{
			Thing thing = null;
			cellBlocker = IntVec3.Invalid;
			IAttackTarget attackTarget = AdvancedAI.AttackTarget(pawn);
			bool flag = attackTarget != null;
			if (flag)
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, attackTarget.Thing.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false), PathEndMode.OnCell, null))
				{
					thing = pawnPath.FirstBlockingBuilding(out cellBlocker, pawn);
				}
			}
			bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag2)
			{
				Log.Message(string.Format("{0} {1}: GetNewTargetWithCellBlocker. target: {2} focusCell: {3}", new object[]
				{
					pawn,
					pawn.Position,
					thing,
					cellBlocker
				}));
			}
			return thing;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x000337FC File Offset: 0x000319FC
		public static IntVec3 GetLeaderFocusCell(Pawn pawn)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null && raidData.raidLeader != null && raidData.raidLeader != pawn;
			if (flag)
			{
				IntVec3 leaderTarget = raidData.leaderTarget;
				bool flag2 = AdvancedAI.IsValidLoc(leaderTarget);
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: Get new leader focus cell: {2}", pawn, pawn.Position, leaderTarget));
					}
					return leaderTarget;
				}
			}
			return IntVec3.Invalid;
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00033894 File Offset: 0x00031A94
		public static Pawn FirstRaidPawnAtFast(RaidData raidData)
		{
			bool flag = raidData != null;
			if (flag)
			{
				foreach (Pawn pawn in raidData.raidPawns)
				{
					bool flag2 = AdvancedAI.IsActivePawn(pawn);
					if (flag2)
					{
						return pawn;
					}
				}
			}
			return null;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x00033908 File Offset: 0x00031B08
		public static bool IsActivePawn(Pawn pawn)
		{
			return pawn != null && pawn.Spawned && !pawn.Downed && !pawn.Dead && !pawn.Destroyed;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x00033944 File Offset: 0x00031B44
		public static LocomotionUrgency ResolveCombatLocomotion(Pawn pawn, Thing enemyTarget, float distanceToTarget = 0f)
		{
			bool flag = AdvancedAI_TendUtility.HasHediffsNeedingTend(pawn);
			LocomotionUrgency result;
			if (flag)
			{
				result = LocomotionUrgency.Jog;
			}
			else
			{
				bool flag2 = enemyTarget != null;
				if (flag2)
				{
					bool flag3 = distanceToTarget <= 0f;
					if (flag3)
					{
						bool flag4 = pawn.mindState != null && pawn.mindState.enemyTarget != null;
						if (flag4)
						{
							distanceToTarget = pawn.Position.DistanceTo(pawn.mindState.enemyTarget.Position);
						}
					}
					bool flag5 = distanceToTarget > 0f;
					if (flag5)
					{
						int value = Mathf.RoundToInt(distanceToTarget);
						bool flag6 = Enumerable.Range(88, 500).Contains(value);
						if (flag6)
						{
							return LocomotionUrgency.Amble;
						}
						bool flag7 = Enumerable.Range(60, 87).Contains(value);
						if (flag7)
						{
							return LocomotionUrgency.Jog;
						}
						bool flag8 = Enumerable.Range(0, 59).Contains(value);
						if (flag8)
						{
							return LocomotionUrgency.Sprint;
						}
					}
				}
				LocomotionUrgency locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Jog);
				bool flag9 = locomotionUrgency > LocomotionUrgency.None;
				if (flag9)
				{
					result = locomotionUrgency;
				}
				else
				{
					result = LocomotionUrgency.Jog;
				}
			}
			return result;
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00033A4C File Offset: 0x00031C4C
		public static int Interval(AdvancedAI.ExpireInterval expireInterval)
		{
			int result;
			switch (expireInterval)
			{
			case AdvancedAI.ExpireInterval.utlrafast:
				result = 180;
				break;
			case AdvancedAI.ExpireInterval.fast:
				result = new IntRange(190, 260).RandomInRange;
				break;
			case AdvancedAI.ExpireInterval.normal:
				result = new IntRange(300, 400).RandomInRange;
				break;
			case AdvancedAI.ExpireInterval.slow:
				result = new IntRange(400, 500).RandomInRange;
				break;
			case AdvancedAI.ExpireInterval.veryslow:
				result = new IntRange(600, 800).RandomInRange;
				break;
			case AdvancedAI.ExpireInterval.ultraslow:
				result = 1000;
				break;
			default:
				result = new IntRange(320, 420).RandomInRange;
				break;
			}
			return result;
		}

		// Token: 0x06000274 RID: 628 RVA: 0x00033B14 File Offset: 0x00031D14
		public static int CombatInterval(Pawn pawn, IntVec3 enemyTargetPosition, float distanceToTarget = 0f)
		{
			bool flag = distanceToTarget == 0f;
			if (flag)
			{
				distanceToTarget = pawn.Position.DistanceTo(enemyTargetPosition);
			}
			return AdvancedAI.Interval(AdvancedAI.CombatExpireInterval(pawn, enemyTargetPosition, distanceToTarget));
		}

		// Token: 0x06000275 RID: 629 RVA: 0x00033B50 File Offset: 0x00031D50
		public static AdvancedAI.ExpireInterval CombatExpireInterval(Pawn pawn, IntVec3 enemyTargetPosition, float distanceToTarget)
		{
			bool flag = AdvancedAI_CoverUtility.IsCovered(pawn, enemyTargetPosition, true, false, true, false, false, false, false);
			int value = Mathf.RoundToInt(distanceToTarget);
			bool flag2 = pawn.mindState.enemyTarget != null && pawn.mindState.enemyTarget.Position == enemyTargetPosition;
			bool flag3 = Enumerable.Range(76, 500).Contains(value);
			AdvancedAI.ExpireInterval result;
			if (flag3)
			{
				result = AdvancedAI.ExpireInterval.slow;
			}
			else
			{
				bool flag4 = Enumerable.Range(54, 75).Contains(value);
				if (flag4)
				{
					bool flag5 = !flag2;
					if (flag5)
					{
						result = AdvancedAI.ExpireInterval.slow;
					}
					else
					{
						result = AdvancedAI.ExpireInterval.normal;
					}
				}
				else
				{
					bool flag6 = Enumerable.Range(0, 53).Contains(value);
					if (flag6)
					{
						bool flag7 = flag || !flag2;
						if (flag7)
						{
							result = AdvancedAI.ExpireInterval.normal;
						}
						else
						{
							result = AdvancedAI.ExpireInterval.fast;
						}
					}
					else
					{
						result = AdvancedAI.ExpireInterval.normal;
					}
				}
			}
			return result;
		}

		// Token: 0x06000276 RID: 630 RVA: 0x00033C1C File Offset: 0x00031E1C
		public static LocalTargetInfo GetEnemyTarget(Pawn pawn, bool checkForLineOfSight, bool checkDutyCell)
		{
			bool flag = pawn.mindState == null;
			LocalTargetInfo result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Thing enemyTarget = pawn.mindState.enemyTarget;
				bool flag2 = enemyTarget == null;
				if (flag2)
				{
					bool flag3 = checkDutyCell && pawn.mindState.duty != null && pawn.mindState.duty.focus != null;
					if (flag3)
					{
						LocalTargetInfo focus = pawn.mindState.duty.focus;
						bool isValid = focus.IsValid;
						if (isValid)
						{
							return focus;
						}
					}
					result = null;
				}
				else
				{
					if (checkForLineOfSight)
					{
						bool flag4 = !GenSight.LineOfSight(pawn.Position, enemyTarget.Position, pawn.Map, false, null, 0, 0);
						if (flag4)
						{
							return null;
						}
					}
					result = enemyTarget;
				}
			}
			return result;
		}

		// Token: 0x06000277 RID: 631 RVA: 0x00033D00 File Offset: 0x00031F00
		public static IntVec3 GetFocusCell(Pawn pawn, IntVec3 focusCell, bool tryToGetValidFocusCell)
		{
			CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
			bool flag = compDoctorRole != null;
			if (flag)
			{
				bool flag2 = !AdvancedAI.IsValidLoc(focusCell);
				if (flag2)
				{
					focusCell = compDoctorRole.focusCell;
				}
				else
				{
					compDoctorRole.focusCell = focusCell;
				}
			}
			IntVec3 intVec = focusCell;
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag3 = raidData != null && (raidData.raidStage == RaidData.RaidStage.attack || raidData.raidStage == RaidData.RaidStage.siege);
			if (flag3)
			{
				bool flag4 = AdvancedAI.IsValidLoc(pawn, focusCell, PathEndMode.None);
				bool flag5 = AdvancedAI_Classes.MapComp(pawn) != null;
				if (flag5)
				{
					bool flag6 = !AdvancedAI_Classes.MapComp(pawn).focusCells.ContainsKey(pawn) && flag4;
					if (flag6)
					{
						AdvancedAI_Classes.MapComp(pawn).focusCells.Add(pawn, focusCell);
					}
					bool flag7 = AdvancedAI_Classes.MapComp(pawn).focusCells.ContainsKey(pawn);
					if (flag7)
					{
						AdvancedAI_Classes.MapComp(pawn).focusCells.TryGetValue(pawn, out intVec);
					}
				}
				bool flag8 = intVec == IntVec3.Invalid && flag4;
				if (flag8)
				{
					Log.Error(string.Format("{0}: Houston we have a problem. For some reason, focus cell is not valid. Updated.", pawn));
					intVec = focusCell;
				}
				int num;
				bool flag9 = pawn.IsHashIntervalTick(40) && AdvancedAI.GetPawnsInRadius(pawn, intVec, 3f, out num) < 1;
				if (flag9)
				{
					bool flag10 = AdvancedAI_Classes.MapComp(pawn).focusCells.ContainsKey(pawn);
					if (flag10)
					{
						AdvancedAI_Classes.MapComp(pawn).focusCells.Remove(pawn);
					}
					intVec = focusCell;
				}
			}
			bool flag11 = tryToGetValidFocusCell && !AdvancedAI.IsValidLoc(focusCell) && pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
			if (flag11)
			{
				bool flag12 = AdvancedAI.GetNewTargetWithCellBlocker(pawn, out intVec) == null;
				if (flag12)
				{
					IntVec3 leaderFocusCell = AdvancedAI.GetLeaderFocusCell(pawn);
					bool flag13 = AdvancedAI.IsValidLoc(leaderFocusCell);
					if (flag13)
					{
						intVec = leaderFocusCell;
						bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag14)
						{
							Log.Message(string.Format("{0} {1}: GetFocusCell. Get leader focus cell: {2}. Previous focus cell was not valid.", pawn, pawn.Position, leaderFocusCell));
						}
					}
					else
					{
						RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(pawn.Position, pawn.Map, 1f, out focusCell);
						bool flag15 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag15)
						{
							Log.Message(string.Format("{0} {1}: GetFocusCell. TryFindRandomCellOutsideColonyNearTheCenterOfTheMap. New focus cell: {2}. Previous focus cell was not valid.", pawn, pawn.Position, focusCell));
						}
					}
				}
				else
				{
					bool flag16 = compDoctorRole != null;
					if (flag16)
					{
						compDoctorRole.focusCell = intVec;
					}
				}
			}
			return intVec;
		}

		// Token: 0x06000278 RID: 632 RVA: 0x00033F84 File Offset: 0x00032184
		public static bool TryFindEnemyBuildingLOSCellNearTheCenterOfTheMap(Faction faction, IntVec3 pos, Map map, out IntVec3 result)
		{
			AdvancedAI.<>c__DisplayClass108_0 CS$<>8__locals1 = new AdvancedAI.<>c__DisplayClass108_0();
			CS$<>8__locals1.map = map;
			CS$<>8__locals1.faction = faction;
			CS$<>8__locals1.pos = pos;
			return RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(new Predicate<IntVec3>(CS$<>8__locals1.<TryFindEnemyBuildingLOSCellNearTheCenterOfTheMap>g__validator|0), CS$<>8__locals1.map, out result);
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00033FCC File Offset: 0x000321CC
		public static bool TryFindGoodAdjacentSpotToTouch(Pawn toucher, IntVec3 touchee, out IntVec3 result)
		{
			IOrderedEnumerable<IntVec3> orderedEnumerable = from c in GenAdjFast.AdjacentCellsCardinal(touchee)
			where c.InBounds(toucher.Map)
			select c into c2
			orderby c2.DistanceTo(toucher.Position)
			select c2;
			foreach (IntVec3 intVec in orderedEnumerable)
			{
				bool flag = intVec.Equals(touchee);
				if (flag)
				{
					result = toucher.Position;
					return false;
				}
			}
			foreach (IntVec3 intVec2 in orderedEnumerable)
			{
				bool flag2 = intVec2.Standable(toucher.Map) && !PawnUtility.KnownDangerAt(intVec2, toucher.Map, toucher);
				if (flag2)
				{
					result = intVec2;
					return true;
				}
			}
			foreach (IntVec3 intVec3 in orderedEnumerable)
			{
				bool flag3 = intVec3.Walkable(toucher.Map);
				if (flag3)
				{
					result = intVec3;
					return true;
				}
			}
			result = toucher.Position;
			return false;
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00034170 File Offset: 0x00032370
		public static bool IsInCloseWithTarget(Thing source, Thing target)
		{
			bool flag = target != null;
			if (flag)
			{
				foreach (IntVec3 c in GenAdjFast.AdjacentCells8Way(source))
				{
					bool flag2 = !c.InBounds(source.Map);
					if (!flag2)
					{
						foreach (Thing thing in source.Map.thingGrid.ThingsListAtFast(c))
						{
							bool flag3 = thing != null && thing == target;
							if (flag3)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00034258 File Offset: 0x00032458
		public static List<IntVec3> LeavingFromCells(Pawn pawn, int stepMax = 15)
		{
			List<IntVec3> list = new List<IntVec3>();
			int num = 0;
			PawnPath curPath = pawn.pather.curPath;
			bool found = curPath.Found;
			if (found)
			{
				List<IntVec3> nodesReversed = curPath.NodesReversed;
				int num2 = nodesReversed.Count - 1;
				for (int i = num2; i > 0; i--)
				{
					num++;
					bool flag = nodesReversed[i] == nodesReversed[0];
					if (flag)
					{
						break;
					}
					bool flag2 = num >= stepMax;
					if (flag2)
					{
						list.Add(nodesReversed[i]);
						num = 0;
					}
				}
			}
			return list;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00034300 File Offset: 0x00032500
		public static IntVec3 GetPawnJobLocation(Pawn pawn)
		{
			bool flag = pawn.jobs != null && pawn.jobs.curJob != null;
			if (flag)
			{
				IntVec3 cell = pawn.CurJob.targetA.Cell;
				bool flag2 = pawn.Position.DistanceTo(cell) > 1f;
				if (flag2)
				{
					return cell;
				}
			}
			return IntVec3.Invalid;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00034364 File Offset: 0x00032564
		public static LocomotionUrgency ResolveLocomotion(Pawn pawn)
		{
			LocomotionUrgency result = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Jog, LocomotionUrgency.Sprint);
			bool flag = pawn.mindState.duty != null && pawn.mindState.duty.focus != null && pawn.Position.DistanceTo(pawn.mindState.duty.focus.Cell) < 70f;
			bool flag2 = AdvancedAI_TendUtility.HasHediffsNeedingTend(pawn) || pawn.mindState.enemyTarget != null || flag || PawnUtility.EnemiesAreNearby(pawn, 40, false);
			if (flag2)
			{
				result = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
			}
			bool flag3 = pawn.mindState.enemyTarget == null && AdvancedAI_SquadUtility.IsStartingRaidStage(pawn);
			if (flag3)
			{
				IntVec3 pawnJobLocation = AdvancedAI.GetPawnJobLocation(pawn);
				bool flag4 = AdvancedAI.IsValidLoc(pawnJobLocation) && (AdvancedAI.PawnIsLeader(pawn) || AdvancedAI.PawnIsDoctor(pawn));
				if (flag4)
				{
					int num = 0;
					int num2 = 0;
					foreach (IntVec3 intVec in GenRadial.RadialCellsAround(pawn.Position, 15f, false))
					{
						bool flag5 = !intVec.InBounds(pawn.Map);
						if (!flag5)
						{
							Pawn pawn2 = pawn.Map.thingGrid.ThingsListAtFast(intVec).OfType<Pawn>().FirstOrDefault<Pawn>();
							bool flag6 = pawn2 != null && AdvancedAI.IsAlly(pawn2, pawn, false);
							if (flag6)
							{
								num++;
								bool flag7 = intVec.DistanceTo(pawnJobLocation) < pawn.Position.DistanceTo(pawnJobLocation);
								if (flag7)
								{
									num2++;
								}
							}
						}
					}
					float num3 = 1.75f;
					bool flag8 = Mathf.RoundToInt((float)num2 * num3) < num;
					if (flag8)
					{
						bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag9)
						{
							Log.Message(string.Format("{0} {1}: Too fast moving, changed locomotion to walk, bcs: {2}/{3}", new object[]
							{
								pawn,
								pawn.Position,
								(float)num2 * num3,
								num
							}));
						}
						result = LocomotionUrgency.Walk;
					}
				}
			}
			return result;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x000345B8 File Offset: 0x000327B8
		public static bool AlreadyHasSameJob(Pawn pawn, JobDef jobDef)
		{
			return pawn.CurJobDef == jobDef;
		}

		// Token: 0x0600027F RID: 639 RVA: 0x000345E0 File Offset: 0x000327E0
		public static bool IsHealthStabilized(Pawn pawn)
		{
			return pawn.health.hediffSet.BleedRateTotal == 0f || AdvancedAI.PawnWontBleedOutSoon(pawn);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00034614 File Offset: 0x00032814
		public static bool PawnWontBleedOutSoon(Pawn pawn)
		{
			return HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) >= 60000;
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00034638 File Offset: 0x00032838
		public static bool IsInDangerHealthReasons(Pawn pawn)
		{
			return HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) <= 25000 || AdvancedAI.IsCloseToCollapse(pawn);
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00034660 File Offset: 0x00032860
		public static bool IsCloseToCollapse(Pawn pawn)
		{
			return pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) <= 0.4f || pawn.GetStatValue(StatDefOf.PainShockThreshold, true) - pawn.health.hediffSet.PainTotal <= 0.1f;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x000346B8 File Offset: 0x000328B8
		public static int InjuriesCount(Pawn pawn)
		{
			int num = 0;
			bool flag = pawn.health != null;
			if (flag)
			{
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				bool flag2 = !hediffs.NullOrEmpty<Hediff>();
				if (flag2)
				{
					for (int i = 0; i < hediffs.Count; i++)
					{
						bool flag3 = hediffs[i].TendableNow(false);
						if (flag3)
						{
							num++;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00034738 File Offset: 0x00032938
		public static bool UnableMoveFast(Pawn pawn)
		{
			return (double)pawn.GetStatValue(StatDefOf.MoveSpeed, true) <= 3.5;
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00034768 File Offset: 0x00032968
		public static bool AlmostNotCapableToFight(Pawn pawn)
		{
			bool flag = AdvancedAI.IsUniquePawn(pawn);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				PawnCapacitiesHandler capacities = pawn.health.capacities;
				bool flag2 = capacities.GetLevel(PawnCapacityDefOf.Moving) <= 0.2f;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = capacities.GetLevel(PawnCapacityDefOf.Sight) <= 0.3f;
					if (flag3)
					{
						result = true;
					}
					else
					{
						bool flag4 = capacities.GetLevel(PawnCapacityDefOf.Manipulation) <= 0.2f;
						result = flag4;
					}
				}
			}
			return result;
		}

		// Token: 0x06000286 RID: 646 RVA: 0x000347F4 File Offset: 0x000329F4
		public static bool IsExhausted(Pawn pawn)
		{
			Pawn_NeedsTracker needs = pawn.needs;
			bool flag = needs != null;
			if (flag)
			{
				bool flag2 = needs.food != null;
				if (flag2)
				{
					return needs.food.CurCategory >= HungerCategory.Starving;
				}
				bool flag3 = needs.rest != null;
				if (flag3)
				{
					return needs.rest.CurCategory >= RestCategory.VeryTired;
				}
			}
			return false;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00034860 File Offset: 0x00032A60
		public static bool HarmedRecently(Pawn pawn)
		{
			return Find.TickManager.TicksGame - pawn.mindState.lastHarmTick < 2500;
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00034890 File Offset: 0x00032A90
		public static bool EngagedEnemyRecently(Pawn pawn)
		{
			return Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick < 300;
		}

		// Token: 0x06000289 RID: 649 RVA: 0x000348CC File Offset: 0x00032ACC
		public static bool EngagedEnemyRecently(Pawn pawn, out Thing enemy)
		{
			bool flag = Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick < 300;
			bool result;
			if (flag)
			{
				enemy = pawn.mindState.enemyTarget;
				result = true;
			}
			else
			{
				enemy = null;
				result = false;
			}
			return result;
		}

		// Token: 0x0600028A RID: 650 RVA: 0x00034918 File Offset: 0x00032B18
		public static bool TakeCombatEnhancingDrugRecently(Pawn pawn)
		{
			return Find.TickManager.TicksGame - pawn.mindState.lastTakeCombatEnhancingDrugTick < 10000;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x00034954 File Offset: 0x00032B54
		public static bool InDangerousCellList(Pawn pawn, IntVec3 cell)
		{
			bool result = false;
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			bool flag = mapComponent_SkyAI != null && !mapComponent_SkyAI.dangerousCells.NullOrEmpty<IntVec3>();
			if (flag)
			{
				for (int i = mapComponent_SkyAI.dangerousCells.Count<IntVec3>() - 1; i >= 0; i--)
				{
					bool flag2 = mapComponent_SkyAI.dangerousCells[i].Equals(cell);
					if (flag2)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x0600028C RID: 652 RVA: 0x000349D0 File Offset: 0x00032BD0
		public static bool TargetCellInDangerousCellList(Pawn pawn, IntVec3 cell)
		{
			bool flag = pawn.jobs == null || pawn.jobs.curJob == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.jobs.curJob.def == JobDefOf.AttackMelee || pawn.jobs.curJob.def == JobDefOf.Goto;
				if (flag2)
				{
					bool flag3 = AdvancedAI.InDangerousCellList(pawn, cell);
					if (flag3)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0600028D RID: 653 RVA: 0x00034A4C File Offset: 0x00032C4C
		public static bool IsMeleeVerb(Thing searcher, Verb verb = null)
		{
			bool flag = searcher != null;
			if (flag)
			{
				bool flag2 = verb == null;
				if (flag2)
				{
					verb = AdvancedAI.PrimaryVerb(searcher);
				}
				bool flag3 = verb != null && verb.verbProps != null;
				if (flag3)
				{
					return verb.verbProps.IsMeleeAttack;
				}
			}
			return false;
		}

		// Token: 0x0600028E RID: 654 RVA: 0x00034AA0 File Offset: 0x00032CA0
		public static bool ActiveThreat(Thing searcher, float maxDistance, bool checkPotencialFriendlyFire, bool IsInTarget, bool useShootline, bool calculateEnemyRange, bool closestEnemyFirst, bool useLeanShootingSourcesCheck, bool checkNearbyCells, out Thing enemy, out List<Thing> targetKeepRangeList)
		{
			AdvancedAI.<>c__DisplayClass130_0 CS$<>8__locals1 = new AdvancedAI.<>c__DisplayClass130_0();
			CS$<>8__locals1.searcher = searcher;
			CS$<>8__locals1.useShootline = useShootline;
			bool flag = false;
			Thing thing = null;
			List<Thing> list = new List<Thing>();
			CS$<>8__locals1.verb = AdvancedAI.PrimaryVerb(CS$<>8__locals1.searcher);
			CS$<>8__locals1.isMelee = AdvancedAI.IsMeleeVerb(CS$<>8__locals1.searcher, CS$<>8__locals1.verb);
			IOrderedEnumerable<Thing> orderedEnumerable = from targetSight in AdvancedAI.PotencialTargets(CS$<>8__locals1.searcher)
			orderby CS$<>8__locals1.searcher.CanSee(targetSight, null)
			select targetSight;
			if (closestEnemyFirst)
			{
				(from targetDistance in orderedEnumerable
				orderby targetDistance.Position.DistanceTo(CS$<>8__locals1.searcher.Position) descending
				select targetDistance).ThenBy((Thing targetSight) => CS$<>8__locals1.searcher.CanSee(targetSight, null));
			}
			targetKeepRangeList = orderedEnumerable.ToList<Thing>();
			foreach (Thing enemyTarget in orderedEnumerable)
			{
				AdvancedAI.<>c__DisplayClass130_1 CS$<>8__locals2;
				CS$<>8__locals2.enemyTarget = enemyTarget;
				bool flag2 = CS$<>8__locals2.enemyTarget != null;
				if (flag2)
				{
					AdvancedAI.<>c__DisplayClass130_2 CS$<>8__locals3;
					CS$<>8__locals3.pawn = (CS$<>8__locals1.searcher as Pawn);
					bool flag3 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: ActiveThreat. Check target1: {2}", CS$<>8__locals3.pawn, CS$<>8__locals3.pawn.Position, CS$<>8__locals2.enemyTarget));
					}
					float num = CS$<>8__locals1.searcher.Position.DistanceTo(CS$<>8__locals2.enemyTarget.Position);
					bool flag4 = num > maxDistance;
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: ActiveThreat. Check target2: {2} dist: {3} passed bcs of distance.", new object[]
							{
								CS$<>8__locals3.pawn,
								CS$<>8__locals3.pawn.Position,
								CS$<>8__locals2.enemyTarget,
								num
							}));
						}
					}
					else
					{
						bool flag6 = !AdvancedAI.ShouldAttackIfHive(CS$<>8__locals1.searcher, CS$<>8__locals2.enemyTarget);
						if (flag6)
						{
							bool flag7 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: ActiveThreat. Check target2.1: {2} dist: {3} passed bcs of hive shouldn't be attacked.", new object[]
								{
									CS$<>8__locals3.pawn,
									CS$<>8__locals3.pawn.Position,
									CS$<>8__locals2.enemyTarget,
									num
								}));
							}
						}
						else
						{
							bool flag8 = SkyAiCore.Settings.considerFriendlyFireWhileSelectTarget && checkPotencialFriendlyFire && num >= 5f && !CS$<>8__locals1.isMelee;
							if (flag8)
							{
								bool flag9 = false;
								foreach (IntVec3 c in GenAdjFast.AdjacentCells8Way(CS$<>8__locals2.enemyTarget.Position))
								{
									bool flag10 = !c.InBounds(CS$<>8__locals1.searcher.Map);
									if (!flag10)
									{
										Pawn firstPawn = c.GetFirstPawn(CS$<>8__locals1.searcher.Map);
										bool flag11 = firstPawn != null && firstPawn.Faction != null && !firstPawn.HostileTo(CS$<>8__locals1.searcher) && CS$<>8__locals1.searcher.Position.DistanceTo(firstPawn.Position) < num;
										if (flag11)
										{
											bool flag12 = CS$<>8__locals3.pawn != null && CS$<>8__locals3.pawn == firstPawn;
											if (!flag12)
											{
												bool flag13 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
												if (flag13)
												{
													Log.Message(string.Format("{0} {1}: ActiveThreat. Check target3: {2} passed bcs of friendlyfire", CS$<>8__locals3.pawn, CS$<>8__locals3.pawn.Position, CS$<>8__locals2.enemyTarget));
												}
												flag9 = true;
												break;
											}
										}
									}
								}
								bool flag14 = AdvancedAI.FriendlyFireThreatInShootline(CS$<>8__locals1.searcher, CS$<>8__locals1.searcher.Position, CS$<>8__locals2.enemyTarget.Position, 2f, 60f, 55f, 3f);
								if (flag14)
								{
									bool flag15 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
									if (flag15)
									{
										Log.Message(string.Format("{0} {1}: ActiveThreat. Check target3.2: {2} passed bcs of friendlyfireThreatInShootline.", CS$<>8__locals3.pawn, CS$<>8__locals3.pawn.Position, CS$<>8__locals2.enemyTarget));
									}
									flag9 = true;
									break;
								}
								bool flag16 = flag9;
								if (flag16)
								{
									bool flag17 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
									if (flag17)
									{
										Log.Message(string.Format("{0} {1}: ActiveThreat. Check target4: {2} passed bcs of friendlyfire", CS$<>8__locals3.pawn, CS$<>8__locals3.pawn.Position, CS$<>8__locals2.enemyTarget));
									}
									continue;
								}
							}
							bool flag18 = CS$<>8__locals1.<ActiveThreat>g__CheckPawn|4(IsInTarget, ref CS$<>8__locals2, ref CS$<>8__locals3);
							if (flag18)
							{
								bool flag19 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
								if (flag19)
								{
									Log.Message(string.Format("{0} {1}: ActiveThreat. Check target6: CheckPawn. Found enemy: {2}", CS$<>8__locals3.pawn, CS$<>8__locals3.pawn.Position, CS$<>8__locals2.enemyTarget));
								}
								thing = CS$<>8__locals2.enemyTarget;
								flag = true;
								break;
							}
							if (useLeanShootingSourcesCheck)
							{
								Verb verb = AdvancedAI.PrimaryVerb(CS$<>8__locals2.enemyTarget);
								bool flag20 = verb != null;
								if (flag20)
								{
									Verb_LaunchProjectileCE verb_LaunchProjectileCE = verb as Verb_LaunchProjectileCE;
									bool flag21 = verb_LaunchProjectileCE != null && AdvancedAI.CamperPawn(CS$<>8__locals2.enemyTarget);
									if (flag21)
									{
										ShootLine shootLine;
										bool flag22 = verb_LaunchProjectileCE.TryFindCEShootLineFromTo(CS$<>8__locals2.enemyTarget.Position, CS$<>8__locals3.pawn, ref shootLine);
										if (flag22)
										{
											bool flag23 = SkyAiCore.Settings.debugActiveThreat && CS$<>8__locals3.pawn != null && SkyAiCore.SelectedPawnDebug(CS$<>8__locals3.pawn);
											if (flag23)
											{
												Log.Message(string.Format("{0} {1}: ActiveThreat. Check target8: {2} coveredTargets. Add: {3}", new object[]
												{
													CS$<>8__locals3.pawn,
													CS$<>8__locals3.pawn.Position,
													CS$<>8__locals2.enemyTarget,
													CS$<>8__locals2.enemyTarget
												}));
											}
											list.Add(CS$<>8__locals2.enemyTarget);
										}
									}
								}
							}
						}
					}
				}
			}
			bool flag24 = flag;
			bool result;
			if (flag24)
			{
				enemy = thing;
				result = true;
			}
			else
			{
				bool flag25 = !list.NullOrEmpty<Thing>();
				if (flag25)
				{
					list.TryMinBy((Thing target) => CS$<>8__locals1.searcher.Position.DistanceTo(target.Position), out enemy);
					result = true;
				}
				else
				{
					enemy = null;
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600028F RID: 655 RVA: 0x000351B0 File Offset: 0x000333B0
		public static bool ActiveThreatInArea(Pawn forPawn, IntVec3 center, Map map, float maxDistance, int maxRegions, out Thing enemy)
		{
			Thing tempThing = null;
			bool foundActiveThreat = false;
			RegionTraverser.BreadthFirstTraverse(center, map, RegionTraverser.PassAll, delegate(Region x)
			{
				IEnumerable<Thing> source = from t in forPawn.Map.attackTargetsCache.GetPotentialTargetsFor(forPawn)
				where center.DistanceTo(t.Thing.Position) <= maxDistance
				select t into thing
				select thing.Thing;
				for (int i = 0; i < source.Count<Thing>(); i++)
				{
					Thing thing2 = source.ElementAt(i);
					bool flag = AdvancedAI.IsActiveTarget(forPawn, thing2, true, false);
					if (flag)
					{
						tempThing = thing2;
						foundActiveThreat = true;
						break;
					}
				}
				return foundActiveThreat;
			}, maxRegions, RegionType.Set_Passable);
			enemy = tempThing;
			return foundActiveThreat;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00035228 File Offset: 0x00033428
		public static void UpdateTarget(Pawn pawn, Thing enemy, bool overrideJob)
		{
			bool flag = SkyAiCore.Settings.enableEnchancedThreatReaction && AdvancedAI.DutyHasAttackSubNodes(pawn, false) && !AdvancedAI.HasExitOrNonCombatJob(pawn) && enemy != null;
			if (flag)
			{
				AdvancedAI.<>c__DisplayClass132_0 CS$<>8__locals1;
				CS$<>8__locals1.currentTarget = AdvancedAI.CurrentLocalTargetInfo(pawn);
				bool flag2 = AdvancedAI.<UpdateTarget>g__checkTarget|132_0(enemy, ref CS$<>8__locals1);
				if (flag2)
				{
					Thing thing = pawn.mindState.enemyTarget;
					bool flag3 = thing != null && thing != enemy && (thing.Destroyed || !AdvancedAI.EngagedEnemyRecently(pawn) || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn));
					if (flag3)
					{
						thing = null;
					}
					bool flag4 = thing == null && Find.TickManager.TicksGame - pawn.mindState.lastHarmTick > 120 && AdvancedAI.ShouldOverrideJob(pawn);
					if (flag4)
					{
						MethodInfo methodInfo = AccessTools.Method(typeof(Pawn_MindState), "Notify_EngagedTarget", null, null);
						methodInfo.Invoke(pawn.mindState, null);
						Lord lord = pawn.GetLord();
						if (lord != null)
						{
							lord.Notify_PawnAcquiredTarget(pawn, enemy);
						}
						pawn.mindState.lastHarmTick = Find.TickManager.TicksGame;
						pawn.mindState.enemyTarget = enemy;
						if (overrideJob)
						{
							pawn.jobs.CheckForJobOverride();
						}
					}
				}
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00035368 File Offset: 0x00033568
		public static bool InDangerousCombat(Pawn pawn, float distance)
		{
			Thing enemy;
			List<Thing> list;
			bool flag = AdvancedAI.ActiveThreat(pawn, distance, false, false, true, true, true, true, true, out enemy, out list);
			bool result;
			if (flag)
			{
				AdvancedAI.UpdateTarget(pawn, enemy, true);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000292 RID: 658 RVA: 0x000353A0 File Offset: 0x000335A0
		public static IEnumerable<Thing> PotencialTargets(Thing searcher)
		{
			return from x in searcher.Map.attackTargetsCache.GetPotentialTargetsFor((Pawn)searcher)
			where searcher.Position.DistanceTo(x.Thing.Position) <= SkyAiCore.Settings.combatKeepRange && AdvancedAI.IsActiveTarget(searcher, x.Thing, true, false)
			select x into thing
			select thing.Thing;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00035414 File Offset: 0x00033614
		public static bool EnemyNearby(Pawn pawn, Thing enemyTarget, float radius, bool useCenter)
		{
			bool result = false;
			IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(enemyTarget.Position, radius, useCenter);
			Func<IntVec3, bool> <>9__0;
			Func<IntVec3, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(pawn.Map) && c.Standable(pawn.Map)));
			}
			foreach (IntVec3 start in source.Where(predicate))
			{
				bool flag = GenSight.LineOfSight(start, pawn.Position, pawn.Map, false, null, 0, 0);
				if (flag)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		// Token: 0x06000294 RID: 660 RVA: 0x000354D0 File Offset: 0x000336D0
		public static bool TryToFindRaidLordPawn(Pawn pawn, RaidData raidData, out Pawn raidPawn)
		{
			raidPawn = null;
			bool result = false;
			IEnumerable<Pawn> source = from p in raidData.raidPawns
			where p != null && p.Spawned && p != pawn && !AdvancedAI.PawnIsGuard(p) && base.<TryToFindRaidLordPawn>g__otherLord|0(p)
			select p;
			for (int i = 0; i < source.Count<Pawn>(); i++)
			{
				Pawn pawn2 = source.ElementAt(i);
				bool flag = !AdvancedAI.HasExitJob(pawn2);
				if (flag)
				{
					raidPawn = pawn2;
					result = true;
					break;
				}
			}
			Pawn pawn3;
			source.TryMinBy((Pawn x) => pawn.Position.DistanceTo(x.Position), out pawn3);
			bool flag2 = pawn3 != null;
			if (flag2)
			{
				raidPawn = pawn3;
				result = true;
			}
			return result;
		}

		// Token: 0x06000295 RID: 661 RVA: 0x00035578 File Offset: 0x00033778
		public static List<Pawn> ClosestLordPawnsInDistance(Pawn pawn, float distance)
		{
			Lord lord = pawn.GetLord();
			List<Pawn> list = new List<Pawn>();
			foreach (Pawn pawn2 in lord.ownedPawns)
			{
				bool flag = pawn2 == pawn || pawn2.Downed;
				if (!flag)
				{
					bool flag2 = pawn.Position.DistanceTo(pawn2.Position) > distance;
					if (!flag2)
					{
						list.Add(pawn2);
					}
				}
			}
			list.SortByDescending((Pawn p) => pawn.Position.DistanceTo(p.Position));
			return list;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0003564C File Offset: 0x0003384C
		public static Thing ClosestLordAllyEnemy(Pawn pawn, Verb verb, int maxIteration, out IntVec3 dest)
		{
			bool flag = pawn.mindState.lastAttackedTarget == LocalTargetInfo.Invalid;
			Thing result;
			if (flag)
			{
				dest = IntVec3.Invalid;
				result = null;
			}
			else
			{
				bool flag2 = AdvancedAI.PawnIsDoctor(pawn) || AdvancedAI.PawnIsSniper(pawn);
				if (flag2)
				{
					dest = IntVec3.Invalid;
					result = null;
				}
				else
				{
					int num = 0;
					foreach (Pawn pawn2 in AdvancedAI.ClosestLordPawnsInDistance(pawn, 22f))
					{
						Thing thing;
						bool flag3 = AdvancedAI.EngagedEnemyRecently(pawn2, out thing) && thing != null;
						if (flag3)
						{
							num++;
							bool flag4 = AdvancedAI.IsMeleeVerb(pawn, verb);
							if (flag4)
							{
								bool flag5 = pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors);
								if (flag5)
								{
									pawn.mindState.enemyTarget = thing;
									dest = thing.Position;
									return thing;
								}
							}
							else
							{
								float effectiveRange = AdvancedAI.EffectiveRange(pawn);
								bool coverRequired = AdvancedAI_CoverUtility.IsCovered(pawn, thing.Position);
								IntVec3 intVec;
								bool flag7;
								bool flag6 = AdvancedAI.TryFindShootingPosition(pawn, verb, coverRequired, effectiveRange, out intVec, out flag7);
								if (flag6)
								{
									pawn.mindState.enemyTarget = thing;
									bool flag8 = AdvancedAI.IsValidLoc(intVec) && intVec != pawn.Position;
									if (flag8)
									{
										dest = intVec;
										return thing;
									}
								}
							}
							bool flag9 = num >= maxIteration;
							if (flag9)
							{
								break;
							}
						}
					}
					dest = IntVec3.Invalid;
					result = null;
				}
			}
			return result;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x00035804 File Offset: 0x00033A04
		public static float EffectiveRange(Thing searcher)
		{
			float num = AdvancedAI.PrimaryWeaponRange(searcher);
			Pawn pawn = searcher as Pawn;
			bool flag = pawn != null;
			float result;
			if (flag)
			{
				float num2 = 0f;
				bool flag2 = AdvancedAI.PrimaryWeapon(pawn) != null && AdvancedAI.PrimaryWeapon(pawn).def.IsRangedWeapon && AdvancedAI.PrimaryWeapon(pawn).GetStatValue(CE_StatDefOf.ShotSpread, true) > 0f;
				if (flag2)
				{
					num2 = AdvancedAI.PrimaryWeapon(pawn).GetStatValue(CE_StatDefOf.ShotSpread, true);
					bool flag3 = num2 <= 0f;
					if (flag3)
					{
						return num;
					}
				}
				result = Mathf.Round(num * AdvancedAI.EffectiveRangeByShotSpreadCurve.Evaluate(num2));
			}
			else
			{
				result = num;
			}
			return result;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x000358B8 File Offset: 0x00033AB8
		public static IntRange PrimaryEffectiveWeaponRange(Pawn pawn, float minDistanceMultiplier = 1f, float maxDistanceMultiplier = 1f, bool useMinDistanceForMaxRange = false, int minDistanceValue = 25)
		{
			int num = useMinDistanceForMaxRange ? Mathf.Max(minDistanceValue, Mathf.RoundToInt(AdvancedAI.EffectiveRange(pawn) * maxDistanceMultiplier)) : Mathf.RoundToInt(AdvancedAI.EffectiveRange(pawn) * maxDistanceMultiplier);
			int min = Math.Max(Mathf.RoundToInt(AdvancedAI.MinDistance(pawn, null)), Mathf.RoundToInt((float)num * minDistanceMultiplier));
			return new IntRange(min, num);
		}

		// Token: 0x06000299 RID: 665 RVA: 0x00035914 File Offset: 0x00033B14
		public static float PrimaryWeaponRange(Thing searcher)
		{
			Pawn pawn = searcher as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				Verb verb = AdvancedAI.PrimaryVerb(pawn);
				bool flag2 = verb != null;
				if (flag2)
				{
					return verb.verbProps.range;
				}
			}
			Building_TurretGunCE building_TurretGunCE = searcher as Building_TurretGunCE;
			bool flag3 = building_TurretGunCE != null && building_TurretGunCE.CurrentEffectiveVerb != null;
			float result;
			if (flag3)
			{
				result = building_TurretGunCE.CurrentEffectiveVerb.verbProps.range;
			}
			else
			{
				Building_TurretGun building_TurretGun = searcher as Building_TurretGun;
				bool flag4 = building_TurretGun != null && building_TurretGun.CurrentEffectiveVerb != null;
				if (flag4)
				{
					result = building_TurretGun.CurrentEffectiveVerb.verbProps.range;
				}
				else
				{
					IAttackTargetSearcher attackTargetSearcher = searcher as IAttackTargetSearcher;
					bool flag5 = attackTargetSearcher != null;
					if (flag5)
					{
						Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
						bool flag6 = currentEffectiveVerb != null;
						if (flag6)
						{
							return currentEffectiveVerb.verbProps.range;
						}
					}
					result = 1f;
				}
			}
			return result;
		}

		// Token: 0x0600029A RID: 666 RVA: 0x00035A00 File Offset: 0x00033C00
		public static bool HasCombatJob(Pawn pawn, bool checkManTurretJob, bool checkReloadJob)
		{
			bool flag = pawn.jobs == null || pawn.CurJob == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Job curJob = pawn.CurJob;
				if (checkManTurretJob)
				{
					result = (curJob.def == JobDefOf.ManTurret);
				}
				else if (checkReloadJob)
				{
					result = (curJob.def == CE_JobDefOf.ReloadWeapon || curJob.def == CE_JobDefOf.ReloadTurret);
				}
				else
				{
					result = (curJob.def == JobDefOf.AttackMelee || curJob.def == JobDefOf.AttackStatic || curJob.def == JobDefOf.UseVerbOnThing || curJob.def == JobDefOf.UseVerbOnThingStatic || curJob.def == JobDefOf.Wait_Combat);
				}
			}
			return result;
		}

		// Token: 0x0600029B RID: 667 RVA: 0x00035ABC File Offset: 0x00033CBC
		public static LocalTargetInfo CurrentLocalTargetInfo(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = pawn.jobs != null && pawn.CurJob != null;
				if (flag2)
				{
					return pawn.CurJob.targetA;
				}
			}
			Building_TurretGunCE building_TurretGunCE = thing as Building_TurretGunCE;
			bool flag3 = building_TurretGunCE != null;
			LocalTargetInfo result;
			if (flag3)
			{
				result = building_TurretGunCE.CurrentTarget;
			}
			else
			{
				Building_TurretGun building_TurretGun = thing as Building_TurretGun;
				bool flag4 = building_TurretGun != null;
				if (flag4)
				{
					result = building_TurretGun.CurrentTarget;
				}
				else
				{
					IAttackTargetSearcher attackTargetSearcher = thing as IAttackTargetSearcher;
					bool flag5 = attackTargetSearcher != null;
					if (flag5)
					{
						Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
						bool flag6 = currentEffectiveVerb != null;
						if (flag6)
						{
							return currentEffectiveVerb.CurrentTarget;
						}
					}
					result = null;
				}
			}
			return result;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x00035B80 File Offset: 0x00033D80
		public static bool HasDefendDuty(Pawn pawn)
		{
			bool flag = pawn.mindState != null && pawn.mindState.duty != null;
			if (flag)
			{
				DutyDef def = pawn.mindState.duty.def;
				bool flag2 = def == DutyDefOf.Defend || def == DutyDefOf.DefendBase || def == DutyDefOf.Escort || def == DutyDefOf.Follow || def == DutyDefOf.Build || def == DutyDefOf.TravelOrLeave || def == DutyDefOf.TravelOrWait;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600029D RID: 669 RVA: 0x00035C08 File Offset: 0x00033E08
		public static bool HasEscortDuty(Pawn pawn)
		{
			bool flag = pawn.mindState != null && pawn.mindState.duty != null;
			if (flag)
			{
				DutyDef def = pawn.mindState.duty.def;
				bool flag2 = def == DutyDefOf.Escort;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600029E RID: 670 RVA: 0x00035C5C File Offset: 0x00033E5C
		public static bool HasExitOrNonCombatJob(Pawn pawn)
		{
			bool flag = AdvancedAI.HasExitJob(pawn);
			return flag || !AdvancedAI.HasCombatJob(pawn, true, false);
		}

		// Token: 0x0600029F RID: 671 RVA: 0x00035C88 File Offset: 0x00033E88
		public static bool HasExitJob(Pawn pawn)
		{
			bool flag = pawn.jobs == null || pawn.CurJob == null;
			return !flag && pawn.CurJob.exitMapOnArrival;
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x00035CC4 File Offset: 0x00033EC4
		public static bool ShouldOverrideJob(Pawn pawn)
		{
			bool flag;
			if (pawn == null)
			{
				flag = (null != null);
			}
			else
			{
				Pawn_StanceTracker stances = pawn.stances;
				flag = (((stances != null) ? stances.curStance : null) != null);
			}
			bool flag2 = flag && pawn.stances.curStance is Stance_Warmup;
			bool result;
			if (flag2)
			{
				result = false;
			}
			else
			{
				bool flag3 = pawn.jobs != null && pawn.CurJob != null;
				if (flag3)
				{
					bool flag4 = pawn.CurJob.exitMapOnArrival || pawn.CurJobDef == CE_JobDefOf.ReloadWeapon || pawn.CurJobDef == CE_JobDefOf.ReloadTurret || pawn.CurJobDef == CE_JobDefOf.RunForCover;
					if (flag4)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x00035D6C File Offset: 0x00033F6C
		public static bool HasCarryThing(Pawn pawn)
		{
			return pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null;
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x00035DA4 File Offset: 0x00033FA4
		public static bool ShouldIgnoreLeaveDesire(Pawn pawn)
		{
			bool flag = AdvancedAI.HasDefendBaseDuty(pawn);
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = AdvancedAI.PawnIsDoctor(pawn) || (SkyAiCore.Settings.enemyRaidLeadersCouldFlee && AdvancedAI.IsRaidLeaderOrSquadCommander(pawn));
				if (flag2)
				{
					AdvancedAI_TendUtility.InjurySeverity injurySeverity;
					AdvancedAI_TendUtility.RequireTreatment(pawn, out injurySeverity);
					result = (injurySeverity < AdvancedAI_TendUtility.InjurySeverity.severe);
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x00035DFC File Offset: 0x00033FFC
		public static Faction DarknessFaction
		{
			get
			{
				bool darknestNightEnabled = SkyAiCore.Settings.DarknestNightEnabled;
				Faction result;
				if (darknestNightEnabled)
				{
					bool flag = AdvancedAI.darknessFaction == null;
					if (flag)
					{
						AdvancedAI.darknessFaction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Darkness", false));
					}
					result = AdvancedAI.darknessFaction;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x00035E50 File Offset: 0x00034050
		public static bool HasFobbidenFaction(Pawn pawn)
		{
			bool flag = pawn.Faction != null;
			if (flag)
			{
				bool flag2 = (AdvancedAI.DarknessFaction != null && pawn.Faction == AdvancedAI.DarknessFaction) || pawn.Faction.def.techLevel == TechLevel.Animal || pawn.Faction == Faction.OfMechanoids;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x00035EB4 File Offset: 0x000340B4
		public static bool HasDefendBaseDuty(Pawn pawn)
		{
			bool flag = pawn.mindState != null && pawn.mindState.duty != null;
			if (flag)
			{
				PawnDuty duty = pawn.mindState.duty;
				bool flag2 = duty.def == DutyDefOf.DefendBase;
				if (flag2)
				{
					return true;
				}
				bool flag3 = duty.def == DutyDefOf.Escort || duty.def == DutyDefOf.Follow;
				if (flag3)
				{
					LocalTargetInfo focus = duty.focus;
					bool flag4 = focus != null && focus.Pawn != null;
					if (flag4)
					{
						bool flag5 = AdvancedAI.IsAlly(pawn, focus.Pawn, false) && focus.Pawn.mindState != null && focus.Pawn.mindState.duty != null && focus.Pawn.mindState.duty.def == DutyDefOf.DefendBase;
						if (flag5)
						{
							return true;
						}
					}
				}
			}
			return AdvancedAI.IsPawnHomeMap(pawn, pawn.Map);
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x00035FD8 File Offset: 0x000341D8
		public static bool IsUniquePawn(Pawn pawn)
		{
			bool flag = pawn.Faction != null && pawn.Faction.def == FactionDefOf.Ancients && pawn.health.hediffSet.HasHediff(HediffDefOf.CryptosleepSickness, false);
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool trader = pawn.kindDef.trader;
				if (trader)
				{
					result = true;
				}
				else
				{
					bool flag2 = !pawn.questTags.NullOrEmpty<string>();
					if (flag2)
					{
						result = true;
					}
					else
					{
						bool flag3;
						if (pawn == null)
						{
							flag3 = (null != null);
						}
						else
						{
							Pawn_MindState mindState = pawn.mindState;
							flag3 = (((mindState != null) ? mindState.duty : null) != null);
						}
						bool flag4 = flag3 && (pawn.mindState.duty.def == DutyDefOf.EnterTransporter || pawn.mindState.duty.def == DutyDefOf.TakeWoundedGuest || pawn.mindState.duty.def == DutyDefOf.LoadAndEnterTransporters);
						result = flag4;
					}
				}
			}
			return result;
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x000360C8 File Offset: 0x000342C8
		public static bool HasKidnapOrStealJob(Pawn pawn)
		{
			bool flag = pawn.jobs == null || pawn.CurJob == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.CurJobDef == JobDefOf.Steal || pawn.CurJobDef == JobDefOf.Kidnap;
				result = flag2;
			}
			return result;
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x00036120 File Offset: 0x00034320
		public static bool InProcessOfTreatmentJob(Pawn pawn)
		{
			bool flag = pawn.jobs == null || pawn.CurJob == null || pawn.Faction == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.CurJob.def == JobDefOf.Wait;
				if (flag2)
				{
					Map map = pawn.MapHeld ?? Find.CurrentMap;
					bool flag3 = map == null;
					if (flag3)
					{
						return false;
					}
					IntVec3 positionHeld = pawn.PositionHeld;
					bool flag4 = !positionHeld.InBounds(map);
					if (flag4)
					{
						return false;
					}
					foreach (IntVec3 c in GenAdjFast.AdjacentCells8Way(positionHeld))
					{
						bool flag5 = !c.InBounds(map);
						if (!flag5)
						{
							List<Thing> list = map.thingGrid.ThingsListAtFast(c);
							bool flag6 = !list.NullOrEmpty<Thing>();
							if (flag6)
							{
								foreach (Thing thing in list)
								{
									Pawn pawn2 = thing as Pawn;
									bool flag7 = pawn2 != null && AdvancedAI.IsAlly(pawn, pawn2, true) && pawn2.jobs != null && pawn2.jobs.curJob != null && pawn2.jobs.curJob.def == JobDefOfAI.AITend;
									if (flag7)
									{
										return true;
									}
								}
							}
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x000362D4 File Offset: 0x000344D4
		public static bool PlayerPawnNotDraftedOrEnemy(Pawn pawn)
		{
			bool flag = SkyAiCore.Settings.colonyPawnsWillUseAdvancedExplosionDetection && pawn.IsColonist && !pawn.Drafted;
			return flag || (pawn != null && !pawn.IsColonist);
		}

		// Token: 0x060002AA RID: 682 RVA: 0x00036320 File Offset: 0x00034520
		public static bool IsActiveTarget(Thing searcher, Thing target, bool checkIsImportantTarget = true, bool checkForHostile = false)
		{
			Pawn pawn = searcher as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				IAttackTarget attackTarget = target as IAttackTarget;
				bool flag2 = attackTarget != null && attackTarget.ThreatDisabled(pawn);
				if (flag2)
				{
					return false;
				}
			}
			bool flag3 = checkIsImportantTarget && !AdvancedAI.IsImportantTarget(target);
			bool result;
			if (flag3)
			{
				result = false;
			}
			else
			{
				Pawn pawn2 = target as Pawn;
				bool flag4 = pawn2 != null;
				if (flag4)
				{
					bool flag5 = !AdvancedAI.IsActivePawn(pawn2) || pawn.IsInvisible();
					if (flag5)
					{
						return false;
					}
				}
				bool flag6 = checkForHostile && target.Faction != null && !searcher.HostileTo(target.Faction);
				result = !flag6;
			}
			return result;
		}

		// Token: 0x060002AB RID: 683 RVA: 0x000363DC File Offset: 0x000345DC
		public static bool IsImportantTarget(Thing target)
		{
			bool flag = !SkyAiCore.Settings.enemiesWillNotAttackNonAggressiveColonyAnimals;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				Pawn pawn = target as Pawn;
				bool flag2 = pawn != null && pawn.RaceProps.Animal;
				result = (!flag2 || pawn.InAggroMentalState || (pawn.mindState.enemyTarget != null && pawn.mindState.enemyTarget.Spawned && Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick <= 360) || pawn.CurJobDef == JobDefOf.AttackMelee || pawn.CurJobDef == JobDefOf.UseVerbOnThing || pawn.CurJobDef == JobDefOf.UseVerbOnThingStatic || pawn.CurJobDef == JobDefOfAI.AnimalRangeAttack);
			}
			return result;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x000364AC File Offset: 0x000346AC
		public static List<Thing> ThingsToSteal(Pawn pawn, float distance, bool checkRegions, bool sort)
		{
			List<Thing> list = new List<Thing>();
			IEnumerable<IntVec3> source = from c in GenRadial.RadialCellsAround(pawn.Position, distance, true)
			where c.InBounds(pawn.Map)
			select c;
			IEnumerable<IntVec3> source2 = from c in source
			where AdvancedAI.PawnAndNeighborRegions(pawn).Contains(c.GetRegion(pawn.Map, RegionType.Set_Passable))
			select c;
			int num = checkRegions ? source2.Count<IntVec3>() : source.Count<IntVec3>();
			for (int i = 0; i < num; i++)
			{
				List<Thing> thingList = source.ElementAt(i).GetThingList(pawn.Map);
				for (int j = 0; j < thingList.Count<Thing>(); j++)
				{
					Thing thing = thingList[j];
					bool flag = pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == Faction.OfPlayer || !pawn.Map.areaManager.Home[thing.Position];
					bool flag2 = flag && pawn.CanReach(thing.Position, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn) && AdvancedAI.IsFreeCell(thing.Position, pawn.Map);
					if (flag2)
					{
						Corpse corpse = thing as Corpse;
						bool flag3 = corpse != null && corpse.GetRotStage() == RotStage.Fresh && corpse.InnerPawn.GetStatValue(StatDefOf.Mass, true) <= 20f;
						if (flag3)
						{
							list.Add(thing);
						}
						bool flag4 = AdvancedAI.IsStealableItem(pawn, thing, 250f);
						if (flag4)
						{
							list.Add(thing);
						}
					}
				}
			}
			bool flag5 = list.Count > 1 && sort;
			if (flag5)
			{
				list.SortBy((Thing t) => AdvancedAI.GetValue(t));
			}
			return list;
		}

		// Token: 0x060002AD RID: 685 RVA: 0x000366C0 File Offset: 0x000348C0
		public static bool IsStealableItem(Pawn thief, Thing item, float minValue)
		{
			bool flag = item == null || !item.def.stealable;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = item.IsBurning();
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable).Accepts(item);
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = AdvancedAI.GetValue(item) * (float)item.stackCount < minValue;
						if (flag4)
						{
							result = false;
						}
						else
						{
							bool flag5 = AdvancedAI.InDangerousCellList(thief, item.Position);
							if (flag5)
							{
								result = false;
							}
							else
							{
								bool flag6 = AdvancedAI_TakeAndEquipUtility.EnemyThreatOnCell(thief, item.Position);
								result = !flag6;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00036768 File Offset: 0x00034968
		public static bool IsStealableItem(Pawn thief, Thing item, float minValue, float chance)
		{
			bool flag = Rand.Chance(chance);
			return flag && AdvancedAI.IsStealableItem(thief, item, minValue);
		}

		// Token: 0x060002AF RID: 687 RVA: 0x00036794 File Offset: 0x00034994
		public static float GetValue(Thing thing)
		{
			return (float)thing.stackCount * thing.MarketValue * (1f / thing.GetStatValue(StatDefOf.Mass, true));
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x000367C8 File Offset: 0x000349C8
		public static bool TryFindBestItemToSteal(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null)
		{
			AdvancedAI.<>c__DisplayClass167_0 CS$<>8__locals1 = new AdvancedAI.<>c__DisplayClass167_0();
			CS$<>8__locals1.thief = thief;
			CS$<>8__locals1.disallowed = disallowed;
			item = null;
			bool flag = CS$<>8__locals1.thief == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = map == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !CS$<>8__locals1.thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = !map.reachability.CanReachMapEdge(CS$<>8__locals1.thief.Position, TraverseParms.For(CS$<>8__locals1.thief, Danger.Some, TraverseMode.ByPawn, false, false, false)) || (CS$<>8__locals1.thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some, false, false, false)));
						if (flag4)
						{
							result = false;
						}
						else
						{
							item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false, false, false), maxDist, new Predicate<Thing>(CS$<>8__locals1.<TryFindBestItemToSteal>g__validator|0), (Thing x) => StealAIUtility.GetValue(x), 15, 15);
							result = (item != null);
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x000368F0 File Offset: 0x00034AF0
		public static bool TryFindGoodKidnapVictim(Pawn kidnapper, float maxDist, out Pawn victim, List<Thing> disallowed = null)
		{
			AdvancedAI.<>c__DisplayClass168_0 CS$<>8__locals1 = new AdvancedAI.<>c__DisplayClass168_0();
			CS$<>8__locals1.kidnapper = kidnapper;
			CS$<>8__locals1.disallowed = disallowed;
			bool flag = !CS$<>8__locals1.kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !CS$<>8__locals1.kidnapper.Map.reachability.CanReachMapEdge(CS$<>8__locals1.kidnapper.Position, TraverseParms.For(CS$<>8__locals1.kidnapper, Danger.Some, TraverseMode.ByPawn, false, false, false));
			bool result;
			if (flag)
			{
				victim = null;
				result = false;
			}
			else
			{
				victim = (Pawn)GenClosest.ClosestThingReachable(CS$<>8__locals1.kidnapper.Position, CS$<>8__locals1.kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false, false, false), maxDist, new Predicate<Thing>(CS$<>8__locals1.<TryFindGoodKidnapVictim>g__validator|0), null, 0, -1, false, RegionType.Set_Passable, false);
				result = (victim != null);
			}
			return result;
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x000369C4 File Offset: 0x00034BC4
		public static bool ShouldAttackIfHive(Thing searcher, Thing target)
		{
			Hive hive = target as Hive;
			bool flag = hive == null || hive.Map == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = AdvancedAI.IsPawnHomeMap(searcher, hive.Map);
				result = flag2;
			}
			return result;
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00036A0C File Offset: 0x00034C0C
		public static bool IsPawnHomeMap(Thing searcher, Map map)
		{
			bool flag = map == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = searcher.Faction != null;
				if (flag2)
				{
					bool isPlayer = searcher.Faction.IsPlayer;
					if (isPlayer)
					{
						result = map.IsPlayerHome;
					}
					else
					{
						result = (map.info != null && map.info.parent != null && map.info.parent.Faction != null && map.info.parent.Faction == searcher.Faction);
					}
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x00036A98 File Offset: 0x00034C98
		public static bool IsManningTurret(Pawn pawn)
		{
			return pawn.jobs != null && pawn.CurJobDef == JobDefOf.ManTurret;
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x00036AC4 File Offset: 0x00034CC4
		public static bool EnemyInCloseBattle(Pawn pawn, Thing enemy)
		{
			LocalTargetInfo a = AdvancedAI.CurrentLocalTargetInfo(enemy);
			return a != null && a.Pawn != null && AdvancedAI.IsInCloseWithTarget(enemy, a.Pawn) && AdvancedAI.IsAlly(pawn, a.Pawn, false);
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x00036B14 File Offset: 0x00034D14
		public static bool ExtraTargetValidator(Thing searcher, Thing target)
		{
			bool flag = !AdvancedAI.ShouldAttackIfHive(searcher, target);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Pawn pawn = searcher as Pawn;
				bool flag2 = pawn != null;
				if (flag2)
				{
					bool flag3 = !pawn.IsColonist && AdvancedAI.FriendlyFireThreatInShootline(pawn, searcher.Position, target.Position, 2f, 60f, 55f, 3f);
					result = (!flag3 && AdvancedAI.IsImportantTarget(target) && !AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, target, AdvancedAI.MinDistance(pawn, null), 0f));
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x00036BB0 File Offset: 0x00034DB0
		public static IAttackTarget AttackTarget(Pawn pawn)
		{
			IAttackTarget result = null;
			IOrderedEnumerable<IAttackTarget> orderedEnumerable = from c in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
			orderby pawn.Position.DistanceTo(c.Thing.Position)
			select c;
			foreach (IAttackTarget attackTarget in orderedEnumerable)
			{
				bool flag = attackTarget.Thing.Faction == Faction.OfPlayer && AdvancedAI.IsActiveTarget(pawn, attackTarget.Thing, true, false) && !AdvancedAI.InDangerousCellList(pawn, attackTarget.Thing.Position);
				if (flag)
				{
					bool flag2 = pawn.CanReach(attackTarget.Thing, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings);
					if (flag2)
					{
						result = attackTarget;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x00036CB8 File Offset: 0x00034EB8
		public static bool PositionUnderCrossfire(Thing searcher, IntVec3 loc, Thing enemyTarget, bool onlyForRanged, bool useHeavySearch)
		{
			bool flag = !SkyAiCore.Settings.enemiesWillBeCheckingCrossfire;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				Pawn pawn = searcher as Pawn;
				bool flag3 = pawn == null;
				if (flag3)
				{
					result = false;
				}
				else
				{
					bool flag4 = onlyForRanged && AdvancedAI.IsMeleeVerb(pawn, null);
					if (flag4)
					{
						result = false;
					}
					else
					{
						IOrderedEnumerable<IAttackTarget> orderedEnumerable = from p in searcher.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
						where loc.DistanceTo(p.Thing.Position) <= SkyAiCore.Settings.combatKeepRange
						select p into c
						orderby loc.DistanceTo(c.Thing.Position)
						select c;
						foreach (IAttackTarget attackTarget in orderedEnumerable)
						{
							bool flag5 = (enemyTarget == null || attackTarget.Thing != enemyTarget) && GenSight.LineOfSight(attackTarget.Thing.Position, loc, searcher.Map, false, null, 0, 0) && AdvancedAI.IsActiveTarget(searcher, attackTarget.Thing, true, false);
							if (flag5)
							{
								Verb verb = AdvancedAI.PrimaryVerb(attackTarget.Thing);
								bool flag6 = verb != null && !verb.IsMeleeAttack;
								if (flag6)
								{
									if (useHeavySearch)
									{
										Verb_LaunchProjectileCE verb_LaunchProjectileCE = verb as Verb_LaunchProjectileCE;
										bool flag7 = verb_LaunchProjectileCE != null;
										if (flag7)
										{
											ShootLine shootLine;
											bool flag8 = pawn.Position.DistanceTo(attackTarget.Thing.Position) <= AdvancedAI.EffectiveRange(attackTarget.Thing) && verb_LaunchProjectileCE.TryFindCEShootLineFromTo(attackTarget.Thing.Position, loc, ref shootLine);
											if (flag8)
											{
												flag2 = true;
												break;
											}
										}
									}
									else
									{
										bool flag9 = pawn.Position.DistanceTo(attackTarget.Thing.Position) <= AdvancedAI.EffectiveRange(attackTarget.Thing);
										if (flag9)
										{
											flag2 = true;
											break;
										}
									}
								}
							}
						}
						result = flag2;
					}
				}
			}
			return result;
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x00036ED4 File Offset: 0x000350D4
		public static bool CanMove(Pawn pawn)
		{
			return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
		}

		// Token: 0x060002BA RID: 698 RVA: 0x00036EFC File Offset: 0x000350FC
		public static bool IsValidLoc(Pawn pawn, IntVec3 intVec, PathEndMode mode = PathEndMode.OnCell)
		{
			bool flag = !AdvancedAI.IsValidLoc(intVec);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = mode > PathEndMode.None;
				result = (!flag2 || pawn.CanReach(intVec, mode, Danger.Deadly, false, false, TraverseMode.ByPawn));
			}
			return result;
		}

		// Token: 0x060002BB RID: 699 RVA: 0x00036F40 File Offset: 0x00035140
		public static bool IsValidLoc(IntVec3 intVec)
		{
			bool flag = intVec.IsValid && intVec != new IntVec3(0, 0, 0);
			return flag || intVec != IntVec3.Invalid;
		}

		// Token: 0x060002BC RID: 700 RVA: 0x00036F80 File Offset: 0x00035180
		public static void Notify_DangerousExploderAboutToExplode(Pawn pawn, Thing exploder)
		{
			bool flag = pawn.RaceProps.intelligence >= Intelligence.Humanlike && exploder != null;
			if (flag)
			{
				pawn.mindState.knownExploder = exploder;
				bool flag2 = pawn.jobs != null;
				if (flag2)
				{
					pawn.jobs.CheckForJobOverride();
				}
			}
		}

		// Token: 0x060002BD RID: 701 RVA: 0x00036FD0 File Offset: 0x000351D0
		public static bool DangerousNonLOSTarget(Pawn pawn, Building building, float radius = 8f)
		{
			bool flag = !SkyAiCore.Settings.enableNonLOSdangerTargetsCheck;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				bool flag3 = !GenSight.LineOfSightToThing(pawn.Position, building, pawn.Map, false, null) && pawn.Position.DistanceTo(building.Position) <= 35f;
				if (flag3)
				{
					IntVec3 loc = AdvancedAI.CellNearPosition(pawn, pawn.Position, building.Position);
					bool isValid = loc.IsValid;
					if (isValid)
					{
						Region region = loc.GetRegion(pawn.Map, RegionType.Set_Passable);
						bool flag4 = region != null;
						if (flag4)
						{
							foreach (IntVec3 c3 in region.Cells)
							{
								Pawn firstPawn = c3.GetFirstPawn(pawn.Map);
								bool flag5 = firstPawn != null && firstPawn != pawn && AdvancedAI.IsHostile(firstPawn, pawn);
								if (flag5)
								{
									flag2 = true;
									break;
								}
							}
						}
					}
					IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(building.Position, radius, true);
					Func<IntVec3, bool> <>9__0;
					Func<IntVec3, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(pawn.Map)));
					}
					foreach (IntVec3 intVec in source.Where(predicate))
					{
						bool flag6 = pawn.Position.DistanceTo(intVec) <= 5f && GenSight.LineOfSight(pawn.Position, intVec, pawn.Map, false, null, 0, 0);
						if (!flag6)
						{
							Pawn firstPawn2 = intVec.GetFirstPawn(pawn.Map);
							bool flag7 = firstPawn2 != null && AdvancedAI.IsHostile(firstPawn2, pawn);
							if (flag7)
							{
								bool enableAdvancedNonLOSdangerTargetCheck = SkyAiCore.Settings.enableAdvancedNonLOSdangerTargetCheck;
								if (!enableAdvancedNonLOSdangerTargetCheck)
								{
									flag2 = true;
									break;
								}
								foreach (IntVec3 c2 in GenRadial.RadialCellsAround(firstPawn2.Position, 3f, true))
								{
									Building firstBuilding = c2.GetFirstBuilding(pawn.Map);
									bool flag8 = (firstBuilding != null && AdvancedAI_SiegeUtility.IsBarricade(firstBuilding)) || firstBuilding is Building_Door;
									if (flag8)
									{
										flag2 = true;
										break;
									}
								}
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x060002BE RID: 702 RVA: 0x000372DC File Offset: 0x000354DC
		public static bool EnemyBehindDefensivePosition(Pawn pawn, IntVec3 position, float radius, out Thing enemy)
		{
			enemy = null;
			bool result = false;
			IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(position, radius, true);
			Func<IntVec3, bool> <>9__0;
			Func<IntVec3, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(pawn.Map)));
			}
			foreach (IntVec3 c3 in source.Where(predicate))
			{
				Pawn firstPawn = c3.GetFirstPawn(pawn.Map);
				bool flag = firstPawn != null;
				if (flag)
				{
					bool flag2 = pawn.Faction == null || AdvancedAI.IsHostile(firstPawn, pawn);
					if (flag2)
					{
						foreach (IntVec3 c2 in GenRadial.RadialCellsAround(firstPawn.Position, 2f, true))
						{
							bool flag3 = !c2.InBounds(pawn.Map);
							if (!flag3)
							{
								Building firstBuilding = c2.GetFirstBuilding(pawn.Map);
								bool flag4 = firstBuilding != null && pawn.Position.DistanceTo(firstBuilding.Position) <= pawn.Position.DistanceTo(position) && (AdvancedAI_SiegeUtility.IsBarricade(firstBuilding) || firstBuilding is Building_Turret || firstBuilding is Building_Door);
								if (flag4)
								{
									enemy = firstPawn;
									result = true;
									break;
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002BF RID: 703 RVA: 0x000374B4 File Offset: 0x000356B4
		public static bool IsDangerousCoverPosition(Pawn pawn, IntVec3 position)
		{
			bool flag = AdvancedAI.AnyAllyPawnsAroundForPawn(pawn, position, pawn.Map, 2f, true);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Thing thing;
				bool flag2 = AdvancedAI.DangerousNonLOSTarget(pawn, position, out thing, 9f, false);
				if (flag2)
				{
					bool flag3;
					if (thing != null && AdvancedAI.IsActiveTarget(pawn, thing, true, false))
					{
						Pawn pawn2 = thing as Pawn;
						if (pawn2 != null && pawn2 != null)
						{
							flag3 = !AdvancedAI.HasCombatJob(pawn2, false, true);
							goto IL_5E;
						}
					}
					flag3 = false;
					IL_5E:
					bool flag4 = flag3;
					if (flag4)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00037530 File Offset: 0x00035730
		public static bool DangerousNonLOSTarget(Pawn pawn, IntVec3 position, out Thing enemy, float radius = 9f, bool ignoreSettingRule = false)
		{
			bool flag = !SkyAiCore.Settings.enableNonLOSdangerTargetsCheck && !ignoreSettingRule;
			bool result;
			if (flag)
			{
				enemy = null;
				result = false;
			}
			else
			{
				bool flag2 = false;
				enemy = null;
				bool flag3 = !GenSight.LineOfSight(pawn.Position, position, pawn.Map, false, null, 0, 0) && pawn.Position.DistanceTo(position) <= 35f;
				if (flag3)
				{
					bool flag4 = position.GetFirstBuilding(pawn.Map) != null;
					if (flag4)
					{
						bool isValid = AdvancedAI.CellNearPosition(pawn, pawn.Position, position).IsValid;
						if (isValid)
						{
						}
					}
					Region region = position.GetRegion(pawn.Map, RegionType.Set_Passable);
					bool flag5 = region != null;
					if (flag5)
					{
						foreach (IntVec3 c in region.Cells)
						{
							Pawn firstPawn = c.GetFirstPawn(pawn.Map);
							bool flag6 = firstPawn != null && AdvancedAI.IsHostile(firstPawn, pawn) && AdvancedAI.CamperPawn(firstPawn);
							if (flag6)
							{
								flag2 = true;
								enemy = firstPawn;
								break;
							}
						}
					}
					foreach (IntVec3 intVec in GenRadial.RadialCellsAround(position, radius, true))
					{
						bool flag7 = !intVec.InBounds(pawn.Map);
						if (!flag7)
						{
							bool flag8 = pawn.Position.DistanceTo(intVec) <= 5f && GenSight.LineOfSight(pawn.Position, intVec, pawn.Map, false, null, 0, 0);
							if (!flag8)
							{
								Pawn firstPawn2 = intVec.GetFirstPawn(pawn.Map);
								bool flag9 = firstPawn2 != null && AdvancedAI.IsHostile(firstPawn2, pawn) && AdvancedAI.CamperPawn(firstPawn2);
								if (flag9)
								{
									bool flag10 = SkyAiCore.Settings.enableAdvancedNonLOSdangerTargetCheck || ignoreSettingRule;
									if (!flag10)
									{
										flag2 = true;
										enemy = firstPawn2;
										break;
									}
									foreach (IntVec3 c2 in GenRadial.RadialCellsAround(firstPawn2.Position, 3f, true))
									{
										Building firstBuilding = c2.GetFirstBuilding(pawn.Map);
										bool flag11 = (firstBuilding != null && AdvancedAI_SiegeUtility.IsBarricade(firstBuilding)) || firstBuilding is Building_Door;
										if (flag11)
										{
											flag2 = true;
											enemy = firstPawn2;
											break;
										}
									}
								}
							}
						}
					}
				}
				result = flag2;
			}
			return result;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00037810 File Offset: 0x00035A10
		public static bool IsWater(IntVec3 position, Map map)
		{
			bool flag = !position.InBounds(map);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
				result = (terrainDef.HasTag("Water") || terrainDef == TerrainDefOfAI.Marsh);
			}
			return result;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0003785C File Offset: 0x00035A5C
		public static bool CanActiveCover(Pawn pawn, out float ratio, float activePawnsRatio = 0.2f)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			Lord lord = pawn.GetLord();
			bool flag = lord == null || mapComponent_SkyAI == null;
			bool result;
			if (flag)
			{
				ratio = 0f;
				result = false;
			}
			else
			{
				bool flag2 = mapComponent_SkyAI.activeCover == null;
				if (flag2)
				{
					mapComponent_SkyAI.activeCover = new Dictionary<Pawn, Thing>();
				}
				int num = 0;
				foreach (KeyValuePair<Pawn, Thing> keyValuePair in mapComponent_SkyAI.activeCover)
				{
					bool flag3 = keyValuePair.Key != null;
					if (flag3)
					{
						Lord lord2 = keyValuePair.Key.GetLord();
						bool flag4 = lord2 != null && lord == lord2;
						if (flag4)
						{
							num++;
						}
					}
				}
				ratio = (float)num / (float)lord.ownedPawns.Count;
				result = (ratio <= activePawnsRatio || AdvancedAI.PrimaryIsMachineGun(pawn));
			}
			return result;
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00037958 File Offset: 0x00035B58
		public static bool ActiveCoverEnemyIsActive(Pawn pawn, Verb verb)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			Dictionary<Pawn, Thing> activeCover = mapComponent_SkyAI.activeCover;
			bool flag = activeCover != null;
			if (flag)
			{
				int i = 0;
				while (i < activeCover.Count)
				{
					KeyValuePair<Pawn, Thing> keyValuePair = activeCover.ElementAt(i);
					bool flag2 = keyValuePair.Key != null && keyValuePair.Value != null && keyValuePair.Key == pawn;
					if (flag2)
					{
						bool flag3 = Rand.Value > 0.95f && AdvancedAI.IsActiveTarget(pawn, keyValuePair.Value, false, false) && AdvancedAI.ClosestEnemyCamperFocusBuilding(pawn, keyValuePair.Value, verb, 3f) != null;
						if (flag3)
						{
							return true;
						}
						activeCover.Remove(keyValuePair.Key);
						return false;
					}
					else
					{
						i++;
					}
				}
			}
			return false;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00037A2C File Offset: 0x00035C2C
		public static bool RaidLeaderIsActive(RaidData raidData)
		{
			bool flag = raidData.raidLeader != null;
			if (flag)
			{
				Pawn raidLeader = raidData.raidLeader;
				bool flag2 = AdvancedAI.IsActivePawn(raidLeader);
				if (flag2)
				{
					bool flag3 = raidData.raidStage != RaidData.RaidStage.fleeing && AdvancedAI.HasExitJob(raidLeader);
					return !flag3;
				}
			}
			return false;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00037A84 File Offset: 0x00035C84
		public static int CalculateDistance(Pawn pawn, LocalTargetInfo targetInfo, int min, int max)
		{
			bool flag = min >= max;
			if (flag)
			{
				max = min + 1;
			}
			float num = 20f;
			bool flag2 = targetInfo != null;
			float t;
			if (flag2)
			{
				num = pawn.Position.DistanceTo(targetInfo.Cell);
				bool flag3 = targetInfo.Thing != null;
				if (flag3)
				{
					float num2 = AdvancedAI.EffectiveRange(targetInfo.Thing);
					t = Mathf.Pow(Mathf.Clamp01(num / num2), 2f);
					return Mathf.RoundToInt(Mathf.Lerp((float)min, (float)max, t));
				}
			}
			t = Mathf.Max(num - (float)max, 0f) / num;
			return Mathf.RoundToInt(Mathf.Lerp((float)min, (float)max, t));
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00037B3C File Offset: 0x00035D3C
		public static bool CanChooseTarget(Pawn pawn, Thing target)
		{
			bool result = true;
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			Dictionary<Pawn, Thing> activeCover = mapComponent_SkyAI.activeCover;
			bool flag = activeCover != null;
			if (flag)
			{
				int num = 0;
				for (int i = 0; i < activeCover.Count; i++)
				{
					KeyValuePair<Pawn, Thing> keyValuePair = activeCover.ElementAt(i);
					bool flag2 = keyValuePair.Value != null && keyValuePair.Value == target;
					if (flag2)
					{
						num++;
					}
					bool flag3 = num > 3;
					if (flag3)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00037BCC File Offset: 0x00035DCC
		public static Building ClosestPlayerDefensivePosition(Pawn pawn, Verb verb, float dist, List<Thing> targetKeepRangeList)
		{
			Building result = null;
			bool flag = !targetKeepRangeList.NullOrEmpty<Thing>();
			if (flag)
			{
				Func<Thing, float> <>9__0;
				Func<Thing, float> keySelector;
				if ((keySelector = <>9__0) == null)
				{
					keySelector = (<>9__0 = ((Thing closestTarget) => pawn.Position.DistanceTo(closestTarget.Position)));
				}
				foreach (Thing thing in targetKeepRangeList.OrderBy(keySelector))
				{
					bool flag2 = thing != null;
					if (flag2)
					{
						Building building = AdvancedAI.ClosestEnemyCamperFocusBuilding(pawn, thing, verb, dist);
						bool flag3 = building != null && AdvancedAI.IsValidLoc(building.Position);
						if (flag3)
						{
							result = building;
							break;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x00037CA0 File Offset: 0x00035EA0
		public static bool CamperPawn(Thing target)
		{
			Pawn pawn = target as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = pawn.Drafted || (!pawn.pather.MovingNow && pawn.Awake());
				if (flag2)
				{
					bool flag3 = AdvancedAI.HasPrimaryLoadedRangedWeapon(pawn);
					if (flag3)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00037D00 File Offset: 0x00035F00
		public static Thing ClosestEnemyCamper(Pawn pawn, Verb verb, float dist, List<Thing> targetKeepRangeList, out Building closestfocusBuilding, bool onlyPlayer)
		{
			Thing result = null;
			closestfocusBuilding = null;
			bool flag = !targetKeepRangeList.NullOrEmpty<Thing>();
			if (flag)
			{
				Func<Thing, float> <>9__0;
				Func<Thing, float> keySelector;
				if ((keySelector = <>9__0) == null)
				{
					keySelector = (<>9__0 = ((Thing closestTarget) => pawn.Position.DistanceTo(closestTarget.Position)));
				}
				foreach (Thing thing in targetKeepRangeList.OrderBy(keySelector))
				{
					bool flag2 = thing != null && ((onlyPlayer && thing.Faction != null && thing.Faction == Faction.OfPlayer) || !onlyPlayer);
					if (flag2)
					{
						bool flag3 = AdvancedAI.CanChooseTarget(pawn, thing) && AdvancedAI.CamperPawn(thing);
						if (flag3)
						{
							Building building = AdvancedAI.ClosestEnemyCamperFocusBuilding(pawn, thing, verb, dist);
							bool flag4 = building != null && AdvancedAI.IsValidLoc(building.Position);
							if (flag4)
							{
								result = thing;
								closestfocusBuilding = building;
								break;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002CA RID: 714 RVA: 0x00037E28 File Offset: 0x00036028
		public static Building ClosestEnemyCamperFocusBuilding(Pawn pawn, Thing target, Verb verb, float dist)
		{
			Building result = null;
			foreach (IntVec3 c in GenRadial.RadialCellsAround(target.Position, dist, true))
			{
				bool flag = !c.InBounds(pawn.Map);
				if (!flag)
				{
					int num = 0;
					Building firstBuilding = c.GetFirstBuilding(pawn.Map);
					bool flag2 = firstBuilding != null;
					if (flag2)
					{
						bool flag3 = AdvancedAI.TryFindShootlineFromTo(pawn.Position, firstBuilding, verb) && (firstBuilding is Building_Door || AdvancedAI_SiegeUtility.IsBarricade(firstBuilding));
						if (flag3)
						{
							bool flag4 = !AdvancedAI.FriendlyFireThreat(pawn, pawn.Position, firstBuilding, AdvancedAI.MinDistance(pawn, verb), 0f) && !AdvancedAI.FriendlyFireThreatInShootline(pawn, pawn.Position, firstBuilding.Position, 2f, 60f, 55f, 3f);
							if (flag4)
							{
								result = firstBuilding;
								break;
							}
							num++;
							bool flag5 = num >= 5;
							if (flag5)
							{
								break;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00037F64 File Offset: 0x00036164
		public static int ShooterSkill(Pawn pawn)
		{
			bool flag = pawn.skills != null && !pawn.skills.skills.NullOrEmpty<SkillRecord>();
			if (flag)
			{
				foreach (SkillRecord skillRecord in pawn.skills.skills)
				{
					bool flag2 = skillRecord.def == SkillDefOf.Shooting;
					if (flag2)
					{
						return skillRecord.Level;
					}
				}
			}
			return 0;
		}

		// Token: 0x060002CC RID: 716 RVA: 0x00038004 File Offset: 0x00036204
		public static int MostExperienced(Pawn pawn)
		{
			int num = 0;
			bool flag = pawn.skills != null && !pawn.skills.skills.NullOrEmpty<SkillRecord>();
			if (flag)
			{
				foreach (SkillRecord skillRecord in pawn.skills.skills)
				{
					num += skillRecord.Level;
				}
			}
			return num;
		}

		// Token: 0x060002CD RID: 717 RVA: 0x00038090 File Offset: 0x00036290
		public static bool IsGoodLeader(Pawn leader)
		{
			return AdvancedAI.IsHumanlikeOnly(leader) && !leader.IsPrisoner && !leader.IsSlave && !leader.WorkTypeIsDisabled(WorkTypeDefOf.Warden) && !leader.WorkTypeIsDisabled(WorkTypeDefOf.Hunting) && !AdvancedAI.PawnIsDoctor(leader) && AdvancedAI.CanMove(leader) && !AdvancedAI.PrimaryIsSiegeWeapon(leader) && !SK_Utility.isMechanical(leader);
		}

		// Token: 0x060002CE RID: 718 RVA: 0x000380F8 File Offset: 0x000362F8
		public static IEnumerable<Pawn> ArmedLordPawns(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			IEnumerable<Pawn> result;
			if (flag)
			{
				result = from p in lord.ownedPawns
				where AdvancedAI.HasPrimaryWeaponOrSwitchToWeaponFromInventory(p, false)
				select p;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060002CF RID: 719 RVA: 0x00038148 File Offset: 0x00036348
		public static IEnumerable<Pawn> RangedLordPawns(Pawn pawn, float rangedRatio)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				IEnumerable<Pawn> enumerable = from p in lord.ownedPawns
				where AdvancedAI.IsRangedPawn(p)
				select p;
				bool flag2 = enumerable.EnumerableNullOrEmpty<Pawn>();
				if (flag2)
				{
					return null;
				}
				bool flag3 = (float)enumerable.Count<Pawn>() / (float)lord.ownedPawns.Count<Pawn>() >= rangedRatio;
				if (flag3)
				{
					return enumerable;
				}
			}
			return null;
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x000381D0 File Offset: 0x000363D0
		public static bool IsRangedPawn(Pawn pawn)
		{
			if (pawn != null)
			{
				ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
				if (thingWithComps != null && thingWithComps != null && thingWithComps.def.IsRangedWeapon)
				{
					return AdvancedAI.PrimaryWeaponRange(pawn) >= 15f;
				}
			}
			return false;
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00038214 File Offset: 0x00036414
		public static List<Thing> HaulableThingsAround(IntVec3 position, Map map, float radius, bool useCenter)
		{
			List<Thing> list = new List<Thing>();
			foreach (IntVec3 c in GenRadial.RadialCellsAround(position, radius, useCenter))
			{
				bool flag = !c.InBounds(map);
				if (!flag)
				{
					List<Thing> thingList = c.GetThingList(map);
					for (int i = 0; i < thingList.Count<Thing>(); i++)
					{
						Thing thing = thingList[i];
						bool flag2;
						if (!thing.def.alwaysHaulable)
						{
							Pawn pawn = thing as Pawn;
							flag2 = (pawn != null && (pawn.Downed || pawn.RaceProps.packAnimal));
						}
						else
						{
							flag2 = true;
						}
						bool flag3 = flag2;
						if (flag3)
						{
							list.Add(thing);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x00038300 File Offset: 0x00036500
		public static int PathNodesCount(Pawn pawn, IntVec3 position)
		{
			PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.NoPassClosedDoorsOrWater, false, false, false), PathEndMode.OnCell, null);
			bool flag = !pawnPath.Found;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int count = pawnPath.NodesReversed.Count;
				pawnPath.ReleaseToPool();
				result = count;
			}
			return result;
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x00038364 File Offset: 0x00036564
		public static void ClearRoleComps(Pawn pawn)
		{
			CompLeaderRole comp = pawn.GetComp<CompLeaderRole>();
			bool flag = comp != null;
			if (flag)
			{
				pawn.AllComps.Remove(comp);
			}
			CompSquadCommanderRole comp2 = pawn.GetComp<CompSquadCommanderRole>();
			bool flag2 = comp2 != null;
			if (flag2)
			{
				pawn.AllComps.Remove(comp2);
			}
			CompGuardRole comp3 = pawn.GetComp<CompGuardRole>();
			bool flag3 = comp3 != null;
			if (flag3)
			{
				pawn.AllComps.Remove(comp3);
			}
			CompDoctorRole comp4 = pawn.GetComp<CompDoctorRole>();
			bool flag4 = comp4 != null;
			if (flag4)
			{
				pawn.AllComps.Remove(comp4);
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x00038529 File Offset: 0x00036729
		[CompilerGenerated]
		internal static bool <DesireToGetRangedWeapon>g__isNotCombatRangedWeapon|23_0(ThingWithComps w)
		{
			return w.def.IsMeleeWeapon || AdvancedAI.IsSiegeWeapon(w);
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x00038541 File Offset: 0x00036741
		[CompilerGenerated]
		internal static bool <MeleeTrashBuilding>g__inDangerousCells|60_4(IntVec3 loc, ref AdvancedAI.<>c__DisplayClass60_1 A_1)
		{
			return A_1.mapComp != null && !A_1.mapComp.dangerousCells.NullOrEmpty<IntVec3>() && A_1.mapComp.dangerousCells.Contains(loc);
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00038574 File Offset: 0x00036774
		[CompilerGenerated]
		internal static bool <UpdateTarget>g__checkTarget|132_0(Thing t, ref AdvancedAI.<>c__DisplayClass132_0 A_1)
		{
			return A_1.currentTarget == null || (A_1.currentTarget != null && A_1.currentTarget.Thing != null && A_1.currentTarget.Thing != t);
		}

		// Token: 0x04000138 RID: 312
		private static readonly SimpleCurve SiegeWidthByRaidCount = new SimpleCurve
		{
			{
				new CurvePoint(20f, 23f),
				true
			},
			{
				new CurvePoint(30f, 28f),
				true
			},
			{
				new CurvePoint(50f, 34f),
				true
			},
			{
				new CurvePoint(70f, 42f),
				true
			},
			{
				new CurvePoint(90f, 50f),
				true
			}
		};

		// Token: 0x04000139 RID: 313
		public static SimpleCurve EffectiveRangeByShotSpreadCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(0.01f, 0.92f),
				true
			},
			{
				new CurvePoint(0.05f, 0.86f),
				true
			},
			{
				new CurvePoint(0.1f, 0.82f),
				true
			},
			{
				new CurvePoint(0.13f, 0.78f),
				true
			},
			{
				new CurvePoint(0.16f, 0.73f),
				true
			},
			{
				new CurvePoint(0.2f, 0.68f),
				true
			}
		};

		// Token: 0x0400013A RID: 314
		private static Faction darknessFaction;

		// Token: 0x020000F6 RID: 246
		public enum ExpireInterval
		{
			// Token: 0x04000310 RID: 784
			utlrafast,
			// Token: 0x04000311 RID: 785
			fast,
			// Token: 0x04000312 RID: 786
			normal,
			// Token: 0x04000313 RID: 787
			slow,
			// Token: 0x04000314 RID: 788
			veryslow,
			// Token: 0x04000315 RID: 789
			ultraslow
		}
	}
}
