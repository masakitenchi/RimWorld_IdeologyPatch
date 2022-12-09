using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000012 RID: 18
	public static class SquadAttackUtility
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00006AC0 File Offset: 0x00004CC0
		public static int CountReachableAdjacentCells(SquadAttackGrid grid, Building b)
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = b.Position + GenAdj.CardinalDirections[i];
				bool flag = c.InBounds(grid.Map) && grid.ReachableGrid[c];
				if (flag)
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00006B2C File Offset: 0x00004D2C
		public static bool ShouldAttackBuilding(SquadAttackData data, Thing thing, Faction sourceFaction)
		{
			Building building = thing as Building;
			bool flag = building != null;
			if (flag)
			{
				bool flag2 = data.dest.DistanceTo(building.Position) > data.dest.DistanceTo(data.start);
				if (flag2)
				{
					return false;
				}
				bool flag3 = !TrashUtility.ShouldTrashBuilding(building) || !PathFinder.IsDestroyable(building) || !building.def.IsEdifice() || building.def.IsFrame || (!building.def.mineable && !building.Faction.HostileTo(sourceFaction) && (building.Faction != null || !SquadAttackUtility.BuildingBlocksPath(building.Map, building.Position)));
				if (flag3)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00006BF8 File Offset: 0x00004DF8
		public static bool BuildingBlocksPath(Map map, IntVec3 c)
		{
			Building edifice = c.GetEdifice(map);
			return (edifice == null || !edifice.def.building.ai_neverTrashThis) && (c.Impassable(map) || c.GetDoor(map) != null);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00006C40 File Offset: 0x00004E40
		public static bool IsWorthAttackBuilding(SquadAttackData data, SquadAttackGrid grid, Building b, Faction sourceFaction)
		{
			bool flag = !SquadAttackUtility.ShouldAttackBuilding(data, b, sourceFaction);
			bool result;
			if (flag)
			{
				bool debugAttackGridBuildingTargets = SkyAiCore.Settings.debugAttackGridBuildingTargets;
				if (debugAttackGridBuildingTargets)
				{
					Log.Message(string.Format("Return false: {0} {1}. Bcs of ShouldAttackBuilding false.", b, b.Position));
				}
				result = false;
			}
			else
			{
				bool flag2 = !SquadAttackUtility.AnyAdjacentImpassibleOnPath(b.Map, grid.AttackGrid, b.Position);
				if (flag2)
				{
					bool debugAttackGridBuildingTargets2 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
					if (debugAttackGridBuildingTargets2)
					{
						Log.Message(string.Format("Return false: {0} {1}. Bcs of AnyAdjacentImpassibleOnPath false.", b, b.Position));
					}
					result = false;
				}
				else
				{
					int num = SquadAttackUtility.CountReachableAdjacentCells(grid, b);
					bool flag3 = num <= 0;
					if (flag3)
					{
						bool debugAttackGridBuildingTargets3 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
						if (debugAttackGridBuildingTargets3)
						{
							Log.Message(string.Format("Return false: {0} {1}. Bcs of CountReachableAdjacentCells false. num: {2}", b, b.Position, num));
						}
						result = false;
					}
					else
					{
						float num3;
						int num2 = SquadAttackUtility.PathNodesCountFromTheSidesOfBuilding(data, grid, b, sourceFaction, out num3);
						int num4 = (b.Map.IsPlayerHome && b.Map.areaManager.Home[b.Position]) ? Mathf.RoundToInt(num3 * 3f) : Mathf.RoundToInt(num3 * 6f);
						bool flag4 = num2 <= num4;
						if (flag4)
						{
							bool debugAttackGridBuildingTargets4 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
							if (debugAttackGridBuildingTargets4)
							{
								Log.Message(string.Format("Return false: {0} {1}. Bcs of PathNodesCountFromTheSidesOfBuilding false. num2: {2}/{3}", new object[]
								{
									b,
									b.Position,
									num2,
									num4
								}));
							}
							result = false;
						}
						else
						{
							bool debugAttackGridBuildingTargets5 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
							if (debugAttackGridBuildingTargets5)
							{
								Log.Message(string.Format("Added building: {0} {1}. Result true.", b, b.Position));
							}
							result = true;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00006E2C File Offset: 0x0000502C
		public static bool ImpassableBuilding(IntVec3 c, Map map, Faction f)
		{
			Building firstBuilding = c.GetFirstBuilding(map);
			bool flag = firstBuilding != null && firstBuilding != null;
			if (flag)
			{
				bool flag2 = firstBuilding.def.passability == Traversability.Impassable;
				if (flag2)
				{
					return true;
				}
				bool flag3 = firstBuilding.def.IsDoor && firstBuilding.Faction != null && firstBuilding.Faction.HostileTo(f);
				if (flag3)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00006EA0 File Offset: 0x000050A0
		public static List<IntVec3> OppositeSideCells(Building building, IntVec3 closestCell, int count)
		{
			List<IntVec3> list = new List<IntVec3>();
			Vector3 vector = (building.Position - closestCell).ToVector3();
			bool flag = vector.z > 0f;
			List<IntVec3> result;
			if (flag)
			{
				for (int i = 1; i <= count; i++)
				{
					list.Add(new IntVec3(building.Position.x, 0, building.Position.z + i));
				}
				result = list;
			}
			else
			{
				bool flag2 = vector.z < 0f;
				if (flag2)
				{
					for (int j = 1; j <= count; j++)
					{
						list.Add(new IntVec3(building.Position.x, 0, building.Position.z - j));
					}
					result = list;
				}
				else
				{
					bool flag3 = vector.x > 0f;
					if (flag3)
					{
						for (int k = 1; k <= count; k++)
						{
							list.Add(new IntVec3(building.Position.x + k, 0, building.Position.z));
						}
						result = list;
					}
					else
					{
						bool flag4 = vector.x < 0f;
						if (flag4)
						{
							for (int l = 1; l <= count; l++)
							{
								list.Add(new IntVec3(building.Position.x - l, 0, building.Position.z));
							}
							result = list;
						}
						else
						{
							result = list;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x0000703C File Offset: 0x0000523C
		public static int PathNodesCountFromTheSidesOfBuilding(SquadAttackData data, SquadAttackGrid grid, Building building, Faction sourceFaction, out float dist)
		{
			dist = 0f;
			IEnumerable<IntVec3> source = from c in GenAdjFast.AdjacentCellsCardinal(building.Position)
			where c.InBounds(building.Map) && c != building.Position && base.<PathNodesCountFromTheSidesOfBuilding>g__standable|0(c)
			select c;
			bool flag = source.Count<IntVec3>() <= 0;
			int result;
			if (flag)
			{
				bool debugAttackGridBuildingTargets = SkyAiCore.Settings.debugAttackGridBuildingTargets;
				if (debugAttackGridBuildingTargets)
				{
					Log.Message(string.Format("PathNodesCountFromTheSidesOfBuilding. {0} {1} check: false. data.start: {2} data.dest: {3} standableAdjCells.Count() <= 0", new object[]
					{
						building,
						building.Position,
						data.start,
						data.dest
					}));
				}
				result = 0;
			}
			else
			{
				IntVec3 intVec;
				source.TryMaxBy((IntVec3 c) => c.DistanceTo(data.dest), out intVec);
				List<IntVec3> source2 = SquadAttackUtility.OppositeSideCells(building, intVec, 10);
				IntVec3 intVec2 = (from cell in source2
				where cell.InBounds(building.Map) && base.<PathNodesCountFromTheSidesOfBuilding>g__standable|0(cell)
				select cell).FirstOrDefault<IntVec3>();
				bool flag2 = !intVec2.IsValid || !grid.AttackGrid[intVec2];
				if (flag2)
				{
					bool debugAttackGridBuildingTargets2 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
					if (debugAttackGridBuildingTargets2)
					{
						Log.Message(string.Format("PathNodesCountFromTheSidesOfBuilding. {0} {1} check: false. data.start: {2} data.dest: {3} closestCell: {4} farthestCell: {5} farthestCell cell invalid or not within attack grid.", new object[]
						{
							building,
							building.Position,
							data.start,
							data.dest,
							intVec,
							intVec2
						}));
					}
					result = 0;
				}
				else
				{
					bool flag3 = data.dest.DistanceTo(intVec2) >= data.dest.DistanceTo(intVec);
					if (flag3)
					{
						bool debugAttackGridBuildingTargets3 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
						if (debugAttackGridBuildingTargets3)
						{
							Log.Message(string.Format(" {0} {1} check: false. data.start: {2} data.dest: {3} closestCell: {4} farthestCell: {5} farthestCell more further than closest.", new object[]
							{
								building,
								building.Position,
								data.start,
								data.dest,
								intVec,
								intVec2
							}));
						}
						result = 0;
					}
					else
					{
						TraverseMode traverseMode = TraverseMode.NoPassClosedDoors;
						bool flag4 = !building.Map.reachability.CanReach(intVec, intVec2, PathEndMode.OnCell, traverseMode);
						if (flag4)
						{
							bool debugAttackGridBuildingTargets4 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
							if (debugAttackGridBuildingTargets4)
							{
								Log.Message(string.Format(" {0} {1} check: false. data.start: {2} data.dest: {3} closestCell: {4} farthestCell: {5} canReach failed.", new object[]
								{
									building,
									building.Position,
									data.start,
									data.dest,
									intVec,
									intVec2
								}));
							}
							result = 200;
						}
						else
						{
							PawnPath pawnPath = building.Map.pathFinder.FindPath(intVec, intVec2, TraverseParms.For(traverseMode, Danger.Deadly, false, false, false), PathEndMode.OnCell, null);
							bool flag5 = !pawnPath.Found;
							if (flag5)
							{
								bool debugAttackGridBuildingTargets5 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
								if (debugAttackGridBuildingTargets5)
								{
									Log.Message(string.Format("PathNodesCountFromTheSidesOfBuilding. {0} {1} check: false. data.start: {2} data.dest: {3} closestCell: {4} farthestCell: {5} Path not found.", new object[]
									{
										building,
										building.Position,
										data.start,
										data.dest,
										intVec,
										intVec2
									}));
								}
								result = 200;
							}
							else
							{
								int count = pawnPath.NodesReversed.Count;
								pawnPath.ReleaseToPool();
								bool debugAttackGridBuildingTargets6 = SkyAiCore.Settings.debugAttackGridBuildingTargets;
								if (debugAttackGridBuildingTargets6)
								{
									Log.Message(string.Format("PathNodesCountFromTheSidesOfBuilding. {0} {1} check: {2}. data.start: {3} data.dest: {4} closestCell: {5} farthestCell: {6} nodesCount: {7}", new object[]
									{
										building,
										building.Position,
										count > 0,
										data.start,
										data.dest,
										intVec,
										intVec2,
										count
									}));
								}
								dist = intVec.DistanceTo(intVec2);
								result = count;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000074FC File Offset: 0x000056FC
		private static bool AnyAdjacentImpassibleOnPath(Map map, BoolGrid grid, IntVec3 position)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = position + GenAdj.AdjacentCellsAround[i];
				bool flag = c.InBounds(map) && grid[c] && SquadAttackUtility.BuildingBlocksPath(map, c);
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x0000755C File Offset: 0x0000575C
		public static Verb FindVerbToUseForSiege(Pawn pawn)
		{
			Pawn_EquipmentTracker equipment = pawn.equipment;
			CompEquippable compEquippable = (equipment != null) ? equipment.PrimaryEq : null;
			bool flag = compEquippable == null;
			Verb result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Verb primaryVerb = compEquippable.PrimaryVerb;
				bool flag2 = SquadAttackUtility.UsableVerb(primaryVerb) && primaryVerb.verbProps.ai_IsBuildingDestroyer;
				if (flag2)
				{
					result = primaryVerb;
				}
				else
				{
					List<Verb> allVerbs = compEquippable.AllVerbs;
					for (int i = 0; i < allVerbs.Count; i++)
					{
						Verb verb = allVerbs[i];
						bool flag3 = SquadAttackUtility.UsableVerb(verb) && verb.verbProps.ai_IsBuildingDestroyer;
						if (flag3)
						{
							return verb;
						}
					}
					bool flag4 = SquadAttackUtility.UsableVerb(primaryVerb);
					if (flag4)
					{
						result = primaryVerb;
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00007628 File Offset: 0x00005828
		private static bool UsableVerb(Verb verb)
		{
			return verb != null && verb.Available() && verb.HarmsHealth();
		}
	}
}
