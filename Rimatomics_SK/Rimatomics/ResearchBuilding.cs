using Verse;

namespace Rimatomics
{
	public class ResearchBuilding : Building
	{
		public int LastTickUsed;

		public virtual bool InUse => Find.TickManager.TicksGame < LastTickUsed + 250;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			DubUtils.GetResearch().NotifyResearch();
		}
	}
}
