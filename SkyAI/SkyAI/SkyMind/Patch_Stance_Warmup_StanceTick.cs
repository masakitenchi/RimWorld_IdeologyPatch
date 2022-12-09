using System;
using HarmonyLib;
using Verse;

namespace SkyMind
{
	// Token: 0x0200004E RID: 78
	[HarmonyPatch(typeof(Stance_Warmup))]
	[HarmonyPatch("StanceTick")]
	public class Patch_Stance_Warmup_StanceTick
	{
		// Token: 0x060001F3 RID: 499 RVA: 0x0002BE28 File Offset: 0x0002A028
		private static void Postfix(ref Stance_Warmup __instance, LocalTargetInfo ___focusTarg, int ___ticksLeft)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = ___focusTarg.HasThing && ___focusTarg.Thing is Pawn;
				if (flag)
				{
					bool flag2 = !Patch_Stance_Warmup_StanceTick.used;
					if (flag2)
					{
						Pawn pawn = (Pawn)___focusTarg.Thing;
						bool flag3 = !pawn.IsColonist && AdvancedAI.IsHumanlikeOnly(pawn) && Find.TickManager.TicksGame - pawn.mindState.lastHarmTick > 170;
						if (flag3)
						{
							bool flag4 = __instance.verb != null && __instance.verb.Caster != null && AdvancedAI.ShouldOverrideJob(pawn);
							if (flag4)
							{
								pawn.mindState.lastHarmTick = Find.TickManager.TicksGame;
								pawn.mindState.enemyTarget = __instance.verb.Caster;
								pawn.jobs.CheckForJobOverride();
							}
						}
						Patch_Stance_Warmup_StanceTick.used = true;
					}
				}
				bool flag5 = ___ticksLeft <= 0;
				if (flag5)
				{
					Patch_Stance_Warmup_StanceTick.used = false;
				}
			}
		}

		// Token: 0x0400011E RID: 286
		private static bool used;
	}
}
