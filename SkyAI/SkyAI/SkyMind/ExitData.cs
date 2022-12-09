using System;
using Verse;

namespace SkyMind
{
	// Token: 0x0200000D RID: 13
	public sealed class ExitData : IExposable
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00005B86 File Offset: 0x00003D86
		public ExitData()
		{
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005B97 File Offset: 0x00003D97
		public ExitData(Pawn pawn, IntVec3 start, IntVec3 dest)
		{
			this.pawn = pawn;
			this.start = start;
			this.dest = dest;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00005BBD File Offset: 0x00003DBD
		public ExitData(Pawn pawn, IntVec3 start, IntVec3 dest, int dirtyTime)
		{
			this.pawn = pawn;
			this.start = start;
			this.dest = dest;
			this.dirtyTime = dirtyTime;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00005BEC File Offset: 0x00003DEC
		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
			Scribe_Values.Look<IntVec3>(ref this.start, "start", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.dest, "dest", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.dirtyTime, "dirtyTime", 0, false);
		}

		// Token: 0x04000019 RID: 25
		public Pawn pawn;

		// Token: 0x0400001A RID: 26
		public IntVec3 start;

		// Token: 0x0400001B RID: 27
		public IntVec3 dest;

		// Token: 0x0400001C RID: 28
		public int dirtyTime = 0;
	}
}
