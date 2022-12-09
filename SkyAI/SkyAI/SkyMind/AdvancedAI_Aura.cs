using System;
using RimWorld;
using Verse;

namespace SkyMind
{
	// Token: 0x02000005 RID: 5
	public static class AdvancedAI_Aura
	{
		// Token: 0x0600000F RID: 15 RVA: 0x0000224C File Offset: 0x0000044C
		public static float AuraLevel(Pawn pawn)
		{
			return AdvancedAI_Aura.SeverityBySkillCurve.Evaluate((float)pawn.skills.GetSkill(SkillDefOf.Social).Level);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002280 File Offset: 0x00000480
		public static HediffDef AuraHediffDef(Pawn pawn)
		{
			bool flag = (double)Rand.Value > 0.75;
			HediffDef result;
			if (flag)
			{
				result = HediffDefOfAI.LeaderAvenger;
			}
			else
			{
				result = (AdvancedAI.IsMeleeVerb(pawn, null) ? HediffDefOfAI.LeaderWarlord : HediffDefOfAI.LeaderWarrior);
			}
			return result;
		}

		// Token: 0x04000005 RID: 5
		public static SimpleCurve SeverityBySkillCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(16f, 1f),
				true
			}
		};
	}
}
