using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000047 RID: 71
	[HarmonyPatch(typeof(LordToil_Siege))]
	[HarmonyPatch("UpdateAllDuties")]
	public class Patch_LordToil_Siege_UpdateAllDuties_Prefix
	{
		// Token: 0x060001E4 RID: 484 RVA: 0x0002A898 File Offset: 0x00028A98
		public static bool Prefix(ref LordToil_Siege __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				Lord lord = __instance.lord;
				bool flag = lord != null;
				if (flag)
				{
					for (int i = 0; i < lord.ownedPawns.Count; i++)
					{
						Pawn pawn = __instance.lord.ownedPawns[i];
						bool flag2 = pawn != null;
						if (flag2)
						{
							bool flag3 = pawn.mindState.duty == null;
							if (flag3)
							{
								bool flag4 = false;
								Dictionary<Pawn, DutyDef> rememberedDuties = __instance.rememberedDuties;
								bool flag5 = !rememberedDuties.EnumerableNullOrEmpty<KeyValuePair<Pawn, DutyDef>>();
								if (flag5)
								{
									foreach (KeyValuePair<Pawn, DutyDef> keyValuePair in rememberedDuties)
									{
										bool flag6 = keyValuePair.Key == pawn;
										if (flag6)
										{
											bool flag7 = keyValuePair.Value != null;
											if (flag7)
											{
												pawn.mindState.duty = new PawnDuty(keyValuePair.Value);
												flag4 = true;
												bool debugLog = SkyAiCore.Settings.debugLog;
												if (debugLog)
												{
													Log.Message(string.Format("LordToil_Siege. UpdateAllDuties. Null duty fix1. Pawn: {0} Set duty: {1}", pawn, pawn.mindState.duty));
												}
											}
										}
									}
								}
								bool flag8 = !flag4;
								if (flag8)
								{
									IEnumerable<Pawn> source = from k in lord.ownedPawns
									where k != pawn && k.mindState.duty != null
									select k;
									IEnumerable<Pawn> enumerable = from m in source
									where m.mindState.duty.def == DutyDefOf.Defend
									select m;
									bool flag9 = !enumerable.EnumerableNullOrEmpty<Pawn>();
									if (flag9)
									{
										pawn.mindState.duty = enumerable.FirstOrDefault<Pawn>().mindState.duty;
										bool debugLog2 = SkyAiCore.Settings.debugLog;
										if (debugLog2)
										{
											Log.Message(string.Format("LordToil_Siege. UpdateAllDuties. Null duty fix2. Pawn: {0} Set duty: {1}", pawn, pawn.mindState.duty));
										}
									}
									else
									{
										pawn.mindState.duty = source.FirstOrDefault<Pawn>().mindState.duty;
										bool debugLog3 = SkyAiCore.Settings.debugLog;
										if (debugLog3)
										{
											Log.Message(string.Format("LordToil_Siege. UpdateAllDuties. Null duty fix3. Pawn: {0} Set duty: {1}", pawn, pawn.mindState.duty));
										}
									}
								}
							}
						}
					}
				}
				result = true;
			}
			return result;
		}
	}
}
