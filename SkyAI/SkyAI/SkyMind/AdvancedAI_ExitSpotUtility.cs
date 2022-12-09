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
	// Token: 0x02000019 RID: 25
	public static class AdvancedAI_ExitSpotUtility
	{
		// Token: 0x060000BD RID: 189 RVA: 0x0000E3FC File Offset: 0x0000C5FC
		public static bool ContainsInExitData(this Pawn pawn)
		{
			bool flag = false;
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag2 = raidData == null;
			bool result;
			if (flag2)
			{
				result = flag;
			}
			else
			{
				bool flag3 = raidData.exitCells == null;
				if (flag3)
				{
					raidData.exitCells = new List<ExitData>();
					result = flag;
				}
				else
				{
					for (int i = raidData.exitCells.Count - 1; i >= 0; i--)
					{
						ExitData exitData = raidData.exitCells[i];
						bool flag4 = exitData != null;
						if (flag4)
						{
							bool flag5 = exitData.pawn != null;
							if (flag5)
							{
								bool flag6 = exitData.pawn == pawn;
								if (flag6)
								{
									flag = true;
									break;
								}
							}
							else
							{
								raidData.exitCells.Remove(exitData);
							}
						}
					}
					result = flag;
				}
			}
			return result;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x0000E4C8 File Offset: 0x0000C6C8
		public static void SaveCommonExitSpot(Pawn pawn, IntVec3 savedSpot)
		{
			Map map = pawn.MapHeld ?? Find.CurrentMap;
			bool flag = pawn.RaceProps.intelligence == Intelligence.Humanlike && pawn.Faction != null && pawn.PositionHeld.InBounds(map);
			if (flag)
			{
				RaidData raidData = AdvancedAI.PawnRaidData(pawn);
				bool flag2 = raidData != null && AdvancedAI.IsValidLoc(pawn, savedSpot, PathEndMode.OnCell) && !pawn.ContainsInExitData();
				if (flag2)
				{
					ExitData item = new ExitData(pawn, pawn.PositionHeld, savedSpot, Find.TickManager.TicksGame);
					raidData.exitCells.Add(item);
				}
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0000E560 File Offset: 0x0000C760
		public static bool TrySaveExitSpot(Pawn pawn, float maxDistanceToEnemy, float skipFirstCellsOnDistance, TraverseMode mode, out IntVec3 spot)
		{
			IntVec3 intVec = IntVec3.Invalid;
			bool flag = !SkyAiCore.Settings.enableSaveExitSpot;
			bool result;
			if (flag)
			{
				spot = intVec;
				result = false;
			}
			else
			{
				Map map = pawn.MapHeld ?? Find.CurrentMap;
				bool flag2 = map != null;
				if (flag2)
				{
					intVec = AdvancedAI_ExitSpotUtility.BestExitMapCell(pawn, pawn.PositionHeld, maxDistanceToEnemy, skipFirstCellsOnDistance, mode);
				}
				bool flag3 = intVec != IntVec3.Invalid;
				if (flag3)
				{
					spot = intVec;
					result = true;
				}
				else
				{
					spot = intVec;
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000E5EC File Offset: 0x0000C7EC
		public static IntVec3 BestExitMapCell(Pawn pawn, IntVec3 fromPosition, float maxDistanceToEnemy, float skipFirstCellsOnDistance, TraverseMode mode)
		{
			IntVec3 intVec = IntVec3.Invalid;
			IEnumerable<Pawn> enumerable = from p in pawn.Map.mapPawns.AllPawnsSpawned
			where p != pawn && AdvancedAI.IsActiveTarget(pawn, p, true, false) && ((p.Faction == null) ? p.InAggroMentalState : (!AdvancedAI.IsAlly(pawn, p, false) || p.InAggroMentalState))
			select p;
			bool flag = enumerable.EnumerableNullOrEmpty<Pawn>();
			IntVec3 result;
			if (flag)
			{
				IntVec3 intVec2;
				bool flag2 = RCellFinder.TryFindBestExitSpot(pawn, out intVec2, TraverseMode.ByPawn);
				if (flag2)
				{
					result = intVec;
				}
				else
				{
					result = IntVec3.Invalid;
				}
			}
			else
			{
				IOrderedEnumerable<IntVec3> orderedEnumerable = from c in CellRect.WholeMap(pawn.Map).EdgeCells
				where c.Standable(pawn.Map) && !pawn.Map.roofGrid.Roofed(c) && c.GetRoom(pawn.Map).TouchesMapEdge && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, false, mode)
				orderby c.DistanceTo(pawn.Position)
				select c;
				bool debugLeaveCells = SkyAiCore.Settings.debugLeaveCells;
				if (debugLeaveCells)
				{
					foreach (IntVec3 c2 in orderedEnumerable)
					{
						pawn.Map.debugDrawer.FlashCell(c2, 0.52f, null, SkyAiCore.Settings.flashCellDelay);
					}
					Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. Cells count: {2} enemies on map: {3}", new object[]
					{
						pawn,
						pawn.Position,
						orderedEnumerable.Count<IntVec3>(),
						enumerable.Count<Pawn>()
					}));
				}
				IntRange intRange = new IntRange(35, 50);
				int num = 20;
				int num2 = 5;
				int num3 = Mathf.Clamp(Mathf.RoundToInt(maxDistanceToEnemy * 0.3f), 18, 36);
				for (int i = intRange.min; i > num; i -= num2)
				{
					foreach (IntVec3 intVec3 in orderedEnumerable)
					{
						int num4 = 0;
						List<IntVec3> list = AdvancedAI.CellsBetweenPositions(fromPosition, intVec3, pawn.Map, false, 0, 0, num3, 0f);
						bool debugLeaveCells2 = SkyAiCore.Settings.debugLeaveCells;
						if (debugLeaveCells2)
						{
							Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. cellsBetweenPositions count: {2} step: {3}", new object[]
							{
								pawn,
								pawn.Position,
								list.Count<IntVec3>(),
								num3
							}));
						}
						using (List<IntVec3>.Enumerator enumerator3 = list.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								IntVec3 cellOnRoute = enumerator3.Current;
								num4++;
								bool flag3 = cellOnRoute.DistanceTo(fromPosition) < skipFirstCellsOnDistance;
								if (!flag3)
								{
									bool debugLeaveCells3 = SkyAiCore.Settings.debugLeaveCells;
									if (debugLeaveCells3)
									{
										Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. check for cell {2} cellOnRoute1: {3} step: {4}/{5}", new object[]
										{
											pawn,
											pawn.Position,
											intVec3,
											cellOnRoute,
											num4,
											list.Count<IntVec3>()
										}));
									}
									Pawn pawn2 = enumerable.MinBy((Pawn byClosestEnemy) => cellOnRoute.DistanceTo(byClosestEnemy.Position));
									bool flag4 = pawn2 != null && cellOnRoute.DistanceTo(pawn2.Position) < maxDistanceToEnemy;
									if (flag4)
									{
										bool flag5 = pawn2.RaceProps.Animal && cellOnRoute.DistanceTo(pawn2.Position) >= 35f;
										if (!flag5)
										{
											float num5 = AdvancedAI.EffectiveRange(pawn2);
											float num6 = Mathf.Clamp(num5, (float)i, (float)(i + 15));
											bool flag6 = cellOnRoute.DistanceTo(pawn2.Position) >= num6;
											if (flag6)
											{
												bool flag7 = num5 >= 16f;
												if (!flag7)
												{
													bool debugLeaveCells4 = SkyAiCore.Settings.debugLeaveCells;
													if (debugLeaveCells4)
													{
														Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. Enemy {2} with effRange/minRange: {3}/{4} attack. Enemy with a close combat weapon. I have a chance to quit, passing. step: {5}/{6}", new object[]
														{
															pawn,
															pawn.Position,
															pawn2,
															num5,
															num6,
															num4,
															list.Count<IntVec3>()
														}));
													}
													continue;
												}
												bool flag8 = !GenSight.LineOfSight(pawn2.Position, cellOnRoute, pawn.Map, false, null, 0, 0);
												if (flag8)
												{
													bool debugLeaveCells5 = SkyAiCore.Settings.debugLeaveCells;
													if (debugLeaveCells5)
													{
														Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. Enemy {2} with effRange/minRange: {3}/{4} attack. No line of sight. I have a chance to quit, passing. step: {5}/{6}", new object[]
														{
															pawn,
															pawn.Position,
															pawn2,
															num5,
															num6,
															num4,
															list.Count<IntVec3>()
														}));
													}
													continue;
												}
											}
											bool debugLeaveCells6 = SkyAiCore.Settings.debugLeaveCells;
											if (debugLeaveCells6)
											{
												Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. Check for cell {2} cellOnRoute2 break: {3} enemy: {4} with effRange/minRange: {5}/{6} step: {7}/{8}", new object[]
												{
													pawn,
													pawn.Position,
													intVec3,
													cellOnRoute,
													pawn2,
													num5,
													num6,
													num4,
													list.Count<IntVec3>()
												}));
											}
											break;
										}
										bool debugLeaveCells7 = SkyAiCore.Settings.debugLeaveCells;
										if (debugLeaveCells7)
										{
											Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. Enemy {2} is animal and not on dangerous distance, passing. step: {3}/{4}", new object[]
											{
												pawn,
												pawn.Position,
												pawn2,
												num4,
												list.Count<IntVec3>()
											}));
										}
									}
									else
									{
										bool flag9 = num4 == list.Count<IntVec3>();
										if (flag9)
										{
											bool debugLeaveCells8 = SkyAiCore.Settings.debugLeaveCells;
											if (debugLeaveCells8)
											{
												Log.Message(string.Format("{0} {1}: TrySaveLeaveSpot debug. result cell: {2} step: {3}/{4}", new object[]
												{
													pawn,
													pawn.Position,
													intVec3,
													num4,
													list.Count<IntVec3>()
												}));
											}
											intVec = intVec3;
										}
									}
								}
							}
						}
						bool flag10 = intVec != IntVec3.Invalid;
						if (flag10)
						{
							break;
						}
					}
					bool flag11 = intVec != IntVec3.Invalid;
					if (flag11)
					{
						break;
					}
				}
				result = intVec;
			}
			return result;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000ED28 File Offset: 0x0000CF28
		public static bool TryCommonExitSpot(Pawn pawn, IntVec3 generatedExitCell, out IntVec3 commonSpot)
		{
			commonSpot = IntVec3.Invalid;
			Map map = pawn.MapHeld ?? Find.CurrentMap;
			bool flag = !SkyAiCore.Settings.enableCommonExitSpot || map == null || pawn.RaceProps.intelligence < Intelligence.Humanlike || pawn.Faction == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				IntVec3 intVec = generatedExitCell;
				MapComponent_SkyAI component = map.GetComponent<MapComponent_SkyAI>();
				Lord lord = pawn.GetLord();
				RaidData raidData = AdvancedAI.PawnRaidData(pawn);
				bool flag2 = component != null && raidData != null;
				if (flag2)
				{
					bool flag3 = AdvancedAI.IsValidLoc(pawn, generatedExitCell, PathEndMode.OnCell) && !pawn.ContainsInExitData();
					if (flag3)
					{
						for (int i = raidData.exitCells.Count - 1; i >= 0; i--)
						{
							ExitData exitData = raidData.exitCells[i];
							bool flag4 = exitData != null && exitData.pawn != null && exitData.pawn.Faction != null;
							if (!flag4)
							{
								ExitData item = new ExitData(pawn, pawn.PositionHeld, generatedExitCell, Find.TickManager.TicksGame);
								raidData.exitCells.Add(item);
								break;
							}
							Lord lord2 = exitData.pawn.GetLord();
							bool flag5 = lord != null && lord2 != null && !lord.loadID.Equals(lord2.loadID);
							bool flag6 = pawn.PositionHeld.DistanceTo(exitData.start) > 35f;
							bool flag7 = flag5 && flag6;
							if (flag7)
							{
								ExitData item2 = new ExitData(pawn, pawn.PositionHeld, generatedExitCell, Find.TickManager.TicksGame);
								raidData.exitCells.Add(item2);
								break;
							}
						}
					}
					else
					{
						IEnumerable<RaidData> raidData2 = component.raidData;
						Func<RaidData, bool> <>9__0;
						Func<RaidData, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((RaidData r) => pawn.Faction == r.faction || !pawn.Faction.HostileTo(r.faction)));
						}
						foreach (RaidData raidData3 in raidData2.Where(predicate))
						{
							List<ExitData> exitCells = raidData3.exitCells;
							bool flag8 = exitCells.NullOrEmpty<ExitData>();
							if (!flag8)
							{
								for (int j = exitCells.Count - 1; j >= 0; j--)
								{
									ExitData exitData2 = exitCells[j];
									bool flag9 = exitData2 != null && exitData2.pawn != null;
									if (flag9)
									{
										Lord lord3 = exitData2.pawn.GetLord();
										bool flag10 = lord != null && lord3 != null && lord.loadID.Equals(lord3.loadID);
										bool flag11 = pawn.PositionHeld.DistanceTo(exitData2.start) <= 35f;
										bool flag12 = flag10 && flag11;
										if (flag12)
										{
											intVec = exitData2.dest;
											break;
										}
									}
								}
							}
						}
					}
				}
				bool flag13 = AdvancedAI.IsValidLoc(pawn, intVec, PathEndMode.OnCell);
				if (flag13)
				{
					commonSpot = intVec;
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000F0A8 File Offset: 0x0000D2A8
		public static bool TryPerfectExitSpot(Pawn pawn, out IntVec3 spot, bool digOnCantReachMapEdge = true, bool saveDig = false)
		{
			spot = IntVec3.Invalid;
			bool flag = false;
			IntVec3 intVec;
			AdvancedAI_ExitSpotUtility.TryCommonExitSpot(pawn, spot, out intVec);
			bool flag2 = AdvancedAI.IsValidLoc(pawn, intVec, PathEndMode.OnCell);
			if (flag2)
			{
				bool debugLeaveCells = SkyAiCore.Settings.debugLeaveCells;
				if (debugLeaveCells)
				{
					Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TryCommonExitSpot: {2} from cur. position: {3}", new object[]
					{
						pawn,
						pawn.Position,
						intVec,
						pawn.Position
					}));
					pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
				}
				flag = true;
				spot = intVec;
			}
			bool flag3 = !flag && AdvancedAI_ExitSpotUtility.TrySaveExitSpot(pawn, 55f, 15f, TraverseMode.ByPawn, out spot);
			if (flag3)
			{
				bool debugLeaveCells2 = SkyAiCore.Settings.debugLeaveCells;
				if (debugLeaveCells2)
				{
					Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TrySaveLeaveSpot TraverseMode.ByPawn: {2} from cur. position: {3}", new object[]
					{
						pawn,
						pawn.Position,
						spot,
						pawn.Position
					}));
					pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
				}
				flag = true;
			}
			bool flag4 = !flag;
			if (flag4)
			{
				if (saveDig)
				{
					bool flag5 = AdvancedAI_ExitSpotUtility.TrySaveExitSpot(pawn, 55f, 15f, TraverseMode.PassAllDestroyableThings, out spot);
					if (flag5)
					{
						bool debugLeaveCells3 = SkyAiCore.Settings.debugLeaveCells;
						if (debugLeaveCells3)
						{
							Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TrySaveLeaveSpot TraverseMode.PassAllDestroyableThings: {2} from cur. position: {3}", new object[]
							{
								pawn,
								pawn.Position,
								spot,
								pawn.Position
							}));
							pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
						}
						flag = true;
					}
				}
				TraverseMode traverseMode = AdvancedAI_ExitSpotUtility.GetTraverseMode(pawn, digOnCantReachMapEdge);
				bool flag6 = traverseMode == TraverseMode.PassAllDestroyableThings;
				if (flag6)
				{
					bool flag7 = AdvancedAI_ExitSpotUtility.TrySaveExitSpot(pawn, 55f, 15f, TraverseMode.PassAllDestroyableThings, out spot);
					if (flag7)
					{
						bool debugLeaveCells4 = SkyAiCore.Settings.debugLeaveCells;
						if (debugLeaveCells4)
						{
							Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TrySaveLeaveSpot TraverseMode.PassAllDestroyableThings: {2} from cur. position: {3}", new object[]
							{
								pawn,
								pawn.Position,
								spot,
								pawn.Position
							}));
							pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
						}
						flag = true;
					}
				}
				bool flag8 = !RCellFinder.TryFindBestExitSpot(pawn, out spot, traverseMode);
				if (flag8)
				{
					bool flag9 = RCellFinder.TryFindRandomExitSpot(pawn, out spot, traverseMode);
					if (flag9)
					{
						bool debugLeaveCells5 = SkyAiCore.Settings.debugLeaveCells;
						if (debugLeaveCells5)
						{
							Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TryFindRandomExitSpot {2}: {3} from cur. position: {4}", new object[]
							{
								pawn,
								pawn.Position,
								traverseMode,
								spot,
								pawn.Position
							}));
							pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
						}
						flag = true;
					}
				}
				else
				{
					bool debugLeaveCells6 = SkyAiCore.Settings.debugLeaveCells;
					if (debugLeaveCells6)
					{
						Log.Message(string.Format("{0} {1}: using TryPerfectExitSpot, found cell to leave map with TryFindBestExitSpot {2}: {3} from cur. position: {4}", new object[]
						{
							pawn,
							pawn.Position,
							traverseMode,
							spot,
							pawn.Position
						}));
						pawn.Map.debugDrawer.FlashCell(spot, 0.47f, "EXT", SkyAiCore.Settings.flashCellDelay);
					}
					flag = true;
				}
			}
			bool flag10 = flag && AdvancedAI.IsValidLoc(spot);
			if (flag10)
			{
				AdvancedAI_ExitSpotUtility.SaveCommonExitSpot(pawn, spot);
			}
			return flag;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000F4FC File Offset: 0x0000D6FC
		public static TraverseMode GetTraverseMode(Pawn pawn, bool digOnCantReachMapEdge = true)
		{
			bool flag = !digOnCantReachMapEdge;
			TraverseMode result;
			if (flag)
			{
				result = TraverseMode.ByPawn;
			}
			else
			{
				result = (pawn.CanReachMapEdge() ? TraverseMode.ByPawn : TraverseMode.PassAllDestroyableThings);
			}
			return result;
		}
	}
}
