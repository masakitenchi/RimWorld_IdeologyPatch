using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class CoolingSystem : Building
	{
		public CompHeatPusherPowered heatpusher;

		public CompPipe Cooling;

		public CompRefuelable fuel;

		public CompPowerTrader powerComp;

		public bool Stalled;

		private StringBuilder stringBuilder = new StringBuilder();

		public CoolingNet CoolingNet => Cooling.net as CoolingNet;

		public float CoolingCapacityWatts => ((RimatomicsThingDef)def).CoolingCapacityWatts;

		public virtual float coolingCapacity
		{
			get
			{
				if ((powerComp == null || powerComp.PowerOn) && (fuel == null || fuel.HasFuel))
				{
					return CoolingCapacityWatts;
				}
				return 0f;
			}
		}

		public virtual float WaterUsage => 1f;

		public virtual float CapUsed()
		{
			return CoolingNet.CoolingLoopRatio;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Cooling = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Cooling);
			heatpusher = GetComp<CompHeatPusherPowered>();
			powerComp = GetComp<CompPowerTrader>();
			fuel = GetComp<CompRefuelable>();
			DubUtils.GetResearch().NotifyResearch();
		}

		public override void Tick()
		{
			base.Tick();
			if (CoolingNet.CoolingLoopRatio > 0f)
			{
				fuel?.ConsumeFuel(WaterUsage / 60f);
			}
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.Append(CoolingNet.CoolingLoopRatio.ToStringPercent("0.00"));
			return stringBuilder.ToString();
		}
	}
}
