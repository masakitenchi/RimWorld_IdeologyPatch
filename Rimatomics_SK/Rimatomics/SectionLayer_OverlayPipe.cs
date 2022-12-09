using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	internal abstract class SectionLayer_OverlayPipe : SectionLayer
	{
		public PipeType mode;

		public SectionLayer_OverlayPipe(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Buildings;
		}

		public override void DrawLayer()
		{
			if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator_Build && designator_Build.PlacingDef is ThingDef thingDef && thingDef.comps.OfType<CompProperties_Pipe>().Any((CompProperties_Pipe x) => x.mode == mode))
			{
				base.DrawLayer();
			}
			if (Find.DesignatorManager.SelectedDesignator is Designator_RemovePipe designator_RemovePipe && mode == designator_RemovePipe.RemovalMode)
			{
				base.DrawLayer();
			}
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			foreach (IntVec3 item in section.CellRect)
			{
				foreach (ThingWithComps item2 in item.GetThingList(base.Map).OfType<ThingWithComps>())
				{
					foreach (CompPipe item3 in item2.AllComps.OfType<CompPipe>())
					{
						if (item3.mode == mode && item2.Position.x == item.x && item2.Position.z == item.z)
						{
							item3.PrintForGrid(this);
						}
					}
				}
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
