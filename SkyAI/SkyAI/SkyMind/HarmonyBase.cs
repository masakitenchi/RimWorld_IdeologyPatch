using System;
using System.Reflection;
using HarmonyLib;

namespace SkyMind
{
	// Token: 0x02000037 RID: 55
	public static class HarmonyBase
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060001BA RID: 442 RVA: 0x00028504 File Offset: 0x00026704
		internal static Harmony instance
		{
			get
			{
				bool flag = HarmonyBase.harmony == null;
				if (flag)
				{
					HarmonyBase.harmony = (HarmonyBase.harmony = new Harmony("net.skyarkhangel.SkyAI"));
				}
				return HarmonyBase.harmony;
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0002853C File Offset: 0x0002673C
		public static void InitPatches()
		{
			Harmony.DEBUG = false;
			HarmonyBase.instance.PatchAll(Assembly.GetExecutingAssembly());
		}

		// Token: 0x04000112 RID: 274
		private static Harmony harmony;
	}
}
