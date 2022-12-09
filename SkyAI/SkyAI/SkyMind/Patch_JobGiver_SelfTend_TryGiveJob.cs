using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000050 RID: 80
	[HarmonyPatch(typeof(JobGiver_SelfTend))]
	[HarmonyPatch("TryGiveJob")]
	public class Patch_JobGiver_SelfTend_TryGiveJob
	{
		// Token: 0x060001F8 RID: 504 RVA: 0x0002C048 File Offset: 0x0002A248
		private static bool Prefix(ref Job __result, Pawn pawn)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = !pawn.RaceProps.Humanlike || !AdvancedAI.IsBioHumanlikeOnly(pawn);
				if (flag)
				{
					__result = null;
					result = false;
				}
				else
				{
					bool flag2 = !pawn.health.HasHediffsNeedingTend(false) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.InAggroMentalState;
					if (flag2)
					{
						__result = null;
						result = false;
					}
					else
					{
						bool flag3 = pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor);
						if (flag3)
						{
							__result = null;
							result = false;
						}
						else
						{
							Job job = JobMaker.MakeJob(JobDefOf.TendPatient, pawn);
							job.endAfterTendedOnce = true;
							__result = job;
							result = false;
						}
					}
				}
			}
			return result;
		}
	}
}
