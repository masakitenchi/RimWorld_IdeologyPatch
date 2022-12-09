using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Verb_ArcFlash : Verb_RimatomicsVerb
	{
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, (!currentTarget.HasThing) ? null : currentTarget.Thing, base.EquipmentSource?.def, null, burst: false));
		}

		public override bool TryCastShot()
		{
			Building_EnergyWeapon getWep = base.GetWep;
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			ShootLine resultingLine;
			bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
			if (verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			bool flag2 = Rand.Chance(0.042f);
			float num = getWep.Damage;
			if (flag2)
			{
				num *= 4.2f;
			}
			DamageWorker.DamageResult damageResult = currentTarget.Thing.TakeDamage(new DamageInfo(DubDef.ArcDischarge, num, 1f, -1f, caster, null, getWep.def.building.turretGunDef));
			if (currentTarget.Thing.Faction == Faction.OfMechanoids)
			{
				currentTarget.Thing.TakeDamage(new DamageInfo(DamageDefOf.EMP, getWep.Damage, 1f, -1f, caster));
			}
			getWep.DamageDealt = damageResult.totalDamageDealt;
			Vector3 drawPos = currentTarget.Thing.DrawPos;
			FleckMaker.ThrowLightningGlow(drawPos, getWep.Map, 1.5f);
			for (int i = 0; i < 3; i++)
			{
				FleckMaker.ThrowSmoke(drawPos, getWep.Map, 1.5f);
			}
			Pawn pawn = currentTarget.Thing as Pawn;
			if ((pawn?.Dead ?? false) && flag2)
			{
				DubDef.Sizzle.PlayOneShot(SoundInfo.InMap(new TargetInfo(currentTarget.Thing)));
				CompRottable compRottable = pawn.Corpse.TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					compRottable.RotProgress = 1E+10f;
				}
			}
			getWep.DissipateCharge(getWep.PulseSize);
			getWep.GatherData("PPCWeapon", 5f);
			getWep.GatherData("PPCTeslaCoil", 5f);
			getWep.PrototypeBang(getWep.GunProps.EnergyWep.PrototypeFailureChance);
			Mote_ArcFlash obj = (Mote_ArcFlash)ThingMaker.MakeThing(DubDef.Mote_ArcFlash);
			obj.SetupMoteArcFlash(GraphicsCache.bolts.RandomElement(), getWep.DrawPos, currentTarget.Thing.DrawPos);
			obj.Attach(getWep);
			GenSpawn.Spawn(obj, getWep.Position, getWep.Map);
			return true;
		}
	}
}
