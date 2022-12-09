using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x0200002D RID: 45
	public class Settings : ModSettings
	{
		// Token: 0x0600016A RID: 362 RVA: 0x000203D0 File Offset: 0x0001E5D0
		public void DoWindowContents(Rect canvas)
		{
			canvas.yMin += 10f;
			canvas.yMax -= 10f;
			float num = canvas.width - 50f;
			Texture2D iconTex = TexturesLoader.IconTex;
			float num2 = 0f;
			bool flag = iconTex != null;
			if (flag)
			{
				float width = canvas.width;
				float num3 = width / (float)iconTex.width;
				num2 = (float)iconTex.height * num3;
				float x = 0f;
				bool flag2 = (float)iconTex.width < width;
				if (flag2)
				{
					x = (width - (float)iconTex.width) / 2f;
				}
				GUI.DrawTexture(new Rect(x, canvas.y, width, num2), iconTex, ScaleMode.StretchToFill, true);
			}
			Rect rect = canvas;
			rect.y += num2;
			rect.height -= num2;
			Widgets.DrawMenuSection(rect);
			List<TabRecord> tabs = new List<TabRecord>
			{
				new TabRecord("Main features", delegate()
				{
					this.tab = 0;
					base.Write();
				}, this.tab == 0),
				new TabRecord("Debug Settings", delegate()
				{
					this.tab = 1;
					base.Write();
				}, this.tab == 1)
			};
			TabDrawer.DrawTabs<TabRecord>(rect, tabs, 250f);
			bool flag3 = this.tab == 0;
			if (flag3)
			{
				this.DoMainTab(canvas.ContractedBy(10f));
			}
			bool flag4 = this.tab == 1;
			if (flag4)
			{
				this.DoDebugTab(canvas.ContractedBy(10f));
			}
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00020568 File Offset: 0x0001E768
		public void DoMainTab(Rect canvas)
		{
			canvas.yMin += 2f;
			canvas.yMax -= 2f;
			float columnWidth = canvas.width - 50f;
			Listing_Standard listing_Standard = new Listing_Standard
			{
				ColumnWidth = columnWidth
			};
			Rect outRect = new Rect(canvas.x, canvas.y + 150f, canvas.width, canvas.height - 150f);
			Rect rect = new Rect(0f, 150f, canvas.width - 16f, canvas.height * 2.2f + 50f);
			Widgets.BeginScrollView(outRect, ref Settings.scrollPosition, rect, true);
			listing_Standard.ColumnWidth = (canvas.width - 40f) / 2f;
			listing_Standard.Begin(rect);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.SiegeAILabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("SkyAI.CanMineMineablesLabel".Translate(), ref this.canMineMineables, "SkyAI.CanMineMineablesDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CanMineWallsLabel".Translate(), ref this.canMineNonMineables, "SkyAI.CanMineWallsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.ScaleRaidWidthLabel".Translate(), ref this.scaleRaidWidthByEnemyRaidCount, "SkyAI.ScaleRaidWidthDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.GatherTroopsLabel".Translate(), ref this.enableRaidLeaderGathersTroopsNearColony, "SkyAI.GatherTroopsDesc".Translate());
			listing_Standard.Label("SkyAI.GatherRangeLabel".Translate() + ": " + this.gatherPositionRangeMultiplier.ToString() + "x", -1f, "SkyAI.GatherRangeDesc".Translate());
			listing_Standard.Slider(ref this.gatherPositionRangeMultiplier, 0.5f, 1.5f, null, 0.05f, 10f);
			listing_Standard.Label("SkyAI.MinSquadLabel".Translate() + ": " + this.minPawnsSquadAmount.ToString(), -1f, "SkyAI.MinSquadDesc".Translate());
			listing_Standard.Slider(ref this.minPawnsSquadAmount, 10, 30, null, 1, 10f);
			listing_Standard.Label("SkyAI.MaxSquadLabel".Translate() + ": " + this.maxSquadUnitsCount.ToString(), -1f, "SkyAI.MaxSquadDesc".Translate());
			listing_Standard.Slider(ref this.maxSquadUnitsCount, 1, 5, null, 1, 10f);
			listing_Standard.Label("SkyAI.DestroyersExDelayLabel".Translate() + ": " + this.destroyersExclusionDelay.ToString(), -1f, "SkyAI.DestroyersExDelayDesc".Translate());
			listing_Standard.Slider(ref this.destroyersExclusionDelay, 1, 20, null, 1, 10f);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.RolesAndModesLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("SkyAI.SupporterRoleLabel".Translate(), ref this.enableSupporterRole, "SkyAI.SupporterRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.MechSupporterRoleLabel".Translate(), ref this.mechanoidUseSupporterRole, "SkyAI.MechSupporterRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.DestroyerRoleLabel".Translate(), ref this.enableDestroyerRole, "SkyAI.DestroyerRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.SniperRoleLabel".Translate(), ref this.enableSniperRole, "SkyAI.SniperRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.DoctorRoleLabel".Translate(), ref this.enableDoctorRole, "SkyAI.DoctorRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.LeaderRoleLabel".Translate(), ref this.enableLeaderRole, "SkyAI.LeaderRoleDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.ExtinguishFireLabel".Translate(), ref this.enableFireExtinguishingMode, "SkyAI.ExtinguishFireDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.ResqueAlliesLabel".Translate(), ref this.enableRescueAlliesMode, "SkyAI.ResqueAlliesDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.StealingLabel".Translate(), ref this.enableStealingMode, "SkyAI.StealingDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.LeaderCombatAuraLabel".Translate(), ref this.enableRaidLeaderAura, "SkyAI.LeaderCombatAuraDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.SquadLeaderCombatAuraLabel".Translate(), ref this.enableSquadCommandersAura, "SkyAI.SquadLeaderCombatAuraDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.SuppressionFireLabel".Translate(), ref this.enableSuppressionFireMode, "SkyAI.SuppressionFireDesc".Translate());
			listing_Standard.Label("SkyAI.SquadSuppressionRatioLabel".Translate() + ": " + Math.Round((double)(this.advancedSuppresionSquadRatio * 100f), 1).ToString() + "%", -1f, "SkyAI.SquadSuppressionRatioDesc".Translate());
			listing_Standard.Slider(ref this.advancedSuppresionSquadRatio, 0f, 0.5f, null, 0.05f, 10f);
			listing_Standard.Label("SkyAI.MinDistanceGoalPositionLabel".Translate() + ": " + this.minDistanceToGoalPosition.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.MinDistanceGoalPositionDesc".Translate());
			listing_Standard.Slider(ref this.minDistanceToGoalPosition, 50f, 150f, null, 5f, 10f);
			listing_Standard.Label("SkyAI.MinWeaponWeightLabel".Translate() + ": " + this.minWeaponWeightForLightWeapon.ToString() + " " + "SkyAI.Kg".Translate(), -1f, "SkyAI.MinWeaponWeightDesc".Translate());
			listing_Standard.Slider(ref this.minWeaponWeightForLightWeapon, 1f, 10f, null, 1f, 10f);
			listing_Standard.Label("SkyAI.MinEffectiveRangeLabel".Translate() + ": " + this.minRangeForEffectiveRange.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.MinEffectiveRangeDesc".Translate());
			listing_Standard.Slider(ref this.minRangeForEffectiveRange, 5, 40, null, 1, 10f);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.LeavingAndRetreatingLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("SkyAI.SaveExitSpotsLabel".Translate(), ref this.enableSaveExitSpot, "SkyAI.SaveExitSpotsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CommonExitSpotsLabel".Translate(), ref this.enableCommonExitSpot, "SkyAI.CommonExitSpotsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.EscapeChanceNoWeaponLabel".Translate(), ref this.enemiesEscapeChanceNoWeapon, "SkyAI.EscapeChanceNoWeaponDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.EscapeChanceExhaustedLabel".Translate(), ref this.enemiesEscapeChanceExhausted, "SkyAI.EscapeChanceExhaustedDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.EscapeChanceWoundedLabel".Translate(), ref this.enemiesEscapeChanceWounded, "SkyAI.EscapeChanceWoundedDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.EscapeChanceTempInjLabel".Translate(), ref this.enemiesEscapeDueToTemperatureInjuries, "SkyAI.EscapeChanceTempInjDesc".Translate());
			listing_Standard.Label("SkyAI.EscapeChanceMultiplierLabel".Translate() + ": " + Math.Round((double)(this.fleeChanceMultiplier * 100f), 1).ToString() + "%", -1f, "SkyAI.EscapeChanceMultiplierDesc".Translate());
			listing_Standard.Slider(ref this.fleeChanceMultiplier, 0f, 1f, null, 0.05f, 10f);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.IconsLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("SkyAI.MainRoleIconLabel".Translate(), ref this.enableRoleIcons, "SkyAI.MainRoleIconDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.TraderIconLabel".Translate(), ref this.enableTraderIcons, "SkyAI.TraderIconDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.AggroIconLabel".Translate(), ref this.enableAgroMentalStateIcons, "SkyAI.AggroIconDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.AnimalAggroIconLabel".Translate(), ref this.enableAnimalAgroMentalStateIcons, "SkyAI.AnimalAggroIconDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CatCrazyIconLabel".Translate(), ref this.enableCrazyTimeIcons, "SkyAI.CatCrazyIconDesc".Translate());
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.VariousLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("SkyAI.RememberOwnedThingsLabel".Translate(), ref this.enablePawnThingOwnerData, "SkyAI.RememberOwnedThingsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CaravansPickUpCargoLabel".Translate(), ref this.enableLostThingsData, "SkyAI.CaravansPickUpCargoDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.ReactToThreatsLabel".Translate(), ref this.enemyWillReactToThreats, "SkyAI.ReactToThreatsDesc".Translate());
			listing_Standard.Label("SkyAI.ReactToThreatsAreaLabel".Translate() + ": " + this.areaToReactAllies.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.ReactToThreatsAreaDesc".Translate());
			listing_Standard.Slider(ref this.areaToReactAllies, 20f, 100f, null, 1f, 10f);
			listing_Standard.Label("SkyAI.СombatEnemyKeepRangeLabel".Translate() + ": " + this.combatKeepRange.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.СombatEnemyKeepRangeDesc".Translate());
			listing_Standard.Slider(ref this.combatKeepRange, 50f, 100f, null, 1f, 10f);
			listing_Standard.Label("SkyAI.ActiveThreatRangeLabel".Translate() + ": " + this.nonCombatActiveThreatRange.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.ActiveThreatRangeDesc".Translate());
			listing_Standard.Slider(ref this.nonCombatActiveThreatRange, 50f, 100f, null, 1f, 10f);
			listing_Standard.CheckboxLabeled("SkyAI.EnchancedReactionToThreatLabel".Translate(), ref this.enableEnchancedThreatReaction, "SkyAI.EnchancedReactionToThreatDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CanBreakDistanceLabel".Translate(), ref this.enableSnipersKiteMechanic, "SkyAI.CanBreakDistanceDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.BoostEnemyDashSpeedLabel".Translate(), ref this.boostEnemyDashSpeed, "SkyAI.BoostEnemyDashSpeedDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.KiteMechanicLabel".Translate(), ref this.enableSnipersKiteMechanic, "SkyAI.KiteMechanicDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.AdvancedExplosionDetectionLabel".Translate(), ref this.enableAdvancedExplosionDetection, "SkyAI.AdvancedExplosionDetectionDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.DangerousCellsDetectionLabel".Translate(), ref this.enableDangerousCellsDetection, "SkyAI.DangerousCellsDetectionDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.AdvancedExplosionDetectionLabel".Translate(), ref this.colonyPawnsWillUseAdvancedExplosionDetection, "SkyAI.AdvancedExplosionDetectionDesc".Translate());
			listing_Standard.Label("SkyAI.ExplosionRadiusDetectionLabel".Translate() + ": " + this.explosionRadiusDetection.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.ExplosionRadiusDetectionDesc".Translate());
			listing_Standard.Slider(ref this.explosionRadiusDetection, 1f, 10f, null, 5f, 10f);
			listing_Standard.CheckboxLabeled("SkyAI.ConsiderAlliesChoosingTargetLabel".Translate(), ref this.considerFriendlyFireWhileSelectTarget, "SkyAI.ConsiderAlliesChoosingTargetDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.StopFiringFriendlyFireLabel".Translate(), ref this.enableAdvancedFriendlyFireUtility, "SkyAI.StopFiringFriendlyFireDesc".Translate());
			bool flag = listing_Standard.ButtonTextLabeled("SkyAI.ShootlineFriendlyFireLabel".Translate(), this.avoidFriendlyFireType.ToString());
			if (flag)
			{
				FloatMenu window = new FloatMenu(new List<FloatMenuOption>
				{
					new FloatMenuOption("SkyAI.ShootlineDisabledMode".Translate(), delegate()
					{
						this.avoidFriendlyFireType = Settings.FriendlyFireType.Disabled;
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0),
					new FloatMenuOption("SkyAI.ShootlineSiegeWeaponMode".Translate(), delegate()
					{
						this.avoidFriendlyFireType = Settings.FriendlyFireType.SiegeWeaponOnly;
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0),
					new FloatMenuOption("SkyAI.ShootlineSiegeMGWeaponMode".Translate(), delegate()
					{
						this.avoidFriendlyFireType = Settings.FriendlyFireType.SiegeAndMachineGunsOnly;
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0),
					new FloatMenuOption("SkyAI.ShootlineAllRangedWeaponMode".Translate(), delegate()
					{
						this.avoidFriendlyFireType = Settings.FriendlyFireType.AllRangedWeapon;
					}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0)
				})
				{
					vanishIfMouseDistant = true
				};
				Find.WindowStack.Add(window);
			}
			listing_Standard.CheckboxLabeled("SkyAI.AutoSwitchRangedWeaponsLabel".Translate(), ref this.autoSwitchToAllRangedWeapons, "SkyAI.AutoSwitchRangedWeaponsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.CanMissFriendlyFireLabel".Translate(), ref this.dumbIgnore, "SkyAI.CanMissFriendlyFireDesc".Translate());
			listing_Standard.Label("SkyAI.ChanceMissFriendlyFireLabel".Translate() + ": " + Math.Round((double)(this.chanceDumbIgnore * 100f), 1).ToString() + "%", -1f, "SkyAI.ChanceMissFriendlyFireDesc".Translate());
			listing_Standard.Slider(ref this.chanceDumbIgnore, 0f, 1f, null, 0.05f, 10f);
			listing_Standard.Label("SkyAI.ExtraFriendlyFireMinDistanceLabel".Translate() + ": " + this.extraFriendlyFireMinDistance.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.ExtraFriendlyFireMinDistanceDesc".Translate());
			listing_Standard.Slider(ref this.extraFriendlyFireMinDistance, 0f, 5f, null, 1f, 10f);
			listing_Standard.CheckboxLabeled("SkyAI.WillSwitchWeaponCombatLabel".Translate(), ref this.checkInventoryForBetterWeapon, "SkyAI.WillSwitchWeaponCombatDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.IncendiaryWeaponUseLabel".Translate(), ref this.useIncendiaryWeaponCheck, "SkyAI.IncendiaryWeaponUseDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.InventoryUseSiegeWeaponLabel".Translate(), ref this.checkInventoryForSiegeWeapon, "SkyAI.InventoryUseSiegeWeaponDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.NonLOSDangerTargetCheckLabel".Translate(), ref this.enableNonLOSdangerTargetsCheck, "SkyAI.NonLOSDangerTargetCheckDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.AdvancedNonLOSDangerTargetCheckLabel".Translate(), ref this.enableAdvancedNonLOSdangerTargetCheck, "SkyAI.AdvancedNonLOSDangerTargetCheckDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.NotAttackAnimalsLabel".Translate(), ref this.enemiesWillNotAttackNonAggressiveColonyAnimals, "SkyAI.NotAttackAnimalsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.UnderCrossfireLabel".Translate(), ref this.enemiesWillBeCheckingCrossfire, "SkyAI.UnderCrossfireDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.PerformTreatmentLabel".Translate(), ref this.enemiesWillApplyFirstAid, "SkyAI.PerformTreatmentDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.PerformTreatmentAlliesLabel".Translate(), ref this.enemyDoctorWillApplyFirstAidToDownedAllies, "SkyAI.PerformTreatmentAlliesDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.InheritAlliesDutyLabel".Translate(), ref this.downedEnemyAfterHealInheritAlliesDuty, "SkyAI.InheritAlliesDutyDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.PerformTreatmentColonyPawnsLabel".Translate(), ref this.allyDoctorCanHealPlayerPawns, "SkyAI.PerformTreatmentColonyPawnsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.PerformTreatmentEnemyPawnsLabel".Translate(), ref this.allyDoctorCanHealPlayerEnemyFactions, "SkyAI.PerformTreatmentEnemyPawnsDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.LayingOnGroundSelfTendLabel".Translate(), ref this.layingOnGroundOnSelfTend, "SkyAI.LayingOnGroundSelfTendDesc".Translate());
			listing_Standard.CheckboxLabeled("SkyAI.PerformСonsciousnessBuffLabel".Translate(), ref this.enemyDoctorWillAddСonsciousnessBuff, "SkyAI.PerformСonsciousnessBuffDesc".Translate());
			listing_Standard.Label("SkyAI.DownedAlliesScanRangeLabel".Translate() + ": " + this.downedAlliesScanRange.ToString() + " " + "SkyAI.Cells".Translate(), -1f, "SkyAI.DownedAlliesScanRangeDesc".Translate());
			listing_Standard.Slider(ref this.downedAlliesScanRange, 20f, 60f, null, 5f, 10f);
			listing_Standard.Label("SkyAI.MinRaidSizeForLeaderLabel".Translate() + ": " + this.minRaidCountForLeader.ToString(), -1f, "SkyAI.MinRaidSizeForLeaderDesc".Translate());
			listing_Standard.Slider(ref this.minRaidCountForLeader, 3, 10, null, 1, 10f);
			listing_Standard.Label("SkyAI.MinRaidSizeForDoctorLabel".Translate() + ": " + this.minRaidCountForDoctor.ToString(), -1f, "SkyAI.MinRaidSizeForDoctorDesc".Translate());
			listing_Standard.Slider(ref this.minRaidCountForDoctor, 5, 15, null, 1, 10f);
			listing_Standard.Label("SkyAI.DoctorFrequencySpawnLabel".Translate() + ": " + this.pawnBecomeDoctorEvery.ToString(), -1f, "SkyAI.DoctorFrequencySpawnDesc".Translate());
			listing_Standard.Slider(ref this.pawnBecomeDoctorEvery, 5, 15, null, 1, 10f);
			listing_Standard.Label("SkyAI.MedicineCountSpawnLabel".Translate() + ": " + this.medicineSpawnCount.ToString(), -1f, "SkyAI.MedicineCountSpawnDesc".Translate());
			listing_Standard.Slider(ref this.medicineSpawnCount, 1, 10, null, 1, 10f);
			listing_Standard.End();
			Widgets.EndScrollView();
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00021A48 File Offset: 0x0001FC48
		public void DoDebugTab(Rect canvas)
		{
			canvas.yMin += 2f;
			canvas.yMax -= 2f;
			float columnWidth = canvas.width - 50f;
			Listing_Standard listing_Standard = new Listing_Standard
			{
				ColumnWidth = columnWidth
			};
			Rect outRect = new Rect(canvas.x, canvas.y + 150f, canvas.width, canvas.height - 150f);
			Rect rect = new Rect(0f, 150f, canvas.width - 16f, canvas.height * 2.2f + 50f);
			Widgets.BeginScrollView(outRect, ref Settings.scrollPosition, rect, true);
			listing_Standard.ColumnWidth = (canvas.width - 40f) / 2f;
			listing_Standard.Begin(rect);
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.DebugSettingsLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.CheckboxLabeled("Debug logs", ref this.debugLog, "");
			listing_Standard.CheckboxLabeled("Debug path", ref this.debugPath, "");
			listing_Standard.CheckboxLabeled("Debug path cover cells", ref this.debugPathCoverCells, "");
			listing_Standard.CheckboxLabeled("Debug targets", ref this.debugTargets, "");
			listing_Standard.CheckboxLabeled("Debug active threat", ref this.debugActiveThreat, "");
			listing_Standard.CheckboxLabeled("Debug raidData", ref this.debugRaidData, "");
			listing_Standard.CheckboxLabeled("Debug cover cells", ref this.debugCoverCells, "");
			listing_Standard.CheckboxLabeled("Debug detail targetLog", ref this.debugDetailTargetLog, "");
			listing_Standard.CheckboxLabeled("Debug potencial building targets (SiegeAI)", ref this.debugPotencialTargets, "");
			listing_Standard.CheckboxLabeled("Debug building connectedCells (SiegeAI)", ref this.debugConnectedCells, "");
			listing_Standard.CheckboxLabeled("Debug possible steal cells", ref this.debugStealCells, "");
			listing_Standard.CheckboxLabeled("Debug fleeExplosion dangerous cells", ref this.debugFleeExplosion, "");
			listing_Standard.CheckboxLabeled("Debug leave cells", ref this.debugLeaveCells, "");
			listing_Standard.CheckboxLabeled("Debug thingOwner", ref this.debugPawnThingsOwner, "");
			listing_Standard.CheckboxLabeled("Debug takeAndEquip", ref this.debugTakeAndEquip, "");
			listing_Standard.CheckboxLabeled("Debug disable SkyAI", ref this.debugDisableSkyAI, "");
			listing_Standard.CheckboxLabeled("Debug set pawn duty to assault colony", ref this.debugSetDutyAttackColony, "It must be disabled in a regular game, because can break siege strategy mechanics, some events, quests etc.");
			listing_Standard.Label("Flash cell delay: " + this.flashCellDelay.ToString() + " ticks", -1f, null);
			listing_Standard.Slider(ref this.flashCellDelay, 200, 2000, null, 100, 10f);
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("SkyAI.UsefulFunctionsLabel".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.GapLine(10f);
			listing_Standard.Label("SkyAI.RestoreToDefaultLabel".Translate(), -1f, null);
			bool flag = listing_Standard.ButtonText("SkyAI.RestoreToDefaultDesc".Translate(), null);
			if (flag)
			{
				this.canMineMineables = false;
				this.canMineNonMineables = false;
				this.additionalCheckForFreeCells = false;
				this.destroyersExclusionDelay = 6;
				this.scaleRaidWidthByEnemyRaidCount = true;
				this.enableRaidLeaderGathersTroopsNearColony = true;
				this.enableMainBlowSiegeTactic = false;
				this.gatherPositionRangeMultiplier = 1.15f;
				this.minPawnsSquadAmount = 15;
				this.maxSquadUnitsCount = 4;
				this.enableSupporterRole = true;
				this.mechanoidUseSupporterRole = false;
				this.enableDestroyerRole = true;
				this.enableSniperRole = true;
				this.enableDoctorRole = true;
				this.enableLeaderRole = true;
				this.enableFireExtinguishingMode = true;
				this.enableRescueAlliesMode = true;
				this.enableStealingMode = true;
				this.enableSaveExitSpot = true;
				this.enableCommonExitSpot = true;
				this.enableSuppressionFireMode = true;
				this.enableAdvancedSuppressionFireMode = false;
				this.advancedSuppresionSquadRatio = 0.25f;
				this.enableRaidLeaderAura = true;
				this.enableSquadCommandersAura = true;
				this.enableRoleIcons = true;
				this.enableTraderIcons = true;
				this.enableAgroMentalStateIcons = true;
				this.enableAnimalAgroMentalStateIcons = false;
				this.enableCrazyTimeIcons = false;
				this.checkInventoryForSiegeWeapon = false;
				this.enableNonLOSdangerTargetsCheck = true;
				this.enableAdvancedNonLOSdangerTargetCheck = true;
				this.enemiesWillNotAttackNonAggressiveColonyAnimals = true;
				this.enemiesWillBeCheckingCrossfire = true;
				this.enemiesWillApplyFirstAid = true;
				this.enemyDoctorWillAddСonsciousnessBuff = true;
				this.enemyDoctorWillApplyFirstAidToDownedAllies = true;
				this.downedAlliesScanRange = 50f;
				this.downedEnemyAfterHealInheritAlliesDuty = true;
				this.allyDoctorCanHealPlayerPawns = false;
				this.allyDoctorCanHealPlayerEnemyFactions = false;
				this.layingOnGroundOnSelfTend = true;
				this.enablePawnThingOwnerData = true;
				this.enableLostThingsData = true;
				this.enemiesCouldFleeAfterLosingLeader = true;
				this.enemiesEscapeChanceNoWeapon = true;
				this.enemiesEscapeChanceExhausted = false;
				this.enemiesEscapeChanceWounded = true;
				this.enemiesEscapeDueToTemperatureInjuries = true;
				this.fleeChanceMultiplier = 0.8f;
				this.medicineSpawnCount = 5;
				this.minRaidCountForLeader = 6;
				this.minRaidCountForDoctor = 7;
				this.pawnBecomeDoctorEvery = 9;
				this.useIncendiaryWeaponCheck = true;
				this.enabledExternalTakeAndEquipJob = true;
				this.extraFriendlyFireMinDistance = 2f;
				this.minDistanceToGoalPosition = 90f;
				this.minWeaponWeightForLightWeapon = 6f;
				this.minRangeForEffectiveRange = 22;
				this.boostEnemyDashSpeed = false;
				this.enemyWillReactToThreats = true;
				this.areaToReactAllies = 80f;
				this.combatKeepRange = 80f;
				this.nonCombatActiveThreatRange = 55f;
				this.enableEnchancedThreatReaction = true;
				this.enableEnemyWithRangedWeaponDistanceBreak = true;
				this.enableSnipersKiteMechanic = true;
				this.enableAdvancedExplosionDetection = true;
				this.enableDangerousCellsDetection = false;
				this.colonyPawnsWillUseAdvancedExplosionDetection = false;
				this.explosionRadiusDetection = 4f;
				this.enableAdvancedFriendlyFireUtility = true;
				this.considerFriendlyFireWhileSelectTarget = true;
				this.avoidFriendlyFireType = Settings.FriendlyFireType.SiegeAndMachineGunsOnly;
				this.autoSwitchToAllRangedWeapons = true;
				this.dumbIgnore = true;
				this.chanceDumbIgnore = 0.1f;
				this.checkInventoryForBetterWeapon = true;
				this.debugLog = false;
				this.debugTargets = false;
				this.debugPath = false;
				this.debugPathCoverCells = false;
				this.debugDetailTargetLog = false;
				this.debugPotencialTargets = false;
				this.debugActiveThreat = false;
				this.debugCoverCells = false;
				this.debugConnectedCells = false;
				this.debugStealCells = false;
				this.debugFleeExplosion = false;
				this.debugSetDutyAttackColony = false;
				this.debugLeaveCells = false;
				this.debugPawnThingsOwner = false;
				this.debugTakeAndEquip = false;
				this.debugRaidData = false;
				this.debugDisableSkyAI = false;
				this.flashCellDelay = 500;
			}
			listing_Standard.Gap(5f);
			listing_Standard.Label("SkyAI.ClearDataLabel".Translate(), -1f, null);
			bool flag2 = listing_Standard.ButtonText("SkyAI.ClearDataDesc".Translate(), null);
			if (flag2)
			{
				using (List<Map>.Enumerator enumerator = Find.Maps.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Map map = enumerator.Current;
						bool flag3 = map != null;
						if (flag3)
						{
							MapComponent_SkyAI component = map.GetComponent<MapComponent_SkyAI>();
							bool flag4 = component != null;
							if (flag4)
							{
								component.ClearData();
								IEnumerable<Pawn> enumerable = from p in map.mapPawns.AllPawns
								where p.RaceProps.intelligence >= Intelligence.Humanlike && !p.RaceProps.Animal && !map.fogGrid.IsFogged(p.Position) && p.Faction != null && !p.IsPrisoner && !p.IsSlave && p.Faction != Faction.OfPlayer
								select p;
								bool flag5 = !enumerable.EnumerableNullOrEmpty<Pawn>();
								if (flag5)
								{
									for (int i = enumerable.Count<Pawn>() - 1; i >= 0; i--)
									{
										enumerable.ElementAt(i).Destroy(DestroyMode.Vanish);
									}
								}
							}
						}
					}
				}
			}
			listing_Standard.End();
			Widgets.EndScrollView();
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000221D0 File Offset: 0x000203D0
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.modVersion, "modVersion", "", true);
			Scribe_Values.Look<bool>(ref this.debugLog, "debugLog", false, false);
			Scribe_Values.Look<bool>(ref this.debugTargets, "debugTargets", false, false);
			Scribe_Values.Look<bool>(ref this.debugPath, "debugPath", false, false);
			Scribe_Values.Look<bool>(ref this.debugPathCoverCells, "debugPathCoverCells", false, false);
			Scribe_Values.Look<bool>(ref this.debugRaidData, "debugRaidData", false, false);
			Scribe_Values.Look<bool>(ref this.debugDisableSkyAI, "debugDisableSkyAI", false, false);
			Scribe_Values.Look<bool>(ref this.debugDetailTargetLog, "debugDetailTargetLog", false, false);
			Scribe_Values.Look<bool>(ref this.debugPotencialTargets, "debugPotencialTargets", false, false);
			Scribe_Values.Look<bool>(ref this.debugActiveThreat, "debugActiveThreat", false, false);
			Scribe_Values.Look<bool>(ref this.debugCoverCells, "debugBehindCoverCells", false, false);
			Scribe_Values.Look<bool>(ref this.debugConnectedCells, "debugConnectedCells", false, false);
			Scribe_Values.Look<bool>(ref this.debugStealCells, "debugStealCells", false, false);
			Scribe_Values.Look<bool>(ref this.debugFleeExplosion, "debugFleeExplosion", false, false);
			Scribe_Values.Look<bool>(ref this.debugLeaveCells, "debugLeaveCells", false, false);
			Scribe_Values.Look<bool>(ref this.debugPawnThingsOwner, "debugPawnThingsOwner", false, false);
			Scribe_Values.Look<bool>(ref this.debugTakeAndEquip, "debugTakeAndEquip", false, false);
			Scribe_Values.Look<bool>(ref this.debugSetDutyAttackColony, "debugSetDutyAttackColony", false, false);
			Scribe_Values.Look<int>(ref this.flashCellDelay, "flashCellDelay", 500, false);
			Scribe_Values.Look<bool>(ref this.canMineMineables, "canMineMineables", false, false);
			Scribe_Values.Look<bool>(ref this.canMineNonMineables, "canMineNonMineables", false, false);
			Scribe_Values.Look<int>(ref this.destroyersExclusionDelay, "destroyersExclusionDelay", 6, false);
			Scribe_Values.Look<bool>(ref this.scaleRaidWidthByEnemyRaidCount, "scaleRaidWidthByEnemyRaidCount", true, false);
			Scribe_Values.Look<bool>(ref this.enableRaidLeaderGathersTroopsNearColony, "enableRaidLeaderGathersTroopsNearColony", true, false);
			Scribe_Values.Look<float>(ref this.gatherPositionRangeMultiplier, "gatherPositionRangeMultiplier", 1.15f, false);
			Scribe_Values.Look<int>(ref this.minPawnsSquadAmount, "minPawnsSquadAmount", 15, false);
			Scribe_Values.Look<int>(ref this.maxSquadUnitsCount, "maxSquadUnitsCount", 4, false);
			Scribe_Values.Look<bool>(ref this.enableSupporterRole, "enableSupporterRole", true, false);
			Scribe_Values.Look<bool>(ref this.mechanoidUseSupporterRole, "mechanoidUseSupporterRole", false, false);
			Scribe_Values.Look<bool>(ref this.enableDestroyerRole, "enableDestroyerRole", true, false);
			Scribe_Values.Look<bool>(ref this.enableSniperRole, "enableSniperRole", true, false);
			Scribe_Values.Look<bool>(ref this.enableDoctorRole, "enableDoctorRole", true, false);
			Scribe_Values.Look<bool>(ref this.enableLeaderRole, "enableLeaderRole", true, false);
			Scribe_Values.Look<bool>(ref this.enableFireExtinguishingMode, "enableFireExtinguishingMode", true, false);
			Scribe_Values.Look<bool>(ref this.enableRescueAlliesMode, "enableRescueAlliesMode", true, false);
			Scribe_Values.Look<bool>(ref this.enableStealingMode, "enableStealingMode", true, false);
			Scribe_Values.Look<bool>(ref this.enableSaveExitSpot, "enableSaveExitSpot", true, false);
			Scribe_Values.Look<bool>(ref this.enableCommonExitSpot, "enableCommonExitSpot", true, false);
			Scribe_Values.Look<bool>(ref this.enableRaidLeaderAura, "enableRaidLeaderAura", true, false);
			Scribe_Values.Look<bool>(ref this.enableSquadCommandersAura, "enableSquadCommandersAura", true, false);
			Scribe_Values.Look<bool>(ref this.enableSuppressionFireMode, "enableSuppressionFireMode", true, false);
			Scribe_Values.Look<float>(ref this.advancedSuppresionSquadRatio, "advancedSuppresionSquadRatio", 0.2f, false);
			Scribe_Values.Look<bool>(ref this.enableRoleIcons, "enableRoleIcons", true, false);
			Scribe_Values.Look<bool>(ref this.enableTraderIcons, "enableTraderIcons", true, false);
			Scribe_Values.Look<bool>(ref this.enableAgroMentalStateIcons, "enableAgroMentalStateIcons", true, false);
			Scribe_Values.Look<bool>(ref this.enableAnimalAgroMentalStateIcons, "enableAnimalAgroMentalStateIcons", false, false);
			Scribe_Values.Look<bool>(ref this.enableCrazyTimeIcons, "enableCrazyTimeIcons", false, false);
			Scribe_Values.Look<bool>(ref this.boostEnemyDashSpeed, "boostEnemyDashSpeed", false, false);
			Scribe_Values.Look<bool>(ref this.enemyWillReactToThreats, "enemyWillReactToThreats", true, false);
			Scribe_Values.Look<float>(ref this.areaToReactAllies, "areaToReactAllies", 80f, false);
			Scribe_Values.Look<float>(ref this.combatKeepRange, "combatKeepRange", 80f, false);
			Scribe_Values.Look<float>(ref this.nonCombatActiveThreatRange, "nonCombatActiveThreatRange", 55f, false);
			Scribe_Values.Look<bool>(ref this.enableEnchancedThreatReaction, "enableEnchancedThreatReaction", true, false);
			Scribe_Values.Look<bool>(ref this.enableEnemyWithRangedWeaponDistanceBreak, "enableEnemyWithRangedWeaponDistanceBreak", true, false);
			Scribe_Values.Look<bool>(ref this.enableSnipersKiteMechanic, "enableSnipersKiteMechanic", true, false);
			Scribe_Values.Look<bool>(ref this.enableAdvancedExplosionDetection, "enableAdvancedExplosionDetection", true, false);
			Scribe_Values.Look<bool>(ref this.enableDangerousCellsDetection, "enableDangerousCellsDetection", false, false);
			Scribe_Values.Look<bool>(ref this.colonyPawnsWillUseAdvancedExplosionDetection, "colonyPawnsWillUseAdvancedExplosionDetection", false, false);
			Scribe_Values.Look<float>(ref this.explosionRadiusDetection, "explosionRadiusDetection", 4f, false);
			Scribe_Values.Look<bool>(ref this.considerFriendlyFireWhileSelectTarget, "considerFriendlyFireWhileSelectTarget", true, false);
			Scribe_Values.Look<bool>(ref this.enableAdvancedFriendlyFireUtility, "enableAdvancedFriendlyFireUtility", true, false);
			Scribe_Values.Look<Settings.FriendlyFireType>(ref this.avoidFriendlyFireType, "avoidFriendlyFireType", Settings.FriendlyFireType.SiegeAndMachineGunsOnly, false);
			Scribe_Values.Look<bool>(ref this.autoSwitchToAllRangedWeapons, "autoSwitchToAllRangedWeapons", true, false);
			Scribe_Values.Look<bool>(ref this.dumbIgnore, "dumbIgnore", true, false);
			Scribe_Values.Look<float>(ref this.chanceDumbIgnore, "chanceDumbIgnore", 0.2f, false);
			Scribe_Values.Look<bool>(ref this.checkInventoryForBetterWeapon, "checkInventoryForBetterWeapon", true, false);
			Scribe_Values.Look<bool>(ref this.checkInventoryForSiegeWeapon, "checkInventoryForSiegeWeapon", false, false);
			Scribe_Values.Look<bool>(ref this.enableNonLOSdangerTargetsCheck, "enableNonLOSdangerTargetsCheck", true, false);
			Scribe_Values.Look<bool>(ref this.enableAdvancedNonLOSdangerTargetCheck, "enableAdvancedNonLOSdangerTargetCheck", false, false);
			Scribe_Values.Look<bool>(ref this.enemiesWillNotAttackNonAggressiveColonyAnimals, "enemiesWillNotAttackNonAggressiveColonyAnimals", true, false);
			Scribe_Values.Look<bool>(ref this.enemiesWillBeCheckingCrossfire, "enemiesWillBeCheckingCrossfire", true, false);
			Scribe_Values.Look<bool>(ref this.enemiesWillApplyFirstAid, "enemiesWillApplyFirstAid", true, false);
			Scribe_Values.Look<bool>(ref this.enemyDoctorWillAddСonsciousnessBuff, "enemyDoctorWillAddСonsciousnessBuff", true, false);
			Scribe_Values.Look<bool>(ref this.enemyDoctorWillApplyFirstAidToDownedAllies, "enemyDoctorWillApplyFirstAidToDownedAllies", true, false);
			Scribe_Values.Look<float>(ref this.downedAlliesScanRange, "downedAlliesScanRange", 50f, false);
			Scribe_Values.Look<bool>(ref this.downedEnemyAfterHealInheritAlliesDuty, "downedEnemyAfterHealInheritAlliesDuty", true, false);
			Scribe_Values.Look<bool>(ref this.allyDoctorCanHealPlayerPawns, "allyDoctorCanHealPlayerPawns", false, false);
			Scribe_Values.Look<bool>(ref this.allyDoctorCanHealPlayerEnemyFactions, "allyDoctorCanHealPlayerEnemyFactions", false, false);
			Scribe_Values.Look<bool>(ref this.layingOnGroundOnSelfTend, "layingOnGroundOnSelfTend", true, false);
			Scribe_Values.Look<bool>(ref this.enemiesCouldFleeAfterLosingLeader, "enemiesCouldFleeAfterLosingLeader", true, false);
			Scribe_Values.Look<bool>(ref this.enemiesEscapeChanceNoWeapon, "enemiesEscapeChanceNoWeapon", true, false);
			Scribe_Values.Look<bool>(ref this.enemiesEscapeChanceWounded, "enemiesEscapeChanceWounded", false, false);
			Scribe_Values.Look<bool>(ref this.enemiesEscapeChanceExhausted, "enemiesEscapeChanceExhausted", false, false);
			Scribe_Values.Look<bool>(ref this.enemiesEscapeDueToTemperatureInjuries, "enemiesEscapeDueToTemperatureInjuries", true, false);
			Scribe_Values.Look<float>(ref this.fleeChanceMultiplier, "fleeChanceMultiplier", 0.8f, false);
			Scribe_Values.Look<int>(ref this.medicineSpawnCount, "medicineSpawnCount", 5, false);
			Scribe_Values.Look<int>(ref this.minRaidCountForLeader, "minRaidCountForLeader", 3, false);
			Scribe_Values.Look<int>(ref this.minRaidCountForDoctor, "minRaidCountForDoctor", 8, false);
			Scribe_Values.Look<int>(ref this.pawnBecomeDoctorEvery, "pawnBecomeDoctorEvery", 9, false);
			Scribe_Values.Look<bool>(ref this.useIncendiaryWeaponCheck, "useIncendiaryWeaponCheck", true, false);
			Scribe_Values.Look<bool>(ref this.enabledExternalTakeAndEquipJob, "enabledExternalTakeAndEquipJob", true, false);
			Scribe_Values.Look<float>(ref this.extraFriendlyFireMinDistance, "extraFriendlyFireMinDistance", 2f, false);
			Scribe_Values.Look<float>(ref this.minDistanceToGoalPosition, "minDistanceToGoalPosition", 90f, false);
			Scribe_Values.Look<float>(ref this.minWeaponWeightForLightWeapon, "minWeaponWeightForLightWeapon", 6f, false);
			Scribe_Values.Look<int>(ref this.minRangeForEffectiveRange, "minRangeForEffectiveRange", 20, false);
		}

		// Token: 0x04000086 RID: 134
		public bool DarknestNightEnabled = false;

		// Token: 0x04000087 RID: 135
		public static Vector2 scrollPosition = Vector2.zero;

		// Token: 0x04000088 RID: 136
		public string modVersion;

		// Token: 0x04000089 RID: 137
		public int tab;

		// Token: 0x0400008A RID: 138
		public bool keepLeaderInReserveIfPossible = true;

		// Token: 0x0400008B RID: 139
		public bool additionalCheckForFreeCells = false;

		// Token: 0x0400008C RID: 140
		public float leaderAuraRange = 20f;

		// Token: 0x0400008D RID: 141
		public bool enemyRaidLeadersCouldFlee = false;

		// Token: 0x0400008E RID: 142
		public const float cellPawnCountLimit = 4f;

		// Token: 0x0400008F RID: 143
		public bool enabledExternalTakeAndEquipJob = false;

		// Token: 0x04000090 RID: 144
		public bool enabledWaitSquad = true;

		// Token: 0x04000091 RID: 145
		public float raidFleeMultiplier = 0f;

		// Token: 0x04000092 RID: 146
		public float spiritLossMultiplierRaidLeaderLost = 0.11f;

		// Token: 0x04000093 RID: 147
		public float spiritLossMultiplierSquadCommanderLost = 0.04f;

		// Token: 0x04000094 RID: 148
		public bool canMineMineables = false;

		// Token: 0x04000095 RID: 149
		public bool canMineNonMineables = false;

		// Token: 0x04000096 RID: 150
		public bool enableMainBlowSiegeTactic = false;

		// Token: 0x04000097 RID: 151
		public bool scaleRaidWidthByEnemyRaidCount = true;

		// Token: 0x04000098 RID: 152
		public bool enableRaidLeaderGathersTroopsNearColony = true;

		// Token: 0x04000099 RID: 153
		public float gatherPositionRangeMultiplier = 1.15f;

		// Token: 0x0400009A RID: 154
		public int minPawnsSquadAmount = 15;

		// Token: 0x0400009B RID: 155
		public int maxSquadUnitsCount = 4;

		// Token: 0x0400009C RID: 156
		public int destroyersExclusionDelay = 6;

		// Token: 0x0400009D RID: 157
		public bool enableSupporterRole = true;

		// Token: 0x0400009E RID: 158
		public bool mechanoidUseSupporterRole = false;

		// Token: 0x0400009F RID: 159
		public bool enableDestroyerRole = true;

		// Token: 0x040000A0 RID: 160
		public bool enableSniperRole = true;

		// Token: 0x040000A1 RID: 161
		public bool enableDoctorRole = true;

		// Token: 0x040000A2 RID: 162
		public bool enableLeaderRole = true;

		// Token: 0x040000A3 RID: 163
		public bool enableFireExtinguishingMode = true;

		// Token: 0x040000A4 RID: 164
		public bool enableRescueAlliesMode = true;

		// Token: 0x040000A5 RID: 165
		public bool enableStealingMode = true;

		// Token: 0x040000A6 RID: 166
		public bool enableRaidLeaderAura = true;

		// Token: 0x040000A7 RID: 167
		public bool enableSquadCommandersAura = true;

		// Token: 0x040000A8 RID: 168
		public bool enableSuppressionFireMode = true;

		// Token: 0x040000A9 RID: 169
		public bool enableAdvancedSuppressionFireMode = false;

		// Token: 0x040000AA RID: 170
		public float advancedSuppresionSquadRatio = 0.25f;

		// Token: 0x040000AB RID: 171
		public float minDistanceToGoalPosition = 90f;

		// Token: 0x040000AC RID: 172
		public float minWeaponWeightForLightWeapon = 6f;

		// Token: 0x040000AD RID: 173
		public int minRangeForEffectiveRange = 22;

		// Token: 0x040000AE RID: 174
		public bool enableSaveExitSpot = true;

		// Token: 0x040000AF RID: 175
		public bool enableCommonExitSpot = true;

		// Token: 0x040000B0 RID: 176
		public bool enemiesCouldFleeAfterLosingLeader = true;

		// Token: 0x040000B1 RID: 177
		public bool enemiesEscapeChanceNoWeapon = true;

		// Token: 0x040000B2 RID: 178
		public bool enemiesEscapeChanceWounded = true;

		// Token: 0x040000B3 RID: 179
		public bool enemiesEscapeChanceExhausted = false;

		// Token: 0x040000B4 RID: 180
		public bool enemiesEscapeDueToTemperatureInjuries = true;

		// Token: 0x040000B5 RID: 181
		public float fleeChanceMultiplier = 0.8f;

		// Token: 0x040000B6 RID: 182
		public bool enableRoleIcons = true;

		// Token: 0x040000B7 RID: 183
		public bool enableTraderIcons = true;

		// Token: 0x040000B8 RID: 184
		public bool enableAgroMentalStateIcons = true;

		// Token: 0x040000B9 RID: 185
		public bool enableAnimalAgroMentalStateIcons = false;

		// Token: 0x040000BA RID: 186
		public bool enableCrazyTimeIcons = false;

		// Token: 0x040000BB RID: 187
		public bool enablePawnThingOwnerData = true;

		// Token: 0x040000BC RID: 188
		public bool enableLostThingsData = true;

		// Token: 0x040000BD RID: 189
		public bool enemyWillReactToThreats = true;

		// Token: 0x040000BE RID: 190
		public float areaToReactAllies = 80f;

		// Token: 0x040000BF RID: 191
		public float combatKeepRange = 80f;

		// Token: 0x040000C0 RID: 192
		public float nonCombatActiveThreatRange = 55f;

		// Token: 0x040000C1 RID: 193
		public bool enableEnchancedThreatReaction = true;

		// Token: 0x040000C2 RID: 194
		public bool enableEnemyWithRangedWeaponDistanceBreak = true;

		// Token: 0x040000C3 RID: 195
		public bool boostEnemyDashSpeed = false;

		// Token: 0x040000C4 RID: 196
		public bool enableSnipersKiteMechanic = true;

		// Token: 0x040000C5 RID: 197
		public bool enableAdvancedExplosionDetection = true;

		// Token: 0x040000C6 RID: 198
		public bool enableDangerousCellsDetection = false;

		// Token: 0x040000C7 RID: 199
		public bool colonyPawnsWillUseAdvancedExplosionDetection = false;

		// Token: 0x040000C8 RID: 200
		public float explosionRadiusDetection = 4f;

		// Token: 0x040000C9 RID: 201
		public bool considerFriendlyFireWhileSelectTarget = true;

		// Token: 0x040000CA RID: 202
		public bool enableAdvancedFriendlyFireUtility = true;

		// Token: 0x040000CB RID: 203
		public Settings.FriendlyFireType avoidFriendlyFireType = Settings.FriendlyFireType.SiegeAndMachineGunsOnly;

		// Token: 0x040000CC RID: 204
		public bool autoSwitchToAllRangedWeapons = true;

		// Token: 0x040000CD RID: 205
		public bool dumbIgnore = true;

		// Token: 0x040000CE RID: 206
		public float chanceDumbIgnore = 0.1f;

		// Token: 0x040000CF RID: 207
		public float extraFriendlyFireMinDistance = 2f;

		// Token: 0x040000D0 RID: 208
		public bool checkInventoryForBetterWeapon = true;

		// Token: 0x040000D1 RID: 209
		public bool useIncendiaryWeaponCheck = true;

		// Token: 0x040000D2 RID: 210
		public bool checkInventoryForSiegeWeapon = false;

		// Token: 0x040000D3 RID: 211
		public bool enableNonLOSdangerTargetsCheck = true;

		// Token: 0x040000D4 RID: 212
		public bool enableAdvancedNonLOSdangerTargetCheck = true;

		// Token: 0x040000D5 RID: 213
		public bool enemiesWillNotAttackNonAggressiveColonyAnimals = true;

		// Token: 0x040000D6 RID: 214
		public bool enemiesWillBeCheckingCrossfire = true;

		// Token: 0x040000D7 RID: 215
		public bool enemiesWillApplyFirstAid = true;

		// Token: 0x040000D8 RID: 216
		public bool enemyDoctorWillApplyFirstAidToDownedAllies = true;

		// Token: 0x040000D9 RID: 217
		public bool downedEnemyAfterHealInheritAlliesDuty = true;

		// Token: 0x040000DA RID: 218
		public bool allyDoctorCanHealPlayerPawns = false;

		// Token: 0x040000DB RID: 219
		public bool allyDoctorCanHealPlayerEnemyFactions = false;

		// Token: 0x040000DC RID: 220
		public bool layingOnGroundOnSelfTend = true;

		// Token: 0x040000DD RID: 221
		public bool enemyDoctorWillAddСonsciousnessBuff = true;

		// Token: 0x040000DE RID: 222
		public float downedAlliesScanRange = 50f;

		// Token: 0x040000DF RID: 223
		public int minRaidCountForLeader = 6;

		// Token: 0x040000E0 RID: 224
		public int minRaidCountForDoctor = 7;

		// Token: 0x040000E1 RID: 225
		public int pawnBecomeDoctorEvery = 9;

		// Token: 0x040000E2 RID: 226
		public int medicineSpawnCount = 5;

		// Token: 0x040000E3 RID: 227
		public bool debugLog = false;

		// Token: 0x040000E4 RID: 228
		public bool debugTargets = false;

		// Token: 0x040000E5 RID: 229
		public bool debugPath = false;

		// Token: 0x040000E6 RID: 230
		public bool debugRaidData = false;

		// Token: 0x040000E7 RID: 231
		public bool debugPathCoverCells = false;

		// Token: 0x040000E8 RID: 232
		public bool debugDetailTargetLog = false;

		// Token: 0x040000E9 RID: 233
		public bool debugPotencialTargets = false;

		// Token: 0x040000EA RID: 234
		public bool debugActiveThreat = false;

		// Token: 0x040000EB RID: 235
		public bool debugCoverCells = false;

		// Token: 0x040000EC RID: 236
		public bool debugConnectedCells = false;

		// Token: 0x040000ED RID: 237
		public bool debugStealCells = false;

		// Token: 0x040000EE RID: 238
		public bool debugFleeExplosion = false;

		// Token: 0x040000EF RID: 239
		public bool debugSetDutyAttackColony = false;

		// Token: 0x040000F0 RID: 240
		public bool debugLeaveCells = false;

		// Token: 0x040000F1 RID: 241
		public bool debugPawnThingsOwner = false;

		// Token: 0x040000F2 RID: 242
		public bool debugTakeAndEquip = false;

		// Token: 0x040000F3 RID: 243
		public bool debugDisableSkyAI = false;

		// Token: 0x040000F4 RID: 244
		public bool debugSquadAttackData = false;

		// Token: 0x040000F5 RID: 245
		public bool debugAttackGridBuildingTargets = false;

		// Token: 0x040000F6 RID: 246
		public int flashCellDelay = 500;

		// Token: 0x020000CA RID: 202
		public enum FriendlyFireType
		{
			// Token: 0x04000288 RID: 648
			Disabled,
			// Token: 0x04000289 RID: 649
			SiegeWeaponOnly,
			// Token: 0x0400028A RID: 650
			SiegeAndMachineGunsOnly,
			// Token: 0x0400028B RID: 651
			AllRangedWeapon
		}
	}
}
