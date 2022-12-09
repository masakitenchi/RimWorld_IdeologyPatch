using System;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x0200005D RID: 93
	[DefOf]
	public static class ThingCategoryDefOfAI
	{
		// Token: 0x060002EA RID: 746 RVA: 0x0003E7AC File Offset: 0x0003C9AC
		static ThingCategoryDefOfAI()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingCategoryDefOfAI));
		}

		// Token: 0x0400015C RID: 348
		public static ThingCategoryDef SRifles;

		// Token: 0x0400015D RID: 349
		public static ThingCategoryDef MachineGun;
	}
}
