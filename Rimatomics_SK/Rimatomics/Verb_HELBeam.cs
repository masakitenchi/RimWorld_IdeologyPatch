using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Verb_HELBeam : Verb_RimatomicsVerb
	{
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, (!currentTarget.HasThing) ? null : currentTarget.Thing, (base.EquipmentSource == null) ? null : base.EquipmentSource.def, null, burst: false));
			base.GetWep.GunProps.EnergyWep.ChargeUpSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(caster)));
			base.GetWep.PrototypeBang(base.GetWep.GunProps.EnergyWep.PrototypeFailureChance);
		}

		public override bool Available()
		{
			Building_EnergyWeapon getWep = base.GetWep;
			return getWep.HasCharge(getWep.PulseSize / (float)ShotsPerBurst);
		}

		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			return true;
		}

		public override bool TryCastShot()
		{
			Building_EnergyWeapon getWep = base.GetWep;
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			if (burstShotsLeft == 1)
			{
				getWep.GatherData("PPCHEL", 10f);
				getWep.GatherData("PPCWeapon", 10f);
				SoundDef.Named("Explosion_Stun").PlayOneShot(SoundInfo.InMap(new TargetInfo(currentTarget.Thing)));
				currentTarget.Thing.Destroy();
				getWep.KillCounter++;
			}
			getWep.DissipateCharge(getWep.PulseSize / (float)ShotsPerBurst);
			Vector3 drawPos = currentTarget.Thing.DrawPos;
			FleckMaker.ThrowSmoke(drawPos, getWep.Map, 1.5f);
			FleckMaker.ThrowMicroSparks(drawPos, getWep.Map);
			FleckMaker.ThrowLightningGlow(drawPos, getWep.Map, 1.5f);
			return true;
		}
	}
}
