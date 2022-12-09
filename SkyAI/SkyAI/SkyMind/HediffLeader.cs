using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SkyMind
{
	// Token: 0x02000006 RID: 6
	public class HediffLeader : HediffWithComps
	{
		// Token: 0x06000012 RID: 18 RVA: 0x000022FF File Offset: 0x000004FF
		public override void ExposeData()
		{
			base.ExposeData();
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002309 File Offset: 0x00000509
		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			this.pawn.health.RemoveHediff(this);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002328 File Offset: 0x00000528
		public override void Tick()
		{
			base.Tick();
			this.counter++;
			bool flag = this.counter >= 250;
			if (flag)
			{
				this.counter = 0;
				bool flag2 = !AdvancedAI.IsActivePawn(this.pawn) || this.pawn.IsPrisoner || this.pawn.IsSlave;
				if (flag2)
				{
					this.pawn.health.RemoveHediff(this);
				}
				else
				{
					bool flag3 = this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep;
					if (!flag3)
					{
						bool inAggroMentalState = this.pawn.InAggroMentalState;
						if (!inAggroMentalState)
						{
							bool flag4 = !this.pawn.Spawned && this.pawn.CarriedBy != null;
							if (!flag4)
							{
								bool flag5 = this.pawn.Map == null;
								if (!flag5)
								{
									IEnumerable<Thing> enumerable = GenRadial.RadialDistinctThingsAround(this.pawn.Position, this.pawn.Map, SkyAiCore.Settings.leaderAuraRange, true);
									HediffDef leaderAuraHediff = this.def.GetModExtension<RaidLeaderAuraExtension>().leaderAuraHediff;
									foreach (Thing thing in enumerable)
									{
										Pawn pawn = thing as Pawn;
										bool flag6 = pawn != null && !AdvancedAI.PawnIsLeader(pawn) && !AdvancedAI.PawnIsSquadLeader(pawn);
										if (flag6)
										{
											bool flag7 = !pawn.Dead && pawn != this.pawn && pawn.Faction == this.pawn.Faction;
											if (flag7)
											{
												bool flag8 = pawn.GetRoom(RegionType.Set_All) == this.pawn.GetRoom(RegionType.Set_All);
												if (flag8)
												{
													Pawn_HealthTracker health = pawn.health;
													bool flag9;
													if (health == null)
													{
														flag9 = (null != null);
													}
													else
													{
														HediffSet hediffSet = health.hediffSet;
														if (hediffSet == null)
														{
															flag9 = (null != null);
														}
														else
														{
															flag9 = ((from h in hediffSet.hediffs
															where h.GetType() == typeof(HediffLeaderAura)
															select h).FirstOrDefault<Hediff>() != null);
														}
													}
													bool flag10 = !flag9 && leaderAuraHediff != null;
													if (flag10)
													{
														Hediff hediff = new Hediff();
														hediff = HediffMaker.MakeHediff(leaderAuraHediff, pawn, null);
														hediff.Severity = AdvancedAI_Aura.AuraLevel(this.pawn);
														HediffLeaderAura hediffLeaderAura = (HediffLeaderAura)hediff;
														hediffLeaderAura.leader = this.pawn;
														pawn.health.AddHediff(hediffLeaderAura, null, null, null);
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x04000006 RID: 6
		private int counter = 0;
	}
}
