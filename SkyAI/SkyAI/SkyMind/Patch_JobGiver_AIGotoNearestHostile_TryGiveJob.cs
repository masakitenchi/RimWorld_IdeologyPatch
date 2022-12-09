using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000044 RID: 68
	[HarmonyPatch(typeof(JobGiver_AIGotoNearestHostile))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_AIGotoNearestHostile_TryGiveJob
	{
		// Token: 0x060001DE RID: 478 RVA: 0x00029F3C File Offset: 0x0002813C
		private static bool Prefix(ref JobGiver_AIGotoNearestHostile __instance, ref Pawn pawn, ref Job __result)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = pawn.RaceProps.Animal || pawn.RaceProps.intelligence == Intelligence.Animal || (pawn.Faction != null && pawn.Faction.def.techLevel == TechLevel.Animal);
				if (flag)
				{
					result = true;
				}
				else
				{
					float num = float.MaxValue;
					Thing thing = null;
					List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
					for (int i = 0; i < potentialTargetsFor.Count; i++)
					{
						IAttackTarget attackTarget = potentialTargetsFor[i];
						Thing thing2 = (Thing)attackTarget;
						bool flag2 = AdvancedAI.IsActiveTarget(pawn, thing2, true, false) && AttackTargetFinder.IsAutoTargetable(attackTarget);
						if (flag2)
						{
							int num2 = thing2.Position.DistanceToSquared(pawn.Position);
							bool flag3 = (float)num2 < num && pawn.CanReach(thing2, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
							if (flag3)
							{
								num = (float)num2;
								thing = thing2;
							}
						}
					}
					bool flag4 = thing != null;
					if (flag4)
					{
						IntVec3 invalid = IntVec3.Invalid;
						ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
						bool flag5 = thingWithComps != null && AdvancedAI.PrimaryWeaponRange(pawn) > 5f;
						if (flag5)
						{
							float effectiveRange = AdvancedAI.EffectiveRange(pawn);
							bool coverRequired = AdvancedAI_CoverUtility.IsCovered(pawn, thing.Position, true, false, true, false, false, false, false);
							bool flag6;
							AdvancedAI.TryFindShootingPosition(pawn, AdvancedAI.PrimaryVerb(pawn), coverRequired, effectiveRange, out invalid, out flag6);
						}
						bool flag7 = invalid.IsValid && pawn.Position != invalid;
						if (flag7)
						{
							Job job = JobMaker.MakeJob(JobDefOf.Goto, invalid);
							job.checkOverrideOnExpire = true;
							job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
							job.locomotionUrgency = AdvancedAI.ResolveCombatLocomotion(pawn, thing, 0f);
							__result = job;
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: AIGotoNearestHostile. Job target: {2} Goto destination: {3}", new object[]
								{
									pawn,
									pawn.Position,
									thing,
									invalid
								}));
							}
							result = false;
						}
						else
						{
							Job job2 = JobMaker.MakeJob(JobDefOf.Goto, thing);
							job2.checkOverrideOnExpire = true;
							job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
							job2.locomotionUrgency = AdvancedAI.ResolveCombatLocomotion(pawn, thing, 0f);
							job2.collideWithPawns = true;
							bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: AIGotoNearestHostile. Job target: {2} Goto: {3}", new object[]
								{
									pawn,
									pawn.Position,
									thing,
									thing.Position
								}));
							}
							__result = job2;
							result = false;
						}
					}
					else
					{
						__result = null;
						result = false;
					}
				}
			}
			return result;
		}
	}
}
