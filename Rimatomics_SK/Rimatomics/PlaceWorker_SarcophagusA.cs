using Verse;

namespace Rimatomics
{
	public class PlaceWorker_SarcophagusA : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing t = null)
		{
			Building firstBuilding = loc.GetFirstBuilding(map);
			if (firstBuilding != null && firstBuilding.def == ThingDef.Named("PoppedReactorCoreA") && firstBuilding.Position == loc)
			{
				return true;
			}
			return "critPlaceOnCore".Translate();
		}
	}
}
