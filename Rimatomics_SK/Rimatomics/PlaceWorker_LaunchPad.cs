using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class PlaceWorker_LaunchPad : PlaceWorker
	{
		private static readonly Material FuelingPortCellMaterial;

		static PlaceWorker_LaunchPad()
		{
			FuelingPortCellMaterial = MaterialPool.MatFrom("UI/Overlays/FuelingPort", ShaderDatabase.Transparent);
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing t = null)
		{
			Map currentMap = Find.CurrentMap;
			if (def.building != null && def.building.hasFuelingPort && center.Standable(currentMap))
			{
				DrawFuelingPortCell(center, rot);
			}
		}

		public static void DrawFuelingPortCell(IntVec3 center, Rot4 rot)
		{
			Vector3 position = center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, FuelingPortCellMaterial, 0);
		}
	}
}
