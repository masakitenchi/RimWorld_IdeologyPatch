using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_HaulRadioactive : WorkGiver_Haul
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return DubDef.RadioactiveThings(pawn.Map);
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return DubDef.RadioactiveThings(pawn.Map).Count() == 0;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
			{
				return null;
			}
			return HaulAIUtility.HaulToStorageJob(pawn, t);
		}
	}
}
