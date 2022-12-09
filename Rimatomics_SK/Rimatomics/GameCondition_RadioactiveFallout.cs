using RimWorld;
using Verse;

namespace Rimatomics
{
	public class GameCondition_RadioactiveFallout : GameCondition
	{
		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		public override void DoCellSteadyEffects(IntVec3 c, Map map)
		{
			if (c.UsesOutdoorTemperature(map))
			{
				Pawn firstPawn = c.GetFirstPawn(map);
				if (firstPawn != null && firstPawn.def.race.IsFlesh)
				{
					DubUtils.applyRads(firstPawn, 5f);
				}
			}
		}

		public override bool AllowEnjoyableOutsideNow(Map map)
		{
			return false;
		}
	}
}
