using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000038 RID: 56
	public class JobDriver_AITend : JobDriver
	{
		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00028555 File Offset: 0x00026755
		protected Thing MedicineUsed
		{
			get
			{
				return this.job.targetB.Thing;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060001BD RID: 445 RVA: 0x000029BD File Offset: 0x00000BBD
		protected Pawn Patient
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00028568 File Offset: 0x00026768
		public static bool EnemyNearby(Pawn pawn, float distance)
		{
			JobDriver_AITend.delay++;
			bool flag = JobDriver_AITend.delay >= JobDriver_AITend.maxDelay;
			bool result;
			if (flag)
			{
				Thing thing;
				List<Thing> list;
				JobDriver_AITend.enemyNearby = AdvancedAI.ActiveThreat(pawn, distance, false, false, true, true, true, true, true, out thing, out list);
				JobDriver_AITend.delay = 0;
				result = JobDriver_AITend.enemyNearby;
			}
			else
			{
				result = JobDriver_AITend.enemyNearby;
			}
			return result;
		}

		// Token: 0x060001BF RID: 447 RVA: 0x000285C4 File Offset: 0x000267C4
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.usesMedicine, "usesMedicine", false, false);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x000285E1 File Offset: 0x000267E1
		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.usesMedicine = (this.MedicineUsed != null);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x000285FC File Offset: 0x000267FC
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			bool flag = this.Patient != null && this.Patient != this.pawn && !this.pawn.Reserve(this.Patient, this.job, 1, -1, null, false);
			bool result;
			if (flag)
			{
				bool debugLog = SkyAiCore.Settings.debugLog;
				if (debugLog)
				{
					Log.Message(string.Format("{0} {1}: AITend job. TryMakePreToilReservations: Patient reservation: {2} failed. Job failed.", this.pawn, this.pawn.Position, this.Patient));
				}
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00028690 File Offset: 0x00026890
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => JobDriver_AITend.enemyNearby);
			base.AddEndCondition(delegate
			{
				bool flag4 = JobDriver_AITend.EnemyNearby(this.pawn, 35f);
				JobCondition result;
				if (flag4)
				{
					JobDriver_AITend.enemyNearby = false;
					result = JobCondition.Succeeded;
				}
				else
				{
					result = (AdvancedAI_TendUtility.HasHediffsNeedingTend(this.Patient) ? JobCondition.Ongoing : JobCondition.Succeeded);
				}
				return result;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil takeMedicine = JobDriver_AITend.TakeMedicineFromInventory(this.Patient, this.MedicineUsed);
			yield return takeMedicine;
			PathEndMode interactionCell = (this.Patient == this.pawn) ? PathEndMode.OnCell : PathEndMode.InteractionCell;
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
			yield return gotoToil;
			int delay = (int)(1f / this.pawn.GetStatValue(StatDefOf.MedicalTendSpeed, true) * 600f);
			Toil toil = Toils_General.Wait(delay, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, interactionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend, 1f);
			int type = 2;
			PawnPosture posture = AdvancedAI.LayingOnGround(type);
			bool flag = this.Patient != null && this.pawn != this.Patient && !this.Patient.Downed;
			if (flag)
			{
				toil.initAction = delegate()
				{
					bool flag4 = this.pawn.Position.z > this.Patient.Position.z || this.pawn.Position.z < this.Patient.Position.z;
					if (flag4)
					{
						this.Patient.Rotation = Rot4.West;
					}
					this.Patient.jobs.posture = posture;
				};
			}
			bool flag2 = SkyAiCore.Settings.layingOnGroundOnSelfTend && this.pawn == this.Patient;
			if (flag2)
			{
				toil.initAction = delegate()
				{
					this.pawn.Rotation = Rot4.West;
					this.pawn.jobs.posture = posture;
				};
			}
			toil.activeSkill = (() => SkillDefOf.Medicine);
			toil.tickAction = delegate()
			{
				bool flag4 = this.pawn.IsHashIntervalTick(100) && !this.pawn.Position.Fogged(this.pawn.Map);
				if (flag4)
				{
					IntVec3 cell = (this.pawn == this.Patient) ? this.pawn.Position : this.Patient.Position;
					FleckMaker.ThrowMetaIcon(cell, this.pawn.Map, FleckDefOf.HealingCross, 0.42f);
				}
			};
			yield return toil;
			yield return JobDriver_AITend.FinalizeTend(this.Patient);
			bool flag3 = this.usesMedicine;
			if (flag3)
			{
				Toil toil2 = new Toil();
				toil2.initAction = delegate()
				{
					bool flag4 = this.MedicineUsed.DestroyedOrNull();
					if (flag4)
					{
						this.JumpToToil(takeMedicine);
					}
				};
				yield return toil2;
				toil2 = null;
			}
			yield return Toils_Jump.Jump(gotoToil);
			yield break;
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x000286A0 File Offset: 0x000268A0
		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			bool flag = dinfo.Def.ExternalViolenceFor(this.pawn) && this.pawn == this.Patient;
			if (flag)
			{
				this.pawn.jobs.CheckForJobOverride();
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x000286F4 File Offset: 0x000268F4
		public static Toil TakeMedicineFromInventory(Pawn patient, Thing thing)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Pawn actor = toil.GetActor();
				bool flag = thing == null || actor.carryTracker.CarriedThing == null;
				if (flag)
				{
					thing = AdvancedAI_TendUtility.StartCarryMedicine(actor, patient, true);
					actor.CurJob.SetTarget(TargetIndex.B, thing);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00028750 File Offset: 0x00026950
		public static Toil FinalizeTend(Pawn patient)
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				Medicine medicine = (Medicine)actor.CurJob.targetB.Thing;
				bool flag = actor.carryTracker.CarriedThing == null;
				if (flag)
				{
					medicine = (Medicine)AdvancedAI_TendUtility.StartCarryMedicine(actor, patient, true);
				}
				float num = patient.RaceProps.Animal ? 175f : 500f;
				float num2 = (medicine != null) ? medicine.def.MedicineTendXpGainFactor : 0.5f;
				actor.skills.Learn(SkillDefOf.Medicine, num * num2, false);
				TendUtility.DoTend(actor, patient, medicine);
				bool debugLog = SkyAiCore.Settings.debugLog;
				if (debugLog)
				{
					Log.Message(string.Format("{0} {1}: AITend job. FinalizeTend. DoTend.", actor, actor.Position));
				}
				HediffDef hediffDef = AdvancedAI_TendUtility.CureDef(actor.Faction);
				bool flag2 = SkyAiCore.Settings.enemyDoctorWillAddСonsciousnessBuff && hediffDef != null && !patient.health.hediffSet.HasHediff(hediffDef, false) && !patient.RaceProps.Animal;
				if (flag2)
				{
					patient.health.AddHediff(hediffDef, null, null, null);
				}
				bool flag3 = medicine != null && medicine.Destroyed;
				if (flag3)
				{
					actor.CurJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid);
				}
				bool flag4 = actor.carryTracker.CarriedThing != null && !actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer, true);
				if (flag4)
				{
					Thing thing;
					actor.carryTracker.TryDropCarriedThing(actor.Position, actor.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out thing, null);
				}
				bool endAfterTendedOnce = toil.actor.CurJob.endAfterTendedOnce;
				if (endAfterTendedOnce)
				{
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		// Token: 0x04000113 RID: 275
		private bool usesMedicine;

		// Token: 0x04000114 RID: 276
		private const int BaseTendDuration = 600;

		// Token: 0x04000115 RID: 277
		private const int TicksBetweenSelfTendMotes = 100;

		// Token: 0x04000116 RID: 278
		public static int maxDelay = 60;

		// Token: 0x04000117 RID: 279
		public static int delay = 0;

		// Token: 0x04000118 RID: 280
		public static bool enemyNearby = false;
	}
}
