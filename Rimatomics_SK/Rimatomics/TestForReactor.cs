using System.Linq;

namespace Rimatomics
{
	public class TestForReactor : ResearchStepDef
	{
		public override bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return bench.Map.listerThings.ThingsOfDef(DubDef.ResearchReactor).OfType<Building_ResearchReactor>().Any((Building_ResearchReactor x) => x.powerComp.PowerOn);
		}
	}
}
