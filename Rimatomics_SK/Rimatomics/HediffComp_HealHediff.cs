using System.Text;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class HediffComp_HealHediff : HediffComp
	{
		private HediffCompProperties_SeverityPerDay Props => (HediffCompProperties_SeverityPerDay)props;

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (props is HediffCompProperties_SeverityPerDay && Props.showHoursToRecover && SeverityChangePerDay() < 0f)
				{
					return Mathf.RoundToInt(parent.Severity / Mathf.Abs(SeverityChangePerDay()) * 24f).ToString() + "LetterHour".Translate();
				}
				return null;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				if (props is HediffCompProperties_SeverityPerDay && Props.showDaysToRecover && SeverityChangePerDay() < 0f)
				{
					return "DaysToRecover".Translate((parent.Severity / Mathf.Abs(SeverityChangePerDay())).ToString("0.0"));
				}
				return null;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			if (base.Pawn.IsHashIntervalTick(200))
			{
				float num = SeverityChangePerDay();
				num *= 0.00333333341f;
				Hediff firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(DubDef.RimatomicsRadiation);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += num;
				}
			}
		}

		protected virtual float SeverityChangePerDay()
		{
			return Props.severityPerDay;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!base.Pawn.Dead)
			{
				stringBuilder.AppendLine("severity/day: " + SeverityChangePerDay().ToString("F3"));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
