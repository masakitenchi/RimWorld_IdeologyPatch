using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_Obelisk : Building_EnergyWeapon
	{
		public Material TipMat;

		public Mesh mesh;

		private float tipAlpha;

		public override bool TurretBased => false;

		public override int Damage
		{
			get
			{
				int num = base.GunProps.EnergyWep.Damage;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num = 104;
				}
				if (UG.HasUpgrade(DubDef.BeamSplitter))
				{
					num /= 2;
				}
				return num;
			}
		}

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

		public override float RangeMin
		{
			get
			{
				float num = base.GunProps.EnergyWep.minRange;
				if (UG.HasUpgrade(DubDef.LenseModule))
				{
					num += 10f;
				}
				AttackVerb.verbProps.minRange = num;
				return num;
			}
		}

		public override float Range
		{
			get
			{
				float num = base.GunProps.EnergyWep.range;
				if (UG.HasUpgrade(DubDef.LenseModule))
				{
					num += 10f;
				}
				AttackVerb.verbProps.range = num;
				return num;
			}
		}

		public override Vector3 TipOffset
		{
			get
			{
				Vector3 vector = new Vector3(0.5f, 0f, 0.75f);
				if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
				{
					vector = new Vector3(0f, 0f, 7f / 32f);
				}
				if (base.Rotation == Rot4.East)
				{
					vector = new Vector3(0.5f, 0f, 1.46875f);
				}
				if (base.Rotation == Rot4.West)
				{
					vector = new Vector3(-0.5f, 0f, 1.46875f);
				}
				return DrawPos + vector;
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (mesh == null)
			{
				mesh = GraphicsCache.obeliskCharge.MeshAt(base.Rotation);
			}
			if (TipMat == null)
			{
				TipMat = GraphicsCache.obeliskCharge.MatAt(base.Rotation);
			}
			if (burstWarmupTicksLeft > 0)
			{
				tipAlpha = GenMath.LerpDoubleClamped(0f, WarmupForShot.SecondsToTicks(), 1f, 0f, burstWarmupTicksLeft);
			}
			else if (tipAlpha > 0f)
			{
				tipAlpha -= 0.01f;
			}
			if (tipAlpha > 0f)
			{
				Color white = Color.white;
				white.a *= tipAlpha;
				TipMat.color = white;
				Vector3 drawPos = DrawPos;
				drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
				Quaternion identity = Quaternion.identity;
				Graphics.DrawMesh(mesh, drawPos, identity, TipMat, 0);
			}
		}
	}
}
