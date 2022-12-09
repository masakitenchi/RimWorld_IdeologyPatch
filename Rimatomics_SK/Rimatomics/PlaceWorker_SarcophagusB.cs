using Verse;

namespace Rimatomics
{
	public class PlaceWorker_SarcophagusB : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			Building firstBuilding = loc.GetFirstBuilding(map);
			if (firstBuilding != null && firstBuilding.def == ThingDef.Named("PoppedReactorCoreB") && firstBuilding.Position == loc)
			{
				return true;
			}
			return "critPlaceOnCore".Translate();
		}
	}
}
