using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200004D RID: 77
	[HarmonyPatch(typeof(Pawn_JobTracker))]
	[HarmonyPatch("StartJob")]
	public class Patch_Pawn_JobTracker_TryFindAndStartJob
	{
		// Token: 0x060001F1 RID: 497 RVA: 0x0002BCD0 File Offset: 0x00029ED0
		private static bool Prefix(ref Pawn_JobTracker __instance, Job newJob, ref Pawn ___pawn)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				bool flag = ___pawn.Map == null || ___pawn.Fogged() || ___pawn.Faction == null || ___pawn.Faction == Faction.OfMechanoids;
				if (flag)
				{
					result = true;
				}
				else
				{
					bool flag2 = !AdvancedAI.IsActivePawn(___pawn);
					if (flag2)
					{
						result = true;
					}
					else
					{
						Pawn pawn;
						bool flag3 = newJob != null && newJob.def != JobDefOf.Wait && AdvancedAI_TendUtility.ReservedByDoctor(___pawn, out pawn) && pawn.Position.DistanceTo(___pawn.Position) <= 2f;
						if (flag3)
						{
							bool flag4 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(___pawn) || SkyAiCore.SelectedPawnDebug(pawn));
							if (flag4)
							{
								Log.Message(string.Format("{0} {1}: From Harmony TryFindAndStartJob patch fix. Is waiting doctor.", ___pawn, ___pawn.Position));
							}
							Job job = JobMaker.MakeJob(JobDefOf.Wait, ___pawn.Position);
							job.expiryInterval = AdvancedAI.Interval(AdvancedAI.ExpireInterval.slow);
							job.checkOverrideOnExpire = true;
							___pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, false, true, null, null, false, false);
							result = false;
						}
						else
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
	}
}
