using System;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x0200005E RID: 94
	[DefOf]
	public static class JobDefOfAI
	{
		// Token: 0x060002EB RID: 747 RVA: 0x0003E7BF File Offset: 0x0003C9BF
		static JobDefOfAI()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOfAI));
		}

		// Token: 0x0400015E RID: 350
		public static JobDef AITend;

		// Token: 0x0400015F RID: 351
		public static JobDef AnimalRangeAttack;

		// Token: 0x04000160 RID: 352
		public static JobDef AddConsciousnessBuff;

		// Token: 0x04000161 RID: 353
		public static JobDef LootItem;
	}
}
