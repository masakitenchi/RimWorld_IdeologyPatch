using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class HighVoltageNet : BasePipeNet
	{
		public float TurbineWatts;

		public float HighVoltageLoad;

		public List<Turbine> Turbines;

		public List<Transformer> Trannys;

		public override void InitNet()
		{
			base.InitNet();
			Trannys = PipedThings.OfType<Transformer>().InRandomOrder().ToList();
			Turbines = PipedThings.OfType<Turbine>().InRandomOrder().ToList();
			Tick();
		}

		public override void Tick()
		{
			TurbineWatts = Turbines.Sum((Turbine x) => x.powerOutput);
			float num = Trannys.Sum((Transformer x) => x.Capacity);
			if (num > 0f)
			{
				float num2 = (num - TurbineWatts) / num;
				HighVoltageLoad = Mathf.Clamp01(1f - num2);
			}
		}
	}
}
