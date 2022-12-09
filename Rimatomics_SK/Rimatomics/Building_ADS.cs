using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_ADS : Building_EnergyWeapon
	{
		private Sustainer sustainer;

		public override bool TurretBased => true;

		public float AOE
		{
			get
			{
				float num = base.GunProps.EnergyWep.AOE;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num += 10f;
				}
				return num;
			}
		}

		public override LocalTargetInfo TryFindNewTarget()
		{
			return null;
		}

		public override void Tick()
		{
			base.Tick();
			if (AttackVerb.Bursting && base.Spawned && (powerComp == null || powerComp.PowerOn) && base.AnyConsoles && (!base.GunProps.EnergyWep.NeedsManning || base.MannedConsole != null))
			{
				StartSustainerPoweredIfInactive();
			}
			else
			{
				EndSustainerPoweredIfActive();
			}
		}

		private void StartSustainerPoweredIfInactive()
		{
			if (sustainer == null)
			{
				SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
				sustainer = DubDef.ADS_Discharge.TrySpawnSustainer(info);
			}
			else
			{
				sustainer.Maintain();
			}
		}

		private void EndSustainerPoweredIfActive()
		{
			if (sustainer != null)
			{
				sustainer.End();
				sustainer = null;
			}
		}
	}
}
