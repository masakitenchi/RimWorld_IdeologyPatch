using Verse;

namespace Rimatomics
{
	public class TestForWeaponsConsole : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.Consoles.Any((WeaponsConsole x) => x.currentPPC > 0f);
		}
	}
}
