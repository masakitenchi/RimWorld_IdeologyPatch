using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_TeslaCoil : Building_EnergyWeapon
	{
		public override bool TurretBased => false;

		public override float PulseSize
		{
			get
			{
				float num = base.GunProps.EnergyWep.PulseSizeScaled;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num *= 1.15f;
				}
				if (UG.HasUpgrade(DubDef.ERS))
				{
					num -= 0.15f * num;
				}
				return num;
			}
		}

		public override int Damage
		{
			get
			{
				int num = base.GunProps.EnergyWep.Damage;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num += 10;
				}
				return num;
			}
		}

		public override float Range
		{
			get
			{
				float num = base.GunProps.EnergyWep.range;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num += 10f;
				}
				AttackVerb.verbProps.range = num;
				return num;
			}
		}

		public override void Draw()
		{
			base.Draw();
			Matrix4x4 matrix4x = default(Matrix4x4);
			Vector3 drawPos = DrawPos;
			drawPos.y = AltitudeLayer.PawnUnused.AltitudeFor();
			matrix4x.SetTRS(drawPos, base.Rotation.AsQuat, new Vector3(def.building.turretTopDrawSize, 1f, def.building.turretTopDrawSize));
			base.TurretGraphic.Draw(drawPos, default(Rot4), this);
		}
	}
}
