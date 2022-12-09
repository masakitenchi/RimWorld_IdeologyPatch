using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_RemoveFuelModule : JobDriver
	{
		private const TargetIndex CoreInd = TargetIndex.A;

		private const int Duration = 200;

		protected reactorCore Core => (reactorCore)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(300).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.FailOn(() => !Core.SlotDesignations.Any((RodDesignate x) => x == RodDesignate.Remove))
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = Core.RemoveFuel();
					StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
					if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, base.Map, currentPriority, pawn.Faction, out var foundCell))
					{
						job.SetTarget(TargetIndex.C, foundCell);
						job.SetTarget(TargetIndex.B, thing);
						job.count = thing.stackCount;
					}
					else
					{
						EndJobWith(JobCondition.Incompletable);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.C);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, storageMode: true);
		}
	}
}
