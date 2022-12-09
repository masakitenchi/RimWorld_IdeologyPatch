using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_Marauder : Building_EnergyWeapon
	{
		private Material MuzzleMat;

		private float tipAlpha;

		private float wobble = 1f;

		public override bool TurretBased => true;

		public override int ShotCount
		{
			get
			{
				int num = AttackVerb.verbProps.burstShotCount;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num += 10;
				}
				return num;
			}
		}

		public override Vector3 TipOffset
		{
			get
			{
				Vector3 drawPos = DrawPos;
				Vector3 v = new Vector3(0f, 1f, 3f);
				v = v.RotatedBy(TurretRotation);
				return drawPos + v;
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (MuzzleMat == null)
			{
				MuzzleMat = MaterialPool.MatFrom("Rimatomics/FX/MarauderMuzzle", ShaderDatabase.MoteGlow, Color.white);
			}
			tipAlpha = 0f;
			if (base.GunCompEq.PrimaryVerb.state == VerbState.Bursting)
			{
				tipAlpha = Rand.Range(0.9f, 1f);
				wobble = Rand.Range(0.9f, 1f);
			}
			if (tipAlpha > 0f)
			{
				Color white = Color.white;
				white.a *= tipAlpha;
				MuzzleMat.color = white;
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(TipOffset, TurretRotation.ToQuat(), new Vector3(5f, 1f, 5f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, MuzzleMat, 0);
			}
		}
	}
}
