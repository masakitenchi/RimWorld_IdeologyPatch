using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Transformer : Building
	{
		public float Capacity = 200000f;

		public CompPipe HighVoltage;

		public Sustainer humSustainer;

		public float MaxCap = 200000f;

		public bool Overcurrent;

		public CompPowerPlant power;

		private readonly StringBuilder stringBuilder = new StringBuilder();

		public Sustainer wickSustainer;

		public HighVoltageNet HighVoltageNet => HighVoltage.net as HighVoltageNet;

		public bool CanPop
		{
			get
			{
				if (power?.PowerNet != null)
				{
					return power.PowerNet.CurrentEnergyGainRate() > 0f;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref Capacity, "Capacity", 200000f);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			power = GetComp<CompPowerPlant>();
			HighVoltage = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.HighVoltage);
			DubUtils.GetResearch().NotifyResearch();
		}

		public void StartHumSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			humSustainer = SoundDef.Named("transformer").TrySpawnSustainer(info);
		}

		public void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			wickSustainer = SoundDef.Named("transformerDischarge").TrySpawnSustainer(info);
		}

		public override void Tick()
		{
			base.Tick();
			if (HighVoltage == null)
			{
				return;
			}
			power.PowerOutput = Capacity * HighVoltageNet.HighVoltageLoad;
			if (power.PowerOutput > 25000f)
			{
				if (humSustainer == null)
				{
					StartHumSustainer();
				}
				else if (humSustainer.Ended)
				{
					StartHumSustainer();
				}
				else
				{
					humSustainer.Maintain();
				}
			}
			if (Overcurrent)
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
				if (power.PowerOutput <= 100f)
				{
					Overcurrent = false;
				}
				if (this.IsHashIntervalTick(15))
				{
					FleckMaker.ThrowSmoke(this.TrueCenter() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), base.Map, 1.5f);
					FleckMaker.ThrowMicroSparks(this.TrueCenter() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), base.Map);
				}
				if (this.IsHashIntervalTick(5))
				{
					FleckMaker.ThrowLightningGlow(this.TrueCenter(), base.Map, 1f);
					TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1f));
				}
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (Overcurrent)
			{
				GenExplosion.DoExplosion(base.Position, base.Map, 5f, DamageDefOf.Flame, this);
			}
			base.Destroy(mode);
		}

		[SyncMethod(SyncContext.None)]
		public void uppers()
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Capacity += 1000f;
			}
			else
			{
				Capacity += 10000f;
			}
			Capacity = Mathf.Min(MaxCap, Capacity);
		}

		[SyncMethod(SyncContext.None)]
		public void downers()
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Capacity -= 1000f;
			}
			else
			{
				Capacity -= 10000f;
			}
			Capacity = Mathf.Max(0f, Capacity);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (power != null)
			{
				yield return new Command_Action
				{
					action = downers,
					defaultLabel = "LowerPowerLimit".Translate(),
					defaultDesc = "PowerLimitDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc5,
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/voltage")
				};
				yield return new Command_Action
				{
					action = uppers,
					defaultLabel = "RaisePowerLimit".Translate(),
					defaultDesc = "PowerLimitDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc3,
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/voltage")
				};
			}
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine();
			stringBuilder.Append("TurbinePower".Translate((HighVoltageNet.TurbineWatts / 1000f).ToString("0.0")));
			stringBuilder.AppendLine();
			stringBuilder.Append("TrannyLimit".Translate(Capacity / 1000f));
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
