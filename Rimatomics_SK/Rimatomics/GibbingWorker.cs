using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class GibbingWorker : DamageWorker_AddInjury
	{
		public override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
		{
			if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal || damagedThings.Contains(t))
			{
				return;
			}
			damagedThings.Add(t);
			if (ignoredThings != null && ignoredThings.Contains(t))
			{
				return;
			}
			if (def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
			{
				t.Destroy();
				return;
			}
			float num = ((!(t.Position == explosion.Position)) ? (t.Position - explosion.Position).AngleFlat : ((float)Rand.RangeInclusive(0, 359)));
			DamageDef damageDef = def;
			float amount = explosion.GetDamageAmountAt(cell);
			float armorPenetrationAt = explosion.GetArmorPenetrationAt(cell);
			float angle = num;
			Thing instigator = explosion.instigator;
			ThingDef weapon = explosion.weapon;
			DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetrationAt, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
			if (def.explosionAffectOutsidePartsOnly)
			{
				dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			}
			BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, def);
				Find.BattleLog.Add(battleLogEntry_ExplosionImpact);
			}
			DamageResult damageResult = t.TakeDamage(dinfo);
			damageResult.AssociateWithLog(battleLogEntry_ExplosionImpact);
			if (pawn != null && damageResult.wounded)
			{
				pawn.stances?.stagger.StaggerFor(95);
			}
			if (!RimatomicsMod.Settings.EnableGiblets || t.def.race == null)
			{
				return;
			}
			float angleFlat = (t.Position - explosion.Position).AngleFlat;
			Pawn p = t as Pawn;
			if (p == null || !p.def.race.IsFlesh)
			{
				return;
			}
			if (t.Destroyed)
			{
				for (int i = 0; i < 3; i++)
				{
					ThingDef obj = (Rand.Chance(0.5f) ? DubDef.Mote_Gibb_C : DubDef.Mote_Gibb_B);
					Rand.PushState();
					MoteThrown obj2 = (MoteThrown)ThingMaker.MakeThing(obj);
					obj2.exactScale = Vector3.one * Rand.Range(0.8f, 1.2f);
					obj2.rotationRate = Rand.Range(180f, 1000f);
					obj2.exactPosition = t.DrawPos;
					obj2.SetVelocity(angleFlat + Rand.Range(-35f, 35f), Rand.Range(1.6f, 4.3f));
					obj2.airTimeLeft = Rand.Range(0.6f, 1.3f);
					GenSpawn.Spawn(obj2, t.Position, explosion.Map);
					Rand.PopState();
				}
			}
			int num2;
			if (damageResult.parts != null)
			{
				num2 = (damageResult.parts.Any((BodyPartRecord x) => p.health.hediffSet.PartIsMissing(x)) ? 1 : 0);
				if (num2 != 0)
				{
					ThingDef obj3 = (Rand.Chance(0.5f) ? DubDef.Mote_Gibb_C : DubDef.Mote_Gibb_B);
					Rand.PushState();
					MoteThrown obj4 = (MoteThrown)ThingMaker.MakeThing(obj3);
					obj4.exactScale = Vector3.one * Rand.Range(0.8f, 1.2f);
					obj4.rotationRate = Rand.Range(180f, 1000f);
					obj4.exactPosition = t.DrawPos;
					obj4.SetVelocity(angleFlat + Rand.Range(-35f, 35f), Rand.Range(1.6f, 4.3f));
					obj4.airTimeLeft = Rand.Range(0.6f, 1.3f);
					GenSpawn.Spawn(obj4, t.Position, explosion.Map);
					Rand.PopState();
				}
			}
			else
			{
				num2 = 0;
			}
			int num3 = ((num2 != 0) ? 12 : 4);
			for (int j = 0; j < num3; j++)
			{
				Rand.PushState();
				MoteThrown obj5 = (MoteThrown)ThingMaker.MakeThing(DubDef.Mote_Gibb_A);
				obj5.exactScale = Vector3.one * Rand.Range(0.1f, 0.4f);
				obj5.rotationRate = Rand.Range(0f, 500f);
				obj5.exactPosition = t.DrawPos;
				obj5.SetVelocity(angleFlat + Rand.Range(-35f, 35f), Rand.Range(1.6f, 4.3f));
				obj5.airTimeLeft = Rand.Range(0.2f, 2f);
				GenSpawn.Spawn(obj5, t.Position, explosion.Map);
				Rand.PopState();
			}
		}
	}
}
