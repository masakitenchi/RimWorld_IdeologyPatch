using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_SuperviseResearch : WorkGiver_Scanner
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.Rimatomics().Facilities;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompResearchFacility compResearchFacility = t.TryGetComp<CompResearchFacility>();
			Building_RimatomicsResearchBench building_RimatomicsResearchBench = DubUtils.GetResearch().ActiveBenches.FirstOrDefault((Building_RimatomicsResearchBench x) => x.currentProj != null && x.currentProj.CurrentStep.UsesFacility(t.def) && x.currentProj.CurrentStep.WorkType == def.workType && x.powerComp.PowerOn);
			if (building_RimatomicsResearchBench == null)
			{
				return false;
			}
			int skillLevel = building_RimatomicsResearchBench.currentProj.CurrentStep.RequiredSkillLevel;
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
			if (!compResearchFacility.IsSafe)
			{
				return false;
			}
			if (!compResearchFacility.powerComp.PowerOn)
			{
				JobFailReason.Is("FacilityNoPower".Translate());
				return false;
			}
			if (building_RimatomicsResearchBench.currentProj.CurrentStep.StandMode == BuildStandingMode.Inside)
			{
				if (!t.OccupiedRect().Cells.Any((IntVec3 c) => pawn.CanReserve(c) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly)))
				{
					JobFailReason.Is("NoStandableCell".Translate());
					return false;
				}
			}
			else if (building_RimatomicsResearchBench.currentProj.CurrentStep.StandMode == BuildStandingMode.Outside && !GenAdjFast.AdjacentCells8Way(t).Any((IntVec3 c) => c.Standable(pawn.Map) && pawn.CanReserve(c) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly)))
			{
				JobFailReason.Is("NoStandableCell".Translate());
				return false;
			}
			return pawn.CanReserve(t, 2, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building building = DubUtils.GetResearch().ActiveBenches.FirstOrDefault((Building_RimatomicsResearchBench x) => x.currentProj != null && x.currentProj.CurrentStep.UsesFacility(t.def) && x.currentProj.CurrentStep.WorkType == def.workType && x.powerComp.PowerOn);
			if (def.workType == WorkTypeDefOf.Construction)
			{
				return new Job(DubDef.SuperviseConstruction, t, building);
			}
			return new Job(DubDef.SuperviseResearch, t, building);
		}
	}
}
