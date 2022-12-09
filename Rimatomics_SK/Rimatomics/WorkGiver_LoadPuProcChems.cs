using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_LoadPuProcChems : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.PlutoniumProcessor);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Building_PlutoniumProc building_PlutoniumProc) || building_PlutoniumProc.IsFull)
			{
				return false;
			}
			if (!forced && !building_PlutoniumProc.ShouldAutoRefuelNow)
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (t is ThingWithComps thingWithComps)
			{
				CompFlickable comp = thingWithComps.GetComp<CompFlickable>();
				if (comp != null && !comp.SwitchIsOn)
				{
					return false;
				}
			}
			if (FindBestFuel(pawn) == null)
			{
				JobFailReason.Is("NoChemfuel".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_PlutoniumProc building_PlutoniumProc = t as Building_PlutoniumProc;
			Thing thing = FindBestFuel(pawn);
			if (thing == null)
			{
				JobFailReason.Is("NoChemfuel".Translate());
				return null;
			}
			return new Job(DefDatabase<JobDef>.GetNamed("LoadSpentFuel"), t, thing)
			{
				count = building_PlutoniumProc.GetFuelCountToFullyRefuel()
			};
		}

		private Thing FindBestFuel(Pawn pawn)
		{
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDef.Named("Chemfuel")), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
			bool Validator(Thing x)
			{
				if (!x.IsForbidden(pawn))
				{
					return pawn.CanReserve(x);
				}
				return false;
			}
		}
	}
}
