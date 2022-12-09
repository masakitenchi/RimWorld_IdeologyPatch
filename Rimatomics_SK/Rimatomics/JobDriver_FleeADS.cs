using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_FleeADS : JobDriver
	{
		private const TargetIndex DestInd = TargetIndex.A;

		private const int CowerTicks = 200;

		private const int CheckFleeAgainInvervalTicks = 35;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.A).Cell);
			return true;
		}

		public override string GetReport()
		{
			if (pawn.Position != job.GetTarget(TargetIndex.A).Cell)
			{
				return base.GetReport();
			}
			return "ReportCowering".Translate();
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					base.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.A).Cell);
					_ = pawn.IsColonist;
				}
			};
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 200,
				tickAction = delegate
				{
					if (pawn.IsHashIntervalTick(35) && SelfDefenseUtility.ShouldStartFleeing(pawn))
					{
						EndJobWith(JobCondition.InterruptForced);
					}
				}
			};
		}
	}
}
