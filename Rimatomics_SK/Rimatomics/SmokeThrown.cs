using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class SmokeThrown : Mote
	{
		private Vector2 realPosition;

		public static readonly Material FlashMat = MaterialPool.MatFrom("Rimatomics/FX/flash", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.Tornado);

		public static readonly Material TornadoMaterial = MaterialPool.MatFrom("Rimatomics/FX/Tornado", ShaderDatabase.Transparent, MapMaterialRenderQueues.Tornado);

		public static readonly Material FireGlow = MaterialPool.MatFrom("Things/Mote/FireGlow", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.Tornado);

		public static readonly Material shadowmat = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Mote);

		private Color value;

		[TweakValue("_Rimatomics", 0f, 10000f)]
		public static int columncount = 600;

		[TweakValue("_Rimatomics", 0f, 10000f)]
		public static int mushroomcount = 1600;

		[TweakValue("_Rimatomics", 0f, 1f)]
		public static float glowscaler = 0.4f;

		[TweakValue("_Rimatomics", 0f, 100f)]
		public static float smokescaler = 6.8f;

		[TweakValue("_Rimatomics", 0f, 1000f)]
		public static float tannen = 1000f;

		[TweakValue("_Rimatomics", 0f, 1000f)]
		public static float biff = 1000f;

		[TweakValue("_Rimatomics", 0f, 2f)]
		public static float multipass1 = 1.9f;

		[TweakValue("_Rimatomics", 0f, 2f)]
		public static float multipass2 = 0.4f;

		[TweakValue("_Rimatomics", 0f, 100f)]
		public static float column = 40f;

		public float AtomicScale = 1f;

		public override void Draw()
		{
			value = Find.CurrentMap.skyManager.CurrentSkyTarget().colors.sky;
			Vector3 drawPos = DrawPos;
			realPosition = new Vector2(drawPos.x, drawPos.z);
			drawPos.y = AltitudeLayer.MoteOverheadLow.AltitudeFor();
			float num = 70f * AtomicScale * (base.AgeSecs / def.mote.Lifespan);
			exactScale = new Vector3(num, 1f, num);
			DrawAt(drawPos);
			float num2 = Mathf.Lerp(1f, 0f, base.AgeSecs / (def.mote.Lifespan / 5f));
			drawPos.y = AltitudeLayer.Weather.AltitudeFor();
			if (num2 > 0f)
			{
				Tornado.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, Color.white * num2);
				Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(140f * AtomicScale * num2, 1f, 140f * AtomicScale * num2));
				Graphics.DrawMesh(MeshPool.plane10, matrix, FlashMat, 0, null, 0, Tornado.matPropertyBlock);
			}
			Tornado.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, Color.black * 1.5f * Alpha);
			float num3 = 50f * AtomicScale * Mathf.Lerp(0.5f, 1.5f, base.AgeSecs / def.mote.Lifespan);
			Matrix4x4 matrix2 = Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(num3, 1f, num3));
			Graphics.DrawMesh(MeshPool.plane10, matrix2, shadowmat, 0, null, 0, Tornado.matPropertyBlock);
			float life = base.AgeSecs / def.mote.Lifespan;
			Rand.PushState();
			Rand.Seed = thingIDNumber;
			for (int i = 0; i < columncount; i++)
			{
				DrawColumnPart(plume: false, life, Rand.Range(0f, 10f), num2, Rand.Range(0f, tannen), Rand.Range(0f, 360f), Rand.Range(0.1f, 0.2f), Rand.Range(0.22f, 0.6f), Rand.Range(0f, 1f), Rand.Range(0f, 0.5f), tannen, multipass2, smokescaler);
			}
			for (int j = 0; j < mushroomcount; j++)
			{
				DrawColumnPart(plume: true, life, Rand.Range(0f, 15f), num2, Rand.Range(0f, biff), Rand.Range(0f, 360f), Rand.Range(0.1f, 0.5f), Rand.Range(0.52f, 1f), 1f, Rand.Range(0.5f, 1f), biff, multipass1, smokescaler);
			}
			Rand.PopState();
		}

		public static float EaseIn(float t)
		{
			return t * t * t;
		}

		private void DrawColumnPart(bool plume, float life, float windage, float flashAlpha, float distanceFromCenter, float initialAngle, float speedMultiplier, float colorMultiplier, float height, float smokeLayer, float ticks, float mushroomWidth, float smokeSize)
		{
			windage *= AtomicScale;
			mushroomWidth *= AtomicScale;
			height *= AtomicScale;
			ticks *= AtomicScale;
			speedMultiplier *= AtomicScale;
			distanceFromCenter *= AtomicScale;
			smokeSize *= AtomicScale;
			int ticksGame = Find.TickManager.TicksGame;
			float num = (distanceFromCenter + (float)ticksGame) % ticks / ticks;
			distanceFromCenter = Mathf.Lerp(0f, 10f * mushroomWidth, num);
			float num2 = 0.5f - Mathf.Cos((float)Math.PI * 2f * num) / 2f;
			num2 = num2 * num2 * num2;
			num2 = Mathf.Min(Alpha, num2, Mathf.Lerp(1f, 0f, flashAlpha));
			float num3 = 1f * speedMultiplier;
			float y = (initialAngle + (float)ticksGame * num3) % 360f;
			Vector2 vector = realPosition.Moved(initialAngle, distanceFromCenter);
			vector.x += windage * EaseIn(life);
			vector.y += height * column * life;
			float num4 = AltitudeLayer.Weather.AltitudeFor();
			Vector3 pos = new Vector3(vector.x, num4 + 3f / 70f * smokeLayer, vector.y);
			Color color = value * colorMultiplier;
			if (!plume)
			{
				color *= Mathf.Lerp(1f, 0f, height * life);
			}
			color.a = num2;
			if (Rand.Chance(glowscaler))
			{
				Tornado.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, Color.white * num2);
				Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(smokeSize, 1f, smokeSize));
				Graphics.DrawMesh(MeshPool.plane10, matrix, FireGlow, 0, null, 0, Tornado.matPropertyBlock, castShadows: false, receiveShadows: false);
			}
			else
			{
				Tornado.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
				Matrix4x4 matrix2 = Matrix4x4.TRS(pos, Quaternion.Euler(0f, y, 0f), new Vector3(smokeSize, 1f, smokeSize));
				Graphics.DrawMesh(MeshPool.plane10, matrix2, TornadoMaterial, 0, null, 0, Tornado.matPropertyBlock, castShadows: false, receiveShadows: false);
			}
		}
	}
}
