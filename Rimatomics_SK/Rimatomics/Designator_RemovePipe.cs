using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Designator_RemovePipe : Designator
	{
		public PipeType RemovalMode = PipeType.ColdWater;

		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;

		public Designator_RemovePipe()
		{
			defaultLabel = "DesignatorRemoveRimatomicsPipes".Translate();
			defaultDesc = "DesignatorRemoveRimatomicsPipesDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/RemovePipes");
			useMouseIcon = true;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			soundSucceeded = SoundDefOf.Designate_SmoothSurface;
			hotKey = KeyBindingDefOf.Misc1;
		}

		public override void ProcessInput(Event ev)
		{
			if (CheckCanInteract())
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("DesignatorRemoveColdWater".Translate(), delegate
				{
					RemovalMode = PipeType.ColdWater;
					base.ProcessInput(ev);
				}, MenuOptionPriority.High));
				list.Add(new FloatMenuOption("DesignatorRemoveCooling".Translate(), delegate
				{
					RemovalMode = PipeType.Cooling;
					base.ProcessInput(ev);
				}, MenuOptionPriority.High));
				list.Add(new FloatMenuOption("DesignatorRemoveHighVoltage".Translate(), delegate
				{
					RemovalMode = PipeType.HighVoltage;
					base.ProcessInput(ev);
				}, MenuOptionPriority.High));
				list.Add(new FloatMenuOption("DesignatorRemoveLoom".Translate(), delegate
				{
					RemovalMode = PipeType.Loom;
					base.ProcessInput(ev);
				}, MenuOptionPriority.High));
				list.Add(new FloatMenuOption("DesignatorRemoveSteam".Translate(), delegate
				{
					RemovalMode = PipeType.Steam;
					base.ProcessInput(ev);
				}, MenuOptionPriority.High));
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!DebugSettings.godMode && c.Fogged(base.Map))
			{
				return false;
			}
			if (TopDeconstructibleInCell(c) == null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			DesignateThing(TopDeconstructibleInCell(loc));
		}

		private Thing TopDeconstructibleInCell(IntVec3 loc)
		{
			foreach (Thing item in from t in base.Map.thingGrid.ThingsAt(loc)
				orderby t.def.altitudeLayer descending
				select t)
			{
				if (CanDesignateThing(item).Accepted)
				{
					return item;
				}
			}
			return null;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (!(t is Building building))
			{
				return false;
			}
			if (building.def.category != ThingCategory.Building)
			{
				return false;
			}
			if (!DebugSettings.godMode && building.Faction != Faction.OfPlayer)
			{
				if (building.Faction != null)
				{
					return false;
				}
				if (!building.ClaimableBy(Faction.OfPlayer))
				{
					return false;
				}
			}
			if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
			{
				return false;
			}
			if (t is Building_Pipe building_Pipe && building_Pipe.pipe.mode == RemovalMode)
			{
				return true;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
