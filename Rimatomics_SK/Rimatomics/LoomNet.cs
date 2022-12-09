using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class LoomNet : BasePipeNet
	{
		public List<ReactorControl> Consoles;

		public List<reactorCore> Cores;

		public override void InitNet()
		{
			base.InitNet();
			Cores = PipedThings.OfType<reactorCore>().InRandomOrder().ToList();
			Consoles = PipedThings.OfType<ReactorControl>().InRandomOrder().ToList();
		}

		public override void Tick()
		{
		}
	}
}
