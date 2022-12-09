using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class BasePipeNet
	{
		public int NetType;

		public int NetID;

		public List<ThingWithComps> PipedThings = new List<ThingWithComps>();

		public List<CompPipe> Pipes = new List<CompPipe>();

		public MapComponent_Rimatomics MapComp;

		public virtual void InitNet()
		{
			PipedThings.RemoveDuplicates();
			PipedThings = PipedThings.InRandomOrder().ToList();
		}

		public void DeregisterPipe(ThingWithComps thing)
		{
			PipedThings.Remove(thing);
			InitNet();
		}

		public virtual void Tick()
		{
		}
	}
}
