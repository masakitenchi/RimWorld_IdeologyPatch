using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_LoadSilo : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.MissileSilo);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is MissileSilo missileSilo))
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
			if (missileSilo.magazine >= missileSilo.magazineCap)
			{
				JobFailReason.Is("SiloFull".Translate());
				return false;
			}
			int count;
			ThingDef thingDef = missileSilo.NextPart(out count);
			if (thingDef == null)
			{
				JobFailReason.Is("SiloFull".Translate());
				return false;
			}
			if (FindPart(pawn, thingDef) == null)
			{
				JobFailReason.Is("SiloMissingPart".Translate(thingDef.label));
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is MissileSilo missileSilo))
			{
				return null;
			}
			int count;
			Thing thing = FindPart(pawn, missileSilo.NextPart(out count));
			if (thing == null)
			{
				return null;
			}
			return new Job(DubDef.LoadSilo, t, thing)
			{
				count = count
			};
		}

		private Thing FindPart(Pawn pawn, ThingDef def)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(def), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
		}
	}
}
