using System.Linq;
using Verse;

namespace Rimatomics
{
	public class TestForCore : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			bool num = bench.mapcomp.PipeNets.OfType<SteamNet>().Any((SteamNet z) => z.Turbines.Any() && z.Cores.Any());
			bool flag = bench.mapcomp.PipeNets.OfType<ColdWaterNet>().Any((ColdWaterNet z) => z.Turbines.Any() && z.Cores.Any());
			return num && flag;
		}
	}
}
