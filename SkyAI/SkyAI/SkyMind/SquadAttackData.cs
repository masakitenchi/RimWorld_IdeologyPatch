using System;
using Verse;

namespace SkyMind
{
	// Token: 0x02000011 RID: 17
	public class SquadAttackData : IExposable
	{
		// Token: 0x06000054 RID: 84 RVA: 0x00006960 File Offset: 0x00004B60
		public SquadAttackData()
		{
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00006980 File Offset: 0x00004B80
		public SquadAttackData(Map map, SquadData squadData)
		{
			this.squadAttackGrid = new SquadAttackGrid(map, squadData);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x000069B0 File Offset: 0x00004BB0
		public void Reset()
		{
			bool debugSquadAttackData = SkyAiCore.Settings.debugSquadAttackData;
			if (debugSquadAttackData)
			{
				Log.Message("SquadAttackData reset.");
			}
			this.dest = IntVec3.Invalid;
			this.start = IntVec3.Invalid;
			this.currentTarget = null;
			this.soloAttacker = null;
			this.squadAttackGrid.Reset();
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00006A0C File Offset: 0x00004C0C
		public void ExposeData()
		{
			Scribe_Values.Look<IntVec3>(ref this.dest, "dest", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.start, "start", default(IntVec3), false);
			Scribe_Values.Look<bool>(ref this.preferMelee, "preferMelee", false, false);
			Scribe_Deep.Look<SquadAttackGrid>(ref this.squadAttackGrid, "squadAttackGrid", Array.Empty<object>());
			Scribe_References.Look<Thing>(ref this.currentTarget, "currentTarget", false);
			Scribe_References.Look<Pawn>(ref this.soloAttacker, "soloAttacker", false);
			Scribe_Values.Look<float>(ref SquadAttackData.maxRange, "maxRange", 0f, false);
		}

		// Token: 0x04000030 RID: 48
		public Map map;

		// Token: 0x04000031 RID: 49
		public IntVec3 dest = IntVec3.Invalid;

		// Token: 0x04000032 RID: 50
		public IntVec3 start = IntVec3.Invalid;

		// Token: 0x04000033 RID: 51
		public bool preferMelee;

		// Token: 0x04000034 RID: 52
		public SquadAttackGrid squadAttackGrid;

		// Token: 0x04000035 RID: 53
		public Thing currentTarget;

		// Token: 0x04000036 RID: 54
		public Pawn soloAttacker;

		// Token: 0x04000037 RID: 55
		[TweakValue("SquadAttackData", 0f, 100f)]
		public static float maxRange = 12f;
	}
}
