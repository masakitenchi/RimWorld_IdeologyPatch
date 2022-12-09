using System.Linq;
using Verse;

namespace Rimatomics
{
	public class SectionLayer_ThingsPipe : SectionLayer
	{
		public SectionLayer_ThingsPipe(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Buildings;
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			foreach (IntVec3 item in section.CellRect)
			{
				foreach (Building_Pipe item2 in base.Map.thingGrid.ThingsListAt(item).OfType<Building_Pipe>())
				{
					item2.PrintForGrid(this);
				}
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
