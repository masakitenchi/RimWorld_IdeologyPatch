using System;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200002A RID: 42
	public class Trigger_RaidPawnsLost : Trigger
	{
		// Token: 0x06000161 RID: 353 RVA: 0x00020037 File Offset: 0x0001E237
		public Trigger_RaidPawnsLost(float fraction)
		{
			this.fraction = fraction;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00020054 File Offset: 0x0001E254
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			bool flag = signal.type == TriggerSignalType.PawnLost && (float)lord.numPawnsLostViolently >= (float)lord.numPawnsEverGained * this.fraction;
			bool flag2 = flag;
			if (flag2)
			{
				Pawn pawn = lord.ownedPawns[0];
				bool flag3 = pawn != null;
				if (flag3)
				{
					RaidData raidData = AdvancedAI.PawnRaidData(pawn);
					bool flag4 = raidData != null;
					if (flag4)
					{
						AdvancedAI_SquadUtility.UpdateStageForSiegeAI(pawn);
						raidData.raidStage = RaidData.RaidStage.startAttacking;
					}
				}
			}
			return flag;
		}

		// Token: 0x04000075 RID: 117
		private float fraction = 0.5f;
	}
}
