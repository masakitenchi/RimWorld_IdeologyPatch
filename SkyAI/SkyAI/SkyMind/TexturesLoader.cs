using System;
using SkyAI.Properties;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x0200002B RID: 43
	[StaticConstructorOnStartup]
	public static class TexturesLoader
	{
		// Token: 0x06000163 RID: 355 RVA: 0x000200D4 File Offset: 0x0001E2D4
		public static void InitTextures()
		{
			Texture2D texture2D = new Texture2D(1000, 167, TextureFormat.ARGB32, false);
			texture2D.LoadImage(TexturesLoader.logo);
			TexturesLoader.IconTex = texture2D;
			Texture2D tex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex.LoadImage(TexturesLoader.leaderIcon);
			TexturesLoader.leaderIconTex = tex;
			Texture2D tex2 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex2.LoadImage(TexturesLoader.doctorIcon);
			TexturesLoader.doctorIconTex = tex2;
			Texture2D tex3 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex3.LoadImage(TexturesLoader.traderIcon);
			TexturesLoader.traderIconTex = tex3;
			Texture2D tex4 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex4.LoadImage(TexturesLoader.raidLeaderIcon);
			TexturesLoader.raidLeaderIconTex = tex4;
			Texture2D tex5 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex5.LoadImage(TexturesLoader.agroIcon);
			TexturesLoader.agroIconTex = tex5;
			Texture2D tex6 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex6.LoadImage(TexturesLoader.animalagroIcon);
			TexturesLoader.animalagroIconTex = tex6;
			Texture2D tex7 = new Texture2D(64, 64, TextureFormat.ARGB32, false);
			tex7.LoadImage(TexturesLoader.lolIcon);
			TexturesLoader.lolIconTex = tex7;
		}

		// Token: 0x04000076 RID: 118
		public static Texture2D IconTex;

		// Token: 0x04000077 RID: 119
		public static byte[] logo = SkyAI.Properties.Resources.menuLogo;

		// Token: 0x04000078 RID: 120
		public static Texture2D leaderIconTex;

		// Token: 0x04000079 RID: 121
		public static byte[] leaderIcon = SkyAI.Properties.Resources.leader;

		// Token: 0x0400007A RID: 122
		public static Texture2D doctorIconTex;

		// Token: 0x0400007B RID: 123
		public static byte[] doctorIcon = SkyAI.Properties.Resources.doctor;

		// Token: 0x0400007C RID: 124
		public static Texture2D traderIconTex;

		// Token: 0x0400007D RID: 125
		public static byte[] traderIcon = SkyAI.Properties.Resources.trader;

		// Token: 0x0400007E RID: 126
		public static Texture2D raidLeaderIconTex;

		// Token: 0x0400007F RID: 127
		public static byte[] raidLeaderIcon = SkyAI.Properties.Resources.raidleader;

		// Token: 0x04000080 RID: 128
		public static Texture2D agroIconTex;

		// Token: 0x04000081 RID: 129
		public static byte[] agroIcon = SkyAI.Properties.Resources.agro;

		// Token: 0x04000082 RID: 130
		public static Texture2D animalagroIconTex;

		// Token: 0x04000083 RID: 131
		public static byte[] animalagroIcon = SkyAI.Properties.Resources.animalagro;

		// Token: 0x04000084 RID: 132
		public static Texture2D lolIconTex;

		// Token: 0x04000085 RID: 133
		public static byte[] lolIcon = SkyAI.Properties.Resources.lol;
	}
}
