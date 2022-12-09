using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000029 RID: 41
	public class LordJob_StageAttack : LordJob
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000158 RID: 344 RVA: 0x0001F7F1 File Offset: 0x0001D9F1
		public override bool GuiltyOnDowned
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0001E072 File Offset: 0x0001C272
		public LordJob_StageAttack()
		{
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0001F7F4 File Offset: 0x0001D9F4
		public LordJob_StageAttack(Pawn commander, Faction faction, IntVec3 stageLoc, IntRange intRange, bool skipStage, int raidSeed)
		{
			this.commander = commander;
			this.faction = faction;
			this.stageLoc = stageLoc;
			this.intRange = intRange;
			this.skipStage = skipStage;
			this.raidSeed = raidSeed;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0001F82C File Offset: 0x0001DA2C
		public override void LordJobTick()
		{
			base.LordJobTick();
			bool flag = Find.TickManager.TicksGame % 300 == 0;
			if (flag)
			{
				bool flag2 = this.lord != null && this.lord.ownedPawns.NullOrEmpty<Pawn>();
				if (flag2)
				{
					this.lord.lordManager.RemoveLord(this.lord);
				}
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0001F894 File Offset: 0x0001DA94
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Stage lordToil_Stage = (LordToil_Stage)(stateGraph.StartingToil = new LordToil_Stage(this.stageLoc));
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true, false, false).CreateGraph()).StartingToil;
			int tickLimit = Rand.RangeSeeded(this.intRange.min, this.intRange.max, this.raidSeed);
			Transition transition = new Transition(lordToil_Stage, startingToil, false, true);
			transition.AddTrigger(new Trigger_TickCondition(() => this.skipStage || (this.lord.ticksInToil >= 200 && LordJob_StageAttack.IsReady(this.lord, this.commander, this.stageLoc, tickLimit)), 600));
			transition.AddTrigger(new Trigger_RaidPawnsLost(0.15f));
			transition.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition, false);
			bool flag = this.commander != null && this.commander.Position.InBounds(Find.CurrentMap);
			if (flag)
			{
				IntVec3 loc = this.commander.Position;
				LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(loc, SkyAiCore.Settings.nonCombatActiveThreatRange, null);
				Transition transition2 = new Transition(lordToil_Stage, lordToil_DefendPoint, false, true);
				transition2.AddTrigger(new Trigger_TickCondition(() => LordJob_StageAttack.SquadDefence(this.lord, this.commander, out loc), 100));
				transition2.AddPostAction(new TransitionAction_EndAllJobs());
				stateGraph.AddToil(lordToil_DefendPoint);
				stateGraph.AddTransition(transition2, false);
				Transition transition3 = new Transition(lordToil_DefendPoint, lordToil_Stage, false, true);
				transition3.AddTrigger(new Trigger_TickCondition(() => !LordJob_StageAttack.SquadDefence(this.lord, this.commander, out loc), 100));
				transition3.AddPostAction(new TransitionAction_EndAllJobs());
				stateGraph.AddTransition(transition3, false);
			}
			int num = SkyAiCore.Settings.boostEnemyDashSpeed ? 4 : 3;
			LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
			lordToil_PanicFlee.useAvoidGrid = true;
			Transition transition4 = new Transition(startingToil, lordToil_PanicFlee, false, true);
			transition4.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(this.faction.def.pawnsPlural.CapitalizeFirst(), this.faction.Name), null, 1f));
			transition4.AddTrigger(new Trigger_TickCondition(() => LordJob_StageAttack.ShouldExit(this.lord), 600));
			stateGraph.AddToil(lordToil_PanicFlee);
			stateGraph.AddTransition(transition4, true);
			return stateGraph;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0001FB14 File Offset: 0x0001DD14
		public static bool ShouldExit(Lord lord)
		{
			bool flag = lord != null;
			if (flag)
			{
				Pawn pawn = lord.ownedPawns.FirstOrDefault<Pawn>();
				bool flag2 = pawn != null && ((pawn.Faction != null && pawn.Faction.neverFlee && !pawn.Faction.def.autoFlee) || AdvancedAI.HasFobbidenFaction(pawn));
				if (flag2)
				{
					return false;
				}
				RaidData raidData = AdvancedAI.PawnRaidData(pawn);
				bool flag3 = raidData != null;
				if (flag3)
				{
					bool flag4 = raidData.raidStage == RaidData.RaidStage.fleeing;
					if (flag4)
					{
						bool debugRaidData = SkyAiCore.Settings.debugRaidData;
						if (debugRaidData)
						{
							Log.Message(string.Format("LordJob_StageAttack. ShouldExit. Raid stage: {0}. Result true.", raidData.raidStage));
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0001FBD8 File Offset: 0x0001DDD8
		public static bool SquadDefence(Lord lord, Pawn commander, out IntVec3 loc)
		{
			bool flag = lord != null && !lord.ownedPawns.NullOrEmpty<Pawn>();
			if (flag)
			{
				Pawn pawn = lord.ownedPawns[0];
				bool flag2 = pawn != null;
				if (flag2)
				{
					RaidData raidData = AdvancedAI.PawnRaidData(pawn);
					bool flag3 = raidData != null;
					if (flag3)
					{
						bool flag4 = raidData.raidStage == RaidData.RaidStage.gathering || raidData.raidStage == RaidData.RaidStage.start;
						if (flag4)
						{
							foreach (KeyValuePair<Lord, bool> keyValuePair in raidData.squadDefence)
							{
								bool flag5 = keyValuePair.Key == lord;
								if (flag5)
								{
									foreach (KeyValuePair<Lord, IntVec3> keyValuePair2 in raidData.squadDefencePoint)
									{
										bool flag6 = keyValuePair2.Key == lord;
										if (flag6)
										{
											loc = keyValuePair2.Value;
											return keyValuePair.Value;
										}
									}
								}
							}
						}
					}
				}
			}
			loc = commander.Position;
			return false;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0001FD2C File Offset: 0x0001DF2C
		public static bool IsReady(Lord lord, Pawn commander, IntVec3 spot, int tickLimit)
		{
			bool flag = false;
			bool flag2 = !AdvancedAI.IsValidLoc(spot);
			bool result;
			if (flag2)
			{
				flag = true;
				bool debugRaidData = SkyAiCore.Settings.debugRaidData;
				if (debugRaidData)
				{
					Log.Message(string.Format("{0} {1}: LordJob_StageAttack. stageLoc is Invalid. Stage skipped. IsReady. result: {2}", commander, commander.Position, flag));
				}
				result = flag;
			}
			else
			{
				bool flag3 = commander != null && AdvancedAI.IsActivePawn(commander);
				if (flag3)
				{
					SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(commander);
					CompLeaderRole comp = commander.GetComp<CompLeaderRole>();
					bool flag4 = comp != null;
					if (flag4)
					{
						bool flag5 = !squadData.isReserved;
						if (flag5)
						{
							flag = squadData.isReady;
						}
						bool debugRaidData2 = SkyAiCore.Settings.debugRaidData;
						if (debugRaidData2)
						{
							Log.Message(string.Format("{0} {1}: LordJob_StageAttack. leader' squad is squadIsReserved: {2} IsReady. result: {3}", new object[]
							{
								commander,
								commander.Position,
								squadData.isReserved,
								flag
							}));
						}
					}
					CompSquadCommanderRole comp2 = commander.GetComp<CompSquadCommanderRole>();
					bool flag6 = comp2 != null;
					if (flag6)
					{
						bool flag7 = !squadData.isReserved;
						if (flag7)
						{
							flag = squadData.isReady;
						}
						bool debugRaidData3 = SkyAiCore.Settings.debugRaidData;
						if (debugRaidData3)
						{
							Log.Message(string.Format("{0} {1}: LordJob_StageAttack. squad commander is squadIsReserved: {2} IsReady. result: {3}", new object[]
							{
								commander,
								commander.Position,
								squadData.isReserved,
								flag
							}));
						}
					}
				}
				else
				{
					bool debugRaidData4 = SkyAiCore.Settings.debugRaidData;
					if (debugRaidData4)
					{
						Log.Message("LordJob_StageAttack. IsReady. Commander null or not active. Result: true.");
					}
					flag = true;
				}
				bool flag8 = lord != null && flag;
				if (flag8)
				{
					Pawn pawn = lord.ownedPawns.FirstOrDefault<Pawn>();
					bool flag9 = pawn != null;
					if (flag9)
					{
						RaidData raidData = AdvancedAI.PawnRaidData(pawn);
						bool flag10 = raidData != null && raidData.raidStage != RaidData.RaidStage.attack;
						if (flag10)
						{
							Pawn pawn2 = commander ?? pawn;
							AdvancedAI_SquadUtility.UpdateStageForSiegeAI(pawn2);
							raidData.raidStage = RaidData.RaidStage.startAttacking;
						}
						bool debugLog = SkyAiCore.Settings.debugLog;
						if (debugLog)
						{
							Log.Message("StageAttack. Clear lord pawn jobs, to start attack in a moment.");
						}
						foreach (Pawn pawn3 in lord.ownedPawns)
						{
							pawn3.jobs.StopAll(false, true);
							pawn3.jobs.ClearQueuedJobs(true);
						}
					}
				}
				result = flag;
			}
			return result;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0001FFBC File Offset: 0x0001E1BC
		public override void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.commander, "commander", false);
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Values.Look<IntVec3>(ref this.stageLoc, "stageLoc", default(IntVec3), false);
			Scribe_Values.Look<IntRange>(ref this.intRange, "intRange", default(IntRange), false);
			Scribe_Values.Look<int>(ref this.raidSeed, "raidSeed", 0, false);
		}

		// Token: 0x0400006F RID: 111
		private Pawn commander;

		// Token: 0x04000070 RID: 112
		private Faction faction;

		// Token: 0x04000071 RID: 113
		private IntVec3 stageLoc;

		// Token: 0x04000072 RID: 114
		private IntRange intRange;

		// Token: 0x04000073 RID: 115
		private bool skipStage;

		// Token: 0x04000074 RID: 116
		private int raidSeed;
	}
}
