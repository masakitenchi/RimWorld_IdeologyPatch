using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Hediff_FatalRad : HediffWithComps
	{
		public override void Tick()
		{
			base.Tick();
			if (!pawn.IsHashIntervalTick(60))
			{
				return;
			}
			HediffSet hediffSet = pawn.health.hediffSet;
			bool flag = hediffSet.HasHediff(DubDef.RimatomicsRadiation);
			bool num = hediffSet.HasHediff(DubDef.RadiationIncurable);
			if (flag && hediffSet.GetFirstHediffOfDef(DubDef.RimatomicsRadiation).Severity > 0.5f && Rand.Chance(0.02f))
			{
				pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 2f));
			}
			if (num)
			{
				if (hediffSet.GetFirstHediffOfDef(DubDef.RadiationIncurable).Severity > 0.5f && Rand.Chance(0.04f))
				{
					pawn.health.DropBloodFilth();
				}
				if (Rand.Chance(0.02f))
				{
					pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 2f));
				}
			}
		}
	}
}
