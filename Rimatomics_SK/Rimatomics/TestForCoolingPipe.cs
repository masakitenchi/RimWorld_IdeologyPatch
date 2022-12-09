using System.Linq;
using Verse;

namespace Rimatomics
{
	public class TestForCoolingPipe : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.PipeNets.OfType<CoolingNet>().Any((CoolingNet x) => x.Coolers.Any());
		}
	}
}
