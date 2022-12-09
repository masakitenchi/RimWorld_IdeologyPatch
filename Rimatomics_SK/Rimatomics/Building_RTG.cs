using System.Text;
using Verse;

namespace Rimatomics
{
	public class Building_RTG : Building
	{
		private StringBuilder stringBuilder = new StringBuilder();

		public override void Tick()
		{
			base.Tick();
			if (this.IsHashIntervalTick(60) && HitPoints < base.MaxHitPoints)
			{
				float num = GenMath.LerpDoubleClamped(1f, 0f, 0f, 6f, (float)HitPoints / (float)base.MaxHitPoints);
				DubUtils.emitRadiation(base.Position, num, num, base.Map);
			}
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine();
			if (HitPoints < base.MaxHitPoints / 10)
			{
				stringBuilder.Append("critreactorcritical".Translate());
			}
			else if (HitPoints < base.MaxHitPoints)
			{
				stringBuilder.Append("critreactorleaking".Translate());
			}
			else
			{
				stringBuilder.Append("critreactionstable".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
