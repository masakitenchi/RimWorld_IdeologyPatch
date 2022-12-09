using Verse;

namespace Rimatomics
{
	public class NuclearFuel
	{
		public bool IsFuel;

		public float PowerOutput = 1000f;

		public SimpleCurve thermalCurve;

		public SimpleCurve fastCurve;

		public bool mox;

		public float Du;

		public float Pu;
	}
}
