using Verse;

namespace Rimatomics
{
	public class SpecialThingFilterWorker_NuclearFuelNew : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (t is Item_NuclearFuel item_NuclearFuel)
			{
				return !(item_NuclearFuel.FuelLevel < 1f);
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (def is RimatomicsThingDef rimatomicsThingDef)
			{
				return rimatomicsThingDef.nuclearFuel != null;
			}
			return false;
		}
	}
}
