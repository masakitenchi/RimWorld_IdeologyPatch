using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_UnloadPlutoniumProc : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.PlutoniumProcessor);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is Building_PlutoniumProc building_PlutoniumProc && building_PlutoniumProc.Fermented && !t.IsBurning() && !t.IsForbidden(pawn))
			{
				return pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced);
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(DefDatabase<JobDef>.GetNamed("UnloadPlutonium"), t);
		}
	}
}
