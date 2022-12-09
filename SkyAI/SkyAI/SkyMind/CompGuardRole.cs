using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200001F RID: 31
	public class CompGuardRole : ThingComp
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600012F RID: 303 RVA: 0x0001D6CB File Offset: 0x0001B8CB
		public CompProperties_BodyGuardRole Props
		{
			get
			{
				return (CompProperties_BodyGuardRole)this.props;
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0001D6D8 File Offset: 0x0001B8D8
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look<Pawn>(ref this.escortee, "escortee", false);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x0001D6F4 File Offset: 0x0001B8F4
		public override void CompTick()
		{
			base.CompTick();
			Pawn pawn = this.parent as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = pawn.IsHashIntervalTick(250);
				if (flag2)
				{
					bool flag3 = pawn.Map == null;
					if (flag3)
					{
						pawn.AllComps.Remove(this);
					}
					else
					{
						bool flag4 = AdvancedAI.IsActivePawn(pawn);
						if (flag4)
						{
							bool flag5 = SkyAiCore.Settings.debugLog && this.escortee == null;
							if (flag5)
							{
								Log.Message(string.Format("{0} {1}: I'm bodyguard and for some reason missing escortee pawn", pawn, pawn.Position));
							}
							bool flag6 = this.escortee != null && AdvancedAI.IsActivePawn(this.escortee) && !AdvancedAI.HasExitJob(this.escortee);
							if (flag6)
							{
								Lord lord = pawn.GetLord();
								bool flag7 = lord != null && !(lord.LordJob is LordJob_Bodyguard);
								if (flag7)
								{
									AdvancedAI_LordUtility.PawnAddBodyGuardLord(pawn, this.escortee, false);
									pawn.jobs.StopAll(false, true);
									pawn.jobs.ClearQueuedJobs(true);
									bool debugLog = SkyAiCore.Settings.debugLog;
									if (debugLog)
									{
										Log.Message(string.Format("{0} {1}: CompBodyGuardRole. i'm become bodyGuard for {2}.", pawn, pawn.Position, this.escortee));
									}
								}
							}
							else
							{
								RaidData raidData = AdvancedAI.PawnRaidData(pawn);
								bool flag8 = raidData != null;
								if (flag8)
								{
									Pawn pawn2;
									bool flag9 = AdvancedAI.TryToFindRaidLordPawn(pawn, raidData, out pawn2);
									if (flag9)
									{
										Lord lord2 = pawn2.GetLord();
										bool flag10 = lord2 != null;
										if (flag10)
										{
											bool debugLog2 = SkyAiCore.Settings.debugLog;
											if (debugLog2)
											{
												Log.Message(string.Format("{0} {1}: CompBodyGuardRole. Escort pawn missed. Find allied lord: {2} with lordJob: {3} and toil: {4}", new object[]
												{
													pawn,
													pawn.Position,
													lord2,
													lord2.LordJob,
													lord2.CurLordToil
												}));
											}
											AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
											pawn.jobs.StopAll(false, true);
											pawn.jobs.ClearQueuedJobs(true);
											lord2.AddPawn(pawn);
										}
										PawnDuty duty = pawn2.mindState.duty;
										bool flag11 = duty != null;
										if (flag11)
										{
											pawn.mindState.duty = new PawnDuty(duty.def);
										}
									}
									else
									{
										bool debugLog3 = SkyAiCore.Settings.debugLog;
										if (debugLog3)
										{
											Log.Message(string.Format("{0} {1}: CompBodyGuardRole. PawnAddExitLord bcs of raidData null.", pawn, pawn.Position));
										}
										AdvancedAI_LordUtility.PawnAddExitLord(pawn, false);
									}
								}
								else
								{
									bool debugLog4 = SkyAiCore.Settings.debugLog;
									if (debugLog4)
									{
										Log.Message(string.Format("{0} {1}: CompBodyGuardRole. PawnAddExitLord bcs of raidData null.", pawn, pawn.Position));
									}
									AdvancedAI_LordUtility.PawnAddExitLord(pawn, false);
								}
								pawn.AllComps.Remove(this);
							}
						}
					}
				}
			}
		}

		// Token: 0x0400005B RID: 91
		public Pawn escortee;
	}
}
