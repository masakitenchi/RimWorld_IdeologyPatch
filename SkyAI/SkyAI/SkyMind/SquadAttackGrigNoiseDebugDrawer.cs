using System;
using Verse;

namespace SkyMind
{
	// Token: 0x02000010 RID: 16
	public static class SquadAttackGrigNoiseDebugDrawer
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00006820 File Offset: 0x00004A20
		public static void DebugDrawNoise(SquadAttackGrid grid)
		{
			Map currentMap = Find.CurrentMap;
			SquadAttackGrigNoiseDebugDrawer.CheckInitDebugDrawGrid(grid);
			foreach (IntVec3 c in currentMap.AllCells)
			{
				bool flag = SquadAttackGrigNoiseDebugDrawer.debugDrawGrid[c] > 0;
				if (flag)
				{
					CellRenderer.RenderCell(c, (float)SquadAttackGrigNoiseDebugDrawer.debugDrawGrid[c] / 100f);
				}
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000068A8 File Offset: 0x00004AA8
		private static void CheckInitDebugDrawGrid(SquadAttackGrid grid)
		{
			bool flag = grid != SquadAttackGrigNoiseDebugDrawer.debugGrid;
			if (flag)
			{
				SquadAttackGrigNoiseDebugDrawer.debugDrawGrid = null;
				SquadAttackGrigNoiseDebugDrawer.debugGrid = grid;
			}
			bool flag2 = SquadAttackGrigNoiseDebugDrawer.debugDrawGrid == null;
			if (flag2)
			{
				SquadAttackGrigNoiseDebugDrawer.debugDrawGrid = new IntGrid(grid.Map);
				SquadAttackGrigNoiseDebugDrawer.debugDrawGrid.Clear(0);
				foreach (IntVec3 intVec in grid.Map.AllCells)
				{
					bool flag3 = SquadAttackGrigNoiseDebugDrawer.debugGrid.WithinNoise(intVec);
					if (flag3)
					{
						SquadAttackGrigNoiseDebugDrawer.debugDrawGrid[intVec] = 1;
					}
				}
			}
		}

		// Token: 0x0400002E RID: 46
		private static SquadAttackGrid debugGrid;

		// Token: 0x0400002F RID: 47
		private static IntGrid debugDrawGrid;
	}
}
