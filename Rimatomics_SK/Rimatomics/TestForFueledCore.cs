using System.Linq;
using Verse;

namespace Rimatomics
{
	public class TestForFueledCore : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.PipeNets.OfType<SteamNet>().Any((SteamNet x) => x.Cores.Any((reactorCore c) => c.RealControlRodPosition > 0.99f));
		}
	}
}
