using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000058 RID: 88
	[HarmonyPatch(typeof(LordManager))]
	[HarmonyPatch("LordManagerTick")]
	public static class Patch_LordManager_LordManagerTick
	{
		// Token: 0x0600020A RID: 522 RVA: 0x0002D7A8 File Offset: 0x0002B9A8
		private static void Postfix(LordManager __instance)
		{
			bool flag = Find.TickManager.TicksGame % 60000 == 0;
			if (flag)
			{
				bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
				if (!debugDisableSkyAI)
				{
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					List<Lord> lords = __instance.lords;
					bool flag2 = !lords.NullOrEmpty<Lord>() && lords.Count > 0;
					if (flag2)
					{
						num = lords.Count;
						IEnumerable<Lord> enumerable = from lord in lords
						where lord.ownedPawns.NullOrEmpty<Pawn>()
						select lord;
						bool flag3 = !enumerable.EnumerableNullOrEmpty<Lord>() && enumerable.Count<Lord>() > 0;
						if (flag3)
						{
							num2 = enumerable.Count<Lord>();
							for (int i = num2 - 1; i >= 0; i--)
							{
								__instance.RemoveLord(enumerable.ElementAt(i));
							}
						}
						num3 = num - lords.Count;
					}
					bool devMode = Prefs.DevMode;
					if (devMode)
					{
						Log.Message(string.Format("Lords cleaning! Found {0} lords in home map. {1} of those have empty pawn list. Cleaned {2} lords.", num, num2, num3));
					}
				}
			}
		}
	}
}
