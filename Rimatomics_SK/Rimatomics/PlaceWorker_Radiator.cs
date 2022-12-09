using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class PlaceWorker_Radiator : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing t = null)
		{
			Map currentMap = Find.CurrentMap;
			List<IntVec3> list = new List<IntVec3>
			{
				center + IntVec3.South.RotatedBy(rot) + IntVec3.South.RotatedBy(rot),
				center + IntVec3.South.RotatedBy(rot) + IntVec3.South.RotatedBy(rot) + IntVec3.West.RotatedBy(rot),
				center + IntVec3.South.RotatedBy(rot) + IntVec3.South.RotatedBy(rot) + IntVec3.East.RotatedBy(rot)
			};
			List<IntVec3> obj = new List<IntVec3>
			{
				center + IntVec3.North.RotatedBy(rot) + IntVec3.North.RotatedBy(rot),
				center + IntVec3.North.RotatedBy(rot) + IntVec3.North.RotatedBy(rot) + IntVec3.West.RotatedBy(rot),
				center + IntVec3.North.RotatedBy(rot) + IntVec3.North.RotatedBy(rot) + IntVec3.East.RotatedBy(rot)
			};
			GenDraw.DrawFieldEdges(list, GenTemperature.ColorSpotCold);
			GenDraw.DrawFieldEdges(obj, GenTemperature.ColorSpotHot);
			Room room = obj[0].GetRoom(currentMap);
			Room room2 = list[0].GetRoom(currentMap);
			if (room == null || room2 == null)
			{
				return;
			}
			if (room == room2 && !room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), new Color(1f, 0.7f, 0f, 0.5f));
				return;
			}
			if (!room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomHot);
			}
			if (!room2.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room2.Cells.ToList(), GenTemperature.ColorRoomCold);
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			if (c.Impassable(map) || c2.Impassable(map))
			{
				return "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			return true;
		}
	}
}
