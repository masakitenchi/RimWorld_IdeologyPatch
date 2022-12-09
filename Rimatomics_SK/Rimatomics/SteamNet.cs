using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class SteamNet : BasePipeNet
	{
		public float SteamLoopRatio;

		public List<reactorCore> Cores;

		public List<Turbine> Turbines;

		public int MeltLoop;

		public int ThingsCount;

		public override void InitNet()
		{
			base.InitNet();
			Cores = PipedThings.OfType<reactorCore>().InRandomOrder().ToList();
			Turbines = PipedThings.OfType<Turbine>().InRandomOrder().ToList();
			ThingsCount = PipedThings.Count;
			Tick();
		}

		public override void Tick()
		{
			float num = Cores.Sum((reactorCore x) => x.ThermalEnergy);
			float num2 = Turbines.Sum((Turbine x) => x.GenerationCapacity);
			SteamLoopRatio = 0f;
			if (num2 > 0f && ThingsCount > 0)
			{
				float num3 = (num2 - num) / num2;
				SteamLoopRatio = 1f - num3;
				MeltLoop++;
				if (MeltLoop >= ThingsCount)
				{
					MeltLoop = 0;
				}
				ThingWithComps thingWithComps = PipedThings[MeltLoop];
				SnowUtility.AddSnowRadial(thingWithComps.Position, thingWithComps.Map, 3f, -0.0300000012f);
			}
		}
	}
}
