using Verse;

namespace Rimatomics
{
	public class SpecialThingFilterWorker_NuclearFuelCracked : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (t is Item_NuclearFuel item_NuclearFuel)
			{
				return item_NuclearFuel.cracked;
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
