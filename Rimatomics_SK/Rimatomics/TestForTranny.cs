using System.Linq;
using Verse;

namespace Rimatomics
{
	public class TestForTranny : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.PipeNets.OfType<HighVoltageNet>().Any((HighVoltageNet x) => x.Trannys.Any() && x.Turbines.Any());
		}
	}
}
