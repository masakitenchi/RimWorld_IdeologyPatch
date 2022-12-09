using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200000F RID: 15
	public static class SquadAttackGridDebug
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00006264 File Offset: 0x00004464
		private static void DebugBreachPickTwoPoints(int radius, int walkMargin, bool useAvoidGrid)
		{
			bool flag = !SquadAttackGridDebug.debugStartCell.IsValid;
			if (flag)
			{
				SquadAttackGridDebug.debugStartCell = UI.MouseCell();
			}
			else
			{
				IntVec3 destCall = UI.MouseCell();
				SquadAttackGridDebug.debugDestCell = destCall;
				SquadAttackGridDebug.DebugCreateSquadAttackPath(radius, walkMargin, useAvoidGrid, SquadAttackGridDebug.debugStartCell, destCall);
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000062AC File Offset: 0x000044AC
		private static void DebugBreachPickOnePoint(int radius, int walkMargin, bool useAvoidGrid)
		{
			IntVec3 intVec = UI.MouseCell();
			IntVec3 destCall = GenAI.RandomRaidDest(intVec, Find.CurrentMap);
			bool flag = !destCall.IsValid;
			if (flag)
			{
				Messages.Message("Could not find a destination for squad attack path", MessageTypeDefOf.RejectInput, false);
			}
			else
			{
				bool flag2 = !SquadAttackGridDebug.debugStartCell.IsValid;
				if (flag2)
				{
					SquadAttackGridDebug.debugStartCell = intVec;
				}
				SquadAttackGridDebug.debugDestCell = destCall;
				SquadAttackGridDebug.DebugCreateSquadAttackPath(radius, walkMargin, useAvoidGrid, intVec, destCall);
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00006318 File Offset: 0x00004518
		private static void DebugCreateSquadAttackPath(int radius, int walkMargin, bool useAvoidGrid, IntVec3 startCell, IntVec3 destCall)
		{
			DebugViewSettings.drawBreachingGrid = true;
			SquadAttackGridDebug.debugSquadAttackGridForDrawing = new SquadAttackGrid(Find.CurrentMap, null);
			SquadAttackGridDebug.debugSquadAttackGridForDrawing.CreatePath(startCell, destCall, radius, walkMargin, useAvoidGrid);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00006342 File Offset: 0x00004542
		public static void ClearDebugPath()
		{
			SquadAttackGridDebug.debugSquadAttackGridForDrawing = null;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000634C File Offset: 0x0000454C
		public static void DebugDrawAllOnMap(Map map)
		{
			bool flag = !DebugViewSettings.drawBreachingGrid && !DebugViewSettings.drawBreachingNoise;
			if (!flag)
			{
				SquadAttackGrid squadAttackGrid = SquadAttackGridDebug.debugSquadAttackGridForDrawing;
				Map map2 = (squadAttackGrid != null) ? squadAttackGrid.Map : null;
				bool flag2 = map2 == map;
				if (flag2)
				{
					SquadAttackGridDebug.DebugDrawSquadAttackGrid(SquadAttackGridDebug.debugSquadAttackGridForDrawing);
				}
				MapComponent_SkyAI component = map.GetComponent<MapComponent_SkyAI>();
				List<RaidData> raidData = component.raidData;
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
								SquadAttackData data = squadData.Data;
								bool flag5 = data != null;
								if (flag5)
								{
									SquadAttackGridDebug.DebugDrawSquadAttackGrid(data.squadAttackGrid);
									bool flag6 = data.currentTarget != null;
									if (flag6)
									{
										CellRenderer.RenderSpot(data.currentTarget.Position, 0.9f, 0.4f);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000064B8 File Offset: 0x000046B8
		private static void DebugDrawMarkerGrid(SquadAttackGrid grid, Map map)
		{
			for (int i = 0; i < map.Size.x; i++)
			{
				for (int j = 0; j < map.Size.z; j++)
				{
					IntVec3 c = new IntVec3(i, 0, j);
					byte b = grid.MarkerGrid[c];
					bool flag = b == 180;
					if (flag)
					{
						CellRenderer.RenderSpot(c, 0.1f, 0.15f);
					}
					else
					{
						bool flag2 = b == 10;
						if (flag2)
						{
							CellRenderer.RenderCell(c, 0.1f);
						}
					}
					bool flag3 = grid.ReachableGrid[c];
					if (flag3)
					{
						CellRenderer.RenderSpot(c, 0.5f, 0.03f);
					}
				}
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00006584 File Offset: 0x00004784
		private static void DebugDrawSquadAttackGrid(SquadAttackGrid grid)
		{
			bool drawBreachingNoise = DebugViewSettings.drawBreachingNoise;
			if (drawBreachingNoise)
			{
				SquadAttackGrigNoiseDebugDrawer.DebugDrawNoise(grid);
			}
			bool drawBreachingGrid = DebugViewSettings.drawBreachingGrid;
			if (drawBreachingGrid)
			{
				SquadAttackGridDebug.DebugDrawMarkerGrid(grid, grid.Map);
				SquadData squadData = grid.squadData;
				bool flag = squadData == null;
				if (flag)
				{
					squadData = new SquadData();
				}
				SquadAttackData data = squadData.Data;
				bool flag2 = !data.start.IsValid;
				if (flag2)
				{
					data.start = SquadAttackGridDebug.debugStartCell;
				}
				bool flag3 = !data.dest.IsValid;
				if (flag3)
				{
					data.dest = SquadAttackGridDebug.debugDestCell;
				}
				Faction ofAncientsHostile = Find.FactionManager.OfAncientsHostile;
				foreach (IntVec3 c in grid.WalkGrid.ActiveCells)
				{
					Building firstBuilding = c.GetFirstBuilding(grid.Map);
					float colorPct = 0.3f;
					bool flag4 = grid.AttackGrid[c];
					if (flag4)
					{
						colorPct = 0.4f;
						bool flag5 = firstBuilding != null && SquadAttackUtility.ShouldAttackBuilding(data, firstBuilding, ofAncientsHostile);
						if (flag5)
						{
							colorPct = 0.1f;
							bool flag6 = SquadAttackUtility.IsWorthAttackBuilding(data, grid, firstBuilding, ofAncientsHostile);
							if (flag6)
							{
								colorPct = 0.8f;
								bool flag7 = SquadAttackUtility.CountReachableAdjacentCells(grid, firstBuilding) > 0;
								if (flag7)
								{
									List<Thing> list = squadData.FindBuildingsToAttack(ofAncientsHostile, 100);
									foreach (Thing thing in list)
									{
										CellRenderer.RenderSpot(thing.Position, colorPct, 0.15f);
									}
								}
							}
						}
					}
					CellRenderer.RenderCell(c, colorPct);
				}
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00006768 File Offset: 0x00004968
		[DebugAction("Pawns", "Draw squad attack grid...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DebugDrawSiegePath()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			for (int i = 1; i <= 5; i++)
			{
				int widthLocal = i;
				list.Add(new DebugMenuOption("width: " + i.ToString(), DebugMenuOptionMode.Action, delegate()
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					for (int j = 1; j < 5; j++)
					{
						int marginLocal = j;
						list2.Add(new DebugMenuOption("margin: " + j.ToString(), DebugMenuOptionMode.Action, delegate()
						{
							List<DebugMenuOption> options = new List<DebugMenuOption>
							{
								new DebugMenuOption("Draw from...", DebugMenuOptionMode.Tool, delegate()
								{
									SquadAttackGridDebug.DebugBreachPickOnePoint(widthLocal, marginLocal, false);
								}),
								new DebugMenuOption("Draw from (with avoid grid)...", DebugMenuOptionMode.Tool, delegate()
								{
									SquadAttackGridDebug.DebugBreachPickOnePoint(widthLocal, marginLocal, true);
								}),
								new DebugMenuOption("Draw between...", DebugMenuOptionMode.Tool, delegate()
								{
									SquadAttackGridDebug.DebugBreachPickTwoPoints(widthLocal, marginLocal, false);
								}),
								new DebugMenuOption("Draw between (with avoid grid)...", DebugMenuOptionMode.Tool, delegate()
								{
									SquadAttackGridDebug.DebugBreachPickTwoPoints(widthLocal, marginLocal, true);
								})
							};
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000067DC File Offset: 0x000049DC
		public static void Notify_BuildingStateChanged(Building b)
		{
			SquadAttackGrid squadAttackGrid = SquadAttackGridDebug.debugSquadAttackGridForDrawing;
			bool flag = squadAttackGrid == null;
			if (!flag)
			{
				squadAttackGrid.Notify_BuildingStateChanged(b);
			}
		}

		// Token: 0x0400002B RID: 43
		private static IntVec3 debugStartCell = IntVec3.Invalid;

		// Token: 0x0400002C RID: 44
		private static IntVec3 debugDestCell = IntVec3.Invalid;

		// Token: 0x0400002D RID: 45
		private static SquadAttackGrid debugSquadAttackGridForDrawing = null;
	}
}
