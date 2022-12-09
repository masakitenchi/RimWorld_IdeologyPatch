using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class PlaceWorker_NeedsLaunchPad : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			Thing firstThing = center.GetFirstThing(map, ThingDef.Named("SCUDLauncher"));
			if (firstThing != null && firstThing.Position == center)
			{
				return true;
			}
			return "MustBePlacedOnLaunchPad".Translate();
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing t = null)
		{
			Map currentMap = Find.CurrentMap;
			foreach (Building item in currentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("SCUDLauncher")))
			{
				if (!Find.Selector.IsSelected(item) && item.Position.Standable(currentMap))
				{
					PlaceWorker_FuelingPort.DrawFuelingPortCell(item.Position, item.Rotation);
				}
			}
		}
	}
}
