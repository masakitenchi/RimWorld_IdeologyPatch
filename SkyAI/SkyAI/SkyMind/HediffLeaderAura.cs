using System;
using Verse;

namespace SkyMind
{
	// Token: 0x02000007 RID: 7
	public class HediffLeaderAura : HediffWithComps
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002608 File Offset: 0x00000808
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.leader, "leader", false);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002309 File Offset: 0x00000509
		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			this.pawn.health.RemoveHediff(this);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002624 File Offset: 0x00000824
		public override void Tick()
		{
			base.Tick();
			this.counter++;
			bool flag = this.counter >= 250;
			if (flag)
			{
				this.counter = 0;
				bool flag2 = this.leader.Map == null;
				if (flag2)
				{
					this.pawn.health.RemoveHediff(this);
				}
				bool flag3 = false;
				bool flag4 = this.leader != null && this.leader.Map == this.pawn.Map;
				if (flag4)
				{
					bool flag5 = (this.pawn.Position - this.leader.Position).LengthHorizontal <= SkyAiCore.Settings.leaderAuraRange;
					if (flag5)
					{
						bool flag6 = this.leader.GetRoom(RegionType.Set_All) == this.pawn.GetRoom(RegionType.Set_All);
						if (flag6)
						{
							RaidLeaderExtension modExtension = this.def.GetModExtension<RaidLeaderExtension>();
							bool flag7 = modExtension != null;
							if (flag7)
							{
								HediffLeader hediffLeader = (HediffLeader)this.leader.health.hediffSet.GetFirstHediffOfDef(modExtension.leaderHediff, false);
								bool flag8 = hediffLeader != null;
								if (flag8)
								{
									flag3 = true;
									bool flag9 = this.leader.CurJob != null && this.leader.jobs.curDriver.asleep;
									if (flag9)
									{
										flag3 = false;
									}
									bool flag10 = this.leader.InAggroMentalState || !AdvancedAI.IsActivePawn(this.leader) || this.leader.IsPrisoner || this.leader.IsSlave;
									if (flag10)
									{
										flag3 = false;
									}
								}
							}
						}
					}
				}
				bool flag11 = !flag3;
				if (flag11)
				{
					this.pawn.health.RemoveHediff(this);
				}
			}
		}

		// Token: 0x04000007 RID: 7
		private int counter = 0;

		// Token: 0x04000008 RID: 8
		public Pawn leader = null;
	}
}
