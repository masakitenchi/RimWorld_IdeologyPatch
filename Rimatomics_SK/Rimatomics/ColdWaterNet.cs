using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class ColdWaterNet : BasePipeNet
	{
		public float ColdWaterFeedReturn;

		public List<reactorCore> Cores;

		public List<Turbine> Turbines;

		public override void InitNet()
		{
			base.InitNet();
			Cores = PipedThings.OfType<reactorCore>().InRandomOrder().ToList();
			Turbines = PipedThings.OfType<Turbine>().InRandomOrder().ToList();
		}

		public override void Tick()
		{
			if (Turbines.Any())
			{
				ColdWaterFeedReturn = Turbines.Sum((Turbine x) => x.UncooledWater) / (float)Cores.Count;
			}
			else
			{
				ColdWaterFeedReturn = Cores.Sum((reactorCore x) => x.ThermalEnergy);
			}
		}
	}
}
