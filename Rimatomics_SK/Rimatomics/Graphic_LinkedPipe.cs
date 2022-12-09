using System.Linq;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Graphic_LinkedPipe : Graphic_Linked
	{
		public PipeType mode;

		public Graphic_LinkedPipe()
		{
		}

		public Graphic_LinkedPipe(Graphic subGraphic, PipeType m)
			: base(subGraphic)
		{
			base.subGraphic = subGraphic;
			mode = m;
		}

		public Graphic_LinkedPipe(Graphic subGraphic)
			: base(subGraphic)
		{
			base.subGraphic = subGraphic;
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return new Graphic_LinkedPipe(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), mode)
			{
				data = data
			};
		}

		public void PrintForGrid(SectionLayer layer, Thing thing)
		{
			IntVec3 position = thing.Position;
			if (RimatomicsMod.Settings.PipeVisibility == 0 || (RimatomicsMod.Settings.PipeVisibility == 1 && position.GetTerrain(thing.Map).layerable))
			{
				return;
			}
			Print(layer, thing, 0f);
			if (RimatomicsMod.Settings.PipeVisibility == 3)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = position + GenAdj.CardinalDirections[i];
				if (ShouldDrawTo(intVec, thing))
				{
					Material mat = LinkedDrawMatFrom(thing, intVec);
					Printer_Plane.PrintPlane(layer, intVec.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, mat);
				}
			}
		}

		public bool ShouldDrawTo(IntVec3 c, Thing parent)
		{
			if (!c.InBounds(parent.Map))
			{
				return false;
			}
			if (RimatomicsMod.Settings.PipeVisibility == 1 && c.GetTerrain(parent.Map).layerable)
			{
				return false;
			}
			if (RimatomicsMod.Settings.PipeVisibility == 3 && !c.GetThingList(parent.Map).OfType<Building_Pipe>().Any((Building_Pipe x) => x.pipe.mode == mode))
			{
				return false;
			}
			if (c.GetThingList(parent.Map).OfType<Building_Pipe>().Any((Building_Pipe x) => x.pipe.mode == mode))
			{
				return false;
			}
			return parent.Map.Rimatomics().ZoneAt(c, mode);
		}

		public override bool ShouldLinkWith(IntVec3 c, Thing parent)
		{
			if (!c.InBounds(parent.Map))
			{
				return false;
			}
			if (RimatomicsMod.Settings.PipeVisibility == 1 && c.GetTerrain(parent.Map).layerable)
			{
				return false;
			}
			if (RimatomicsMod.Settings.PipeVisibility == 3 && !c.GetThingList(parent.Map).OfType<Building_Pipe>().Any((Building_Pipe x) => x.pipe.mode == mode))
			{
				return false;
			}
			return parent.Map.Rimatomics().ZoneAt(c, mode);
		}
	}
}
