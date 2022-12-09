using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	internal class PlaceWorker_Pipe : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			ThingDef thingDef = def as ThingDef;
			CompProperties_Pipe pipe = thingDef.GetCompProperties<CompProperties_Pipe>();
			if (map.Rimatomics().ZoneAt(loc, pipe.mode))
			{
				return false;
			}
			List<Thing> thingList = loc.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def != null && thingList[i].def.comps.OfType<CompProperties_Pipe>().Any((CompProperties_Pipe x) => x.mode == pipe.mode))
				{
					return false;
				}
				if (thingList[i].def != null && thingList[i].def.entityDefToBuild != null && thingList[i].def.entityDefToBuild is ThingDef thingDef2 && thingDef != thingDef2 && thingDef2.comps.OfType<CompProperties_Pipe>().Any((CompProperties_Pipe x) => x.mode == pipe.mode))
				{
					return false;
				}
			}
			return true;
		}
	}
}
