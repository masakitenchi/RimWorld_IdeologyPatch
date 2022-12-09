using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	public class RimatomicsThingDef : ThingDef
	{
		public string turretTopGraphicPath;

		public NuclearFuel nuclearFuel;

		public EnergyWep EnergyWep;

		public List<ThingDef> Bilderbergs;

		public float CoolingCapacityWatts = 25000f;

		public float TurbineCapacityWatts = 100000f;

		public List<ResearchStepDef> StepsThatUnlock;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			StepsThatUnlock = DefDatabase<ResearchStepDef>.AllDefsListForReading.Where((ResearchStepDef x) => x.Unlocks.Contains(this)).ToList();
		}
	}
}
