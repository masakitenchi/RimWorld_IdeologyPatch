using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000009 RID: 9
	public class JobDriver_AddConsciousnessBuff : JobDriver
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001F RID: 31 RVA: 0x000029BD File Offset: 0x00000BBD
		protected Pawn Patient
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000029D4 File Offset: 0x00000BD4
		public static bool EnemyNearby(Pawn pawn, float distance)
		{
			JobDriver_AddConsciousnessBuff.delay++;
			bool flag = JobDriver_AddConsciousnessBuff.delay >= JobDriver_AddConsciousnessBuff.maxDelay;
			bool result;
			if (flag)
			{
				Thing thing;
				List<Thing> list;
				JobDriver_AddConsciousnessBuff.enemyNearby = AdvancedAI.ActiveThreat(pawn, distance, false, false, true, true, true, true, true, out thing, out list);
				JobDriver_AddConsciousnessBuff.delay = 0;
				result = JobDriver_AddConsciousnessBuff.enemyNearby;
			}
			else
			{
				result = JobDriver_AddConsciousnessBuff.enemyNearby;
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002A30 File Offset: 0x00000C30
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002A43 File Offset: 0x00000C43
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(() => JobDriver_AddConsciousnessBuff.enemyNearby);
			base.AddEndCondition(delegate
			{
				bool flag = JobDriver_AddConsciousnessBuff.EnemyNearby(this.pawn, 35f);
				JobCondition result;
				if (flag)
				{
					JobDriver_AddConsciousnessBuff.enemyNearby = false;
					result = JobCondition.Succeeded;
				}
				else
				{
					result = (AdvancedAI_TendUtility.RequireConsciousnessBuff(this.pawn, this.Patient) ? JobCondition.Ongoing : JobCondition.Succeeded);
				}
				return result;
			});
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return gotoToil;
			Toil toil = Toils_General.Wait((int)(1f / this.pawn.GetStatValue(StatDefOf.MedicalTendSpeed, true) * 600f), TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend, 1f);
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				HediffDef hediffDef = AdvancedAI_TendUtility.CureDef(actor.Faction);
				bool flag = hediffDef != null && !this.Patient.health.hediffSet.HasHediff(hediffDef, false);
				if (flag)
				{
					this.Patient.health.AddHediff(hediffDef, null, null, null);
				}
			};
			toil.activeSkill = (() => SkillDefOf.Medicine);
			toil.tickAction = delegate()
			{
				bool flag = this.pawn.IsHashIntervalTick(100) && !this.pawn.Position.Fogged(this.pawn.Map);
				if (flag)
				{
					IntVec3 cell = (this.pawn == this.Patient) ? this.pawn.Position : this.Patient.Position;
					FleckMaker.ThrowMetaIcon(cell, this.pawn.Map, FleckDefOf.HealingCross, 0.42f);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield break;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002A54 File Offset: 0x00000C54
		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			bool flag = dinfo.Def.ExternalViolenceFor(this.pawn);
			if (flag)
			{
				this.pawn.jobs.CheckForJobOverride();
			}
		}

		// Token: 0x0400000A RID: 10
		private const int BaseTendDuration = 600;

		// Token: 0x0400000B RID: 11
		private const int TicksBetweenSelfTendMotes = 100;

		// Token: 0x0400000C RID: 12
		public static int maxDelay = 60;

		// Token: 0x0400000D RID: 13
		public static int delay = 0;

		// Token: 0x0400000E RID: 14
		public static bool enemyNearby;
	}
}
