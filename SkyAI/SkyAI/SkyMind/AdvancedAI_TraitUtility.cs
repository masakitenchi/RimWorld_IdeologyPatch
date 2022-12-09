using System;
using System.Collections.Generic;
using CombatExtended;
using RimWorld;
using SK;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x02000033 RID: 51
	public static class AdvancedAI_TraitUtility
	{
		// Token: 0x06000198 RID: 408 RVA: 0x00024ADC File Offset: 0x00022CDC
		public static bool HasDumbTrait(Pawn pawn)
		{
			bool flag = pawn.story != null && pawn.story.traits != null && pawn.story.traits.allTraits.Count > 0;
			if (flag)
			{
				foreach (Trait trait in pawn.story.traits.allTraits)
				{
					bool flag2 = AdvancedAI_TraitUtility.dumbList.Contains(new AdvancedAI_TraitUtility.TraitDefWithDegree(trait.def, trait.Degree));
					if (flag2)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x00024B9C File Offset: 0x00022D9C
		public static bool HasTrait(Pawn pawn, TraitDef trait, int degree = 0)
		{
			bool flag = pawn.story == null || pawn.story.traits == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.story.traits.HasTrait(trait) && pawn.story.traits.GetTrait(trait).Degree == degree;
				result = flag2;
			}
			return result;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00024C0C File Offset: 0x00022E0C
		public static float FleeChance(Pawn pawn)
		{
			float num = 0f;
			AdvancedAI_TendUtility.InjurySeverity injurySeverity;
			AdvancedAI_TendUtility.RequireTreatment(pawn, out injurySeverity);
			bool flag = injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.severe;
			if (flag)
			{
				num += 0.7f;
			}
			bool flag2 = injurySeverity == AdvancedAI_TendUtility.InjurySeverity.extreme;
			if (flag2)
			{
				num += 0.38f;
			}
			bool flag3 = injurySeverity == AdvancedAI_TendUtility.InjurySeverity.minor;
			if (flag3)
			{
				num += 0.1f;
			}
			CompSuppressable comp = pawn.GetComp<CompSuppressable>();
			bool flag4 = comp != null && comp.isSuppressed;
			if (flag4)
			{
				num += 0.4f;
			}
			CompLeaderRole comp2 = pawn.GetComp<CompLeaderRole>();
			bool flag5 = comp2 != null;
			if (flag5)
			{
				num -= 0.4f;
			}
			CompSquadCommanderRole comp3 = pawn.GetComp<CompSquadCommanderRole>();
			bool flag6 = comp3 != null;
			if (flag6)
			{
				num -= 0.4f;
			}
			bool flag7 = pawn.story != null && pawn.story.traits != null && pawn.story.traits.allTraits.Count > 0;
			if (flag7)
			{
				num += AdvancedAI_TraitUtility.BraveryVsCowardly(pawn);
			}
			bool flag8 = pawn.Faction != null && pawn.Faction.def == FactionDefOfAI.OgreMutants;
			if (flag8)
			{
				num += 0.3f;
			}
			bool flag9 = num < 0f;
			if (flag9)
			{
				num = 0f;
			}
			return Mathf.Max(num, 1f);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00024D58 File Offset: 0x00022F58
		public static float BraveryVsCowardly(Pawn pawn)
		{
			float num = 0f;
			bool flag = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Paranoid, 0);
			if (flag)
			{
				num += 0.05f;
			}
			bool flag2 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Pragmatist, 0);
			if (flag2)
			{
				num += 0.12f;
			}
			bool flag3 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.ChaoticGenius, 0);
			if (flag3)
			{
				num += 0.1f;
			}
			bool flag4 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Constitution, 0);
			if (flag4)
			{
				num += 0.07f;
			}
			bool flag5 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Wimp, 0);
			if (flag5)
			{
				num += 0.1f;
			}
			bool flag6 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.NaturalMood, -2);
			if (flag6)
			{
				num += 0.1f;
			}
			bool flag7 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.NaturalMood, -1);
			if (flag7)
			{
				num += 0.06f;
			}
			bool flag8 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Nerves, -2);
			if (flag8)
			{
				num += 0.2f;
			}
			bool flag9 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Nerves, -1);
			if (flag9)
			{
				num += 0.12f;
			}
			bool flag10 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Neurotic, 2);
			if (flag10)
			{
				num += 0.11f;
			}
			bool flag11 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Neurotic, 1);
			if (flag11)
			{
				num += 0.06f;
			}
			bool flag12 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Bravery, -1);
			if (flag12)
			{
				num += 0.3f;
			}
			bool flag13 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Nerves, 2);
			if (flag13)
			{
				num -= 0.3f;
			}
			bool flag14 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Weak, 0);
			if (flag14)
			{
				num -= 0.25f;
			}
			bool flag15 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Nerves, 1);
			if (flag15)
			{
				num -= 0.17f;
			}
			bool flag16 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.NaturalMood, 2);
			if (flag16)
			{
				num -= 0.1f;
			}
			bool flag17 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.NaturalMood, 1);
			if (flag17)
			{
				num -= 0.06f;
			}
			bool flag18 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Tough, 0);
			if (flag18)
			{
				num -= 0.15f;
			}
			bool flag19 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Psychopath, 0);
			if (flag19)
			{
				num -= 0.12f;
			}
			bool flag20 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Masochist, 0);
			if (flag20)
			{
				num -= 0.16f;
			}
			bool flag21 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Brawler, 0);
			if (flag21)
			{
				num -= 0.12f;
			}
			bool flag22 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOf.Bloodlust, 0);
			if (flag22)
			{
				num -= 0.14f;
			}
			bool flag23 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Personality, 1);
			if (flag23)
			{
				num -= 0.17f;
			}
			bool flag24 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Bravery, 1);
			if (flag24)
			{
				num -= 0.3f;
			}
			bool flag25 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Bravery, 2);
			if (flag25)
			{
				num -= 0.4f;
			}
			bool flag26 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfLocal.Hardened, 0);
			if (flag26)
			{
				num -= 0.3f;
			}
			bool flag27 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfLocal.Confident, 0);
			if (flag27)
			{
				num -= 0.22f;
			}
			bool flag28 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfLocal.Numb, 0);
			if (flag28)
			{
				num -= 0.2f;
			}
			bool flag29 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Dumb, 0);
			if (flag29)
			{
				num -= 0.12f;
			}
			bool flag30 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Reaver, 0);
			if (flag30)
			{
				num -= 0.14f;
			}
			bool flag31 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Strong, 0);
			if (flag31)
			{
				num -= 0.1f;
			}
			bool flag32 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Ignorant, 0);
			if (flag32)
			{
				num -= 0.1f;
			}
			bool flag33 = AdvancedAI_TraitUtility.HasTrait(pawn, TraitDefOfAI.Constitution, 1);
			if (flag33)
			{
				num -= 0.08f;
			}
			return Mathf.Clamp(num, 0f, 1f);
		}

		// Token: 0x04000107 RID: 263
		public static List<AdvancedAI_TraitUtility.TraitDefWithDegree> dumbList = new List<AdvancedAI_TraitUtility.TraitDefWithDegree>
		{
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Dumb, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.Nerves, -1),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.Nerves, -2),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.TooSmart, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.Psychopath, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Ignorant, 0)
		};

		// Token: 0x04000108 RID: 264
		public static List<AdvancedAI_TraitUtility.TraitDefWithDegree> braveList = new List<AdvancedAI_TraitUtility.TraitDefWithDegree>
		{
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Bravery, -1),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Neurotic, 1),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Neurotic, 2),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.Nerves, -1),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.Nerves, -2),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.NaturalMood, -1),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOf.NaturalMood, -2),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Wimp, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Constitution, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.ChaoticGenius, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Pragmatist, 0),
			new AdvancedAI_TraitUtility.TraitDefWithDegree(TraitDefOfAI.Paranoid, 0)
		};

		// Token: 0x020000D2 RID: 210
		public struct TraitDefWithDegree
		{
			// Token: 0x06000455 RID: 1109 RVA: 0x00040C7B File Offset: 0x0003EE7B
			public TraitDefWithDegree(TraitDef t, int d)
			{
				this.traitDef = t;
				this.degree = d;
			}

			// Token: 0x0400029A RID: 666
			public TraitDef traitDef;

			// Token: 0x0400029B RID: 667
			public int degree;
		}
	}
}
