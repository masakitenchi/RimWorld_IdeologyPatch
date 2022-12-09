using RimWorld;
using Verse;

namespace Rimatomics
{
	public class FuelValueStat : StatPart
	{
		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing is Item_NuclearFuel item_NuclearFuel)
			{
				float num = GenMath.LerpDoubleClamped(0f, 1f, 0.1f, 1f, item_NuclearFuel.FuelLevel);
				return "FuelLevelOffset".Translate(num);
			}
			return null;
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && req.Thing is Item_NuclearFuel item_NuclearFuel)
			{
				val *= GenMath.LerpDoubleClamped(0f, 1f, 0.1f, 1f, item_NuclearFuel.FuelLevel);
			}
		}
	}
}
