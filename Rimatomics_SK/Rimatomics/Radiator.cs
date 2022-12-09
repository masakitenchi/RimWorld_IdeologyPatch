using UnityEngine;
using Verse;

namespace Rimatomics
{
	internal class Radiator : CoolingSystem
	{
		private bool hasBoilerComp;

		public override float coolingCapacity
		{
			get
			{
				if ((powerComp == null || powerComp.PowerOn) && (fuel == null || fuel.HasFuel))
				{
					return ((RimatomicsThingDef)def).CoolingCapacityWatts;
				}
				return 0f;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			hasBoilerComp = ((ThingWithComps)this).comps.Any((ThingComp x) => x.props.compClass.ToString().Contains("CompBoilerNuclear"));
		}

		public override void Tick()
		{
			base.Tick();
			if (!hasBoilerComp && this.IsHashIntervalTick(30))
			{
				float coolingLoopRatio = base.CoolingNet.CoolingLoopRatio;
				if (coolingLoopRatio > 0f && powerComp.PowerOn)
				{
					float ambientTemperature = base.AmbientTemperature;
					float num = ((ambientTemperature < 20f) ? 1f : ((!(ambientTemperature > 200f)) ? Mathf.InverseLerp(200f, 20f, ambientTemperature) : 0f));
					float energy = 80f * num * coolingLoopRatio;
					GenTemperature.PushHeat(this, energy);
				}
			}
		}
	}
}
