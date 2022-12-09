using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000015 RID: 21
	public static class AdvancedAI_CoverUtility
	{
		// Token: 0x06000083 RID: 131 RVA: 0x00008680 File Offset: 0x00006880
		public static IEnumerable<IntVec3> ClosestBehindCells(Pawn pawn, IntVec3 fromTargetPosition, float radius, float closestFrontCells = -3f)
		{
			bool debugCoverCells = SkyAiCore.Settings.debugCoverCells;
			IEnumerable<IntVec3> result;
			if (debugCoverCells)
			{
				IEnumerable<IntVec3> enumerable = GenRadial.RadialCellsAround(pawn.Position, radius, true);
				IEnumerable<IntVec3> enumerable2 = from c in enumerable
				where c.DistanceTo(fromTargetPosition) >= pawn.Position.DistanceTo(fromTargetPosition) + closestFrontCells
				select c;
				bool flag = SkyAiCore.SelectedPawnDebug(pawn);
				if (flag)
				{
					foreach (IntVec3 c3 in enumerable)
					{
						pawn.Map.debugDrawer.FlashCell(c3, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
					}
					foreach (IntVec3 c2 in enumerable2)
					{
						pawn.Map.debugDrawer.FlashCell(c2, 0.64f, "B", SkyAiCore.Settings.flashCellDelay);
					}
				}
				result = enumerable2;
			}
			else
			{
				result = from c in GenRadial.RadialCellsAround(pawn.Position, radius, true)
				where c.DistanceTo(fromTargetPosition) >= pawn.Position.DistanceTo(fromTargetPosition) + closestFrontCells
				select c;
			}
			return result;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000087F4 File Offset: 0x000069F4
		public static IEnumerable<IntVec3> ClosestFrontCells(Pawn pawn, IntVec3 fromTargetPosition, float radius, float closestBehindCells = 2f)
		{
			bool debugCoverCells = SkyAiCore.Settings.debugCoverCells;
			IEnumerable<IntVec3> result;
			if (debugCoverCells)
			{
				IEnumerable<IntVec3> enumerable = GenRadial.RadialCellsAround(pawn.Position, radius, true);
				IEnumerable<IntVec3> enumerable2 = from c in enumerable
				where c.DistanceTo(fromTargetPosition) <= pawn.Position.DistanceTo(fromTargetPosition) + closestBehindCells
				select c;
				bool flag = SkyAiCore.SelectedPawnDebug(pawn);
				if (flag)
				{
					foreach (IntVec3 c3 in enumerable)
					{
						pawn.Map.debugDrawer.FlashCell(c3, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
					}
					foreach (IntVec3 c2 in enumerable2)
					{
						pawn.Map.debugDrawer.FlashCell(c2, 0.64f, "B", SkyAiCore.Settings.flashCellDelay);
					}
				}
				result = enumerable2;
			}
			else
			{
				result = from c in GenRadial.RadialCellsAround(pawn.Position, radius, true)
				where c.DistanceTo(fromTargetPosition) <= pawn.Position.DistanceTo(fromTargetPosition) - closestBehindCells
				select c;
			}
			return result;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00008968 File Offset: 0x00006B68
		public static IEnumerable<IntVec3> ClosestFrontCellsFromDuty(Pawn pawn, IntVec3 fromTargetPosition, float radius, float closestBehindCells = 5f, float closestFrontCells = 15f, IntVec3 dutyDefendFocusCell = default(IntVec3))
		{
			bool flag = !dutyDefendFocusCell.IsValid || dutyDefendFocusCell == new IntVec3(0, 0, 0) || dutyDefendFocusCell == IntVec3.Invalid;
			IEnumerable<IntVec3> result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool debugCoverCells = SkyAiCore.Settings.debugCoverCells;
				if (debugCoverCells)
				{
					IEnumerable<IntVec3> enumerable = GenRadial.RadialCellsAround(pawn.Position, radius, true);
					IOrderedEnumerable<IntVec3> orderedEnumerable = from c in enumerable
					where c.DistanceTo(fromTargetPosition) <= dutyDefendFocusCell.DistanceTo(fromTargetPosition) + closestBehindCells && c.DistanceTo(fromTargetPosition) + closestFrontCells >= dutyDefendFocusCell.DistanceTo(fromTargetPosition)
					select c into distCell
					orderby distCell.DistanceTo(dutyDefendFocusCell)
					select distCell;
					bool flag2 = SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						foreach (IntVec3 c3 in enumerable)
						{
							pawn.Map.debugDrawer.FlashCell(c3, 0.35f, null, SkyAiCore.Settings.flashCellDelay);
						}
						foreach (IntVec3 c2 in orderedEnumerable)
						{
							pawn.Map.debugDrawer.FlashCell(c2, 0.64f, "B", SkyAiCore.Settings.flashCellDelay);
						}
					}
					result = orderedEnumerable;
				}
				else
				{
					result = from c in GenRadial.RadialCellsAround(pawn.Position, radius, true)
					where c.DistanceTo(fromTargetPosition) <= dutyDefendFocusCell.DistanceTo(fromTargetPosition) - closestBehindCells && c.DistanceTo(fromTargetPosition) + closestFrontCells >= dutyDefendFocusCell.DistanceTo(fromTargetPosition)
					select c;
				}
			}
			return result;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00008B20 File Offset: 0x00006D20
		public static bool GetCoverPositionFrom(Pawn pawn, IntVec3 fromPosition, float maxDist, float minDist, bool distanceRequired, bool useCover, bool useLineOfSight, bool useEffectiveRange, bool ignoreCurrentCoverRating, bool checkRegion, AdvancedAI_CoverUtility.CoverPositionType coverPositioning, out IntVec3 coverPosition)
		{
			float radius = Mathf.Min(maxDist, 80f);
			IEnumerable<IntVec3> enumerable = null;
			switch (coverPositioning)
			{
			case AdvancedAI_CoverUtility.CoverPositionType.Normal:
				enumerable = from cell in GenRadial.RadialCellsAround(pawn.Position, radius, true)
				where base.<GetCoverPositionFrom>g__furtherThanMinDistance|0(cell)
				select cell;
				break;
			case AdvancedAI_CoverUtility.CoverPositionType.BehindCellsOnly:
				enumerable = from cell in AdvancedAI_CoverUtility.ClosestBehindCells(pawn, fromPosition, radius, -3f)
				where base.<GetCoverPositionFrom>g__furtherThanMinDistance|0(cell)
				select cell;
				break;
			case AdvancedAI_CoverUtility.CoverPositionType.FrontCellsOnly:
				enumerable = from cell in AdvancedAI_CoverUtility.ClosestFrontCells(pawn, fromPosition, radius, 2f)
				where base.<GetCoverPositionFrom>g__furtherThanMinDistance|0(cell)
				select cell;
				break;
			case AdvancedAI_CoverUtility.CoverPositionType.DutyFrontCellsOnly:
			{
				IntVec3 dutyDefendFocusCell = IntVec3.Invalid;
				bool flag = pawn.mindState.duty != null && pawn.mindState.duty.focus != null;
				if (flag)
				{
					dutyDefendFocusCell = pawn.mindState.duty.focus.Cell;
				}
				enumerable = from cell in AdvancedAI_CoverUtility.ClosestFrontCellsFromDuty(pawn, fromPosition, radius, 6f, 13f, dutyDefendFocusCell)
				where base.<GetCoverPositionFrom>g__furtherThanMinDistance|0(cell)
				select cell;
				break;
			}
			}
			IntVec3 intVec = pawn.Position;
			bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
			string text;
			float num = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, fromPosition, distanceRequired, useCover, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
			bool flag2 = SkyAiCore.Settings.debugPathCoverCells && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag2)
			{
				Log.Message(string.Format("{0} {1}: GetCoverPositionFrom. CellList count: {2} Pawn pos cell rating: {3}  Result: {4} ", new object[]
				{
					pawn,
					pawn.Position,
					enumerable.Count<IntVec3>(),
					num,
					text
				}));
			}
			bool flag3 = num <= 0f;
			if (flag3)
			{
				IEnumerable<IntVec3> source = enumerable;
				Func<IntVec3, bool> <>9__5;
				Func<IntVec3, bool> predicate;
				if ((predicate = <>9__5) == null)
				{
					predicate = (<>9__5 = ((IntVec3 x) => x.InBounds(pawn.Map)));
				}
				IEnumerable<IntVec3> source2 = source.Where(predicate);
				Func<IntVec3, float> <>9__6;
				Func<IntVec3, float> keySelector;
				if ((keySelector = <>9__6) == null)
				{
					keySelector = (<>9__6 = ((IntVec3 r) => r.DistanceTo(pawn.Position)));
				}
				foreach (IntVec3 intVec2 in source2.OrderByDescending(keySelector))
				{
					bool flag4 = intVec2.InBounds(pawn.Map) && AdvancedAI.PawnAndNeighborRegions(pawn).Contains(intVec2.GetRegion(pawn.Map, RegionType.Set_Passable));
					if (flag4)
					{
						string text2;
						float cellCoverRatingForPawn = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, fromPosition, distanceRequired, useCover, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text2);
						bool flag5 = SkyAiCore.SelectedPawnDebug(pawn);
						if (flag5)
						{
							bool debugPath = SkyAiCore.Settings.debugPath;
							if (debugPath)
							{
								pawn.Map.debugDrawer.FlashCell(intVec2, 0.92f, null, SkyAiCore.Settings.flashCellDelay);
							}
							bool debugPathCoverCells = SkyAiCore.Settings.debugPathCoverCells;
							if (debugPathCoverCells)
							{
								Log.Message(string.Format("{0} {1}: GetCoverPositionFrom. CellRating for cell: {2} Result: {3}", new object[]
								{
									pawn,
									pawn.Position,
									intVec2,
									text2
								}));
							}
						}
						bool flag6 = cellCoverRatingForPawn > num;
						if (flag6)
						{
							bool flag7 = SkyAiCore.SelectedPawnDebug(pawn);
							if (flag7)
							{
								bool debugPath2 = SkyAiCore.Settings.debugPath;
								if (debugPath2)
								{
									pawn.Map.debugDrawer.FlashCell(intVec2, 0.85f, Math.Round((double)cellCoverRatingForPawn, 2).ToString(), SkyAiCore.Settings.flashCellDelay);
								}
								bool debugPathCoverCells2 = SkyAiCore.Settings.debugPathCoverCells;
								if (debugPathCoverCells2)
								{
									Log.Message(string.Format("{0} {1}: GetCoverPositionFrom. Found best cellRating: {2} Result: {3}", new object[]
									{
										pawn,
										pawn.Position,
										cellCoverRatingForPawn,
										text2
									}));
								}
							}
							num = cellCoverRatingForPawn;
							intVec = intVec2;
						}
					}
				}
			}
			coverPosition = intVec;
			return num >= 0f;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00008FF8 File Offset: 0x000071F8
		public static bool GetCoverCloserToAllyFrom(Pawn pawn, IntVec3 fromPosition, float maxDist, bool distanceRequired, bool useRandValue, bool useLineOfSight, bool useEffectiveRange, bool ignoreCurrentCoverRating, bool checkRegion, out IntVec3 coverPosition, Pawn patient = null)
		{
			float radius = Mathf.Min(maxDist, 80f);
			bool flag = patient != null && pawn.Position.DistanceTo(patient.Position) < 10f;
			bool result;
			if (flag)
			{
				coverPosition = pawn.Position;
				result = true;
			}
			else
			{
				IntVec3 center = default(IntVec3);
				bool flag2 = patient == null;
				if (flag2)
				{
					IEnumerable<IntVec3> enumerable = from c in GenRadial.RadialCellsAround(pawn.Position, radius, true)
					where c.InBounds(pawn.Map) && base.<GetCoverCloserToAllyFrom>g__allyPawn|1(c.GetFirstPawn(pawn.Map))
					select c;
					bool flag3 = !enumerable.EnumerableNullOrEmpty<IntVec3>();
					if (flag3)
					{
						center = (from c in GenAdjFast.AdjacentCells8Way(enumerable.MinBy((IntVec3 closest) => pawn.Position.DistanceTo(closest)))
						where c.InBounds(pawn.Map)
						select c).FirstOrDefault<IntVec3>();
					}
				}
				else
				{
					center = (from c in GenAdjFast.AdjacentCells8Way(patient.Position)
					where c.InBounds(pawn.Map)
					select c).FirstOrDefault<IntVec3>();
				}
				bool flag4 = !center.IsValid;
				if (flag4)
				{
					center = pawn.Position;
				}
				IEnumerable<IntVec3> enumerable2 = GenRadial.RadialCellsAround(center, (float)Mathf.RoundToInt(maxDist * 0.5f), true);
				IntVec3 intVec = pawn.Position;
				bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
				string text;
				float num = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
				bool flag5 = num <= 0f;
				if (flag5)
				{
					bool flag6 = enumerable2.Contains(pawn.Position);
					foreach (IntVec3 intVec2 in enumerable2)
					{
						bool flag7 = intVec2.InBounds(pawn.Map) && (!flag6 || AdvancedAI.PawnAndNeighborRegions(pawn).Contains(intVec2.GetRegion(pawn.Map, RegionType.Set_Passable)));
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								pawn.Map.debugDrawer.FlashCell(intVec2, 0.92f, null, SkyAiCore.Settings.flashCellDelay);
							}
							float cellCoverRatingForPawn = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
							bool flag9 = cellCoverRatingForPawn > num;
							if (flag9)
							{
								bool flag10 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag10)
								{
									pawn.Map.debugDrawer.FlashCell(intVec2, 0.85f, Math.Round((double)cellCoverRatingForPawn, 2).ToString(), SkyAiCore.Settings.flashCellDelay);
								}
								num = cellCoverRatingForPawn;
								intVec = intVec2;
							}
						}
					}
				}
				coverPosition = intVec;
				result = (num >= 0f);
			}
			return result;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000933C File Offset: 0x0000753C
		public static bool GetCoverNearAllyFrom(Pawn pawn, IntVec3 fromPosition, List<Pawn> lordPawns, float maxDist, bool distanceRequired, bool useRandValue, bool useLineOfSight, bool useEffectiveRange, bool ignoreCurrentCoverRating, bool checkRegion, out IntVec3 coverPosition, out Pawn closestPawn)
		{
			coverPosition = pawn.Position;
			bool flag = lordPawns.NullOrEmpty<Pawn>();
			bool result;
			if (flag)
			{
				closestPawn = null;
				result = false;
			}
			else
			{
				float num = Mathf.Min(maxDist, 80f);
				(from p1 in lordPawns
				where p1 != pawn
				select p1).TryMinBy((Pawn p2) => p2.Position.DistanceTo(pawn.Position), out closestPawn);
				bool flag2 = closestPawn == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					IEnumerable<IntVec3> enumerable = GenRadial.RadialCellsAround(closestPawn.Position, maxDist, false);
					IntVec3 intVec = coverPosition;
					bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
					string text;
					float num2 = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
					bool flag3 = num2 <= 0f;
					if (flag3)
					{
						bool flag4 = enumerable.Contains(pawn.Position);
						IEnumerable<IntVec3> source = enumerable;
						Func<IntVec3, float> <>9__2;
						Func<IntVec3, float> keySelector;
						if ((keySelector = <>9__2) == null)
						{
							keySelector = (<>9__2 = ((IntVec3 r) => r.DistanceTo(pawn.Position)));
						}
						foreach (IntVec3 intVec2 in source.OrderByDescending(keySelector))
						{
							bool flag5 = intVec2.InBounds(pawn.Map) && (!flag4 || AdvancedAI.PawnAndNeighborRegions(pawn).Contains(intVec2.GetRegion(pawn.Map, RegionType.Set_Passable)));
							if (flag5)
							{
								bool flag6 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag6)
								{
									pawn.Map.debugDrawer.FlashCell(intVec2, 0.92f, null, SkyAiCore.Settings.flashCellDelay);
								}
								float cellCoverRatingForPawn = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
								bool flag7 = cellCoverRatingForPawn > num2;
								if (flag7)
								{
									bool flag8 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag8)
									{
										pawn.Map.debugDrawer.FlashCell(intVec2, 0.85f, Math.Round((double)cellCoverRatingForPawn, 2).ToString(), SkyAiCore.Settings.flashCellDelay);
									}
									num2 = cellCoverRatingForPawn;
									intVec = intVec2;
								}
							}
						}
					}
					coverPosition = intVec;
					result = (num2 >= 0f);
				}
			}
			return result;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000095F8 File Offset: 0x000077F8
		public static bool GetCoverFromNewPositionFrom(Pawn pawn, IntVec3 newPosition, IntVec3 fromPosition, float maxDist, bool distanceRequired, bool useRandValue, bool useLineOfSight, bool useEffectiveRange, bool ignoreCurrentCoverRating, bool useBehindCellsOnly, bool checkRegion, out IntVec3 coverPosition)
		{
			float radius = Mathf.Min(maxDist, 80f);
			IEnumerable<IntVec3> enumerable;
			if (useBehindCellsOnly)
			{
				enumerable = AdvancedAI_CoverUtility.ClosestBehindCells(pawn, newPosition, radius, -2f);
			}
			else
			{
				enumerable = new List<IntVec3>(GenRadial.RadialCellsAround(newPosition, radius, true));
			}
			IntVec3 intVec = pawn.Position;
			bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
			string text;
			float num = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
			bool flag = num <= 0f;
			if (flag)
			{
				bool flag2 = enumerable.Contains(pawn.Position);
				foreach (IntVec3 intVec2 in enumerable)
				{
					bool flag3 = intVec2.InBounds(pawn.Map) && (!flag2 || AdvancedAI.PawnAndNeighborRegions(pawn).Contains(intVec2.GetRegion(pawn.Map, RegionType.Set_Passable)));
					if (flag3)
					{
						bool flag4 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag4)
						{
							pawn.Map.debugDrawer.FlashCell(intVec2, 0.92f, null, SkyAiCore.Settings.flashCellDelay);
						}
						float cellCoverRatingForPawn = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, intVec2, fromPosition, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text);
						bool flag5 = cellCoverRatingForPawn > num;
						if (flag5)
						{
							bool flag6 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag6)
							{
								pawn.Map.debugDrawer.FlashCell(intVec2, 0.85f, Math.Round((double)cellCoverRatingForPawn, 2).ToString(), SkyAiCore.Settings.flashCellDelay);
							}
							num = cellCoverRatingForPawn;
							intVec = intVec2;
						}
					}
				}
			}
			coverPosition = intVec;
			return num >= 0f;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000097F8 File Offset: 0x000079F8
		public static bool IsCovered(Pawn pawn, IntVec3 focusCell, bool distanceRequired, bool useCover = true, bool lineOfsight = false, bool useEffectiveRange = false, bool takeCover = false, bool ignoreCurrentCoverRating = false, bool checkRegion = false)
		{
			bool useFriendlyFire = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
			bool result;
			if (takeCover)
			{
				string text;
				result = (AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, focusCell, distanceRequired, useCover, lineOfsight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text) >= 0.5f);
			}
			else
			{
				string text2;
				bool flag = AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, focusCell, distanceRequired, useCover, lineOfsight, useEffectiveRange, useFriendlyFire, ignoreCurrentCoverRating, checkRegion, out text2) > 0f;
				bool flag2 = SkyAiCore.Settings.debugLog && pawn != null && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: IsCovered: {2} FocusCell: {3} Reason: {4}", new object[]
					{
						pawn,
						pawn.Position,
						flag,
						focusCell,
						text2
					}));
				}
				result = flag;
			}
			return result;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000098C8 File Offset: 0x00007AC8
		public static bool IsCovered(Pawn pawn, IntVec3 focusCell)
		{
			bool flag = pawn.mindState.enemyTarget != null;
			bool result;
			if (flag)
			{
				result = (AdvancedAI_CoverUtility.IsCovered(pawn, focusCell, true, true, true, false, false, false, false) || AdvancedAI_CoverUtility.IsCovered(pawn, focusCell, true, true, true, true, false, false, false));
			}
			else
			{
				result = (AdvancedAI_CoverUtility.IsCovered(pawn, focusCell, true, true, false, false, false, false, false) || AdvancedAI_CoverUtility.IsCovered(pawn, focusCell, true, true, false, true, false, false, false));
			}
			return result;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00009934 File Offset: 0x00007B34
		public static bool CoverRequired(Pawn pawn, IntVec3 intVec3, float checkDistance, int maxBusyCount)
		{
			int num;
			AdvancedAI.GetPawnsInRadius(pawn, intVec3, checkDistance, out num);
			return num >= maxBusyCount;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00009958 File Offset: 0x00007B58
		private static float GetCoverRating(Thing cover)
		{
			bool flag = cover == null;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = cover.def.category == ThingCategory.Plant;
				if (flag2)
				{
					result = cover.def.fillPercent;
				}
				else
				{
					float num = 1f;
					bool useHitPoints = cover.def.useHitPoints;
					if (useHitPoints)
					{
						int num2 = 200;
						int hitPoints = cover.HitPoints;
						num = Mathf.Lerp((float)hitPoints, (float)num2, (float)(Mathf.Min(num2, hitPoints) / num2));
					}
					bool flag3 = cover.TryGetComp<CompExplosive>() != null;
					if (flag3)
					{
						num = 0f;
					}
					result = num;
				}
			}
			return result;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000099F4 File Offset: 0x00007BF4
		public static float GetCellCoverRatingForPawn(Pawn pawn, IntVec3 cell, IntVec3 shooterPos, bool distanceRequired, bool useCover, bool useLingOfSight, bool useEffectiveRange, bool useFriendlyFire, bool ignoreCurrentCoverRating, bool checkRegion, out string result)
		{
			bool flag = !cell.IsValid;
			float result2;
			if (flag)
			{
				result = "-1. Cell is not valid.";
				result2 = -1f;
			}
			else
			{
				bool flag2 = !cell.Standable(pawn.Map);
				if (flag2)
				{
					result = "-1. Cell is not standable.";
					result2 = -1f;
				}
				else
				{
					bool flag3 = cell.ContainsStaticFire(pawn.Map);
					if (flag3)
					{
						result = "-1. Cell contains static fire.";
						result2 = -1f;
					}
					else
					{
						bool flag4 = ignoreCurrentCoverRating && cell == pawn.Position;
						if (flag4)
						{
							result = "-1. IgnoreCurrentCoverRating is true.";
							result2 = -1f;
						}
						else
						{
							bool flag5 = distanceRequired && AdvancedAI.CellAlreadyOccupied(pawn, cell);
							if (flag5)
							{
								result = "-1. Cell already occupied and distance required.";
								result2 = -1f;
							}
							else
							{
								bool enableMainBlowSiegeTactic = SkyAiCore.Settings.enableMainBlowSiegeTactic;
								if (enableMainBlowSiegeTactic)
								{
									bool flag6 = !AdvancedAI.CellReservedForMainBlow(pawn).NullOrEmpty<IntVec3>() && AdvancedAI.CellReservedForMainBlow(pawn).Contains(cell);
									if (flag6)
									{
										result = "-1. Reserved for mainblow tactic.";
										return -1f;
									}
								}
								MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
								bool flag7 = !mapComponent_SkyAI.dangerousCells.NullOrEmpty<IntVec3>() && mapComponent_SkyAI.dangerousCells.Contains(cell);
								if (flag7)
								{
									result = "-1. Dangerous cells list contains cell.";
									result2 = -1f;
								}
								else
								{
									bool flag8 = checkRegion && !pawn.Position.WithinRegions(cell, pawn.Map, 6, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), RegionType.Set_Passable);
									if (flag8)
									{
										result = "-1. By regions.";
										result2 = -1f;
									}
									else
									{
										bool flag9 = !pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false);
										if (flag9)
										{
											result = "-1. Cant reserve and reach cell.";
											result2 = -1f;
										}
										else
										{
											float num = 0f;
											bool flag10 = !shooterPos.InBounds(pawn.Map);
											if (flag10)
											{
												result = "1. Shooter position out of bounds, but first cell checks are fine.";
												result2 = 1f;
											}
											else
											{
												Vector3 normalized = (shooterPos - cell).ToVector3().normalized;
												IntVec3 c = (cell.ToVector3Shifted() + normalized).ToIntVec3();
												Thing cover = c.GetCover(pawn.Map);
												num += AdvancedAI_CoverUtility.GetCoverRating(cover);
												if (useCover)
												{
													bool flag11 = num <= 0f;
													if (flag11)
													{
														result = string.Format("-1. CellRating: {0} <= 0.", num);
														return -1f;
													}
												}
												bool flag12 = !pawn.Position.Equals(cell);
												if (flag12)
												{
													float num2 = (pawn.Position - cell).LengthHorizontal;
													bool flag13 = !GenSight.LineOfSight(pawn.Position, cell, pawn.Map, false, null, 0, 0);
													if (flag13)
													{
														num2 *= 5f;
													}
													num /= num2;
												}
												float num3 = cell.DistanceTo(shooterPos);
												float num4 = AdvancedAI.EffectiveRange(pawn);
												bool flag14 = useEffectiveRange && num > 0f;
												if (flag14)
												{
													bool flag15 = num4 >= (float)SkyAiCore.Settings.minRangeForEffectiveRange;
													if (flag15)
													{
														bool flag16 = num3 >= num4;
														if (flag16)
														{
															result = string.Format("-1. Effective range: {0}/{1} ", num4, num3);
															return -1f;
														}
													}
												}
												bool flag17 = AdvancedAI.FriendlyFireThreatInShootline(pawn, cell, shooterPos, 4f, 60f, 55f, 3f);
												if (flag17)
												{
													if (useFriendlyFire)
													{
														result = "-1. FriendlyFire in shootline";
														return -1f;
													}
													num *= 0.5f;
												}
												bool flag18 = useLingOfSight && num > 0f;
												if (flag18)
												{
													Pawn firstPawn = shooterPos.GetFirstPawn(pawn.Map);
													bool flag19 = firstPawn != null;
													if (flag19)
													{
														bool flag20 = num3 > num4 || num3 > 85f || pawn.kindDef.aiAvoidCover;
														if (flag20)
														{
															bool flag21 = !GenSight.LineOfSight(cell, shooterPos, pawn.Map, false, null, 0, 0);
															if (flag21)
															{
																result = "-1. No line of sight.";
																return -1f;
															}
														}
														else
														{
															bool flag22 = !AdvancedAI.TryFindShootlineFromTo(cell, firstPawn, AdvancedAI.PrimaryVerb(pawn));
															if (flag22)
															{
																result = string.Format("-1. No shootline. cell: {0} enemy: {1} {2}", cell, firstPawn, firstPawn.Position);
																return -1f;
															}
														}
													}
													else
													{
														bool flag23 = !GenSight.LineOfSight(cell, shooterPos, pawn.Map, false, null, 0, 0);
														if (flag23)
														{
															result = "-1. No line of sight.";
															return -1f;
														}
													}
												}
												result = string.Format("{0}. Got result.", num);
												result2 = num;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result2;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00009EC0 File Offset: 0x000080C0
		public static bool GetSniperCoverPosition(Pawn pawn, IntVec3 center, IntRange intRange, bool distanceRequired, bool useRandValue, bool useLineOfSight, bool useEffectiveRange, bool ignoreCurrentCoverRating, bool inRandomOrder, bool checkRegion, out IntVec3 coverPosition)
		{
			bool flag = !center.IsValid;
			bool result;
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1} GetSniperCoverPosition. Focus cell is Invalid", pawn, pawn.Position));
				}
				coverPosition = IntVec3.Invalid;
				result = false;
			}
			else
			{
				float maxRadius = (float)Mathf.Min(intRange.max, 80);
				IEnumerable<IntVec3> enumerable = (from c in GenRadial.RadialCellsAround(center, (float)intRange.min, maxRadius)
				where pawn.Position.DistanceTo(center) * 1.2f >= pawn.Position.DistanceTo(c)
				select c).InRandomOrder(null);
				bool flag3 = !inRandomOrder;
				if (flag3)
				{
					(from c in enumerable
					orderby c.DistanceTo(center)
					select c).ThenByDescending((IntVec3 d) => pawn.Position.DistanceTo(d));
				}
				bool flag4 = !pawn.pather.Moving;
				if (flag4)
				{
					bool hasDangerousWeapon = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
					string text;
					bool flag5 = Mathf.RoundToInt(pawn.Position.DistanceTo(center)) > intRange.max || !GenSight.LineOfSight(pawn.Position, center, pawn.Map, false, null, 0, 0) || AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, center, true, false, true, false, hasDangerousWeapon, false, checkRegion, out text) < 0f;
					if (flag5)
					{
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1} GetSniperCoverPosition. Too high distance: {2} with intRange: {3}", new object[]
							{
								pawn,
								pawn.Position,
								Mathf.RoundToInt(pawn.Position.DistanceTo(center)),
								intRange
							}));
						}
						IEnumerable<IntVec3> enumerable2 = enumerable.Where(delegate(IntVec3 cell)
						{
							string text2;
							return cell.DistanceTo(center) < (float)intRange.max && AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, cell, center, true, false, true, false, hasDangerousWeapon, false, false, out text2) > 0f;
						});
						bool flag7 = !enumerable2.EnumerableNullOrEmpty<IntVec3>();
						if (!flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1} GetSniperCoverPosition. xPosition is Invalid: Focus cell: {2} Distance from current cell to focus cell : {3} intRange: {4}", new object[]
								{
									pawn,
									pawn.Position,
									center,
									pawn.Position.DistanceTo(center),
									intRange
								}));
							}
							coverPosition = IntVec3.Invalid;
							return false;
						}
						bool flag9 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag9)
						{
							Log.Message(string.Format("{0} {1}: GetSniperCoverPosition. StartPositions count: {2}", pawn, pawn.Position, enumerable2.Count<IntVec3>()));
							foreach (IntVec3 c2 in enumerable2)
							{
								pawn.Map.debugDrawer.FlashCell(c2, 0.62f, null, SkyAiCore.Settings.flashCellDelay);
							}
						}
						bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag10)
						{
							Log.Message(string.Format("{0} {1} GetSniperCoverPosition. Too high distance: {2} with intRange: {3}", new object[]
							{
								pawn,
								pawn.Position,
								pawn.Position.DistanceTo(center),
								intRange
							}));
						}
						coverPosition = enumerable2.MinBy((IntVec3 nearest) => pawn.Position.DistanceTo(nearest));
						bool flag11 = AdvancedAI.IsValidLoc(coverPosition);
						if (flag11)
						{
							bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag12)
							{
								Log.Message(string.Format("{0} {1} GetSniperCoverPosition. xPosition: {2} Focus cell: {3} Distance from cell to focus cell : {4} My distance to cover cell: {5} intRange: {6}", new object[]
								{
									pawn,
									pawn.Position,
									coverPosition,
									center,
									coverPosition.DistanceTo(center),
									pawn.Position.DistanceTo(coverPosition),
									intRange
								}));
							}
							return true;
						}
					}
				}
				foreach (IntVec3 intVec in enumerable)
				{
					bool flag13 = GenSight.LineOfSight(intVec, center, pawn.Map, false, null, 0, 0) && Mathf.RoundToInt(intVec.DistanceTo(center)) <= intRange.max;
					if (flag13)
					{
						bool flag14 = AdvancedAI_CoverUtility.GetCoverFromNewPositionFrom(pawn, intVec, center, 15f, distanceRequired, useRandValue, useLineOfSight, useEffectiveRange, ignoreCurrentCoverRating, false, checkRegion, out coverPosition) && pawn.CanReserve(intVec, 1, -1, null, false);
						if (flag14)
						{
							bool flag15 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag15)
							{
								pawn.Map.debugDrawer.FlashCell(coverPosition, 0.57f, null, SkyAiCore.Settings.flashCellDelay);
							}
							bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag16)
							{
								Log.Message(string.Format("{0} {1} GetSniperCoverPosition. GetCoverFromNewPositionFrom: {2} with cover position {3} Focus cell: {4} Distance from cell to focus cell : {5} My distance to cover cell: {6} intRange: {7}", new object[]
								{
									pawn,
									pawn.Position,
									intVec,
									coverPosition,
									center,
									coverPosition.DistanceTo(center),
									pawn.Position.DistanceTo(coverPosition),
									intRange
								}));
							}
							return true;
						}
					}
				}
				bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag17)
				{
					Log.Message(string.Format("{0} {1} GetSniperCoverPosition. I will stay on current position: {2} Focus cell: {3} Distance from cell to focus cell : {4} intRange: {5}", new object[]
					{
						pawn,
						pawn.Position,
						pawn.Position,
						center,
						pawn.Position.DistanceTo(center),
						intRange
					}));
				}
				coverPosition = pawn.Position;
				result = true;
			}
			return result;
		}

		// Token: 0x06000090 RID: 144 RVA: 0x0000A7F0 File Offset: 0x000089F0
		public static bool GetDoctorCoverPosition(Pawn pawn, IntVec3 center, out IntVec3 coverPosition)
		{
			bool flag = !center.IsValid;
			bool result;
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1} Focus cell is Invalid", pawn, pawn.Position));
				}
				coverPosition = IntVec3.Invalid;
				result = false;
			}
			else
			{
				Lord lord = pawn.GetLord();
				bool flag3 = lord == null || lord.ownedPawns.NullOrEmpty<Pawn>();
				if (flag3)
				{
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. lord null or lord pawn list null or empty.", pawn, pawn.Position));
					}
					coverPosition = IntVec3.Invalid;
					result = false;
				}
				else
				{
					IEnumerable<IntVec3> enumerable = from p in lord.ownedPawns
					where p != pawn && !AdvancedAI.PawnIsDoctor(p) && AdvancedAI.IsActivePawn(p)
					select p into ps
					select ps.Position;
					bool flag5 = enumerable.EnumerableNullOrEmpty<IntVec3>();
					if (flag5)
					{
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. lordPositions null or empty.", pawn, pawn.Position));
						}
						coverPosition = IntVec3.Invalid;
						result = false;
					}
					else
					{
						float lordAverageDistanceToFocusCell = enumerable.Average((IntVec3 xx) => xx.DistanceTo(center));
						IEnumerable<IntVec3> enumerable2 = from intVec in enumerable
						where intVec.DistanceTo(center) >= lordAverageDistanceToFocusCell
						select intVec;
						bool flag7 = enumerable2.EnumerableNullOrEmpty<IntVec3>();
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. fixedLordPositions null or empty.", pawn, pawn.Position));
							}
							coverPosition = IntVec3.Invalid;
							result = false;
						}
						else
						{
							float num = 15f;
							bool hasDangerousWeapon = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
							IntVec3 lordCenterPosition = new IntVec3(Mathf.RoundToInt((float)enumerable2.Average((IntVec3 x) => x.x)), pawn.Position.y, Mathf.RoundToInt((float)enumerable2.Average((IntVec3 z) => z.z)));
							string text;
							bool flag9 = pawn.Position.DistanceTo(lordCenterPosition) < num && AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, center, true, false, true, false, hasDangerousWeapon, false, false, out text) > 0f;
							if (flag9)
							{
								coverPosition = pawn.Position;
								bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag10)
								{
									Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. Using current position for cover: {2}. Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
									{
										pawn,
										pawn.Position,
										coverPosition,
										center,
										lordCenterPosition,
										lordAverageDistanceToFocusCell,
										pawn.Position.DistanceTo(lordCenterPosition)
									}));
								}
								result = true;
							}
							else
							{
								IEnumerable<IntVec3> source = from c in GenRadial.RadialCellsAround(lordCenterPosition, 15f, true)
								where lordCenterPosition.DistanceTo(center) * 1.2f >= lordCenterPosition.DistanceTo(c)
								select c;
								IEnumerable<IntVec3> enumerable3 = source.Where(delegate(IntVec3 cell)
								{
									string text2;
									return AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, cell, center, true, false, true, false, hasDangerousWeapon, false, false, out text2) > 0f;
								});
								bool flag11 = !enumerable3.EnumerableNullOrEmpty<IntVec3>();
								if (flag11)
								{
									bool flag12 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag12)
									{
										Log.Message(string.Format("{0} {1}: GetDoctorCoverPosition coverPositions count: {2}", pawn, pawn.Position, enumerable3.Count<IntVec3>()));
										foreach (IntVec3 c2 in enumerable3)
										{
											pawn.Map.debugDrawer.FlashCell(c2, 0.62f, null, SkyAiCore.Settings.flashCellDelay);
										}
									}
									bool flag13 = enumerable3.Contains(pawn.Position);
									if (flag13)
									{
										bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag14)
										{
											Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. CoverPositions contains current position: {2} Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
											{
												pawn,
												pawn.Position,
												pawn.Position,
												center,
												lordCenterPosition,
												lordAverageDistanceToFocusCell,
												pawn.Position.DistanceTo(lordCenterPosition)
											}));
										}
										coverPosition = pawn.Position;
										result = true;
									}
									else
									{
										Pawn closestAllyPawn;
										(from p1 in lord.ownedPawns
										where p1 != pawn && !AdvancedAI.PawnIsDoctor(p1) && AdvancedAI.IsActivePawn(p1)
										select p1).TryMinBy((Pawn p2) => p2.Position.DistanceTo(pawn.Position), out closestAllyPawn);
										bool flag15 = closestAllyPawn != null;
										if (flag15)
										{
											IntVec3 intVec2 = enumerable3.MinBy((IntVec3 d) => d.DistanceTo(closestAllyPawn.Position));
											bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag16)
											{
												Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. Doctor position: {2} Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
												{
													pawn,
													pawn.Position,
													intVec2,
													center,
													lordCenterPosition,
													lordAverageDistanceToFocusCell,
													pawn.Position.DistanceTo(lordCenterPosition)
												}));
											}
											coverPosition = intVec2;
											result = true;
										}
										else
										{
											bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag17)
											{
												Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. Position is Invalid. Closest ally pawn null. Focus cell: {2} lordCenterPosition: {3} lordAverageDistanceToFocusCell: {4} Distance to lordCenterPosition: {5}", new object[]
												{
													pawn,
													pawn.Position,
													center,
													lordCenterPosition,
													lordAverageDistanceToFocusCell,
													pawn.Position.DistanceTo(lordCenterPosition)
												}));
											}
											coverPosition = IntVec3.Invalid;
											result = false;
										}
									}
								}
								else
								{
									bool flag18 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag18)
									{
										Log.Message(string.Format("{0} {1} GetDoctorCoverPosition. Position is Invalid. CoverPositions null or empty. Focus cell: {2} lordCenterPosition: {3} lordAverageDistanceToFocusCell: {4} Distance to lordCenterPosition: {5}", new object[]
										{
											pawn,
											pawn.Position,
											center,
											lordCenterPosition,
											lordAverageDistanceToFocusCell,
											pawn.Position.DistanceTo(lordCenterPosition)
										}));
									}
									coverPosition = IntVec3.Invalid;
									result = false;
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x0000B04C File Offset: 0x0000924C
		public static bool GetLeaderCoverPosition(Pawn pawn, IntVec3 center, out IntVec3 coverPosition)
		{
			bool flag = !center.IsValid;
			bool result;
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1} Focus cell is Invalid", pawn, pawn.Position));
				}
				coverPosition = IntVec3.Invalid;
				result = false;
			}
			else
			{
				Lord lord = pawn.GetLord();
				bool flag3 = lord == null || lord.ownedPawns.NullOrEmpty<Pawn>() || lord.ownedPawns.Count < SkyAiCore.Settings.minRaidCountForLeader;
				if (flag3)
				{
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. lord null or lord pawn list null or empty.", pawn, pawn.Position));
					}
					coverPosition = IntVec3.Invalid;
					result = false;
				}
				else
				{
					IEnumerable<IntVec3> enumerable = from p in lord.ownedPawns
					where p != pawn && AdvancedAI.IsActivePawn(p)
					select p into ps
					select ps.Position;
					bool flag5 = enumerable.EnumerableNullOrEmpty<IntVec3>();
					if (flag5)
					{
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. lordPositions null or empty.", pawn, pawn.Position));
						}
						coverPosition = IntVec3.Invalid;
						result = false;
					}
					else
					{
						float lordAverageDistanceToFocusCell = enumerable.Average((IntVec3 xx) => xx.DistanceTo(center));
						IEnumerable<IntVec3> enumerable2 = from intVec in enumerable
						where intVec.DistanceTo(center) >= lordAverageDistanceToFocusCell
						select intVec;
						bool flag7 = enumerable2.EnumerableNullOrEmpty<IntVec3>();
						if (flag7)
						{
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. fixedLordPositions null or empty.", pawn, pawn.Position));
							}
							coverPosition = IntVec3.Invalid;
							result = false;
						}
						else
						{
							float num = 12f;
							bool hasDangerousWeapon = AdvancedAI.PrimaryIsSiegeWeapon(pawn);
							IntVec3 LordCenterPosition = new IntVec3(Mathf.RoundToInt((float)enumerable2.Average((IntVec3 x) => x.x)), pawn.Position.y, Mathf.RoundToInt((float)enumerable2.Average((IntVec3 z) => z.z)));
							string text;
							bool flag9 = pawn.Position.DistanceTo(LordCenterPosition) < num && AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, pawn.Position, center, true, false, true, false, hasDangerousWeapon, false, false, out text) > 0f;
							if (flag9)
							{
								coverPosition = pawn.Position;
								bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag10)
								{
									Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. Using current position for cover: {2}. Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
									{
										pawn,
										pawn.Position,
										coverPosition,
										center,
										LordCenterPosition,
										lordAverageDistanceToFocusCell,
										pawn.Position.DistanceTo(LordCenterPosition)
									}));
								}
								result = true;
							}
							else
							{
								IEnumerable<IntVec3> source = from c in GenRadial.RadialCellsAround(LordCenterPosition, 15f, true)
								where LordCenterPosition.DistanceTo(center) * 1.2f >= LordCenterPosition.DistanceTo(c)
								select c;
								IEnumerable<IntVec3> enumerable3 = source.Where(delegate(IntVec3 cell)
								{
									string text2;
									return AdvancedAI_CoverUtility.GetCellCoverRatingForPawn(pawn, cell, center, true, false, true, false, hasDangerousWeapon, false, false, out text2) > 0f;
								});
								bool flag11 = !enumerable3.EnumerableNullOrEmpty<IntVec3>();
								if (flag11)
								{
									bool flag12 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag12)
									{
										Log.Message(string.Format("{0} {1}: GetLeaderCoverPosition coverPositions count: {2}", pawn, pawn.Position, enumerable3.Count<IntVec3>()));
										foreach (IntVec3 c2 in enumerable3)
										{
											pawn.Map.debugDrawer.FlashCell(c2, 0.62f, null, SkyAiCore.Settings.flashCellDelay);
										}
									}
									bool flag13 = enumerable3.Contains(pawn.Position);
									if (flag13)
									{
										bool flag14 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag14)
										{
											Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. CoverPositions contains current position: {2} Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
											{
												pawn,
												pawn.Position,
												pawn.Position,
												center,
												LordCenterPosition,
												lordAverageDistanceToFocusCell,
												pawn.Position.DistanceTo(LordCenterPosition)
											}));
										}
										coverPosition = pawn.Position;
										result = true;
									}
									else
									{
										Pawn closestAllyPawn;
										(from p1 in lord.ownedPawns
										where p1 != pawn && !AdvancedAI.PawnIsDoctor(p1) && AdvancedAI.IsActivePawn(p1)
										select p1).TryMinBy((Pawn p2) => p2.Position.DistanceTo(pawn.Position), out closestAllyPawn);
										bool flag15 = closestAllyPawn != null;
										if (flag15)
										{
											IntVec3 intVec2 = enumerable3.MinBy((IntVec3 d) => d.DistanceTo(closestAllyPawn.Position));
											bool flag16 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag16)
											{
												Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. Leader position: {2} Focus cell: {3} lordCenterPosition: {4} lordAverageDistanceToFocusCell: {5} Distance to lordCenterPosition: {6}", new object[]
												{
													pawn,
													pawn.Position,
													intVec2,
													center,
													LordCenterPosition,
													lordAverageDistanceToFocusCell,
													pawn.Position.DistanceTo(LordCenterPosition)
												}));
											}
											coverPosition = intVec2;
											result = true;
										}
										else
										{
											bool flag17 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
											if (flag17)
											{
												Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. Position is Invalid. Closest ally pawn null. Focus cell: {2} lordCenterPosition: {3} lordAverageDistanceToFocusCell: {4} Distance to lordCenterPosition: {5}", new object[]
												{
													pawn,
													pawn.Position,
													center,
													LordCenterPosition,
													lordAverageDistanceToFocusCell,
													pawn.Position.DistanceTo(LordCenterPosition)
												}));
											}
											coverPosition = IntVec3.Invalid;
											result = false;
										}
									}
								}
								else
								{
									bool flag18 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag18)
									{
										Log.Message(string.Format("{0} {1} GetLeaderCoverPosition. Position is Invalid. CoverPositions null or empty. Focus cell: {2} lordCenterPosition: {3} lordAverageDistanceToFocusCell: {4} Distance to lordCenterPosition: {5}", new object[]
										{
											pawn,
											pawn.Position,
											center,
											LordCenterPosition,
											lordAverageDistanceToFocusCell,
											pawn.Position.DistanceTo(LordCenterPosition)
										}));
									}
									coverPosition = IntVec3.Invalid;
									result = false;
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0200007B RID: 123
		public enum CoverPositionType
		{
			// Token: 0x040001C2 RID: 450
			Normal,
			// Token: 0x040001C3 RID: 451
			BehindCellsOnly,
			// Token: 0x040001C4 RID: 452
			FrontCellsOnly,
			// Token: 0x040001C5 RID: 453
			DutyFrontCellsOnly
		}
	}
}
