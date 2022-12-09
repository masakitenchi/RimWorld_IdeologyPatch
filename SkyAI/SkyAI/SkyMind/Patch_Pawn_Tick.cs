using System;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000039 RID: 57
	[HarmonyPatch(typeof(Pawn), "Tick")]
	internal static class Patch_Pawn_Tick
	{
		// Token: 0x060001C8 RID: 456 RVA: 0x000287B8 File Offset: 0x000269B8
		private static void Postfix(Pawn __instance)
		{
			bool flag = __instance.IsHashIntervalTick(300);
			if (flag)
			{
				bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
				if (debugDisableSkyAI)
				{
					return;
				}
				bool flag2 = AdvancedAI.IsSuitablePawn(__instance) && !AdvancedAI.IsUniquePawn(__instance) && __instance.Faction != null && !AdvancedAI.HasFobbidenFaction(__instance) && !__instance.IsPrisoner && !__instance.IsSlave && !__instance.IsColonist && !AdvancedAI.InProcessOfTreatmentJob(__instance);
				if (flag2)
				{
					bool flag3 = __instance.RaceProps.intelligence >= Intelligence.Humanlike && __instance.Faction.def.techLevel != TechLevel.Animal;
					if (flag3)
					{
						Lord lord = __instance.GetLord();
						AdvancedAI_Classes.CheckPawnLord(__instance);
						RaidData raidData = AdvancedAI.PawnRaidData(__instance);
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(__instance);
						if (flag4)
						{
							string text = "";
							string text2 = "";
							string text3 = "";
							bool flag5 = __instance.mindState.duty != null;
							if (flag5)
							{
								text = __instance.mindState.duty.ToString();
								text2 = __instance.mindState.duty.def.ToString();
								bool flag6 = __instance.mindState.duty.focus != null;
								if (flag6)
								{
									text3 = __instance.mindState.duty.focus.ToString();
								}
							}
							bool flag7 = lord != null;
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: Duty-Lord log. LordPawns cur/max/percent: {2}/{3}/{4}  lordJob: {5} curLordToil: {6} ticksInToil: {7} duty: {8} dutyDef: {9} dutyFocus: {10} raidStage: {11}", new object[]
								{
									__instance,
									__instance.Position,
									lord.ownedPawns.Count,
									lord.numPawnsEverGained,
									(float)lord.ownedPawns.Count / (float)lord.numPawnsEverGained,
									lord.LordJob,
									lord.CurLordToil,
									lord.ticksInToil,
									text,
									text2,
									text3,
									raidData.raidStage
								}));
							}
						}
						AdvancedAI_Classes.DutyAttackSubNodes(__instance, lord, raidData);
						bool debugSetDutyAttackColony = SkyAiCore.Settings.debugSetDutyAttackColony;
						if (debugSetDutyAttackColony)
						{
							AdvancedAI_Classes.DebugSetDutyAttackColony(__instance);
						}
						AdvancedAI_Classes.EvacuateSpecialPawn(__instance, raidData);
						bool enableLostThingsData = SkyAiCore.Settings.enableLostThingsData;
						if (enableLostThingsData)
						{
							AdvancedAI_Classes.ThingOwnerData(__instance);
						}
						bool flag8 = __instance.Faction != null && !__instance.HostileTo(Faction.OfPlayer);
						bool flag9 = !flag8;
						if (flag9)
						{
							bool flag10 = __instance.IsHashIntervalTick(600) && !AdvancedAI.HasPrimaryWeaponOrSwitchToWeaponFromInventory(__instance, true);
							if (flag10)
							{
								AdvancedAI_Classes.EscapeWithoutWeapon(__instance);
							}
							bool flag11 = !SK_Utility.isMechanical(__instance);
							if (flag11)
							{
								bool flag12 = SkyAiCore.Settings.enemiesEscapeChanceExhausted && AdvancedAI.IsExhausted(__instance);
								if (flag12)
								{
									AdvancedAI_Classes.EscapeChanceExhausted(__instance);
								}
								bool flag13 = SkyAiCore.Settings.enemiesEscapeDueToTemperatureInjuries && __instance.health != null && __instance.health.hediffSet.HasTemperatureInjury(TemperatureInjuryStage.Minor);
								if (flag13)
								{
									AdvancedAI_Classes.EscapeDueToTemperatureInjuries(__instance);
								}
							}
							bool flag14 = AdvancedAI.IsHumanlikeOnly(__instance) && !AdvancedAI.HasFleeingDuty(__instance);
							if (flag14)
							{
								bool enableFireExtinguishingMode = SkyAiCore.Settings.enableFireExtinguishingMode;
								if (enableFireExtinguishingMode)
								{
									AdvancedAI_Classes.ExtinguishFire(__instance);
								}
								bool checkInventoryForBetterWeapon = SkyAiCore.Settings.checkInventoryForBetterWeapon;
								if (checkInventoryForBetterWeapon)
								{
									Thing enemyTarget = __instance.mindState.enemyTarget;
									AdvancedAI_Classes.ScanInventoryForBetterWeapon(__instance, SpecialStatic.EnemyCamperFound(__instance, enemyTarget));
								}
								bool flag15 = SkyAiCore.Settings.enableSnipersKiteMechanic && AdvancedAI.PawnIsSniper(__instance);
								if (flag15)
								{
									AdvancedAI_Classes.UseSnipersKiteMechanic(__instance);
								}
							}
						}
						bool enableEnemyWithRangedWeaponDistanceBreak = SkyAiCore.Settings.enableEnemyWithRangedWeaponDistanceBreak;
						if (enableEnemyWithRangedWeaponDistanceBreak)
						{
						}
						bool enemyWillReactToThreats = SkyAiCore.Settings.enemyWillReactToThreats;
						if (enemyWillReactToThreats)
						{
							AdvancedAI_Classes.ReactToThreats(__instance, lord, raidData);
						}
					}
				}
			}
			bool flag16 = __instance.IsHashIntervalTick(55) && SkyAiCore.Settings.enableAdvancedExplosionDetection;
			if (flag16)
			{
				AdvancedAI_Classes.AdvancedExplosionDetection(__instance);
			}
			bool flag17 = __instance.IsHashIntervalTick(32) && SkyAiCore.Settings.enableAdvancedFriendlyFireUtility;
			if (flag17)
			{
				AdvancedAI_Classes.AdvancedFriendlyFireUtility(__instance);
			}
		}
	}
}
