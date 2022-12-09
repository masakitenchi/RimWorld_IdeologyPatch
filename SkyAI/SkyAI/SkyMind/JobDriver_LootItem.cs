using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000008 RID: 8
	public class JobDriver_LootItem : JobDriver
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00002814 File Offset: 0x00000A14
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002846 File Offset: 0x00000A46
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil toil = new Toil
			{
				initAction = delegate()
				{
					this.pawn.pather.StartPath(base.TargetThingA, PathEndMode.ClosestTouch);
				},
				defaultCompleteMode = ToilCompleteMode.PatherArrival
			};
			bool flag = this.pawn.Faction != null && this.pawn.Faction.IsPlayer;
			if (flag)
			{
				toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			}
			yield return toil;
			yield return Toils_General.Wait(40, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			Toil toil2 = new Toil
			{
				initAction = delegate()
				{
					ThingWithComps thingWithComps = this.job.targetB.Thing as ThingWithComps;
					bool flag2 = thingWithComps != null;
					if (flag2)
					{
						bool flag3 = base.TargetA.Thing is Corpse;
						if (flag3)
						{
							Corpse corpse = base.TargetA.Thing as Corpse;
							Pawn innerPawn = corpse.InnerPawn;
							AdvancedAI_MoveItemUtility.MoveItemsToInventory(thingWithComps, innerPawn, this.pawn);
							bool flag4 = this.pawn.Faction != null && this.pawn.Faction.IsPlayer;
							if (flag4)
							{
								this.pawn.records.Increment(RecordDefOf.BodiesStripped);
							}
						}
						else
						{
							bool flag5 = base.TargetA.Thing is Pawn;
							if (flag5)
							{
								Pawn moveFrom = base.TargetA.Thing as Pawn;
								AdvancedAI_MoveItemUtility.MoveItemsToInventory(thingWithComps, moveFrom, this.pawn);
								bool flag6 = this.pawn.Faction != null && this.pawn.Faction.IsPlayer;
								if (flag6)
								{
									this.pawn.records.Increment(RecordDefOf.BodiesStripped);
								}
							}
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return toil2;
			yield break;
		}

		// Token: 0x04000009 RID: 9
		private const int LootTicks = 40;
	}
}
