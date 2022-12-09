using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class PlaceWorker_WaterStation : PlaceWorker
	{
		private List<IntVec3> radialWater = new List<IntVec3>();

		private List<IntVec3> water = new List<IntVec3>();

		private IntVec3 currentCell;

		public bool IsWatah(IntVec3 intVec)
		{
			TerrainDef terrain = intVec.GetTerrain(Find.CurrentMap);
			if (terrain != null && terrain.edgeType == TerrainDef.TerrainEdgeType.Water)
			{
				return true;
			}
			return false;
		}

		public void FloodIt(IntVec3 center, Rot4 rot)
		{
			IntVec3 intVec = center + rot.FacingCell * 3;
			if (intVec != currentCell && intVec.InBounds(Find.CurrentMap) && IsWatah(intVec))
			{
				water = new List<IntVec3>();
				currentCell = intVec;
				radialWater = (from x in GenRadial.RadialCellsAround(intVec, 15f, useCenter: true)
					where x.InBounds(Find.CurrentMap)
					select x).ToList();
				Predicate<IntVec3> passCheck = (IntVec3 c) => radialWater.Contains(c) && IsWatah(c);
				Action<IntVec3> processor = delegate(IntVec3 c)
				{
					water.Add(c);
				};
				Find.CurrentMap.floodFiller.FloodFill(intVec, passCheck, processor);
			}
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing t = null)
		{
			FloodIt(center, rot);
			if (!water.NullOrEmpty())
			{
				GenDraw.DrawFieldEdges(water, GenTemperature.ColorSpotCold);
				return;
			}
			GenDraw.DrawFieldEdges(new List<IntVec3> { center + rot.FacingCell * 3 }, GenTemperature.ColorSpotCold);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			FloodIt(center, rot);
			if (water.NullOrEmpty())
			{
				return "NeedsRiver".Translate();
			}
			if (water.Count < 100)
			{
				return "critMoreWater".Translate();
			}
			return true;
		}
	}
}
