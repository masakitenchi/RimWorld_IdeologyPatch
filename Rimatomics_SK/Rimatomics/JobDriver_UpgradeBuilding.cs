using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class JobDriver_UpgradeBuilding : JobDriver
	{
		private const TargetIndex BuildingInd = TargetIndex.A;

		private const TargetIndex ComponentInd = TargetIndex.B;

		private const int TicksDuration = 1000;

		private Building Building => (Building)job.GetTarget(TargetIndex.A).Thing;

		private Thing Components => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = Toils_General.Wait(1000);
			toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			toil.WithEffect(Building.def.repairEffect, TargetIndex.A);
			toil.WithProgressBarToilDelay(TargetIndex.A);
			yield return toil;
			yield return new Toil
			{
				initAction = delegate
				{
					Components.Destroy();
					Building.GetComp<CompUpgradable>().InstallUpgrade(Components.def);
				}
			};
		}
	}
}
