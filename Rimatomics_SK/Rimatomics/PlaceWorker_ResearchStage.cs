using System.Linq;
using Verse;

namespace Rimatomics
{
	public class PlaceWorker_ResearchStage : PlaceWorker
	{
		public bool GetBuild(ThingDef checkingDef, Map map)
		{
			return map.listerThings.ThingsOfDef(checkingDef).Any();
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			if (DebugSettings.godMode)
			{
				return true;
			}
			RimatomicsThingDef rimatomicsThingDef = checkingDef as RimatomicsThingDef;
			if (!rimatomicsThingDef.StepsThatUnlock.FirstOrDefault().GetParentProject().IsFinished && (GetBuild(checkingDef.blueprintDef, map) || GetBuild(rimatomicsThingDef, map)))
			{
				return new AcceptanceReport("StillTestingDef".Translate());
			}
			return true;
		}
	}
}
