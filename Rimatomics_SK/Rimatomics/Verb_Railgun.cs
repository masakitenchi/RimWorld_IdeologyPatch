using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Verb_Railgun : Verb_RimatomicsVerb
	{
		public override ThingDef Projectile
		{
			get
			{
				CompChangeableProjectile compChangeableProjectile = base.EquipmentSource?.GetComp<CompChangeableProjectile>();
				if (compChangeableProjectile != null && compChangeableProjectile.Loaded)
				{
					return compChangeableProjectile.Projectile;
				}
				return verbProps.defaultProjectile;
			}
		}

		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			if (base.GetWep.longTarget.IsValid)
			{
				return true;
			}
			return base.CanHitTargetFrom(root, targ);
		}

		protected bool TryCastFireMission()
		{
			Building_Railgun building_Railgun = caster as Building_Railgun;
			Vector3 vector = Vector3.forward.RotatedBy(building_Railgun.TurretRotation);
			IntVec3 intVec = (building_Railgun.DrawPos + vector * 500f).ToIntVec3();
			WorldObject_Sabot worldObject_Sabot = (WorldObject_Sabot)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("Sabot"));
			worldObject_Sabot.railgun = building_Railgun;
			worldObject_Sabot.Tile = building_Railgun.Map.Tile;
			worldObject_Sabot.destinationTile = building_Railgun.longTarget.Tile;
			worldObject_Sabot.destinationCell = building_Railgun.longTarget.Cell;
			worldObject_Sabot.spread = building_Railgun.spread;
			worldObject_Sabot.Projectile = Projectile;
			Find.WorldObjects.Add(worldObject_Sabot);
			building_Railgun.GatherData("PPCWeapon", 5f);
			building_Railgun.GatherData("PPCFireMission", 10f);
			building_Railgun.GatherData("PPCRailgun", 10f);
			building_Railgun.PrototypeBang(building_Railgun.GunProps.EnergyWep.PrototypeFailureChance);
			building_Railgun.MuzzleFlash();
			Find.CameraDriver.shaker.SetMinShake(0.1f);
			((Projectile)GenSpawn.Spawn(Projectile, building_Railgun.Position, caster.Map)).Launch(building_Railgun, building_Railgun.DrawPos, intVec, null, ProjectileHitFlags.None, preventFriendlyFire: false, base.EquipmentSource);
			(base.EquipmentSource?.GetComp<CompChangeableProjectile>())?.Notify_ProjectileLaunched();
			building_Railgun.DissipateCharge(building_Railgun.PulseSize);
			return true;
		}

		public override bool TryCastShot()
		{
			Building_Railgun building_Railgun = caster as Building_Railgun;
			if (!building_Railgun.top.TargetInSights)
			{
				return false;
			}
			if (Projectile == null)
			{
				return false;
			}
			bool flag = false;
			if (building_Railgun.longTarget.IsValid)
			{
				flag = TryCastFireMission();
			}
			else
			{
				flag = base.TryCastShot();
				if (flag)
				{
					building_Railgun.DissipateCharge(building_Railgun.PulseSize);
					building_Railgun.GatherData("PPCWeapon", 5f);
					building_Railgun.GatherData("PPCRailgun", 10f);
					building_Railgun.PrototypeBang(building_Railgun.GunProps.EnergyWep.PrototypeFailureChance);
					building_Railgun.MuzzleFlash();
					Find.CameraDriver.shaker.SetMinShake(0.1f);
				}
			}
			building_Railgun.TryChamberRound();
			return flag;
		}
	}
}
