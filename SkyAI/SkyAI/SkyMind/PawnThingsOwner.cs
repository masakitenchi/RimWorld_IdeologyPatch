using System;
using System.Collections.Generic;
using Verse;

namespace SkyMind
{
	// Token: 0x0200002E RID: 46
	public sealed class PawnThingsOwner : IExposable
	{
		// Token: 0x06000176 RID: 374 RVA: 0x00002050 File Offset: 0x00000250
		public PawnThingsOwner()
		{
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00022CDC File Offset: 0x00020EDC
		public PawnThingsOwner(Pawn owner, List<ThingCountClass> thingCount)
		{
			this.owner = owner;
			this.thingCount = thingCount;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00022CF4 File Offset: 0x00020EF4
		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.owner, "owner", false);
			Scribe_Collections.Look<ThingCountClass>(ref this.thingCount, "thingCount", LookMode.Deep, Array.Empty<object>());
		}

		// Token: 0x040000F7 RID: 247
		public Pawn owner;

		// Token: 0x040000F8 RID: 248
		public List<ThingCountClass> thingCount;
	}
}
