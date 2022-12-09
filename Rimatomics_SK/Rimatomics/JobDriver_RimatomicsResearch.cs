using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_RimatomicsResearch : JobDriver
	{
		private const int JobEndInterval = 4000;

		private RimatomicResearchDef Proj => Bench.currentProj;

		private Building_RimatomicsResearchBench Bench => (Building_RimatomicsResearchBench)base.TargetThingA;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			ResearchStepDef step = Proj.CurrentStep;
			Toil research = new Toil();
			research.tickAction = delegate
			{
				Pawn actor = research.actor;
				if (step.WorkType == DubDef.Research)
				{
					float statValue = actor.GetStatValue(StatDefOf.ResearchSpeed);
					statValue *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
					Bench.ResearchPerformed(statValue, actor, Bench);
					actor.skills?.GetSkill(SkillDefOf.Intellectual).Learn(0.11f);
				}
				else if (step.WorkType == DubDef.Crafting)
				{
					float statValue2 = actor.GetStatValue(DubDef.CraftingResearchSpeed);
					statValue2 *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
					Bench.ResearchPerformed(statValue2, actor, Bench);
					actor.skills?.GetSkill(SkillDefOf.Crafting).Learn(0.11f);
				}
				else if (step.WorkType == DubDef.Construction)
				{
					float statValue3 = actor.GetStatValue(DubDef.ConstructionResearchSpeed);
					statValue3 *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
					Bench.ResearchPerformed(statValue3, actor, Bench);
					actor.skills?.Learn(SkillDefOf.Construction, 0.11f);
				}
				actor.GainComfortFromCellIfPossible();
			};
			research.FailOn(() => Proj == null);
			research.FailOn(() => step.WorkType != Proj.CurrentStep.WorkType);
			research.FailOn(() => !step.CanBeResearchedAt(Bench, ignoreResearchBenchPowerStatus: false));
			research.FailOn(() => !Proj.CurrentStep.requiredResearchFacilities.NullOrEmpty());
			research.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			research.WithEffect(DubDef.RimatomicsResearchEffect, TargetIndex.A);
			research.WithProgressBar(TargetIndex.A, () => Bench.Research.GetProgressPct(step));
			research.defaultCompleteMode = ToilCompleteMode.Delay;
			research.defaultDuration = 4000;
			yield return research;
			yield return Toils_General.Wait(2);
		}
	}
}
