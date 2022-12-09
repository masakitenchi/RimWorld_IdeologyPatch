using System;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000063 RID: 99
	[DefOf]
	public static class TerrainDefOfAI
	{
		// Token: 0x060002F0 RID: 752 RVA: 0x0003E81E File Offset: 0x0003CA1E
		static TerrainDefOfAI()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TerrainDefOfAI));
		}

		// Token: 0x04000177 RID: 375
		public static TerrainDef Marsh;
	}
}
