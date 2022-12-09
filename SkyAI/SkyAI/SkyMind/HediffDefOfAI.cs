using System;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000061 RID: 97
	[DefOf]
	public static class HediffDefOfAI
	{
		// Token: 0x060002EE RID: 750 RVA: 0x0003E7F8 File Offset: 0x0003C9F8
		static HediffDefOfAI()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOfAI));
		}

		// Token: 0x04000171 RID: 369
		public static HediffDef Berserk_herb_high;

		// Token: 0x04000172 RID: 370
		public static HediffDef IbuprofenHigh;

		// Token: 0x04000173 RID: 371
		public static HediffDef LeaderWarrior;

		// Token: 0x04000174 RID: 372
		public static HediffDef LeaderWarlord;

		// Token: 0x04000175 RID: 373
		public static HediffDef LeaderAvenger;
	}
}
