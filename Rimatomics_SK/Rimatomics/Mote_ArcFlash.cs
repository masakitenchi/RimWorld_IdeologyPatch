using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Mote_ArcFlash : MoteDualAttached
	{
		public Material arcflash;

		public Vector3 arcTarget;

		public Vector3 origin;

		public void SetupMoteArcFlash(Texture2D flash, Vector3 origin, Vector3 target)
		{
			arcflash = MaterialPool.MatFrom(flash, ShaderDatabase.MoteGlow, Color.white);
			arcTarget = target;
			this.origin = origin;
		}

		public override void Draw()
		{
			if (!(arcflash != null))
			{
				return;
			}
			Vector3 vector = origin;
			Vector3 vector2 = arcTarget;
			vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			vector2.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			float alpha = Alpha;
			if (alpha <= 0f)
			{
				return;
			}
			Color color = instanceColor;
			color.a *= alpha;
			Material material = arcflash;
			if (color != material.color)
			{
				material = MaterialPool.MatFrom((Texture2D)material.mainTexture, ShaderDatabase.MoteGlow, color);
			}
			if (!(Mathf.Abs(vector.x - vector2.x) < 0.01f) || !(Mathf.Abs(vector.z - vector2.z) < 0.01f))
			{
				Vector3 pos = (vector + vector2) / 2f;
				if (!(vector == vector2))
				{
					float num = (vector - vector2).MagnitudeHorizontal();
					Quaternion q = Quaternion.LookRotation(vector - vector2);
					Vector3 s = new Vector3(num * 0.666f, 1f, num);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(pos, q, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
				}
			}
		}
	}
}
