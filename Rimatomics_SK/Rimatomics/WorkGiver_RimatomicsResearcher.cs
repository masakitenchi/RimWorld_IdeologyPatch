using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_RimatomicsResearcher : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.NuclearResearchBench);

		public override bool Prioritized => true;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_RimatomicsResearchBench building_RimatomicsResearchBench = t as Building_RimatomicsResearchBench;
			RimatomicResearchDef currentProj = building_RimatomicsResearchBench.currentProj;
			if (currentProj == null)
			{
				return false;
			}
			if (!currentProj.CurrentStep.requiredResearchFacilities.NullOrEmpty())
			{
				return false;
			}
			if (currentProj.CurrentStep.WorkType != def.workType)
			{
				return false;
			}
			int skillLevel = currentProj.CurrentStep.RequiredSkillLevel;
			if (skillLevel > 0)
			{
				foreach (SkillDef relevantSkill in def.workType.relevantSkills)
				{
					float skill = pawn.GetSkill(relevantSkill);
					if (skill < (float)skillLevel)
					{
						JobFailReason.Is("SkillTooLow".Translate(relevantSkill.label, skill, skillLevel));
						return false;
					}
				}
			}
			if (currentProj.CurrentStep.CanBeResearchedAt(building_RimatomicsResearchBench, ignoreResearchBenchPowerStatus: false))
			{
				return pawn.CanReserve(t, 1, -1, null, forced);
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(DubDef.RimatomicsResearch, t);
		}

		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor);
		}
	}
}
