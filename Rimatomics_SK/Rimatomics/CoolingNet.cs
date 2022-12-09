using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class CoolingNet : BasePipeNet
	{
		public float CoolingCapacity;

		public float CoolingLoopRatio;

		public List<CoolingSystem> Coolers;

		public List<Turbine> Turbines;

		public override void InitNet()
		{
			base.InitNet();
			Coolers = PipedThings.OfType<CoolingSystem>().InRandomOrder().ToList();
			Turbines = PipedThings.OfType<Turbine>().InRandomOrder().ToList();
			Tick();
		}

		public override void Tick()
		{
			CoolingLoopRatio = 0f;
			if (Coolers.Any())
			{
				float num = Turbines.Sum((Turbine x) => x.UncappedPowerGeneration);
				CoolingCapacity = Coolers.Sum((CoolingSystem x) => x.coolingCapacity);
				if (CoolingCapacity > 1f)
				{
					float num2 = (CoolingCapacity - num) / CoolingCapacity;
					CoolingLoopRatio = 1f - num2;
				}
			}
		}
	}
}
