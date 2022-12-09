using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	internal class JobDriver_SuperviseConstruction : JobDriver
	{
		public const int ShotDuration = 600;

		public Building_RimatomicsResearchBench Bench => (Building_RimatomicsResearchBench)base.TargetThingB;

		public CompResearchFacility Facility => base.TargetThingA.TryGetComp<CompResearchFacility>();

		public RimatomicResearchDef Proj => Bench.currentProj;

		public static Toil FindRandomInsideReachableCell(TargetIndex adjacentToInd, TargetIndex cellInd)
		{
			Toil findCell = new Toil();
			findCell.initAction = delegate
			{
				Pawn actor = findCell.actor;
				Job curJob = actor.CurJob;
				LocalTargetInfo target = curJob.GetTarget(adjacentToInd);
				if (target.HasThing && (!target.Thing.Spawned || target.Thing.Map != actor.Map))
				{
					Log.Error(string.Concat(actor, " could not find standable cell adjacent to ", target, " because this thing is either unspawned or spawned somewhere else."));
					actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
				}
				else
				{
					foreach (IntVec3 item in target.Thing.OccupiedRect().Cells.InRandomOrder())
					{
						if (item.Standable(actor.Map) && actor.CanReserve(item) && actor.CanReach(item, PathEndMode.OnCell, Danger.Deadly))
						{
							curJob.SetTarget(cellInd, item);
							return;
						}
					}
					string obj = actor?.ToString();
					LocalTargetInfo localTargetInfo = target;
					Log.Error(obj + " could not find standable cell adjacent to " + localTargetInfo.ToString());
					actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
				}
			};
			return findCell;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 2, 0);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A);
			if (Proj.CurrentStep.StandMode == BuildStandingMode.Inside)
			{
				yield return FindRandomInsideReachableCell(TargetIndex.A, TargetIndex.C);
			}
			else if (Proj.CurrentStep.StandMode == BuildStandingMode.Outside)
			{
				yield return Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.C);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.C);
			yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
			Toil supervise = new Toil();
			supervise.initAction = delegate
			{
				GenClamor.DoClamor(supervise.actor, 15f, ClamorDefOf.Construction);
			};
			supervise.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().CenterCell);
				Pawn actor = supervise.actor;
				if (Proj.CurrentStep.WorkType == WorkTypeDefOf.Construction)
				{
					if (Facility.Used(actor.GetSkill(SkillDefOf.Construction), Proj))
					{
						EndJobWith(JobCondition.Incompletable);
					}
					float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed);
					Bench.ResearchPerformed(statValue, actor, Bench);
					actor.skills?.Learn(SkillDefOf.Construction, 0.11f);
				}
				else
				{
					if (Facility.Used(actor.GetSkill(SkillDefOf.Intellectual), Proj))
					{
						EndJobWith(JobCondition.Incompletable);
					}
					float statValue2 = actor.GetStatValue(StatDefOf.ResearchSpeed);
					Bench.ResearchPerformed(statValue2, actor, Bench);
					actor.skills?.Learn(SkillDefOf.Intellectual, 0.11f);
				}
			};
			supervise.handlingFacing = true;
			supervise.defaultCompleteMode = ToilCompleteMode.Delay;
			supervise.defaultDuration = 2000;
			supervise.WithProgressBar(TargetIndex.A, () => Bench.Research.GetProgressPct(Proj.CurrentStep));
			supervise.FailOn(() => Proj == null || !Proj.CurrentStep.UsesFacility(Facility.parent.def) || !Facility.IsSafe);
			supervise.activeSkill = () => SkillDefOf.Construction;
			if (Rand.Value > 0.5f)
			{
				supervise.WithEffect(() => DubDef.RimatomicsConstructWeld, TargetIndex.A);
			}
			else
			{
				supervise.WithEffect(() => DubDef.RimatomicsConstructDrill, TargetIndex.A);
			}
			yield return supervise;
			yield return Toils_Reserve.Release(TargetIndex.C);
		}
	}
}
