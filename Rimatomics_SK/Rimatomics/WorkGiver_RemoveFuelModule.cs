using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_RemoveFuelModule : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return DubDef.AllReactors(pawn.Map);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is reactorCore reactorCore2))
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (!reactorCore2.SlotDesignations.Any((RodDesignate d) => d == RodDesignate.Remove))
			{
				return false;
			}
			if (!reactorCore2.BreederHotLoad && !reactorCore2.coldAndDead)
			{
				JobFailReason.Is("critActiveCore".Translate());
				return false;
			}
			return !t.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(DefDatabase<JobDef>.GetNamed("RemoveFuelModule"), t, null);
		}
	}
}
