using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	internal class PlaceWorker_StoragePool : PlaceWorker
	{
		public static Graphic bay;

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing t = null)
		{
			base.DrawGhost(def, center, rot, ghostCol);
			if (bay == null)
			{
				bay = GraphicDatabase.Get(typeof(Graphic_Single), "Rimatomics/Things/RimatomicsBuildings/loadingBay", ShaderDatabase.CutoutComplex, def.graphicData.drawSize, Color.white, Color.white);
			}
			Graphic graphic = GhostUtility.GhostGraphicFor(bay, def, ghostCol);
			Vector3 loc = GenThing.TrueCenter(center, rot, def.Size, AltitudeLayer.Blueprint.AltitudeFor());
			graphic.DrawFromDef(loc, rot, def);
		}
	}
}
