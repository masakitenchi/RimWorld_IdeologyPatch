using Verse;

namespace Rimatomics
{
	public class TestForPPCCharge : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.mapcomp.PPCs.Any((Building_PPC x) => x.batt.StoredEnergyPct > 0.5f);
		}
	}
}
