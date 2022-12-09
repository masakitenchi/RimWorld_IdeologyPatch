using System;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000028 RID: 40
	public class LordJob_Bodyguard : LordJob
	{
		// Token: 0x06000153 RID: 339 RVA: 0x0001E072 File Offset: 0x0001C272
		public LordJob_Bodyguard()
		{
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0001F6FA File Offset: 0x0001D8FA
		public LordJob_Bodyguard(Pawn bodyguard, Pawn escortee)
		{
			this.bodyguard = bodyguard;
			this.escortee = escortee;
			this.escorteeFaction = escortee.Faction;
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0001F720 File Offset: 0x0001D920
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_EscortPawn lordToil_EscortPawn = new LordToil_EscortPawn(this.escortee, 10f);
			stateGraph.AddToil(lordToil_EscortPawn);
			stateGraph.StartingToil = lordToil_EscortPawn;
			LordToil_End lordToil_End = new LordToil_End();
			Transition transition = new Transition(lordToil_EscortPawn, lordToil_End, true, false);
			Trigger_Custom trigger = new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && (this.escortee == null || this.escortee.Dead || this.escortee.Downed));
			transition.AddTrigger(trigger);
			stateGraph.AddToil(lordToil_End);
			stateGraph.AddTransition(transition, false);
			return stateGraph;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0001F799 File Offset: 0x0001D999
		public override void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.escortee, "escortee", false);
			Scribe_References.Look<Faction>(ref this.escorteeFaction, "escorteeFaction", false);
		}

		// Token: 0x0400006C RID: 108
		public Pawn bodyguard;

		// Token: 0x0400006D RID: 109
		public Pawn escortee;

		// Token: 0x0400006E RID: 110
		private Faction escorteeFaction;
	}
}
