using System.Linq;
using Verse;

namespace Rimatomics
{
	public class TestForReactorControl : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.PipeNets.OfType<LoomNet>().Any((LoomNet x) => x.Cores.Any() && x.Consoles.Any());
		}
	}
}
