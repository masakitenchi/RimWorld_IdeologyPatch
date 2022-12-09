using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_LoadFuelModule : WorkGiver_Scanner
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
			if (!reactorCore2.BreederHotLoad && !reactorCore2.coldAndDead)
			{
				JobFailReason.Is("critActiveCore".Translate());
				return false;
			}
			bool flag = reactorCore2.SlotDesignations.Any((RodDesignate d) => d == RodDesignate.Fuel);
			bool flag2 = reactorCore2.SlotDesignations.Any((RodDesignate d) => d == RodDesignate.MOX);
			if (!flag && !flag2)
			{
				JobFailReason.Is("critNoSlotDes".Translate());
				return false;
			}
			Thing thing = null;
			if (flag)
			{
				thing = FindAssembly(pawn, reactorCore2, MOX: false);
			}
			if (thing == null && flag2)
			{
				thing = FindAssembly(pawn, reactorCore2, MOX: true);
			}
			if (thing == null)
			{
				JobFailReason.Is("critnoFuelRods".Translate());
				return false;
			}
			return !t.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			reactorCore reactorCore2 = t as reactorCore;
			bool num = reactorCore2.SlotDesignations.Any((RodDesignate d) => d == RodDesignate.Fuel);
			bool flag = reactorCore2.SlotDesignations.Any((RodDesignate d) => d == RodDesignate.MOX);
			Thing thing = null;
			if (num)
			{
				thing = FindAssembly(pawn, reactorCore2, MOX: false);
			}
			if (thing == null && flag)
			{
				thing = FindAssembly(pawn, reactorCore2, MOX: true);
			}
			if (thing == null)
			{
				return null;
			}
			return new Job(DefDatabase<JobDef>.GetNamed("HaulModuletoCore"), t, thing)
			{
				count = 1
			};
		}

		private Thing FindAssembly(Pawn pawn, reactorCore core, bool MOX)
		{
			StorageSettings settings = core.GetStoreSettings();
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Predicate, DubDef.NuclearFuel(pawn.Map));
			bool Predicate(Thing x)
			{
				if (!x.IsForbidden(pawn) && pawn.CanReserve(x) && settings.AllowedToAccept(x) && x is Item_NuclearFuel item_NuclearFuel && item_NuclearFuel.MOX == MOX)
				{
					IFuelFilter fuelFilter = core;
					if (fuelFilter != null)
					{
						return fuelFilter.FuelLifeFilter.Includes(item_NuclearFuel.FuelLevel);
					}
				}
				return false;
			}
		}
	}
}
