using System;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x02000027 RID: 39
	[StaticConstructorOnStartup]
	internal static class Materials
	{
		// Token: 0x04000065 RID: 101
		public static readonly Material leaderIconMat = MaterialPool.MatFrom(TexturesLoader.leaderIconTex);

		// Token: 0x04000066 RID: 102
		public static readonly Material doctorIconMat = MaterialPool.MatFrom(TexturesLoader.doctorIconTex);

		// Token: 0x04000067 RID: 103
		public static readonly Material traderIconMat = MaterialPool.MatFrom(TexturesLoader.traderIconTex);

		// Token: 0x04000068 RID: 104
		public static readonly Material raidLeaderIconMat = MaterialPool.MatFrom(TexturesLoader.raidLeaderIconTex);

		// Token: 0x04000069 RID: 105
		public static readonly Material agroIconMat = MaterialPool.MatFrom(TexturesLoader.agroIconTex);

		// Token: 0x0400006A RID: 106
		public static readonly Material animalagroIconMat = MaterialPool.MatFrom(TexturesLoader.animalagroIconTex);

		// Token: 0x0400006B RID: 107
		public static readonly Material lolIconMat = MaterialPool.MatFrom(TexturesLoader.lolIconTex);
	}
}
