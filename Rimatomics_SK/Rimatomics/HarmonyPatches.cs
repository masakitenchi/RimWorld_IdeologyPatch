using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	internal class HarmonyPatches
	{
		internal static class H_CheckForFreeInterceptBetween
		{
			public static bool Prefix(Projectile __instance, Vector3 lastExactPos, Vector3 newExactPos, ref bool __result)
			{
				if (lastExactPos == newExactPos)
				{
					return false;
				}
				if (DubDef.RimatomicsShieldGenerator == null)
				{
					return true;
				}
				List<Thing> list = __instance.Map.listerThings.ThingsOfDef(DubDef.RimatomicsShieldGenerator);
				for (int i = 0; i < list.Count; i++)
				{
					try
					{
						if (list[i] is Building_ShieldArray building_ShieldArray && building_ShieldArray.CompShield.CheckIntercept(__instance, lastExactPos, newExactPos))
						{
							__instance.Destroy();
							__result = true;
							return false;
						}
					}
					catch (Exception)
					{
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Faction), "Notify_RelationKindChanged")]
		internal static class Harmony_TryAffectGoodwillWith
		{
			public static void Postfix()
			{
				DubUtils.GetResearch().FactionUpdates();
			}
		}

		[HarmonyPatch(typeof(ResearchManager), "ResetAllProgress")]
		internal static class Harmony_ResetAllProgress
		{
			public static void Postfix()
			{
				DubUtils.GetResearch().ResetAllResearch();
			}
		}

		[HarmonyPatch(typeof(Site), "ShouldRemoveMapNow")]
		internal static class Harmony_Site_ShouldRemoveMapNow
		{
			public static void Postfix(DestroyedSettlement __instance, ref bool __result)
			{
				if (!__instance.Map.listerThings.ThingsOfDef(DubDef.ICBMStrike).NullOrEmpty())
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(Settlement), "ShouldRemoveMapNow")]
		internal static class Harmony_SettlementBase_ShouldRemoveMapNow
		{
			public static void Postfix(DestroyedSettlement __instance, ref bool __result)
			{
				if (!__instance.Map.listerThings.ThingsOfDef(DubDef.ICBMStrike).NullOrEmpty())
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(DestroyedSettlement), "ShouldRemoveMapNow")]
		internal static class Harmony_DestroyedSettlement_ShouldRemoveMapNow
		{
			public static void Postfix(DestroyedSettlement __instance, ref bool __result)
			{
				if (!__instance.Map.listerThings.ThingsOfDef(DubDef.ICBMStrike).NullOrEmpty())
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(ThingSetMaker_ResourcePod), "PossiblePodContentsDefs")]
		internal static class Harmony_PossiblePodContentsDefs
		{
			public static void Postfix(ref IEnumerable<ThingDef> __result)
			{
				__result = __result.Where((ThingDef x) => x.modContentPack == null || x.modContentPack != RimatomicsMod.Settings.Mod.Content);
			}
		}

		[HarmonyPatch(typeof(CompSpawnerHives), "CanSpawnHiveAt")]
		internal static class Harmony_CanSpawnHiveAt
		{
			public static void Postfix(ref IntVec3 c, ref Map map, ref bool __result)
			{
				if (c.GetThingList(map).Any((Thing x) => x.def.modContentPack != null && x.def.modContentPack == RimatomicsMod.Settings.Mod.Content))
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(InfestationCellFinder), "GetScoreAt")]
		internal static class Harmony_GetScoreAt
		{
			public static void Postfix(ref IntVec3 cell, ref Map map, ref float __result)
			{
				if (cell.GetThingList(map).Any((Thing x) => x.def.modContentPack != null && x.def.modContentPack == RimatomicsMod.Settings.Mod.Content))
				{
					__result = 0f;
				}
			}
		}

		[HarmonyPatch(typeof(Pawn), "Kill")]
		internal static class Harmony_Pawn_Kill
		{
			public static void Postfix(ref DamageInfo? dinfo)
			{
				if (dinfo?.Instigator != null && dinfo.Value.Instigator is Building_EnergyWeapon building_EnergyWeapon)
				{
					building_EnergyWeapon.DamageDealt += dinfo.Value.Amount;
					building_EnergyWeapon.KillCounter++;
				}
			}
		}

		[HarmonyPatch(typeof(BuildableDef))]
		[HarmonyPatch("IsResearchFinished", MethodType.Getter)]
		internal static class Harmony_IsResearchFinished
		{
			public static void Postfix(BuildableDef __instance, ref bool __result)
			{
				if (__instance is RimatomicsThingDef rimatomicsThingDef && !DebugSettings.godMode && !rimatomicsThingDef.StepsThatUnlock.NullOrEmpty())
				{
					if (rimatomicsThingDef.StepsThatUnlock.Any((ResearchStepDef x) => x.IsFinished))
					{
						__result = true;
					}
					else
					{
						__result = false;
					}
				}
			}
		}

		[HarmonyPatch(typeof(RecipeDef))]
		[HarmonyPatch("AvailableNow", MethodType.Getter)]
		internal static class Harmony_RecipeDef_AvailableNow
		{
			public static void Postfix(RecipeDef __instance, ref bool __result)
			{
				if (DubUtils.GetResearch().AllSteps.Any((ResearchStepDef x) => x.RecipeUnlocks.Contains(__instance) && !x.IsFinished))
				{
					__result = false;
				}
			}
		}

		[HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld")]
		public static class Harmony_ClearAllMapsAndWorld
		{
			public static void Prefix()
			{
				RimatomicsResearch._instance = null;
			}
		}

		[HarmonyPatch(typeof(PawnsArrivalModeWorker_CenterDrop), "TryResolveRaidSpawnCenter", null)]
		public static class Harmony_CenterDrop_TryResolveRaidSpawnCenter
		{
			public static void Postfix(IncidentParms parms, ref bool __result)
			{
				DubUtils.GetResearch().DonkeyRubarb(parms, ref __result);
				if (!__result || !DubUtils.GetResearch().ScrambleMode || !(parms.target is Map map) || !map.Rimatomics().AtomActive)
				{
					return;
				}
				if (parms.faction.HostileTo(Faction.OfPlayer))
				{
					float charge = 2f * parms.points;
					charge = Mathf.Min(charge, 11999f);
					Building_Radar building_Radar = map.Rimatomics().Radars.FirstOrDefault((Building_Radar x) => x.HasATOM && x.Working && x.powerComp.PowerNet.HasCharge(charge));
					if (building_Radar != null)
					{
						building_Radar.powerComp.PowerNet.DissipateCharge(charge);
						parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
						parms.spawnCenter = DropCellFinder.FindRaidDropCenterDistant(map);
						Find.LetterStack.ReceiveLetter("PodsScrambledOK".Translate(), "PodsScrambledDesc".Translate(charge), LetterDefOf.PositiveEvent);
					}
					else
					{
						Find.LetterStack.ReceiveLetter("PodsScrambledFail".Translate(), "PodsScrambledFailDesc".Translate(charge), LetterDefOf.NegativeEvent);
					}
				}
			}
		}

		[HarmonyPatch(typeof(RimWorld.ScenPart_CreateIncident), "Tick")]
		public static class Harmony_ScenPart_CreateIncident
		{
			public static bool detected;

			public static RimWorld.ScenPart_CreateIncident repeater;

			public static void Prefix(RimWorld.ScenPart_CreateIncident __instance)
			{
				if (__instance.repeat)
				{
					repeater = __instance;
				}
			}

			public static void Postfix(RimWorld.ScenPart_CreateIncident __instance)
			{
				if (detected)
				{
					if (__instance.repeat && __instance.intervalDays > 0f)
					{
						__instance.occurTick += __instance.IntervalTicks;
					}
					__instance.isFinished = false;
				}
				detected = false;
				repeater = null;
			}
		}

		[HarmonyPatch(typeof(Targeter), "TargeterUpdate")]
		public static class Harmony_Targeter_TargeterUpdate
		{
			public static bool DrawLowYieldField;

			public static bool Prefix(Targeter __instance)
			{
				if (__instance.mouseAttachment == Building_LaunchPad.TargeterMouseAttachment && DrawLowYieldField)
				{
					GenDraw.DrawRadiusRing(UI.MouseCell(), 55f);
					GenDraw.DrawCircleOutline(UI.MouseCell().ToVector3Shifted(), 55f, SimpleColor.Red);
				}
				if (__instance.targetingSource != null && __instance.targetingSource is Verb_RimatomicsVerb verb_RimatomicsVerb)
				{
					Building_EnergyWeapon getWep = verb_RimatomicsVerb.GetWep;
					if (getWep.RangeMin > 0f && getWep.RangeMin < GenRadial.MaxRadialPatternRadius)
					{
						GenDraw.DrawRadiusRing(getWep.Position, getWep.RangeMin);
					}
					if (getWep.Range < (float)(Find.CurrentMap.Size.x + Find.CurrentMap.Size.z) && getWep.Range < GenRadial.MaxRadialPatternRadius)
					{
						GenDraw.DrawRadiusRing(getWep.Position, getWep.Range);
					}
					LocalTargetInfo localTargetInfo = LocalTargetInfo.Invalid;
					if (__instance.IsTargeting)
					{
						using (IEnumerator<LocalTargetInfo> enumerator = GenUI.TargetsAtMouse(__instance.targetingSource.GetVerb.targetParams).GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								localTargetInfo = enumerator.Current;
							}
						}
						if (localTargetInfo.IsValid && !__instance.targetingSource.CanHitTarget(localTargetInfo))
						{
							localTargetInfo = LocalTargetInfo.Invalid;
						}
					}
					if (localTargetInfo.IsValid)
					{
						GenDraw.DrawTargetHighlight(localTargetInfo);
						bool needLOSToCenter;
						float num = verb_RimatomicsVerb.HighlightFieldRadiusAroundTarget(out needLOSToCenter);
						if (num > 0.2f && verb_RimatomicsVerb.TryFindShootLineFromTo(getWep.Position, localTargetInfo, out var resultingLine))
						{
							if (needLOSToCenter)
							{
								GenExplosion.RenderPredictedAreaOfEffect(resultingLine.Dest, num);
							}
							else
							{
								GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(resultingLine.Dest, num, useCenter: true)
									where x.InBounds(Find.CurrentMap)
									select x).ToList());
							}
						}
					}
					return false;
				}
				return true;
			}
		}

		public static bool Pstrike;

		public static bool SoS;

		public static void Prefix(ThingComp __instance, ref int visionRange, ref int detectionRange)
		{
			if (__instance.parent is Building_Radar building_Radar && building_Radar.HasATOM)
			{
				visionRange = 8;
				detectionRange = 25;
			}
		}

		public static void DonkeyRubarbPostfix(IncidentParms parms, ref bool __result)
		{
			DubUtils.GetResearch().DonkeyRubarb(parms, ref __result);
		}

		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("Dubwise.Rimatomics");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			if (MP.enabled)
			{
				MP.RegisterAll(Assembly.GetExecutingAssembly());
			}
			HarmonyMethod postfix = new HarmonyMethod(typeof(HarmonyPatches), "DonkeyRubarbPostfix");
			string name = "TryResolveRaidSpawnCenter";
			harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeDrop), name), null, postfix);
			harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeDropGroups), name), null, postfix);
			harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeWalkIn), name), null, postfix);
			harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeWalkInGroups), name), null, postfix);
			harmony.Patch(AccessTools.Method(typeof(PawnsArrivalModeWorker_RandomDrop), name), null, postfix);
			MethodInfo methodInfo = AccessTools.Method("PreemptiveStrike.Things.CompDetection:UpdateDetectionAbility");
			if (methodInfo != null)
			{
				Log.Message("Rimatomics found Preemptive Strike -- Patching");
				Pstrike = true;
				harmony.Patch(methodInfo, new HarmonyMethod(typeof(HarmonyPatches), "Prefix"));
			}
			ThingDef namedSilentFail = DefDatabase<ThingDef>.GetNamedSilentFail("ShipCapacitor");
			if (namedSilentFail != null)
			{
				Log.Message("Rimatomics found SoS2 -- Patching");
				SoS = true;
				namedSilentFail.thingClass = typeof(Building_ShipCapacitorPPC);
			}
			if (ModLister.RoyaltyInstalled)
			{
				harmony.Patch(AccessTools.Method(typeof(Projectile), "CheckForFreeInterceptBetween"), new HarmonyMethod(typeof(H_CheckForFreeInterceptBetween), "Prefix"));
			}
		}
	}
}
