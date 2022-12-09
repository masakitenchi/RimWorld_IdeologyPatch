using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace SkyMind
{
	// Token: 0x02000013 RID: 19
	public class SquadAttackGrid : IExposable
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000063 RID: 99 RVA: 0x0000764E File Offset: 0x0000584E
		public BoolGrid WalkGrid
		{
			get
			{
				return this.walkGrid;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000064 RID: 100 RVA: 0x00007656 File Offset: 0x00005856
		public BoolGrid AttackGrid
		{
			get
			{
				return this.squadAttackGrid;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000065 RID: 101 RVA: 0x0000765E File Offset: 0x0000585E
		public Map Map
		{
			get
			{
				return this.map;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00007666 File Offset: 0x00005866
		public int Radius
		{
			get
			{
				return this.radius;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00007670 File Offset: 0x00005870
		public ByteGrid MarkerGrid
		{
			get
			{
				this.RegenerateCachedGridIfDirty();
				return this.markerGrid;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000068 RID: 104 RVA: 0x00007690 File Offset: 0x00005890
		public BoolGrid ReachableGrid
		{
			get
			{
				this.RegenerateCachedGridIfDirty();
				return this.reachableGrid;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000069 RID: 105 RVA: 0x000076B0 File Offset: 0x000058B0
		public Perlin Noise
		{
			get
			{
				bool flag = this.perlinCached == null;
				if (flag)
				{
					this.perlinCached = SquadAttackGrid.CreatePerlinNoise(this.perlinSeed);
				}
				return this.perlinCached;
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x000076E8 File Offset: 0x000058E8
		public SquadAttackGrid()
		{
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0000770C File Offset: 0x0000590C
		public SquadAttackGrid(Map map, SquadData squadData)
		{
			this.map = map;
			this.squadData = squadData;
			this.walkGrid = new BoolGrid(map);
			this.squadAttackGrid = new BoolGrid(map);
			this.perlinSeed = Rand.Int;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x0000776C File Offset: 0x0000596C
		public void ExposeData()
		{
			Scribe_Deep.Look<BoolGrid>(ref this.walkGrid, "walkGrid", Array.Empty<object>());
			Scribe_Deep.Look<BoolGrid>(ref this.squadAttackGrid, "squadAttackGrid", Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.perlinSeed, "perlinSeed", 0, false);
			Scribe_Values.Look<int>(ref this.radius, "radius", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.start, "start", default(IntVec3), false);
			Scribe_References.Look<Map>(ref this.map, "map", false);
			Scribe_Deep.Look<SquadData>(ref this.squadData, "squadData", new object[]
			{
				false
			});
		}

		// Token: 0x0600006D RID: 109 RVA: 0x0000781C File Offset: 0x00005A1C
		public static Perlin CreatePerlinNoise(int seed)
		{
			return new Perlin((double)SquadAttackGrid.perlinFrequency, (double)SquadAttackGrid.perlinLacunarity, (double)SquadAttackGrid.perlinPersistence, (int)SquadAttackGrid.perlinOctaves, seed, QualityMode.Medium);
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00007850 File Offset: 0x00005A50
		public void Notify_PawnStateChanged(Pawn pawn)
		{
			this.cachedGridsDirty = true;
			bool flag = this.squadData != null;
			if (flag)
			{
				this.squadData.shouldUpdateBuildingAttackList = true;
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00007880 File Offset: 0x00005A80
		public void Notify_BuildingStateChanged(Building b)
		{
			this.cachedGridsDirty = true;
			bool flag = this.squadData != null;
			if (flag)
			{
				this.squadData.shouldUpdateBuildingAttackList = true;
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000078B0 File Offset: 0x00005AB0
		public bool WithinNoise(IntVec3 cell)
		{
			return this.Noise.GetValue(cell) >= SquadAttackGrid.perlinThres;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000078D8 File Offset: 0x00005AD8
		public void CreatePath(IntVec3 start, IntVec3 end, int radius, int walkMargin, bool useAvoidGrid = false)
		{
			this.radius = radius;
			this.start = start;
			this.SetupCostOffsets();
			PathFinderCostTuning pathFinderCostTuning = new PathFinderCostTuning();
			pathFinderCostTuning.costBlockedDoor = SquadAttackGrid.tweakWallCost;
			pathFinderCostTuning.costBlockedWallBase = SquadAttackGrid.tweakWallCost;
			pathFinderCostTuning.costBlockedDoorPerHitPoint = SquadAttackGrid.tweakWallHpCost;
			pathFinderCostTuning.costBlockedWallExtraPerHitPoint = SquadAttackGrid.tweakWallHpCost;
			pathFinderCostTuning.costOffLordWalkGrid = SquadAttackGrid.tweakOffWalkGridPathCost;
			pathFinderCostTuning.costBlockedWallExtraForNaturalWalls = SquadAttackGrid.tweakNaturalWallExtraCost;
			pathFinderCostTuning.custom = new SquadAttackGrid.CustomTuning(radius, this, pathFinderCostTuning);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false, useAvoidGrid, false);
			using (PawnPath pawnPath = this.map.pathFinder.FindPath(start, end, traverseParms, PathEndMode.OnCell, pathFinderCostTuning))
			{
				foreach (IntVec3 c in pawnPath.NodesReversed)
				{
					this.squadAttackGrid[c] = true;
					this.walkGrid[c] = true;
				}
			}
			for (int i = 0; i < radius; i++)
			{
				this.WidenGrid(this.squadAttackGrid);
				this.WidenGrid(this.walkGrid);
			}
			for (int j = 0; j < walkMargin; j++)
			{
				this.WidenGrid(this.walkGrid);
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00007A50 File Offset: 0x00005C50
		public List<Thing> FindBuildingTargets(SquadAttackData data, Faction sourceFaction, int maxCount)
		{
			List<Thing> result = new List<Thing>();
			int count = 0;
			this.RegenerateCachedGridIfDirty();
			this.Map.floodFiller.FloodFill(this.start, (IntVec3 c) => this.AttackGrid[c], delegate(IntVec3 c)
			{
				List<Thing> thingList = c.GetThingList(this.Map);
				int count;
				for (int i = 0; i < thingList.Count; i++)
				{
					Building building = thingList[i] as Building;
					bool flag = building != null && SquadAttackUtility.IsWorthAttackBuilding(data, this, building, sourceFaction);
					if (flag)
					{
						count = count;
						count++;
						result.Add(building);
					}
				}
				return count >= maxCount;
			}, int.MaxValue, false, null);
			return result;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00007ADC File Offset: 0x00005CDC
		private void WidenGrid(BoolGrid grid)
		{
			SquadAttackGrid.tmpWidenGrid.ClearAndResizeTo(this.map);
			foreach (IntVec3 a in grid.ActiveCells)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = a + GenAdj.AdjacentCells[i];
					bool flag = c.InBounds(this.map);
					if (flag)
					{
						SquadAttackGrid.tmpWidenGrid[c] = true;
					}
				}
			}
			foreach (IntVec3 c2 in SquadAttackGrid.tmpWidenGrid.ActiveCells)
			{
				grid[c2] = true;
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00007BCC File Offset: 0x00005DCC
		public void Reset()
		{
			bool debugSquadAttackData = SkyAiCore.Settings.debugSquadAttackData;
			if (debugSquadAttackData)
			{
				Log.Message("SquadAttackGrid reset.");
			}
			this.squadAttackGrid.Clear();
			this.walkGrid.Clear();
			IntGrid intGrid = this.cellCostOffset;
			bool flag = intGrid != null;
			if (flag)
			{
				intGrid.Clear(0);
			}
			ByteGrid byteGrid = this.markerGrid;
			bool flag2 = byteGrid != null;
			if (flag2)
			{
				byteGrid.Clear(0);
			}
			BoolGrid boolGrid = this.reachableGrid;
			bool flag3 = boolGrid != null;
			if (flag3)
			{
				boolGrid.Clear();
			}
			this.cachedGridsDirty = true;
			SquadAttackGridDebug.ClearDebugPath();
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00007C6C File Offset: 0x00005E6C
		private void RegenerateCachedGridIfDirty()
		{
			bool flag = this.cachedGridsDirty;
			if (flag)
			{
				this.RegenerateCachedGrid();
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00007C90 File Offset: 0x00005E90
		private void RegenerateCachedGrid()
		{
			this.cachedGridsDirty = false;
			bool flag = this.markerGrid == null;
			if (flag)
			{
				this.markerGrid = new ByteGrid(this.Map);
			}
			else
			{
				this.markerGrid.Clear(0);
			}
			bool flag2 = this.reachableGrid == null;
			if (flag2)
			{
				this.reachableGrid = new BoolGrid(this.Map);
			}
			else
			{
				this.reachableGrid.Clear();
			}
			SquadAttackGrid.cachedWalkReachabilityPainter.PaintWalkReachability(this);
			bool flag3 = this.squadData != null;
			if (flag3)
			{
				for (int i = 0; i < this.squadData.squadPawns.Count; i++)
				{
					Pawn pawn = this.squadData.squadPawns[i];
					bool flag4 = pawn.mindState.breachingTarget != null && !pawn.mindState.breachingTarget.target.Destroyed;
					if (flag4)
					{
						this.PaintDangerFromPawn(pawn);
					}
				}
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00007D94 File Offset: 0x00005F94
		private void PaintDangerFromPawn(Pawn pawn)
		{
			BreachingTargetData breachingTarget = pawn.mindState.breachingTarget;
			bool flag = breachingTarget == null;
			if (!flag)
			{
				IntVec3 position = breachingTarget.target.Position;
				bool flag2 = !position.IsValid;
				if (!flag2)
				{
					Verb verb = SquadAttackUtility.FindVerbToUseForSiege(pawn);
					bool flag3 = verb != null;
					if (flag3)
					{
						IntVec3 firingPosition = breachingTarget.firingPosition;
						bool isValid = firingPosition.IsValid;
						if (isValid)
						{
							bool flag4 = this.markerGrid[firingPosition] == 0;
							if (flag4)
							{
								this.markerGrid[firingPosition] = 180;
							}
							this.VisitDangerousCellsOfAttack(firingPosition, position, verb, delegate(IntVec3 cell)
							{
								this.markerGrid[cell] = 10;
							});
						}
					}
				}
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00007E48 File Offset: 0x00006048
		public void VisitDangerousCellsOfAttack(IntVec3 firingPosition, IntVec3 targetPosition, Verb verb, Action<IntVec3> visitor)
		{
			bool flag = !verb.IsMeleeAttack;
			if (flag)
			{
				SquadAttackGrid.cachedDangerLineOfSightPainter.PaintLoS(this.map, firingPosition, targetPosition, visitor);
				this.PaintSplashDamage(verb, targetPosition, visitor);
			}
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00007E88 File Offset: 0x00006088
		private void PaintSplashDamage(Verb verb, IntVec3 center, Action<IntVec3> visitor)
		{
			float a = 2f;
			ThingDef projectile = verb.GetProjectile();
			bool flag = projectile != null && projectile.projectile.explosionRadius > 0f;
			if (flag)
			{
				a = Mathf.Max(a, projectile.projectile.explosionRadius);
			}
			int num = GenRadial.NumCellsInRadius(a);
			for (int i = 0; i < num; i++)
			{
				IntVec3 obj = (center + GenRadial.RadialPattern[i]).ClampInsideMap(this.map);
				visitor(obj);
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00007F20 File Offset: 0x00006120
		private void SetupCostOffsets()
		{
			bool flag = this.cellCostOffset == null;
			if (flag)
			{
				this.cellCostOffset = new IntGrid(this.map);
			}
			this.cellCostOffset.Clear(0);
			bool flag2 = !SquadAttackGrid.tweakAvoidDangerousRooms;
			if (!flag2)
			{
				foreach (Room room in this.map.regionGrid.allRooms)
				{
					int num = this.DangerousRoomCost(room);
					bool flag3 = num != 0;
					if (flag3)
					{
						foreach (IntVec3 c in room.Cells)
						{
							this.cellCostOffset[c] = num;
						}
						foreach (IntVec3 c2 in room.BorderCells)
						{
							bool flag4 = c2.InBounds(this.map);
							if (flag4)
							{
								this.cellCostOffset[c2] = num;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000808C File Offset: 0x0000628C
		private int DangerousRoomCost(Room room)
		{
			bool flag = !room.Fogged;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				foreach (Thing thing in room.ContainedAndAdjacentThings)
				{
					Pawn pawn;
					bool flag2 = (pawn = (thing as Pawn)) != null && pawn.mindState != null && !pawn.mindState.Active;
					if (flag2)
					{
						return 600;
					}
					bool flag3 = thing.def == ThingDefOf.Hive;
					if (flag3)
					{
						return 600;
					}
					bool flag4 = thing.def == ThingDefOf.AncientCryptosleepCasket;
					if (flag4)
					{
						return 600;
					}
				}
				result = 0;
			}
			return result;
		}

		// Token: 0x04000038 RID: 56
		public const byte Marker_FiringPosition = 180;

		// Token: 0x04000039 RID: 57
		public const byte Marker_Dangerous = 10;

		// Token: 0x0400003A RID: 58
		public const byte Marker_UnUsed = 0;

		// Token: 0x0400003B RID: 59
		private const int DangerousRoomPathCost = 600;

		// Token: 0x0400003C RID: 60
		private static SquadAttackGrid.WalkReachabilityPainter cachedWalkReachabilityPainter = new SquadAttackGrid.WalkReachabilityPainter();

		// Token: 0x0400003D RID: 61
		private static SquadAttackGrid.DangerLineOfSightPainter cachedDangerLineOfSightPainter = new SquadAttackGrid.DangerLineOfSightPainter();

		// Token: 0x0400003E RID: 62
		private BoolGrid walkGrid;

		// Token: 0x0400003F RID: 63
		private BoolGrid squadAttackGrid;

		// Token: 0x04000040 RID: 64
		private int perlinSeed;

		// Token: 0x04000041 RID: 65
		private int radius = 1;

		// Token: 0x04000042 RID: 66
		private IntVec3 start = IntVec3.Invalid;

		// Token: 0x04000043 RID: 67
		private Map map;

		// Token: 0x04000044 RID: 68
		public SquadData squadData;

		// Token: 0x04000045 RID: 69
		private Perlin perlinCached;

		// Token: 0x04000046 RID: 70
		private BoolGrid reachableGrid;

		// Token: 0x04000047 RID: 71
		private ByteGrid markerGrid;

		// Token: 0x04000048 RID: 72
		private bool cachedGridsDirty = true;

		// Token: 0x04000049 RID: 73
		private IntGrid cellCostOffset;

		// Token: 0x0400004A RID: 74
		[TweakValue("SquadAttackData", 0f, 70f)]
		private static int tweakWallCost = 35;

		// Token: 0x0400004B RID: 75
		[TweakValue("SquadAttackData", 0f, 1f)]
		private static float tweakWallHpCost = 0.02f;

		// Token: 0x0400004C RID: 76
		[TweakValue("SquadAttackData", 0f, 100f)]
		private static bool tweakUsePerlin = true;

		// Token: 0x0400004D RID: 77
		[TweakValue("SquadAttackData", -70f, 70f)]
		private static int tweakPerlinCost = 30;

		// Token: 0x0400004E RID: 78
		[TweakValue("SquadAttackData", 1f, 7f)]
		public static int tweakOffWalkGridPathCost = 140;

		// Token: 0x0400004F RID: 79
		[TweakValue("SquadAttackData", 0f, 100f)]
		private static bool tweakAvoidDangerousRooms = true;

		// Token: 0x04000050 RID: 80
		[TweakValue("SquadAttackData", -70f, 70f)]
		private static int tweakNaturalWallExtraCost = 20;

		// Token: 0x04000051 RID: 81
		[TweakValue("SquadAttackData", 0f, 0.1f)]
		private static float perlinFrequency = 0.06581f;

		// Token: 0x04000052 RID: 82
		[TweakValue("SquadAttackData", 1f, 2f)]
		private static float perlinLacunarity = 1.5516f;

		// Token: 0x04000053 RID: 83
		[TweakValue("SquadAttackData", 0f, 2f)]
		private static float perlinPersistence = 1.6569f;

		// Token: 0x04000054 RID: 84
		[TweakValue("SquadAttackData", 1f, 5f)]
		private static float perlinOctaves = 4f;

		// Token: 0x04000055 RID: 85
		[TweakValue("SquadAttackData", 0f, 1f)]
		private static float perlinThres = 0.5f;

		// Token: 0x04000056 RID: 86
		private static BoolGrid tmpWidenGrid = new BoolGrid();

		// Token: 0x02000076 RID: 118
		private class CustomTuning : PathFinderCostTuning.ICustomizer
		{
			// Token: 0x0600033B RID: 827 RVA: 0x0003F24F File Offset: 0x0003D44F
			public CustomTuning(int radius, SquadAttackGrid grid, PathFinderCostTuning tuning)
			{
				this.radius = radius;
				this.grid = grid;
				this.tuning = tuning;
			}

			// Token: 0x0600033C RID: 828 RVA: 0x0003F270 File Offset: 0x0003D470
			public int CostOffset(IntVec3 from, IntVec3 to)
			{
				IntVec3 a = (to - from).RotatedBy(Rot4.East);
				int num = 0;
				for (int i = -this.radius; i <= this.radius; i++)
				{
					IntVec3 intVec = to + a * i;
					bool flag = intVec.InBounds(this.grid.Map) && i != 0;
					if (flag)
					{
						num += this.CostOffAdjacent(intVec) + this.grid.cellCostOffset[intVec];
					}
				}
				bool flag2 = SquadAttackGrid.tweakUsePerlin && this.grid.WithinNoise(to);
				if (flag2)
				{
					num += SquadAttackGrid.tweakPerlinCost;
				}
				return num;
			}

			// Token: 0x0600033D RID: 829 RVA: 0x0003F330 File Offset: 0x0003D530
			private int CostOffAdjacent(IntVec3 cell)
			{
				Building edifice = cell.GetEdifice(this.grid.Map);
				bool flag = edifice != null && PathFinder.IsDestroyable(edifice);
				int result;
				if (flag)
				{
					result = this.tuning.costBlockedWallBase + (int)((float)edifice.HitPoints * this.tuning.costBlockedWallExtraPerHitPoint) + (edifice.def.IsBuildingArtificial ? 0 : this.tuning.costBlockedWallExtraForNaturalWalls);
				}
				else
				{
					result = 0;
				}
				return result;
			}

			// Token: 0x040001AF RID: 431
			private readonly int radius;

			// Token: 0x040001B0 RID: 432
			private readonly SquadAttackGrid grid;

			// Token: 0x040001B1 RID: 433
			private readonly PathFinderCostTuning tuning;
		}

		// Token: 0x02000077 RID: 119
		private class DangerLineOfSightPainter
		{
			// Token: 0x0600033E RID: 830 RVA: 0x0003F3A5 File Offset: 0x0003D5A5
			public DangerLineOfSightPainter()
			{
				this.skipThenVisitFunc = new Action<IntVec3>(this.SkipThenVisit);
			}

			// Token: 0x0600033F RID: 831 RVA: 0x0003F3C4 File Offset: 0x0003D5C4
			private void SkipThenVisit(IntVec3 cell)
			{
				bool flag = this.skipCount <= 0;
				if (flag)
				{
					this.visitor(cell);
				}
				this.skipCount--;
			}

			// Token: 0x06000340 RID: 832 RVA: 0x0003F400 File Offset: 0x0003D600
			public void PaintLoS(Map map, IntVec3 start, IntVec3 end, Action<IntVec3> visitor)
			{
				bool flag = !start.InBounds(map) || !end.InBounds(map);
				if (!flag)
				{
					this.visitor = visitor;
					this.skipCount = Mathf.FloorToInt(5f);
					GenSight.PointsOnLineOfSight(start, end, this.skipThenVisitFunc);
				}
			}

			// Token: 0x040001B2 RID: 434
			private Action<IntVec3> visitor;

			// Token: 0x040001B3 RID: 435
			private int skipCount;

			// Token: 0x040001B4 RID: 436
			private Action<IntVec3> skipThenVisitFunc;
		}

		// Token: 0x02000078 RID: 120
		private class WalkReachabilityPainter
		{
			// Token: 0x06000341 RID: 833 RVA: 0x0003F451 File Offset: 0x0003D651
			public WalkReachabilityPainter()
			{
				this.floodFillPassCheckFunc = new Predicate<IntVec3>(this.FloodFillPassCheck);
				this.floodFillProcessorFunc = new Func<IntVec3, int, bool>(this.FloodFillProcessor);
			}

			// Token: 0x06000342 RID: 834 RVA: 0x0003F47F File Offset: 0x0003D67F
			public void PaintWalkReachability(SquadAttackGrid squadAttackGrid)
			{
				this.squadAttackGrid = squadAttackGrid;
				squadAttackGrid.map.floodFiller.FloodFill(this.squadAttackGrid.start, this.floodFillPassCheckFunc, this.floodFillProcessorFunc, int.MaxValue, false, null);
			}

			// Token: 0x06000343 RID: 835 RVA: 0x0003F4B8 File Offset: 0x0003D6B8
			private bool FloodFillProcessor(IntVec3 c, int traversalDist)
			{
				this.squadAttackGrid.reachableGrid[c] = true;
				return false;
			}

			// Token: 0x06000344 RID: 836 RVA: 0x0003F4E0 File Offset: 0x0003D6E0
			private bool FloodFillPassCheck(IntVec3 c)
			{
				return this.squadAttackGrid.WalkGrid[c] && !SquadAttackUtility.BuildingBlocksPath(this.squadAttackGrid.map, c);
			}

			// Token: 0x040001B5 RID: 437
			private SquadAttackGrid squadAttackGrid;

			// Token: 0x040001B6 RID: 438
			private Predicate<IntVec3> floodFillPassCheckFunc;

			// Token: 0x040001B7 RID: 439
			private Func<IntVec3, int, bool> floodFillProcessorFunc;
		}
	}
}
