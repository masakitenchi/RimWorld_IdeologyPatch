using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_UpgradeBuilding : WorkGiver_Scanner
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.Rimatomics().Upgradables.Where((Thing x) => HasJobOnThing(pawn, x));
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Building building))
			{
				return false;
			}
			CompUpgradable comp = building.GetComp<CompUpgradable>();
			if (comp == null)
			{
				return false;
			}
			if (!comp.NeedsUpgrade)
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (t.IsBrokenDown())
			{
				return false;
			}
			if (!pawn.CanReserve(building, 1, -1, null, forced))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (building.IsBurning())
			{
				return false;
			}
			if (comp.ThingsToInstall().All((ThingDef x) => FindClosestComponent(pawn, x) == null))
			{
				return false;
			}
			return true;
		}

		private Thing FindClosestComponent(Pawn pawn, ThingDef part)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(part), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, validator);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			foreach (ThingDef item in t.TryGetComp<CompUpgradable>().ThingsToInstall())
			{
				Thing thing = FindClosestComponent(pawn, item);
				if (thing != null)
				{
					return new Job(DubDef.UpgradeBuilding, t, thing)
					{
						count = 1
					};
				}
			}
			return null;
		}
	}
}
