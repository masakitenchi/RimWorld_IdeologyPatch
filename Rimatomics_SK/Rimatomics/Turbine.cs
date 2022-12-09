using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Turbine : Building
	{
		public CompPipe Cooling;

		public CompPipe HighVoltage;

		public CompPowerTrader powerComp;

		public float powerOutput;

		public float RadiationLeak;

		public float RPM;

		private float SpinVelocity;

		public CompPipe Steam;

		public float UncappedPowerGeneration;

		public float UncooledWater;

		private Sustainer wickSustainer;

		private StringBuilder stringBuilder = new StringBuilder();

		public SteamNet SteamNet => Steam.net as SteamNet;

		public CoolingNet CoolingNet => Cooling.net as CoolingNet;

		public HighVoltageNet highVoltageNet => HighVoltage.net as HighVoltageNet;

		public virtual float GenerationCapacity => ((RimatomicsThingDef)def).TurbineCapacityWatts;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Steam = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Steam);
			Cooling = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Cooling);
			HighVoltage = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.HighVoltage);
			powerComp = GetComp<CompPowerTrader>();
			DubUtils.GetResearch().NotifyResearch();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref SpinVelocity, "SpinVelocity", 0f);
			Scribe_Values.Look(ref powerOutput, "powerOutput", 0f);
			Scribe_Values.Look(ref RPM, "RPM", 0f);
		}

		public void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			wickSustainer = DubDef.GeothermalPlant_Ambience.TrySpawnSustainer(info);
		}

		public override void Tick()
		{
			base.Tick();
			if (this.IsHashIntervalTick(10) && RadiationLeak > 0f)
			{
				DubUtils.emitRadiation(base.Position, RadiationLeak, 12f, base.Map);
			}
			UncappedPowerGeneration = GenerationCapacity * SteamNet.SteamLoopRatio;
			if (CoolingNet.CoolingCapacity > 0f)
			{
				if (CoolingNet.CoolingLoopRatio < 1f)
				{
					UncooledWater = 0f;
				}
				else
				{
					UncooledWater = UncappedPowerGeneration * Mathf.Min(2f, CoolingNet.CoolingLoopRatio) - UncappedPowerGeneration;
				}
			}
			else
			{
				UncooledWater = UncappedPowerGeneration;
			}
			float target = Mathf.Clamp01(SteamNet.SteamLoopRatio);
			RPM = Mathf.SmoothDamp(RPM, target, ref SpinVelocity, 1f, 100f, 0.0166f);
			powerOutput = GenerationCapacity * RPM;
			if (RPM > 0.01f)
			{
				if (wickSustainer == null)
				{
					StartWickSustainer();
				}
				else if (wickSustainer.Ended)
				{
					StartWickSustainer();
				}
				else
				{
					wickSustainer.Maintain();
				}
			}
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.AppendLine(base.GetInspectString());
			if (DebugSettings.godMode)
			{
				stringBuilder.AppendLine("UncooledWater: " + UncooledWater);
			}
			stringBuilder.AppendLine("TurbineRPM".Translate(Mathf.Lerp(0f, 3600f, RPM).ToString("0"), RPM.ToStringPercent("0.0")));
			stringBuilder.AppendLine("critPowerGeneration".Translate(powerOutput.ToString("0.00")));
			stringBuilder.Append("critCoolingCapShare".Translate(CoolingNet.CoolingLoopRatio.ToStringPercent("0.0"), CoolingNet.Turbines.Count));
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
