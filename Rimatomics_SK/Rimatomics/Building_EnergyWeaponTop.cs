using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Building_EnergyWeaponTop
	{
		public Building_EnergyWeapon parentTurret;

		public float rotTo = 10f;

		public float velocity;

		public bool TargetInSights
		{
			get
			{
				if (parentTurret.CurrentTarget.IsValid)
				{
					rotTo = (parentTurret.CurrentTarget.CenterVector3 - parentTurret.DrawPos).AngleFlat();
				}
				else if (parentTurret.longTarget.IsValid)
				{
					rotTo = Find.World.grid.GetHeadingFromTo(parentTurret.Map.Tile, parentTurret.longTarget.Tile);
				}
				return Quaternion.Angle(parentTurret.TurretRotation.ToQuat(), rotTo.ToQuat()) < parentTurret.GunProps.EnergyWep.TurretAimSnapAngle;
			}
		}

		public Building_EnergyWeaponTop(Building_EnergyWeapon ParentTurret)
		{
			parentTurret = ParentTurret;
		}

		public void TurretTopTick()
		{
			LocalTargetInfo currentTarget = parentTurret.CurrentTarget;
			if (currentTarget.IsValid)
			{
				rotTo = (currentTarget.CenterVector3 - parentTurret.DrawPos).AngleFlat();
			}
			else if (parentTurret.longTarget.IsValid)
			{
				rotTo = Find.World.grid.GetHeadingFromTo(parentTurret.Map.Tile, parentTurret.longTarget.Tile);
			}
			if (parentTurret.AttackVerb.Bursting)
			{
				parentTurret.TurretRotation = rotTo;
			}
			else
			{
				parentTurret.TurretRotation = Mathf.SmoothDampAngle(parentTurret.TurretRotation, rotTo, ref velocity, 0.01f, parentTurret.turretSpeed, 0.01666f);
			}
		}

		public void DrawTurret()
		{
			Vector3 drawPos = parentTurret.DrawPos;
			drawPos.y = AltitudeLayer.PawnUnused.AltitudeFor();
			parentTurret.TurretGraphic.Draw(drawPos, default(Rot4), parentTurret, parentTurret.TurretRotation);
		}
	}
}
