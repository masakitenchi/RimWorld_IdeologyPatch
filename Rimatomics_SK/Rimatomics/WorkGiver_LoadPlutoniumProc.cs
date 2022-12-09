using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_LoadPlutoniumProc : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.PlutoniumProcessor);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Building_PlutoniumProc building_PlutoniumProc) || building_PlutoniumProc.Fermented || building_PlutoniumProc.SpaceLeftForWort <= 0)
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
			if (t.IsBurning())
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
			if (FindAssembly(pawn, building_PlutoniumProc) == null)
			{
				JobFailReason.Is("critnoFuelRods".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing thing = FindAssembly(pawn, t);
			if (thing == null)
			{
				JobFailReason.Is("critnoFuelRods".Translate());
				return null;
			}
			return new Job(DefDatabase<JobDef>.GetNamed("LoadSpentFuel"), t, thing)
			{
				count = 1
			};
		}

		private Thing FindAssembly(Pawn pawn, Thing proc)
		{
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForUndefined(), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Predicate, DubDef.NuclearFuel(pawn.Map));
			bool Predicate(Thing x)
			{
				if (!x.IsForbidden(pawn) && pawn.CanReserve(x) && x is Item_NuclearFuel item_NuclearFuel && item_NuclearFuel.Reprocessable && proc is IFuelFilter fuelFilter)
				{
					return fuelFilter.FuelLifeFilter.Includes(item_NuclearFuel.FuelLevel);
				}
				return false;
			}
		}
	}
}
