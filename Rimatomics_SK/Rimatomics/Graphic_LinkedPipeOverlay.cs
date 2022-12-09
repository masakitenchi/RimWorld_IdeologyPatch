using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Graphic_LinkedPipeOverlay : Graphic_Linked
	{
		public PipeType mode;

		public Graphic_LinkedPipeOverlay()
		{
		}

		public Graphic_LinkedPipeOverlay(Graphic subGraphic)
			: base(subGraphic)
		{
		}

		public Graphic_LinkedPipeOverlay(Graphic subGraphic, PipeType m)
			: base(subGraphic)
		{
			mode = m;
		}

		public override bool ShouldLinkWith(IntVec3 c, Thing parent)
		{
			if (c.InBounds(parent.Map))
			{
				return parent.Map.Rimatomics().ZoneAt(c, mode);
			}
			return false;
		}

		public override void Print(SectionLayer layer, Thing parent, float rot)
		{
			foreach (IntVec3 item in parent.OccupiedRect())
			{
				Vector3 center = item.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
				Printer_Plane.PrintPlane(layer, center, Vector2.one, LinkedDrawMatFrom(parent, item));
			}
		}
	}
}
