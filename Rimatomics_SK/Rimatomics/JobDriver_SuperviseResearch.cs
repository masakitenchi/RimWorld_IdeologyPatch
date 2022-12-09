using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	internal class JobDriver_SuperviseResearch : JobDriver
	{
		public const int ShotDuration = 600;

		public Building_RimatomicsResearchBench Bench => (Building_RimatomicsResearchBench)base.TargetThingB;

		public CompResearchFacility Facility => base.TargetThingA.TryGetComp<CompResearchFacility>();

		public RimatomicResearchDef Proj => Bench.currentProj;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 2, 0);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.C);
			yield return Toils_Reserve.Reserve(TargetIndex.C);
			yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
			Toil supervise = new Toil();
			supervise.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
				Pawn actor = supervise.actor;
				if (Facility.Used(actor.GetSkill(SkillDefOf.Intellectual), Proj))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				float statValue = actor.GetStatValue(StatDefOf.ResearchSpeed);
				if (Bench != null)
				{
					statValue *= Bench.GetStatValue(StatDefOf.ResearchSpeedFactor);
					Bench.ResearchPerformed(statValue, actor, Bench);
					actor.skills?.GetSkill(SkillDefOf.Intellectual).Learn(0.11f);
				}
			};
			supervise.handlingFacing = true;
			supervise.defaultCompleteMode = ToilCompleteMode.Delay;
			supervise.defaultDuration = 6000;
			supervise.WithProgressBar(TargetIndex.A, () => Bench.Research.GetProgressPct(Proj.CurrentStep));
			supervise.FailOn(() => Proj == null || !Facility.IsSafe || !Facility.powerComp.PowerOn || !Proj.CurrentStep.UsesFacility(Facility.parent.def));
			supervise.activeSkill = () => SkillDefOf.Intellectual;
			yield return supervise;
			yield return Toils_Reserve.Release(TargetIndex.C);
		}
	}
}
