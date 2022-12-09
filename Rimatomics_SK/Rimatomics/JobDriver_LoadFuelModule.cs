using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_LoadFuelModule : JobDriver
	{
		private const TargetIndex CoreInd = TargetIndex.A;

		private const TargetIndex FuelInd = TargetIndex.B;

		private const int Duration = 200;

		protected reactorCore Core => (reactorCore)job.GetTarget(TargetIndex.A).Thing;

		protected Item_NuclearFuel Fuel => job.GetTarget(TargetIndex.B).Thing as Item_NuclearFuel;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			AddFailCondition(() => !Core.BreederHotLoad && !Core.coldAndDead);
			AddFailCondition(delegate
			{
				if (Fuel.MOX)
				{
					if (!Core.SlotDesignations.Any((RodDesignate x) => x == RodDesignate.MOX))
					{
						return true;
					}
				}
				else if (!Core.SlotDesignations.Any((RodDesignate x) => x == RodDesignate.Fuel))
				{
					return true;
				}
				return !Core.GetStoreSettings().AllowedToAccept(base.TargetThingB) || base.TargetThingA.IsForbidden(pawn);
			});
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
			yield return reserveFuel;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					Core.InsertFuel(Fuel);
					pawn.carryTracker.innerContainer.Remove(Fuel);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
