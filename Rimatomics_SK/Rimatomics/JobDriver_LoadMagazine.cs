using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_LoadMagazine : JobDriver
	{
		private const int Duration = 200;

		protected Building_Railgun railgun => (Building_Railgun)job.GetTarget(TargetIndex.A).Thing;

		protected Thing sabots => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			Toil reservefuel = Toils_Reserve.Reserve(TargetIndex.B);
			yield return reservefuel;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reservefuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(100).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					for (int i = 0; i < sabots.stackCount; i++)
					{
						railgun.magazine.Add(sabots.def);
						railgun.TryChamberRound();
					}
					pawn.carryTracker.innerContainer.Remove(sabots);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
