using System;
using System.Collections.Generic;
using System.Linq;
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
	// Token: 0x0200001B RID: 27
	public static class AdvancedAI_Jobs
	{
		// Token: 0x060000D3 RID: 211 RVA: 0x0001010C File Offset: 0x0000E30C
		public static Job MainPassBlockerJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker, Verb verb)
		{
			bool flag = AdvancedAI.DangerousNonLOSTarget(pawn, (Building)blocker, 8f) || (blocker != null && AdvancedAI.CellReservedForMainBlow(pawn).Contains(cellBeforeBlocker));
			if (flag)
			{
				bool flag2 = !pawn.CanReserveAndReach(blocker, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false);
				if (flag2)
				{
					blocker = AdvancedAI.MeleeTrashBuilding(blocker.Position, pawn, 16f, 0f);
				}
				bool flag3 = blocker != null;
				if (flag3)
				{
					bool flag4 = SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						bool debugLog = SkyAiCore.Settings.debugLog;
						if (debugLog)
						{
							Log.Message(string.Format("{0} {1}: Siege AI. MainPassBlockerJob. newMainTarget3: {2} {3} cellBeforeBlocker: {4}", new object[]
							{
								pawn,
								pawn.Position,
								blocker,
								blocker.Position,
								cellBeforeBlocker
							}));
						}
						bool debugTargets = SkyAiCore.Settings.debugTargets;
						if (debugTargets)
						{
							Log.Message(string.Format("{0} {1} Siege AI. MainPassBlockerJob. attack building6: {2} : {3} cellBeforeBlocker: {4}", new object[]
							{
								pawn,
								pawn.Position,
								blocker,
								blocker.Position,
								cellBeforeBlocker
							}));
							pawn.Map.debugDrawer.FlashCell(blocker.Position, 0.45f, "M7", SkyAiCore.Settings.flashCellDelay);
						}
					}
					bool flag5 = !pawn.CanReserve(blocker, 1, -1, null, false);
					if (flag5)
					{
						return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
					}
					Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, blocker);
					job.ignoreDesignations = true;
					job.expiryInterval = AdvancedAI.CombatInterval(pawn, blocker.Position, 0f);
					job.checkOverrideOnExpire = true;
					return job;
				}
			}
			else
			{
				bool flag6 = blocker != null;
				if (flag6)
				{
					Building building = blocker as Building;
					bool flag7 = building != null && verb != null && cellBeforeBlocker.DistanceTo(blocker.Position) <= verb.verbProps.minRange;
					if (flag7)
					{
						IntRange distance = new IntRange(Mathf.RoundToInt(AdvancedAI.MinDistance(pawn, verb)), Mathf.Max(25, Mathf.RoundToInt(AdvancedAI.EffectiveRange(pawn))));
						IntVec3 intVec = AdvancedAI.NewCellPositionNearBuilding(pawn, building, verb, distance, false);
						bool isValid = intVec.IsValid;
						if (isValid)
						{
							bool flag8 = !pawn.CanReserve(intVec, 1, -1, null, false) || intVec == pawn.Position;
							if (flag8)
							{
								return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
							}
							bool flag9 = SkyAiCore.SelectedPawnDebug(pawn) && SkyAiCore.Settings.debugPath;
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: Siege AI. MainPassBlockerJob. Destroyer Goto1 to {2}", pawn, pawn.Position, intVec));
								pawn.Map.debugDrawer.FlashCell(cellBeforeBlocker, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
							}
							Job job2 = JobMaker.MakeJob(JobDefOf.Goto, intVec);
							job2.expiryInterval = AdvancedAI.CombatInterval(pawn, building.Position, 0f);
							job2.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
							job2.checkOverrideOnExpire = true;
							job2.collideWithPawns = true;
							return job2;
						}
						else
						{
							bool flag10 = !pawn.CanReserve(cellBeforeBlocker, 1, -1, null, false) || cellBeforeBlocker == pawn.Position;
							if (flag10)
							{
								return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
							}
							bool flag11 = SkyAiCore.SelectedPawnDebug(pawn) && SkyAiCore.Settings.debugPath;
							if (flag11)
							{
								Log.Message(string.Format("{0} {1}: Siege AI. MainPassBlockerJob. Destroyer Goto2 to {2}", pawn, pawn.Position, cellBeforeBlocker));
								pawn.Map.debugDrawer.FlashCell(cellBeforeBlocker, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
							}
							Job job3 = JobMaker.MakeJob(JobDefOf.Goto, cellBeforeBlocker);
							job3.expiryInterval = AdvancedAI.CombatInterval(pawn, cellBeforeBlocker, 0f);
							job3.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
							job3.checkOverrideOnExpire = true;
							job3.collideWithPawns = true;
							return job3;
						}
					}
					else
					{
						Job job4 = AdvancedAI_Jobs.PassBlockerJob(pawn, blocker, cellBeforeBlocker, verb);
						bool flag12 = job4 != null;
						if (flag12)
						{
							bool flag13 = !pawn.CanReserve(blocker, 1, -1, null, false);
							if (flag13)
							{
								return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
							}
							bool flag14 = SkyAiCore.SelectedPawnDebug(pawn);
							if (flag14)
							{
								bool debugTargets2 = SkyAiCore.Settings.debugTargets;
								if (debugTargets2)
								{
									Log.Message(string.Format("{0} {1}: Siege AI. MainPassBlockerJob. PassBlockerJob on {2}", pawn, pawn.Position, blocker));
									pawn.Map.debugDrawer.FlashCell(blocker.Position, 0.5f, "M1", SkyAiCore.Settings.flashCellDelay);
								}
								bool debugPath = SkyAiCore.Settings.debugPath;
								if (debugPath)
								{
									Log.Message(string.Format("{0} {1}: Siege AI. MainPassBlockerJob. PassBlockerJob on {2}", pawn, pawn.Position, cellBeforeBlocker));
									pawn.Map.debugDrawer.FlashCell(cellBeforeBlocker, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
								}
							}
							return job4;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0001065C File Offset: 0x0000E85C
		public static Job PassBlockerJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker, Verb verb)
		{
			bool flag = SkyAiCore.Settings.canMineMineables;
			bool flag2 = SkyAiCore.Settings.canMineNonMineables;
			bool flag3 = StatDefOf.MiningSpeed.Worker.IsDisabledFor(pawn);
			if (flag3)
			{
				flag = false;
				flag2 = false;
			}
			bool mineable = blocker.def.mineable;
			Job result;
			if (mineable)
			{
				bool flag4 = flag;
				if (flag4)
				{
					result = AdvancedAI_Jobs.MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
				}
				else
				{
					result = AdvancedAI_Jobs.MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
				}
			}
			else
			{
				bool flag5 = verb != null && verb.IsMeleeAttack && verb.verbProps.range > 8f && verb.verbProps.ai_IsBuildingDestroyer && AdvancedAI.CanSetFireOnTarget(pawn, blocker);
				if (flag5)
				{
					bool flag6 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag6)
					{
						Log.Message(string.Format("{0} {1}: Siege AI. PassBlockerJob. Blocker pos: {2} cellBeforeBlocker: {3} dist: {4} min: {5} result: {6}", new object[]
						{
							pawn,
							pawn.Position,
							blocker.Position,
							cellBeforeBlocker,
							cellBeforeBlocker.DistanceTo(blocker.Position),
							AdvancedAI.MinDistance(pawn, verb),
							cellBeforeBlocker.DistanceTo(blocker.Position) > AdvancedAI.MinDistance(pawn, verb)
						}));
					}
					Job job = JobMaker.MakeJob(verb.verbProps.ai_IsWeapon ? JobDefOf.AttackStatic : JobDefOf.UseVerbOnThing);
					job.targetA = blocker;
					job.verbToUse = verb;
					bool ai_IsWeapon = verb.verbProps.ai_IsWeapon;
					if (ai_IsWeapon)
					{
						job.endIfCantShootTargetFromCurPos = true;
					}
					job.expiryInterval = AdvancedAI.CombatInterval(pawn, blocker.Position, 0f);
					result = job;
				}
				else
				{
					bool flag7 = flag2;
					if (flag7)
					{
						result = AdvancedAI_Jobs.MineOrWaitJob(pawn, blocker, cellBeforeBlocker);
					}
					else
					{
						result = AdvancedAI_Jobs.MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
					}
				}
			}
			return result;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00010840 File Offset: 0x0000EA40
		public static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			List<IntVec3> list = AdvancedAI.CellReservedForMainBlow(pawn);
			bool flag = blocker != null && (AdvancedAI.DangerousNonLOSTarget(pawn, (Building)blocker, 8f) || (!list.NullOrEmpty<IntVec3>() && list.Count > 0 && list.Contains(blocker.Position)));
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn) && list.Contains(blocker.Position);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: reservedCells not null. Count: {2}", pawn, pawn.Position, list.Count));
				}
				Building building = AdvancedAI.MeleeTrashBuilding(blocker.Position, pawn, 20f, 3f);
				bool flag3 = building != null;
				if (flag3)
				{
					bool flag4 = !pawn.CanReserve(building, 1, -1, null, false);
					if (flag4)
					{
						return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
					}
					Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, building);
					job.ignoreDesignations = true;
					job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
					job.checkOverrideOnExpire = true;
					return job;
				}
			}
			bool flag5 = !pawn.CanReserve(blocker, 1, -1, null, false);
			Job result;
			if (flag5)
			{
				result = AdvancedAI_Jobs.WaitNearJob(pawn, cellBeforeBlocker);
			}
			else
			{
				Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, blocker);
				job2.ignoreDesignations = true;
				job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
				job2.checkOverrideOnExpire = true;
				result = job2;
			}
			return result;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x000109C8 File Offset: 0x0000EBC8
		private static Job MineOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			List<IntVec3> list = AdvancedAI.CellReservedForMainBlow(pawn);
			bool flag = blocker != null && (AdvancedAI.DangerousNonLOSTarget(pawn, (Building)blocker, 8f) || (!list.NullOrEmpty<IntVec3>() && list.Count > 0 && list.Contains(blocker.Position)));
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn) && list.Contains(blocker.Position);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: reservedCells not null. Count: {2}", pawn, pawn.Position, list.Count));
				}
				Building building = AdvancedAI.MeleeTrashBuilding(blocker.Position, pawn, 20f, 3f);
				bool flag3 = building != null;
				if (flag3)
				{
					bool flag4 = !pawn.CanReserve(building, 1, -1, null, false);
					if (flag4)
					{
						return AdvancedAI_Jobs.WaitNearJob(pawn, pawn.Position);
					}
					Job job = JobMaker.MakeJob(JobDefOf.Mine, building);
					job.ignoreDesignations = true;
					job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
					job.checkOverrideOnExpire = true;
					return job;
				}
			}
			bool flag5 = !pawn.CanReserve(blocker, 1, -1, null, false);
			Job result;
			if (flag5)
			{
				result = AdvancedAI_Jobs.WaitNearJob(pawn, cellBeforeBlocker);
			}
			else
			{
				Job job2 = JobMaker.MakeJob(JobDefOf.Mine, blocker);
				job2.ignoreDesignations = true;
				job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
				job2.checkOverrideOnExpire = true;
				result = job2;
			}
			return result;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00010B50 File Offset: 0x0000ED50
		private static Job WaitNearJob(Pawn pawn, IntVec3 cellBeforeBlocker)
		{
			Thing thing;
			bool flag = !AdvancedAI.DangerousNonLOSTarget(pawn, cellBeforeBlocker, out thing, 9f, false);
			Job result;
			if (flag)
			{
				IntVec3 intVec = CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10, null);
				bool flag2 = intVec == pawn.Position;
				if (flag2)
				{
					result = JobMaker.MakeJob(JobDefOf.Wait, 40, true);
				}
				else
				{
					result = JobMaker.MakeJob(JobDefOf.Goto, intVec, AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow), true);
				}
			}
			else
			{
				result = JobMaker.MakeJob(JobDefOf.Wait, 80, true);
			}
			return result;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00010BD8 File Offset: 0x0000EDD8
		public static Job GetCoverOrMeleeJob(Pawn pawn, Verb verb, Thing blocker, IntVec3 cellBeforeBlocker)
		{
			Thing thing;
			bool flag = !AdvancedAI.DangerousNonLOSTarget(pawn, cellBeforeBlocker, out thing, 9f, false) && AdvancedAI_CoverUtility.CoverRequired(pawn, cellBeforeBlocker, 2f, 3);
			if (flag)
			{
				IntVec3 intVec;
				bool coverPositionFrom = AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, cellBeforeBlocker, (float)Mathf.RoundToInt(Mathf.Max(10f, verb.verbProps.range * 0.5f)), 3f, true, false, true, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out intVec);
				if (coverPositionFrom)
				{
					bool flag2 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						Log.Message(string.Format("{0} from using GetCoverOrMeleeJob going to coverJob on: {1}", pawn, intVec));
						pawn.Map.debugDrawer.FlashCell(intVec, 0.45f, "ELC", SkyAiCore.Settings.flashCellDelay);
					}
					bool isValid = intVec.IsValid;
					if (isValid)
					{
						return AdvancedAI_Jobs.GetCoverJob(pawn, intVec, cellBeforeBlocker, AdvancedAI.ExpireInterval.fast, false, false, true);
					}
				}
			}
			else
			{
				bool flag3 = blocker != null;
				if (flag3)
				{
					Job job = AdvancedAI_Jobs.MeleeOrWaitJob(pawn, blocker, cellBeforeBlocker);
					bool flag4 = job != null && blocker != null;
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugTargets && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} from using GetCoverOrMeleeJob going melee attack", pawn));
							pawn.Map.debugDrawer.FlashCell(cellBeforeBlocker, 0.45f, "ELM", SkyAiCore.Settings.flashCellDelay);
						}
						return job;
					}
				}
			}
			return null;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00010D58 File Offset: 0x0000EF58
		public static Job GetCoverJob(Pawn pawn, IntVec3 position, IntVec3 focusCell, AdvancedAI.ExpireInterval interval, bool ignoreFocusCellMinDistance = false, bool isReserved = false, bool useCheckOverrideOnExpire = true)
		{
			bool aiAvoidCover = pawn.kindDef.aiAvoidCover;
			Job result;
			if (aiAvoidCover)
			{
				result = null;
			}
			else
			{
				if (isReserved)
				{
					ignoreFocusCellMinDistance = true;
				}
				bool flag = !focusCell.IsValid;
				if (flag)
				{
					LocalTargetInfo enemyTarget = AdvancedAI.GetEnemyTarget(pawn, false, true);
					bool flag2 = enemyTarget != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1}: Cover job decisions. FocusCell is not valid. GetEnemyTarget. New target is: {2} {3}", new object[]
							{
								pawn,
								pawn.Position,
								enemyTarget,
								enemyTarget.Cell
							}));
						}
						focusCell = enemyTarget.Cell;
					}
				}
				float num = focusCell.IsValid ? pawn.Position.DistanceTo(focusCell) : 0f;
				bool flag4 = !ignoreFocusCellMinDistance;
				if (flag4)
				{
					float num2 = AdvancedAI.PrimaryWeaponRange(pawn);
					float num3 = 55f;
					bool flag5 = AdvancedAI.IsRaidLeaderOrSquadCommander(pawn) && num2 < num3;
					if (flag5)
					{
						num2 = Math.Max(num3, num2);
					}
					bool flag6 = num2 < num;
					bool flag7 = flag6;
					if (flag7)
					{
						bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag8)
						{
							Log.Message(string.Format("{0} {1}: Cover job decisions. Too long distance to focus cell. Return. Range/MinRange: {2}/{3} longDistanceToFocusCell: {4}", new object[]
							{
								pawn,
								pawn.Position,
								num2,
								num3,
								flag6
							}));
						}
						return null;
					}
				}
				bool flag9 = AdvancedAI_CoverUtility.IsCovered(pawn, focusCell);
				int num4 = (int)AccessTools.Field(typeof(Pawn_PathFollower), "lastMovedTick").GetValue(pawn.pather);
				bool flag10 = Find.TickManager.TicksGame - num4 <= 300;
				bool flag11 = AdvancedAI.IsDangerousCoverPosition(pawn, position);
				bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag12)
				{
					Log.Message(string.Format("{0} {1}: Cover job decisions. isCovered: {2} movedRecently: {3}/{4}/{5} IsDangerousPosition: {6} focuscell: {7} My distance to focus cell: {8}", new object[]
					{
						pawn,
						pawn.Position,
						flag9,
						Find.TickManager.TicksGame - num4,
						300,
						flag10,
						flag11,
						focusCell,
						num
					}));
				}
				bool flag13 = pawn.Position != position && !flag11;
				if (flag13)
				{
					Job job = JobMaker.MakeJob(JobDefOf.Goto, position);
					job.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
					job.canUseRangedWeapon = true;
					if (useCheckOverrideOnExpire)
					{
						job.checkOverrideOnExpire = true;
						job.expiryInterval = AdvancedAI.CombatInterval(pawn, focusCell, num);
					}
					bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag14)
					{
						Log.Message(string.Format("{0} {1}: Cover job. Moving to position {2} focuscell: {3} my distance to new position: {4} Distance from pos to focus cell: {5}", new object[]
						{
							pawn,
							pawn.Position,
							position,
							focusCell,
							pawn.Position.DistanceTo(position),
							position.DistanceTo(focusCell)
						}));
					}
					result = job;
				}
				else
				{
					bool flag15 = isReserved && AdvancedAI_CoverUtility.IsCovered(pawn, focusCell, true, false, false, false, false, false, false);
					if (flag15)
					{
						bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag16)
						{
							Log.Message(string.Format("{0} {1}: is reserved and waiting in covered position. shouldNotBeReserved?", pawn, pawn.Position));
						}
						CompDoctorRole comp = pawn.GetComp<CompDoctorRole>();
						bool flag17 = comp != null && comp.treatmentType == CompDoctorRole.TreatmentType.remote && (comp.Patient == null || (comp.Patient != null && !AdvancedAI.IsActivePawn(comp.Patient)));
						bool flag18 = !flag17;
						if (flag18)
						{
							bool flag19 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag19)
							{
								Log.Message(string.Format("{0} {1}: is reserved and waiting in covered position.", pawn, pawn.Position));
							}
							bool flag20 = pawn.Position == position;
							if (flag20)
							{
								Job job2 = JobMaker.MakeJob(JobDefOf.Wait, position);
								job2.canUseRangedWeapon = true;
								job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
								job2.checkOverrideOnExpire = true;
								bool flag21 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag21)
								{
									Log.Message(string.Format("{0} {1}: Cover job. Already covered reserved for treatment. Wait on: {2} focuscell: {3} distance: {4} IsDangerousPosition: {5}", new object[]
									{
										pawn,
										pawn.Position,
										position,
										focusCell,
										position.DistanceTo(focusCell),
										flag11
									}));
								}
								return job2;
							}
							Job job3 = JobMaker.MakeJob(JobDefOf.Goto, position);
							job3.canUseRangedWeapon = true;
							job3.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
							job3.checkOverrideOnExpire = true;
							bool flag22 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag22)
							{
								Log.Message(string.Format("{0} {1}: Cover job. Move to reserved for treatment position: {2} focuscell: {3} distance: {4} IsDangerousPosition: {5}", new object[]
								{
									pawn,
									pawn.Position,
									position,
									focusCell,
									position.DistanceTo(focusCell),
									flag11
								}));
							}
							return job3;
						}
					}
					bool flag23 = AdvancedAI.CellAlreadyOccupied(pawn, pawn.Position);
					if (flag23)
					{
						IntVec3 intVec = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, 3, (IntVec3 cell) => cell.Standable(pawn.Map) && !cell.ContainsStaticFire(pawn.Map));
						Job job4 = JobMaker.MakeJob(JobDefOf.Goto, intVec);
						job4.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
						job4.canUseRangedWeapon = true;
						if (useCheckOverrideOnExpire)
						{
							job4.checkOverrideOnExpire = true;
							job4.expiryInterval = AdvancedAI.CombatInterval(pawn, focusCell, num);
						}
						bool flag24 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag24)
						{
							Log.Message(string.Format("{0} {1}: Cover job. Somebody bothers me. Moving to near position {2}", pawn, pawn.Position, intVec));
						}
						result = job4;
					}
					else
					{
						bool flag25 = !flag11;
						if (flag25)
						{
							bool flag26 = position == pawn.Position;
							if (flag26)
							{
								bool flag27 = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
								if (flag27)
								{
									Job job5 = JobMaker.MakeJob(JobDefOf.Wait_Combat, position);
									job5.canUseRangedWeapon = true;
									job5.expiryInterval = AdvancedAI.Interval(interval);
									job5.checkOverrideOnExpire = true;
									bool flag28 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag28)
									{
										Log.Message(string.Format("{0} {1}: Cover job. Already covered on: {2} focuscell: {3} distance: {4} IsDangerousPosition: {5}", new object[]
										{
											pawn,
											pawn.Position,
											position,
											focusCell,
											position.DistanceTo(focusCell),
											flag11
										}));
									}
									result = job5;
								}
								else
								{
									result = null;
								}
							}
							else
							{
								Job job6 = JobMaker.MakeJob(JobDefOf.Goto, position);
								job6.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
								job6.canUseRangedWeapon = true;
								if (useCheckOverrideOnExpire)
								{
									job6.checkOverrideOnExpire = true;
									job6.expiryInterval = AdvancedAI.CombatInterval(pawn, focusCell, num);
								}
								bool flag29 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag29)
								{
									Log.Message(string.Format("{0} {1}: Cover job. Moving to position: {2} IsDangerousPosition: {3}", new object[]
									{
										pawn,
										pawn.Position,
										position,
										flag11
									}));
								}
								result = job6;
							}
						}
						else
						{
							bool flag30 = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
							if (flag30)
							{
								Job job7 = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
								job7.canUseRangedWeapon = true;
								job7.expiryInterval = AdvancedAI.Interval(interval);
								job7.checkOverrideOnExpire = true;
								bool flag31 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag31)
								{
									Log.Message(string.Format("{0} {1}: Cover job on current position: {2} focuscell: {3} distance: {4} IsDangerousPosition: {5}", new object[]
									{
										pawn,
										pawn.Position,
										pawn.Position,
										focusCell,
										pawn.Position.DistanceTo(focusCell),
										flag11
									}));
								}
								result = job7;
							}
							else
							{
								result = null;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00011820 File Offset: 0x0000FA20
		public static Job EscapeExplosionJob(Pawn pawn)
		{
			bool flag = pawn.RaceProps.intelligence < Intelligence.Humanlike;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.mindState.knownExploder == null;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = !pawn.mindState.knownExploder.Spawned;
					if (flag3)
					{
						pawn.mindState.knownExploder = null;
						result = null;
					}
					else
					{
						bool flag4 = PawnUtility.PlayerForcedJobNowOrSoon(pawn);
						if (flag4)
						{
							result = null;
						}
						else
						{
							Thing knownExploder = pawn.mindState.knownExploder;
							bool flag5 = (float)(pawn.Position - knownExploder.Position).LengthHorizontalSquared > 81f;
							if (flag5)
							{
								result = null;
							}
							else
							{
								IntVec3 c;
								bool flag6 = !RCellFinder.TryFindDirectFleeDestination(knownExploder.Position, 9f, pawn, out c);
								if (flag6)
								{
									result = null;
								}
								else
								{
									bool flag7 = c.Equals(pawn.Position);
									if (flag7)
									{
										result = null;
									}
									else
									{
										Job job = JobMaker.MakeJob(JobDefOf.Goto, c);
										job.locomotionUrgency = LocomotionUrgency.Sprint;
										result = job;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00011940 File Offset: 0x0000FB40
		public static Job GetTendJob(Pawn pawn, IntVec3 position, Pawn patient)
		{
			bool flag = !AdvancedAI.IsActivePawn(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = AdvancedAI.IsValidLoc(position);
				if (flag2)
				{
					IntVec3 intVec = position;
					bool flag3 = patient != null && position == patient.Position;
					if (flag3)
					{
						AdvancedAI.TryFindGoodAdjacentSpotToTouch(pawn, position, out intVec);
					}
					bool flag4 = pawn.Position != intVec;
					if (flag4)
					{
						Job tendJob = JobMaker.MakeJob(JobDefOfAI.AITend, patient);
						CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
						bool flag5 = compDoctorRole != null && compDoctorRole.treatmentType == CompDoctorRole.TreatmentType.standart;
						bool flag6 = !pawn.jobs.jobQueue.Any((QueuedJob q) => q.job == tendJob) && flag5;
						if (flag6)
						{
							bool flag7 = (SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn)) || SkyAiCore.SelectedPawnDebug(patient);
							if (flag7)
							{
								Log.Message(string.Format("{0} {1} is Doctor. Patient: {2} on distance. Add jobQueue.EnqueueLast: JobDefOfAI.AITend", pawn, pawn.Position, patient));
							}
							pawn.jobs.ClearQueuedJobs(true);
							pawn.jobs.jobQueue.EnqueueLast(tendJob, null);
						}
						Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
						job.checkOverrideOnExpire = true;
						job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
						job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
						bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag8)
						{
							Log.Message(string.Format("{0} {1}: Tend job. Moving to position {2} for treatment. My distance to cell: {3}", new object[]
							{
								pawn,
								pawn.Position,
								intVec,
								pawn.Position.DistanceTo(intVec)
							}));
						}
						return job;
					}
				}
				bool flag9 = patient != null && patient != pawn;
				if (flag9)
				{
					bool flag10 = AdvancedAI_TendUtility.HasHediffsNeedingTend(patient) && pawn.CanReserve(patient, 1, -1, null, true);
					if (flag10)
					{
						bool flag11 = !AdvancedAI.IsValidLoc(position);
						if (flag11)
						{
							Thing thing = AdvancedAI_TendUtility.StartCarryMedicine(pawn, patient, false);
							bool flag12 = thing != null;
							if (flag12)
							{
								bool flag13 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(patient));
								if (flag13)
								{
									Log.Message(string.Format("{0} {1}: Tend job. Position is not valid. Tend patient: {2} with medicine: {3}", new object[]
									{
										pawn,
										pawn.Position,
										patient,
										thing
									}));
								}
								return JobMaker.MakeJob(JobDefOfAI.AITend, patient, thing);
							}
							bool flag14 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(patient));
							if (flag14)
							{
								Log.Message(string.Format("{0} {1}: Tend job. Position is not valid. Tend patient without medicine: {2}", pawn, pawn.Position, patient));
							}
							return JobMaker.MakeJob(JobDefOfAI.AITend, patient);
						}
						else
						{
							Thing thing2 = AdvancedAI_TendUtility.StartCarryMedicine(pawn, patient, false);
							bool flag15 = thing2 != null;
							if (flag15)
							{
								bool flag16 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(patient));
								if (flag16)
								{
									Log.Message(string.Format("{0} {1}: Tend job. Position is valid. Tend patient: {2} with medicine: {3}", new object[]
									{
										pawn,
										pawn.Position,
										patient,
										thing2
									}));
								}
								return JobMaker.MakeJob(JobDefOfAI.AITend, patient, thing2);
							}
							bool flag17 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(patient));
							if (flag17)
							{
								Log.Message(string.Format("{0} {1}: Tend job. Position is valid. Tend patient without medicine: {2}", pawn, pawn.Position, patient));
							}
							return JobMaker.MakeJob(JobDefOfAI.AITend, patient);
						}
					}
					else
					{
						bool flag18 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag18)
						{
							Log.Message(string.Format("{0} {1}: Tend job. Can't start tend bcs already hase same job or failed to reserve patient: {2}", pawn, pawn.Position, patient));
						}
					}
				}
				bool flag19 = AdvancedAI_TendUtility.IsReadyForSelfTend(pawn) && !AdvancedAI_TendUtility.HasAlreadyStartTendJob(pawn, false) && AdvancedAI_TendUtility.HasHediffsNeedingTend(pawn);
				if (flag19)
				{
					Job job2 = JobMaker.MakeJob(JobDefOfAI.AITend, pawn);
					bool flag20 = job2 != null;
					if (flag20)
					{
						job2.reactingToMeleeThreat = true;
						job2.canUseRangedWeapon = true;
						job2.endAfterTendedOnce = true;
						bool flag21 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag21)
						{
							Log.Message(string.Format("{0} {1}: Tend job. Tend self on: {2}", pawn, pawn.Position, position));
						}
						result = job2;
					}
					else
					{
						result = null;
					}
				}
				else
				{
					bool flag22 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag22)
					{
						Log.Message(string.Format("{0} {1}: Tend job. Waiting for self tend on: {2}", pawn, pawn.Position, pawn.Position));
					}
					Job job3 = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
					job3.canUseRangedWeapon = true;
					job3.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
					job3.checkOverrideOnExpire = true;
					result = job3;
				}
			}
			return result;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00011EB0 File Offset: 0x000100B0
		public static Job GetTreatmentReservationJob(Pawn pawn)
		{
			Pawn pawn2 = AdvancedAI_TendUtility.ClosestDoctor(pawn);
			bool flag = pawn2 != null;
			if (flag)
			{
				CompDoctorRole compDoctorRole = pawn2.TryGetComp<CompDoctorRole>();
				bool flag2 = compDoctorRole != null;
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: Reservation Job. Trying to reserve doctor {2}.", pawn, pawn.Position, pawn2));
					}
					bool flag4 = compDoctorRole.Patient != null && compDoctorRole.Patient == pawn;
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: Reservation Job. Already reserved doctor: {2}. TreatmentType: {3}", new object[]
							{
								pawn,
								pawn.Position,
								pawn2,
								compDoctorRole.GetTreatmentType
							}));
						}
						return AdvancedAI_Jobs.GetTreatmentJob(pawn);
					}
					bool flag6 = !compDoctorRole.Reserved;
					if (flag6)
					{
						pawn2.TryGetComp<CompDoctorRole>().patient = pawn;
						pawn2.TryGetComp<CompDoctorRole>().treatmentType = CompDoctorRole.TreatmentType.remote;
						bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag7)
						{
							Log.Message(string.Format("{0} {1}: Reservation Job. Reserve {2} success. TreatmentType: {3}", new object[]
							{
								pawn,
								pawn.Position,
								pawn2,
								compDoctorRole.GetTreatmentType
							}));
						}
						return AdvancedAI_Jobs.GetTreatmentJob(pawn);
					}
					bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag8)
					{
						Log.Message(string.Format("{0} {1}: Reservation Job. Problem with doctor reservation.", pawn, pawn.Position));
					}
				}
			}
			bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag9)
			{
				Log.Message(string.Format("{0} {1}: Reservation Job return null.", pawn, pawn.Position));
			}
			return null;
		}

		// Token: 0x060000DD RID: 221 RVA: 0x000120A4 File Offset: 0x000102A4
		public static Job GetTreatmentJob(Pawn pawn)
		{
			Pawn pawn2 = AdvancedAI_TendUtility.ClosestDoctor(pawn);
			bool flag = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag)
			{
				Log.Message(string.Format("{0} {1}: Treatment Job. Check for doctor first!", pawn, pawn.Position));
			}
			bool flag2 = pawn2 != null;
			if (flag2)
			{
				try
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: Treatment Job. Closest doctor found: {2} {3}", new object[]
						{
							pawn,
							pawn.Position,
							pawn2,
							pawn2.Position
						}));
					}
					IntVec3 intVec;
					AdvancedAI.TryFindGoodAdjacentSpotToTouch(pawn, pawn2.Position, out intVec);
					bool flag4 = pawn.Position != intVec;
					if (flag4)
					{
						IntVec3 cell = AdvancedAI.GetEnemyTarget(pawn2, false, true).Cell;
						bool flag5 = pawn2.pather.MovingNow && (!AdvancedAI_CoverUtility.IsCovered(pawn2, cell) || pawn.Position.DistanceTo(pawn.mindState.duty.focus.Cell) > 70f);
						if (flag5)
						{
							int num = Mathf.RoundToInt(pawn2.Position.DistanceTo(pawn.Position) + 1f);
							IntVec3 intVec2;
							bool flag6 = !AdvancedAI_CoverUtility.GetCoverCloserToAllyFrom(pawn2, pawn2.Position, (float)num, true, false, false, false, false, true, out intVec2, pawn);
							if (flag6)
							{
								bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag7)
								{
									Log.Message(string.Format("{0} {1}: Treatment Job. GetCoverCloserToAllyFrom failed. doctor meeting position: {2}", pawn, pawn.Position, intVec2));
								}
							}
							bool flag8 = pawn2.jobs.curJob != null;
							if (flag8)
							{
								pawn2.jobs.StopAll(false, true);
								pawn2.jobs.ClearQueuedJobs(true);
							}
							bool flag9 = (SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn)) || SkyAiCore.SelectedPawnDebug(pawn2);
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: Treatment Job. Force move doctor to doctorCoverPosition: {2}", pawn, pawn.Position, intVec2));
							}
							Job coverJob = AdvancedAI_Jobs.GetCoverJob(pawn2, intVec2, cell, AdvancedAI.ExpireInterval.normal, true, true, true);
							bool flag10 = coverJob != null;
							if (flag10)
							{
								pawn2.jobs.StartJob(coverJob, JobCondition.InterruptForced, null, false, true, null, null, false, false);
							}
						}
						else
						{
							bool flag11 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn2, cell, 10f, 0f, true, false, false, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.Normal, out intVec);
							if (flag11)
							{
								bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag12)
								{
									Log.Message(string.Format("{0} {1}: Treatment Job. GetCoverPositionFrom failed. doctorPosition: {2}", pawn, pawn.Position, intVec));
								}
							}
						}
						bool flag13 = pawn.CurJob != null;
						if (flag13)
						{
							pawn.jobs.StopAll(false, true);
							pawn.jobs.ClearQueuedJobs(true);
						}
						Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
						job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
						bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag14)
						{
							Log.Message(string.Format("{0} {1}: Treatment Job. Moving to position {2} for treatment. Doctor: {3} my distance to doctor: {4} doctorCoverPosition: {5}", new object[]
							{
								pawn,
								pawn.Position,
								intVec,
								pawn2,
								pawn.Position.DistanceTo(pawn2.Position),
								intVec
							}));
						}
						return job;
					}
					Job job2 = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
					job2.canUseRangedWeapon = true;
					job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
					job2.checkOverrideOnExpire = true;
					bool flag15 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag15)
					{
						Log.Message(string.Format("{0} {1}: Treatment Job. Waiting for medical help on: {2}", pawn, pawn.Position, pawn.Position));
					}
					return job2;
				}
				catch (Exception arg)
				{
					Log.Error(string.Format("{0} {1} : GetTreatmentJob: part exception: {2}", pawn, pawn.Position, arg));
				}
			}
			bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag16)
			{
				Log.Message(string.Format("{0} {1}: Treatment Job. For some reason return null.", pawn, pawn.Position));
			}
			return null;
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0001254C File Offset: 0x0001074C
		public static Job GetEvacuateLeavingJob(Pawn pawn, IntVec3 exitSpot, bool useExitMap, bool resqueClosestAlly, bool carryAllyCorpse)
		{
			bool flag = SK_Utility.isMechanical(pawn) || AdvancedAI.HasFobbidenFaction(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.Faction != null && !pawn.Faction.IsPlayer && !pawn.Faction.def.autoFlee && pawn.Faction.neverFlee;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = AdvancedAI.UnableMoveFast(pawn);
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = pawn.RaceProps.intelligence != Intelligence.Humanlike;
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = pawn.CurJobDef == JobDefOf.Steal;
							if (flag5)
							{
								result = null;
							}
							else
							{
								bool flag6 = resqueClosestAlly || !exitSpot.IsValid;
								if (flag6)
								{
									result = null;
								}
								else
								{
									bool flag7 = AdvancedAI.IsUniquePawn(pawn) || AdvancedAI.IsRaidLeaderOrSquadCommander(pawn);
									if (flag7)
									{
										result = null;
									}
									else
									{
										bool flag8 = useExitMap && pawn.CurJob != null && !pawn.CurJob.exitMapOnArrival;
										if (flag8)
										{
											AdvancedAI_LordUtility.PawnAddExitLord(pawn, false);
										}
										Pawn pawn2 = null;
										Corpse corpse = null;
										int num = 10;
										List<IntVec3> list = AdvancedAI.LeavingFromCells(pawn, 10);
										Func<IntVec3, bool> <>9__0;
										for (int i = 0; i < list.Count; i++)
										{
											bool flag9 = i == 0;
											if (flag9)
											{
												num = 4;
											}
											bool flag10 = i == 1;
											if (flag10)
											{
												num = 7;
											}
											bool flag11 = i == 2;
											if (flag11)
											{
												num = 10;
											}
											bool flag12 = i > 2;
											if (flag12)
											{
												num = 15;
											}
											IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(list[i], (float)num, true);
											Func<IntVec3, bool> predicate;
											if ((predicate = <>9__0) == null)
											{
												predicate = (<>9__0 = ((IntVec3 c) => c.InBounds(pawn.Map)));
											}
											List<IntVec3> list2 = source.Where(predicate).ToList<IntVec3>();
											for (int j = 0; j < list2.Count<IntVec3>(); j++)
											{
												IntVec3 c2 = list2[j];
												List<Thing> thingList = c2.GetThingList(pawn.Map);
												foreach (Thing thing in thingList)
												{
													Corpse corpse2 = thing as Corpse;
													bool flag13 = corpse2 != null;
													if (flag13)
													{
														bool flag14 = corpse2.InnerPawn != null && corpse2.InnerPawn.Faction != null && corpse2.InnerPawn.Faction == pawn.Faction && pawn.CanReserve(corpse2, 1, -1, null, false);
														if (flag14)
														{
															corpse = corpse2;
														}
													}
													Pawn pawn3 = thing as Pawn;
													bool flag15 = pawn3 != null;
													if (flag15)
													{
														bool flag16 = pawn3 != null && pawn3.Downed && pawn3.Faction != null && pawn3.Faction == pawn.Faction && pawn.CanReserve(pawn3, 1, -1, null, false);
														if (flag16)
														{
															pawn2 = pawn3;
															break;
														}
													}
												}
											}
										}
										bool flag17 = pawn2 != null;
										if (flag17)
										{
											Job job = JobMaker.MakeJob(JobDefOf.Steal);
											job.targetA = pawn2;
											job.targetB = exitSpot;
											job.count = 1;
											job.exitMapOnArrival = true;
											job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
											bool flag18 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag18)
											{
												Log.Message(string.Format("{0} {1}: Evacuate Leaving Job. Evacuating: {2}", pawn, pawn.Position, pawn2));
											}
											result = job;
										}
										else
										{
											bool flag19 = carryAllyCorpse && corpse != null;
											if (flag19)
											{
												Job job2 = JobMaker.MakeJob(JobDefOf.Steal);
												job2.targetA = corpse;
												job2.targetB = exitSpot;
												job2.count = 1;
												job2.exitMapOnArrival = true;
												job2.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
												bool flag20 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag20)
												{
													Log.Message(string.Format("{0} {1}: Evacuate Leaving Job. Evacuating with corse: {2}", pawn, pawn.Position, corpse));
												}
												result = job2;
											}
											else
											{
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
			return result;
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00012A3C File Offset: 0x00010C3C
		public static Job EvacuateSpecialPawnJob(Pawn pawn, Pawn patient)
		{
			bool flag = SK_Utility.isMechanical(pawn) || AdvancedAI.HasFobbidenFaction(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = pawn.RaceProps.intelligence != Intelligence.Humanlike;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = AdvancedAI.UnableMoveFast(pawn);
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = pawn.CurJobDef == JobDefOf.Steal;
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = AdvancedAI.HarmedRecently(pawn) && pawn.Position.DistanceTo(patient.Position) >= 4f;
							if (flag5)
							{
								result = null;
							}
							else
							{
								bool flag6 = !pawn.CanReserveAndReach(patient, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false);
								if (flag6)
								{
									result = null;
								}
								else
								{
									IntVec3 c;
									bool flag7 = !AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(pawn, out c, false, false);
									if (flag7)
									{
										result = null;
									}
									else
									{
										bool flag8 = AdvancedAI.IsUniquePawn(pawn) || AdvancedAI.IsRaidLeaderOrSquadCommander(pawn);
										if (flag8)
										{
											result = null;
										}
										else
										{
											AdvancedAI_LordUtility.PawnAddExitLord(pawn, false);
											Job job = JobMaker.MakeJob(JobDefOf.Steal);
											job.targetA = patient;
											job.targetB = c;
											job.count = 1;
											job.exitMapOnArrival = true;
											job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
											bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag9)
											{
												Log.Message(string.Format("{0} {1}: Evacuate special pawn Job. Evacuating : {2}", pawn, pawn.Position, patient));
											}
											result = job;
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

		// Token: 0x060000E0 RID: 224 RVA: 0x00012BD4 File Offset: 0x00010DD4
		public static Job EvacuateNearestAllyJob(Pawn pawn, float resqueAllyDistance)
		{
			AdvancedAI_Jobs.<>c__DisplayClass13_0 CS$<>8__locals1 = new AdvancedAI_Jobs.<>c__DisplayClass13_0();
			CS$<>8__locals1.pawn = pawn;
			bool flag = SK_Utility.isMechanical(CS$<>8__locals1.pawn) || AdvancedAI.HasFobbidenFaction(CS$<>8__locals1.pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = CS$<>8__locals1.pawn.RaceProps.intelligence != Intelligence.Humanlike;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = CS$<>8__locals1.pawn.CurJobDef == JobDefOf.Steal;
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = AdvancedAI.UnableMoveFast(CS$<>8__locals1.pawn);
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = AdvancedAI.IsUniquePawn(CS$<>8__locals1.pawn) || AdvancedAI.IsRaidLeaderOrSquadCommander(CS$<>8__locals1.pawn);
							if (flag5)
							{
								result = null;
							}
							else
							{
								IntVec3 c;
								bool flag6 = !AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(CS$<>8__locals1.pawn, out c, false, false);
								if (flag6)
								{
									result = null;
								}
								else
								{
									Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(CS$<>8__locals1.pawn.Position, CS$<>8__locals1.pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch, TraverseParms.For(CS$<>8__locals1.pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), resqueAllyDistance, new Predicate<Thing>(CS$<>8__locals1.<EvacuateNearestAllyJob>g__isDownedPawn|0), null, 0, -1, false, RegionType.Set_Passable, false);
									bool flag7 = pawn2 != null;
									if (flag7)
									{
										AdvancedAI_LordUtility.PawnAddExitLord(CS$<>8__locals1.pawn, false);
										Job job = JobMaker.MakeJob(JobDefOf.Steal);
										job.targetA = pawn2;
										job.targetB = c;
										job.count = 1;
										job.exitMapOnArrival = true;
										job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
										bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(CS$<>8__locals1.pawn);
										if (flag8)
										{
											Log.Message(string.Format("{0} {1}: Evacuate Nearest Ally Job. Evacuating: {2}", CS$<>8__locals1.pawn, CS$<>8__locals1.pawn.Position, pawn2));
										}
										result = job;
									}
									else
									{
										result = null;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00012DD0 File Offset: 0x00010FD0
		public static Job GetExitJob(Pawn pawn, bool resqueClosestAlly, bool carryAllyCorpse, bool panicFlee)
		{
			bool flag = SK_Utility.isMechanical(pawn) || AdvancedAI.HasFobbidenFaction(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = panicFlee && pawn.Faction != null && !pawn.Faction.IsPlayer && !pawn.Faction.def.autoFlee && pawn.Faction.neverFlee;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = AdvancedAI.IsUniquePawn(pawn);
					if (flag3)
					{
						result = null;
					}
					else
					{
						bool flag4 = AdvancedAI.HasExitJob(pawn);
						if (flag4)
						{
							result = null;
						}
						else
						{
							bool flag5 = (pawn.guest != null && pawn.guest.IsPrisoner) || pawn.IsPrisoner || pawn.IsSlave || pawn.Downed;
							if (flag5)
							{
								bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag6)
								{
									Log.Message(string.Format("{0} {1}: is guest and tried to leave from the map. Action prohibited, it could break quests or hospitality AI.", pawn, pawn.Position));
								}
								result = null;
							}
							else
							{
								IntVec3 intVec;
								bool flag7 = !AdvancedAI_ExitSpotUtility.TryPerfectExitSpot(pawn, out intVec, true, false);
								if (flag7)
								{
									result = null;
								}
								else
								{
									pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, false, false, null, false, false, false);
									AdvancedAI_LordUtility.PawnAddExitLord(pawn, panicFlee);
									if (resqueClosestAlly)
									{
										Job evacuateLeavingJob = AdvancedAI_Jobs.GetEvacuateLeavingJob(pawn, intVec, false, resqueClosestAlly, carryAllyCorpse);
										bool flag8 = evacuateLeavingJob != null;
										if (flag8)
										{
											return evacuateLeavingJob;
										}
									}
									bool flag9 = pawn.CurJob != null && !pawn.CurJob.exitMapOnArrival;
									if (flag9)
									{
										pawn.CurJob.Clear();
									}
									Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
									job.locomotionUrgency = (SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog);
									job.exitMapOnArrival = true;
									bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag10)
									{
										Log.Message(string.Format("{0} {1}: Leave Job.", pawn, pawn.Position));
									}
									result = job;
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00012FE0 File Offset: 0x000111E0
		public static Job StealToInventoryJob(Pawn pawn)
		{
			bool flag = AdvancedAI.InDangerousCombat(pawn, 30f);
			bool checkRegions = GenAI.InDangerousCombat(pawn);
			List<Thing> list = AdvancedAI.ThingsToSteal(pawn, flag ? 10f : 3f, checkRegions, false);
			list.SortBy((Thing d) => AdvancedAI.GetValue(d) * Mathf.Sqrt(pawn.Position.DistanceTo(d.Position)));
			CompInventory compInventory = pawn.TryGetComp<CompInventory>();
			bool flag2 = compInventory != null && MassUtility.EncumbrancePercent(pawn) < 0.9f;
			if (flag2)
			{
				Thing thing = null;
				foreach (Thing thing2 in list)
				{
					bool flag3 = thing2 != null && !AdvancedAI.PositionUnderCrossfire(pawn, thing2.Position, null, false, false) && pawn.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn) && pawn.CanReserve(thing2, 1, -1, null, false);
					if (flag3)
					{
						thing = thing2;
						break;
					}
				}
				bool flag4 = thing != null;
				if (flag4)
				{
					int count = 0;
					bool flag5 = compInventory.CanFitInInventory(thing, ref count, false, false);
					if (flag5)
					{
						Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
						job.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
						job.count = count;
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1}: Steal to Inventory Job. Stealing: {2}", pawn, pawn.Position, thing));
						}
						return job;
					}
				}
			}
			return null;
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000131CC File Offset: 0x000113CC
		public static Job TakeThingsOfGroupJob(Pawn pawn, ThingRequestGroup thingRequestGroup, float radius)
		{
			bool flag = AdvancedAI.HasCarryThing(pawn);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = AdvancedAI.EngagedEnemyRecently(pawn);
				if (flag2)
				{
					result = null;
				}
				else
				{
					CompInventory comp = pawn.GetComp<CompInventory>();
					float radius2 = AdvancedAI.InDangerousCombat(pawn, 30f) ? 7f : radius;
					CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
					bool flag3 = compDoctorRole != null;
					if (flag3)
					{
						Thing thing = AdvancedAI_TendUtility.MedicineInInventory(pawn);
						bool flag4 = thing != null && thing.stackCount >= 15;
						if (flag4)
						{
							return null;
						}
						bool flag5 = !compDoctorRole.DoctorHasMedicine;
						if (flag5)
						{
							radius2 = 20f;
						}
					}
					bool flag6 = comp != null;
					if (flag6)
					{
						Thing thing2 = AdvancedAI.ClosestThing(pawn, thingRequestGroup, radius2);
						bool flag7 = thing2 != null;
						if (flag7)
						{
							int b = 0;
							bool flag8 = comp.CanFitInInventory(thing2, ref b, false, false);
							if (flag8)
							{
								Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing2);
								job.expiryInterval = 300;
								job.checkOverrideOnExpire = true;
								job.count = Mathf.Min(thing2.stackCount, b);
								bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag9)
								{
									Log.Message(string.Format("{0} {1}: TakeThingsOfGroup Job. Take: {2}", pawn, pawn.Position, thing2));
								}
								return job;
							}
						}
					}
					result = null;
				}
			}
			return result;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00013338 File Offset: 0x00011538
		public static Job UseCombatEnhancingDrugJob(Pawn pawn, bool onlyIfInDanger)
		{
			bool flag = pawn.IsTeetotaler();
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = AdvancedAI.EngagedEnemyRecently(pawn);
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = AdvancedAI.TakeCombatEnhancingDrugRecently(pawn);
					if (flag3)
					{
						result = null;
					}
					else
					{
						Thing thing = pawn.inventory.FindCombatEnhancingDrug();
						bool flag4 = thing == null;
						if (flag4)
						{
							result = null;
						}
						else
						{
							if (onlyIfInDanger)
							{
								Lord lord = pawn.GetLord();
								bool flag5 = lord == null;
								if (flag5)
								{
									bool flag6 = !AdvancedAI.HarmedRecently(pawn);
									if (flag6)
									{
										return null;
									}
								}
								else
								{
									int num = 0;
									int num2 = Mathf.Clamp(lord.ownedPawns.Count / 2, 1, 4);
									for (int i = 0; i < lord.ownedPawns.Count; i++)
									{
										bool flag7 = AdvancedAI.HarmedRecently(lord.ownedPawns[i]);
										if (flag7)
										{
											num++;
											bool flag8 = num >= num2;
											if (flag8)
											{
												break;
											}
										}
									}
									bool flag9 = num < num2;
									if (flag9)
									{
										return null;
									}
								}
							}
							Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
							job.count = 1;
							bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag10)
							{
								Log.Message(string.Format("{0} {1}: Use CombatEnhancingDrug Job. Use: {2}", pawn, pawn.Position, thing));
							}
							result = job;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x000134B8 File Offset: 0x000116B8
		public static Job LordFollowJob(Pawn follower, float radius, int expireInterval, bool excludeDoctors, bool necessarilyRangedFollowee, bool excludeLeaders)
		{
			Pawn followeeFromLord = AdvancedAI_FollowUtility.GetFolloweeFromLord(follower, excludeDoctors, necessarilyRangedFollowee, true);
			bool flag = followeeFromLord == null;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = !AdvancedAI_FollowUtility.FarEnoughAndPossibleToStartJob(follower, followeeFromLord, radius);
				if (flag2)
				{
					IntVec3 intVec = RCellFinder.RandomWanderDestFor(follower, follower.Position, 3f, null, PawnUtility.ResolveMaxDanger(follower, Danger.Some));
					bool flag3 = (double)Rand.Value > 0.5 && intVec.IsValid && follower.Position != intVec;
					if (flag3)
					{
						Job job = JobMaker.MakeJob(JobDefOf.GotoWander, intVec);
						job.locomotionUrgency = LocomotionUrgency.Walk;
						job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
						job.checkOverrideOnExpire = true;
						bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(follower);
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: LordFollowJob. Go to wonder on pos {2}.", follower, follower.Position, intVec));
						}
						result = job;
					}
					else
					{
						Job job2 = JobMaker.MakeJob(JobDefOf.Wait, follower.Position);
						job2.canUseRangedWeapon = true;
						job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
						job2.checkOverrideOnExpire = true;
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(follower);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: LordFollowJob. Wait on current position.", follower, follower.Position));
						}
						result = job2;
					}
				}
				else
				{
					Job job3 = JobMaker.MakeJob(JobDefOf.FollowClose, followeeFromLord);
					job3.expiryInterval = expireInterval;
					job3.checkOverrideOnExpire = true;
					job3.followRadius = radius;
					bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(follower);
					if (flag6)
					{
						Log.Message(string.Format("{0} {1}: LordFollowJob. Start to follow: {2} {3}", new object[]
						{
							follower,
							follower.Position,
							followeeFromLord,
							followeeFromLord.Position
						}));
					}
					result = job3;
				}
			}
			return result;
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x000136B4 File Offset: 0x000118B4
		public static Job WaitSquadCoverJob(Pawn pawn, IntVec3 focusCell, RaidData raidData)
		{
			bool flag = AdvancedAI_SquadUtility.ShouldWaitCoverJob(pawn, focusCell, raidData);
			if (flag)
			{
				bool useLineOfSight = GenSight.LineOfSight(pawn.Position, focusCell, pawn.Map, false, null, 0, 0);
				int num = AdvancedAI.CalculateDistance(pawn, focusCell, 4, 14);
				IntVec3 intVec;
				bool coverPositionFrom = AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, (float)num, 0f, true, true, useLineOfSight, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out intVec);
				if (coverPositionFrom)
				{
					Job coverJob = AdvancedAI_Jobs.GetCoverJob(pawn, intVec, focusCell, AdvancedAI.ExpireInterval.normal, true, false, true);
					bool flag2 = coverJob != null;
					if (flag2)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag3)
						{
							Log.Message(string.Format("{0} {1}: WaitSquadCoverJob. Moving too fast. Wait cover job on: {2}", pawn, pawn.Position, intVec));
						}
						return coverJob;
					}
				}
				else
				{
					bool flag4 = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: WaitSquadCoverJob. Moving too fast. Wait combat job on current position.", pawn, pawn.Position));
						}
						Job job = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
						job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
						job.checkOverrideOnExpire = true;
						return job;
					}
					IntVec3 intVec2 = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 4f, null, Danger.None);
					bool flag6 = intVec2.IsValid && pawn.Position != intVec2;
					if (flag6)
					{
						bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag7)
						{
							Log.Message(string.Format("{0} {1}: WaitSquadCoverJob. Moving too fast. Wander around for a time.", pawn, pawn.Position));
						}
						Job job2 = JobMaker.MakeJob(JobDefOf.GotoWander, intVec2);
						job2.locomotionUrgency = LocomotionUrgency.Walk;
						job2.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
						job2.checkOverrideOnExpire = true;
						return job2;
					}
					bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag8)
					{
						Log.Message(string.Format("{0} {1}: WaitSquadCoverJob. Moving too fast. Wait-idle on current position.", pawn, pawn.Position));
					}
					Job job3 = JobMaker.MakeJob(JobDefOf.Wait, pawn.Position);
					job3.canUseRangedWeapon = true;
					job3.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.normal);
					job3.checkOverrideOnExpire = true;
					return job3;
				}
			}
			return null;
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0001394C File Offset: 0x00011B4C
		public static Job StealDecisions(Pawn pawn, bool useLeaveJob)
		{
			AdvancedAI_Classes.StealDebug(pawn);
			bool enableStealingMode = SkyAiCore.Settings.enableStealingMode;
			if (enableStealingMode)
			{
				bool flag = pawn.Faction != null && !pawn.Faction.Hidden && pawn.Faction.def.humanlikeFaction && pawn.RaceProps.intelligence >= Intelligence.Humanlike && !AdvancedAI.HasFobbidenFaction(pawn) && pawn.Faction.HostileTo(Faction.OfPlayer);
				if (flag)
				{
					bool flag2 = AdvancedAI.InDangerousCombat(pawn, 35f);
					Job job = AdvancedAI_Jobs.StealToInventoryJob(pawn);
					bool flag3 = job != null;
					if (flag3)
					{
						return job;
					}
					if (useLeaveJob)
					{
						IntVec3 c;
						RCellFinder.TryFindBestExitSpot(pawn, out c, TraverseMode.ByPawn);
						Thing thing;
						bool flag4 = c.IsValid && AdvancedAI.TryFindBestItemToSteal(pawn.Position, pawn.Map, flag2 ? 18f : 4f, out thing, pawn, null);
						if (flag4)
						{
							bool flag5 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag5)
							{
								Log.Message(string.Format("{0} {1} StealDecisions. TryFindBestItemToSteal on: {2}", pawn, pawn.Position, thing.Position));
								pawn.Map.debugDrawer.FlashCell(thing.Position, 0.85f, null, SkyAiCore.Settings.flashCellDelay);
								pawn.Map.debugDrawer.FlashCell(c, 0.45f, null, SkyAiCore.Settings.flashCellDelay);
							}
							Job job2 = JobMaker.MakeJob(JobDefOf.Steal);
							job2.targetA = thing;
							job2.targetB = c;
							job2.exitMapOnArrival = true;
							job2.locomotionUrgency = AdvancedAI.ResolveLocomotion(pawn);
							job2.count = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit));
							bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag6)
							{
								Log.Message(string.Format("{0} {1}: StealDecisions. Steal: {2}", pawn, pawn.Position, thing));
							}
							return job2;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00013B88 File Offset: 0x00011D88
		public static Job SurvivalDecisions(Pawn pawn, IntVec3 focusCell)
		{
			bool flag = false;
			bool flag2 = pawn.Faction != null && !pawn.Faction.IsPlayer && !pawn.Faction.Hidden && pawn.Faction.def.humanlikeFaction;
			if (flag2)
			{
				bool flag3 = AdvancedAI.AlmostNotCapableToFight(pawn);
				if (flag3)
				{
					bool debugLog = SkyAiCore.Settings.debugLog;
					if (debugLog)
					{
						Log.Message(string.Format("{0} {1}: SurvivalDecisions. Almost not capable to fight. Leave map.", pawn, pawn.Position));
					}
					return AdvancedAI_Jobs.GetExitJob(pawn, false, false, true);
				}
				bool enemiesWillApplyFirstAid = SkyAiCore.Settings.enemiesWillApplyFirstAid;
				if (enemiesWillApplyFirstAid)
				{
					bool flag4 = AdvancedAI_TendUtility.IsReservedForTreatment(pawn, false, true);
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: SurvivalDecisions. GetTreatmentReservationJob?", pawn, pawn.Position));
						}
						Job treatmentReservationJob = AdvancedAI_Jobs.GetTreatmentReservationJob(pawn);
						bool flag6 = treatmentReservationJob != null;
						if (flag6)
						{
							bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: SurvivalDecisions. go to GetTreatmentReservationJob1.", pawn, pawn.Position));
							}
							return treatmentReservationJob;
						}
					}
					AdvancedAI_TendUtility.InjurySeverity injurySeverity;
					bool flag8 = AdvancedAI_TendUtility.RequireTreatment(pawn, out injurySeverity);
					if (flag8)
					{
						bool flag9 = AdvancedAI.CanMove(pawn);
						if (flag9)
						{
							bool flag10 = AdvancedAI.InDangerousCombat(pawn, 35f);
							bool flag11 = AdvancedAI_TendUtility.IsReservedForTreatment(pawn, true, false);
							if (flag11)
							{
								bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag12)
								{
									Log.Message(string.Format("{0} {1}: SurvivalDecisions. IsReservedForTreatment by Doctor and try to cover first.", pawn, pawn.Position));
								}
								bool isValid = focusCell.IsValid;
								if (isValid)
								{
									IntVec3 position;
									bool flag13 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, 8f, 0f, true, false, false, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out position);
									if (flag13)
									{
										position = pawn.Position;
									}
									bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag14)
									{
										Log.Message(string.Format("{0} {1}: SurvivalDecisions. IsReservedForTreatment by Doctor and going to cover on: {2}", pawn, pawn.Position, position));
									}
									return AdvancedAI_Jobs.GetCoverJob(pawn, position, focusCell, AdvancedAI.ExpireInterval.fast, true, true, true);
								}
							}
							bool flag15 = injurySeverity == AdvancedAI_TendUtility.InjurySeverity.minor && !AdvancedAI.IsHealthStabilized(pawn);
							if (flag15)
							{
								flag = true;
								bool enableDoctorRole = SkyAiCore.Settings.enableDoctorRole;
								if (enableDoctorRole)
								{
									bool flag16 = !AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
									if (flag16)
									{
										bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag17)
										{
											Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2}. GetTreatmentReservationJob.", pawn, pawn.Position, AdvancedAI.IsCloseToCollapse(pawn)));
										}
										Job treatmentReservationJob2 = AdvancedAI_Jobs.GetTreatmentReservationJob(pawn);
										bool flag18 = treatmentReservationJob2 != null;
										if (flag18)
										{
											bool flag19 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag19)
											{
												Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2}. GetTreatmentReservationJob2.", pawn, pawn.Position, AdvancedAI.IsCloseToCollapse(pawn)));
											}
											return treatmentReservationJob2;
										}
									}
								}
								bool flag20 = !flag10 || AdvancedAI.IsCloseToCollapse(pawn);
								if (flag20)
								{
									bool flag21 = AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
									if (flag21)
									{
										bool flag22 = AdvancedAI.InjuriesCount(pawn) > 2 && AdvancedAI.IsInDangerHealthReasons(pawn);
										bool flag23 = !flag22;
										if (flag23)
										{
											bool isValid2 = focusCell.IsValid;
											if (isValid2)
											{
												IntVec3 position2;
												bool flag24 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, 22f, 4f, true, true, false, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out position2);
												if (flag24)
												{
													position2 = pawn.Position;
												}
												bool flag25 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag25)
												{
													Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2} GetTendJob on coverPosition: {3}", new object[]
													{
														pawn,
														pawn.Position,
														AdvancedAI.IsCloseToCollapse(pawn),
														position2
													}));
												}
												return AdvancedAI_Jobs.GetTendJob(pawn, position2, null);
											}
											bool flag26 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag26)
											{
												Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2} Focus cell is not valid. GetTendJob on current position.", pawn, pawn.Position, AdvancedAI.IsCloseToCollapse(pawn)));
											}
											return AdvancedAI_Jobs.GetTendJob(pawn, pawn.Position, null);
										}
									}
									bool flag27 = AdvancedAI.IsCloseToCollapse(pawn);
									if (flag27)
									{
										bool enableDoctorRole2 = SkyAiCore.Settings.enableDoctorRole;
										if (enableDoctorRole2)
										{
											bool flag28 = !AdvancedAI_TendUtility.IsReservedForTreatment(pawn, false, false);
											if (flag28)
											{
												bool flag29 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(3) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
												if (flag29)
												{
													bool flag30 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag30)
													{
														Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2}. GetExitJob.", pawn, pawn.Position, AdvancedAI.IsCloseToCollapse(pawn)));
													}
													return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, true, true);
												}
											}
										}
										else
										{
											bool flag31 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(5) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
											if (flag31)
											{
												bool flag32 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag32)
												{
													Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: Minor. CloseToCollapse: {2}. GetExitJob.", pawn, pawn.Position, AdvancedAI.IsCloseToCollapse(pawn)));
												}
												return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, true, true);
											}
										}
									}
								}
							}
							bool flag33 = injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.severe && !AdvancedAI.IsHealthStabilized(pawn);
							if (flag33)
							{
								flag = true;
								bool enableDoctorRole3 = SkyAiCore.Settings.enableDoctorRole;
								if (enableDoctorRole3)
								{
									bool flag34 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag34)
									{
										Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. GetTreatmentReservationJob1.", new object[]
										{
											pawn,
											pawn.Position,
											injurySeverity,
											AdvancedAI.IsCloseToCollapse(pawn)
										}));
									}
									Job treatmentReservationJob3 = AdvancedAI_Jobs.GetTreatmentReservationJob(pawn);
									bool flag35 = treatmentReservationJob3 != null;
									if (flag35)
									{
										bool flag36 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag36)
										{
											Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. GetTreatmentReservationJob2.", new object[]
											{
												pawn,
												pawn.Position,
												injurySeverity,
												AdvancedAI.IsCloseToCollapse(pawn)
											}));
										}
										return treatmentReservationJob3;
									}
								}
								bool flag37 = !flag10 || AdvancedAI.IsCloseToCollapse(pawn);
								if (flag37)
								{
									bool flag38 = AdvancedAI.IsValidLoc(focusCell) && AdvancedAI_CoverUtility.IsCovered(pawn, focusCell);
									if (flag38)
									{
										bool onlyIfInDanger = !AdvancedAI.IsInDangerHealthReasons(pawn);
										Job job = AdvancedAI_Jobs.UseCombatEnhancingDrugJob(pawn, onlyIfInDanger);
										bool flag39 = job != null;
										if (flag39)
										{
											bool flag40 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag40)
											{
												Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Using drugs!", new object[]
												{
													pawn,
													pawn.Position,
													injurySeverity,
													AdvancedAI.IsCloseToCollapse(pawn)
												}));
											}
											return AdvancedAI_Jobs.UseCombatEnhancingDrugJob(pawn, onlyIfInDanger);
										}
									}
									bool flag41 = !AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
									if (flag41)
									{
										bool flag42 = AdvancedAI.IsInDangerHealthReasons(pawn);
										if (flag42)
										{
											bool enableDoctorRole4 = SkyAiCore.Settings.enableDoctorRole;
											if (enableDoctorRole4)
											{
												bool flag43 = !AdvancedAI_TendUtility.IsReservedForTreatment(pawn, false, false);
												if (flag43)
												{
													bool flag44 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(4) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
													if (flag44)
													{
														bool flag45 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
														if (flag45)
														{
															Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Can't UseFirstAidSkills and no any available doctors. GetExitJob.", new object[]
															{
																pawn,
																pawn.Position,
																injurySeverity,
																AdvancedAI.IsCloseToCollapse(pawn)
															}));
														}
														return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
													}
												}
											}
											else
											{
												bool flag46 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(3) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
												if (flag46)
												{
													bool flag47 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag47)
													{
														Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Can't UseFirstAidSkills and no doctors. GetExitJob.", new object[]
														{
															pawn,
															pawn.Position,
															injurySeverity,
															AdvancedAI.IsCloseToCollapse(pawn)
														}));
													}
													return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
												}
											}
										}
									}
									else
									{
										bool flag48 = AdvancedAI.InjuriesCount(pawn) > 2 && AdvancedAI.IsInDangerHealthReasons(pawn);
										bool flag49 = !flag48;
										if (flag49)
										{
											bool isValid3 = focusCell.IsValid;
											if (isValid3)
											{
												bool flag50 = AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
												IntVec3 position3;
												if (flag50)
												{
													bool flag51 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, 35f, 15f, true, true, false, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out position3);
													if (flag51)
													{
														position3 = pawn.Position;
													}
													bool flag52 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag52)
													{
														Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. CanUseFirstAidSkills. GetTendJob on coverPosition: {4}", new object[]
														{
															pawn,
															pawn.Position,
															injurySeverity,
															AdvancedAI.IsCloseToCollapse(pawn),
															position3
														}));
													}
													return AdvancedAI_Jobs.GetTendJob(pawn, position3, null);
												}
												bool flag53 = Rand.Chance(0.6f);
												if (flag53)
												{
													bool flag54 = !AdvancedAI_CoverUtility.GetCoverCloserToAllyFrom(pawn, focusCell, 37f, true, false, true, false, false, true, out position3, null);
													if (flag54)
													{
														position3 = pawn.Position;
													}
												}
												else
												{
													bool flag55 = !AdvancedAI_CoverUtility.GetCoverPositionFrom(pawn, focusCell, 35f, 15f, true, false, false, false, false, false, AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly, out position3);
													if (flag55)
													{
														position3 = pawn.Position;
													}
												}
												bool flag56 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag56)
												{
													Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. CanUseFirstAidSkills. GetCoverJob on coverPosition: {4}", new object[]
													{
														pawn,
														pawn.Position,
														injurySeverity,
														AdvancedAI.IsCloseToCollapse(pawn),
														position3
													}));
												}
												return AdvancedAI_Jobs.GetCoverJob(pawn, position3, focusCell, AdvancedAI.ExpireInterval.normal, true, true, true);
											}
											else
											{
												bool flag57 = AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
												if (flag57)
												{
													bool flag58 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
													if (flag58)
													{
														Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. CanUseFirstAidSkills. Focus cell not found. GetTendJob on current position.", new object[]
														{
															pawn,
															pawn.Position,
															injurySeverity,
															AdvancedAI.IsCloseToCollapse(pawn)
														}));
													}
													return AdvancedAI_Jobs.GetTendJob(pawn, pawn.Position, null);
												}
											}
										}
									}
								}
								bool flag59 = AdvancedAI.IsInDangerHealthReasons(pawn);
								if (flag59)
								{
									bool enableDoctorRole5 = SkyAiCore.Settings.enableDoctorRole;
									if (enableDoctorRole5)
									{
										bool flag60 = !AdvancedAI_TendUtility.IsReservedForTreatment(pawn, false, false);
										if (flag60)
										{
											bool flag61 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(5) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
											if (flag61)
											{
												bool flag62 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag62)
												{
													Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. No any available doctor. GetExitJob.", new object[]
													{
														pawn,
														pawn.Position,
														injurySeverity,
														AdvancedAI.IsCloseToCollapse(pawn)
													}));
												}
												return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
											}
										}
									}
									else
									{
										bool flag63 = SkyAiCore.Settings.enemiesEscapeChanceWounded && pawn.IsHashIntervalTick(5) && !AdvancedAI_TendUtility.AnyAllyDoctorNearby(pawn, 2f, true) && Rand.Chance(AdvancedAI_TraitUtility.FleeChance(pawn));
										if (flag63)
										{
											bool flag64 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag64)
											{
												Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Doctors not found. GetExitJob.", new object[]
												{
													pawn,
													pawn.Position,
													injurySeverity,
													AdvancedAI.IsCloseToCollapse(pawn)
												}));
											}
											return AdvancedAI_Jobs.GetExitJob(pawn, SkyAiCore.Settings.enableRescueAlliesMode, false, true);
										}
									}
								}
							}
						}
						else
						{
							flag = true;
							bool flag65 = !pawn.Downed;
							if (flag65)
							{
								bool flag66 = !AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true) || injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.severe;
								if (flag66)
								{
									Job treatmentReservationJob4 = AdvancedAI_Jobs.GetTreatmentReservationJob(pawn);
									bool flag67 = treatmentReservationJob4 != null;
									if (flag67)
									{
										bool flag68 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag68)
										{
											Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Can't move! Need help. GetTreatmentReservationJob.", new object[]
											{
												pawn,
												pawn.Position,
												injurySeverity,
												AdvancedAI.IsCloseToCollapse(pawn)
											}));
										}
										bool flag69 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag69)
										{
											Log.Message(string.Format("{0} {1}: SurvivalDecisions. go to GetTreatmentReservationJob75.", pawn, pawn.Position));
										}
										return treatmentReservationJob4;
									}
								}
								bool flag70 = AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
								if (flag70)
								{
									bool flag71 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag71)
									{
										Log.Message(string.Format("{0} {1}: SurvivalDecisions. Injury: {2}. CloseToCollapse: {3}. Can't move! Need help.", new object[]
										{
											pawn,
											pawn.Position,
											injurySeverity,
											AdvancedAI.IsCloseToCollapse(pawn)
										}));
									}
									return AdvancedAI_Jobs.GetTendJob(pawn, pawn.Position, null);
								}
							}
						}
					}
					bool flag72 = flag && SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag72)
					{
						bool flag73 = AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
						if (flag73)
						{
							bool flag74 = !AdvancedAI.InDangerousCombat(pawn, 35f);
							if (flag74)
							{
								Log.Message(string.Format("{0} {1}: Houston we have a problem. I am wounded, can self tend and not in combat, but pass all treatment jobs.", pawn, pawn.Position));
							}
						}
						else
						{
							bool flag75 = !AdvancedAI.InDangerousCombat(pawn, 35f);
							if (flag75)
							{
								Log.Message(string.Format("{0} {1}: Houston we have a problem. I am wounded, and can't self tend, not in close combat and no doctor found. Pass all treatment jobs.", pawn, pawn.Position));
							}
						}
					}
				}
			}
			bool flag76 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag76)
			{
				AdvancedAI_TendUtility.InjurySeverity injurySeverity2;
				AdvancedAI_TendUtility.RequireTreatment(pawn, out injurySeverity2);
				Log.Message(string.Format("{0} {1}: SurvivalDecisions returns null. Injury: {2} CanUseMedSkill: {3} inDangerousCombat: {4}", new object[]
				{
					pawn,
					pawn.Position,
					injurySeverity2,
					AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true),
					AdvancedAI.InDangerousCombat(pawn, 30f)
				}));
			}
			return null;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00014B90 File Offset: 0x00012D90
		public static Job KiteMechanic(Pawn pawn, Pawn enemy)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null && (raidData.raidStage == RaidData.RaidStage.start || raidData.raidStage == RaidData.RaidStage.gathering);
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Verb verb = AdvancedAI.PrimaryVerb(pawn);
				float num;
				bool flag2 = AdvancedAI.EnemyTooClose(pawn, enemy, 0.5f, out num);
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0} {1}: KiteMechanic. Enemy too close. Range: {2}! Change the position!", pawn, pawn.Position, num));
					}
					bool flag4 = num <= 30f && pawn.Position.CloseToEdge(pawn.Map, 20);
					if (flag4)
					{
						bool flag5 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: KiteMechanic. Enemy too close. Range: {2} and i close to edge. Leave map.", pawn, pawn.Position, num));
						}
						result = AdvancedAI_Jobs.GetExitJob(pawn, false, false, false);
					}
					else
					{
						float num2 = num / verb.verbProps.range;
						int num3 = Mathf.RoundToInt(AdvancedAI.EffectiveRange(pawn));
						int num4 = Mathf.RoundToInt(Mathf.Lerp((float)num3, verb.verbProps.range, num2));
						IntRange intRange = new IntRange(num3, num4);
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1}: KiteMechanic. SniperWeaponRange: {2} t: {3} e: {4} l: {5}", new object[]
							{
								pawn,
								pawn.Position,
								intRange,
								num2,
								num3,
								num4
							}));
						}
						IntVec3 intVec;
						bool sniperCoverPosition = AdvancedAI_CoverUtility.GetSniperCoverPosition(pawn, enemy.Position, intRange, true, false, true, true, true, false, false, out intVec);
						if (sniperCoverPosition)
						{
							bool flag7 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: KiteMechanic. GetSniperCoverPosition on: {2}", pawn, pawn.Position, intVec));
								pawn.Map.debugDrawer.FlashCell(intVec, 0.85f, null, SkyAiCore.Settings.flashCellDelay);
							}
							result = AdvancedAI_Jobs.GetCoverJob(pawn, intVec, enemy.Position, AdvancedAI.ExpireInterval.fast, false, false, false);
						}
						else
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: KiteMechanic. Enemy too close. Range: {2}. Not found new position. Leave map?!", pawn, pawn.Position, num));
							}
							result = AdvancedAI_Jobs.GetCoverJob(pawn, intVec, enemy.Position, AdvancedAI.ExpireInterval.fast, false, false, true);
						}
					}
				}
				else
				{
					result = null;
				}
			}
			return result;
		}
	}
}
