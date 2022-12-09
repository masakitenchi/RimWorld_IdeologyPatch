using System.Collections.Generic;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_UseReactorConsole : JobDriver
	{
		private ReactorControl Control => (ReactorControl)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !Control.CanUseConsole);
			Toil toil = new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Never,
				tickAction = delegate
				{
					if (Control.console == null || !Control.console.IsOpen)
					{
						Control.TryOpenTerminal(GetActor());
					}
				}
			};
			toil.AddFinishAction(delegate
			{
				Control.TryCloseTerminal();
			});
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			toil.AddEndCondition(() => Control.CanUseConsole ? JobCondition.Ongoing : JobCondition.Incompletable);
			yield return toil;
		}
	}
}
