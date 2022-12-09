using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class notReal : Thing
	{
		public Pawn target;

		public int destinationTile;

		public IntVec3 destinationCell;

		public WorldObjectDef missileDef;

		public ThingDef warheadDef;

		public Vector3 targetAltitude = Vector3.zero;

		public Vector3 currentAltitude = Vector3.zero;

		public Vector3 acceleration = Vector3.zero;

		public int graphint;

		public bool failure;

		public bool crashing;

		public int upgradeStage;

		public float realRot;

		public float crashrot = 180f;

		public float rotceleration;

		private float tipAlpha;

		private int life;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			SoundDef.Named("cloak").PlayOneShot(this);
			targetAltitude = DrawPos + new Vector3(2f, 0f, 2f);
			currentAltitude = DrawPos + new Vector3(-2f, 0f, -2f);
			currentAltitude.y = AltitudeLayer.VisEffects.AltitudeFor();
			targetAltitude.y = AltitudeLayer.VisEffects.AltitudeFor();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref acceleration, "acceleration", Vector3.one);
			Scribe_Values.Look(ref targetAltitude, "targetAltitude", Vector3.one);
			Scribe_Values.Look(ref currentAltitude, "currentAltitude", Vector3.one);
		}

		public override void Draw()
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(currentAltitude, realRot.ToQuat(), new Vector3(12f, 1f, 12f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, FadedMaterialPool.FadedVersionOf(Graphic.MatSingle, tipAlpha), 0);
			matrix.SetTRS(currentAltitude + new Vector3(0f, 1f, 0f), realRot.ToQuat(), new Vector3(12f, 1f, 12f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, FadedMaterialPool.FadedVersionOf(GraphicsCache.underGlow, tipAlpha), 0);
		}

		public override void Tick()
		{
			currentAltitude = Vector3.SmoothDamp(currentAltitude, targetAltitude, ref acceleration, 3f, 1f, 0.001f);
			if (tipAlpha < 1f && life < 240)
			{
				tipAlpha += 0.01f;
			}
			if (tipAlpha > 0f && life > 240)
			{
				tipAlpha -= 0.01f;
			}
			if (life == 120 && target != null)
			{
				cut(target, BodyPartDefOf.Brain);
				target.health.AddHediff(HediffDef.Named("Exsanguination"));
				cut(target, DefDatabase<BodyPartDef>.GetNamed("AnimalJaw"));
				cut(target, DefDatabase<BodyPartDef>.GetNamed("Jowl"));
				cut(target, BodyPartDefOf.Jaw);
				cut(target, BodyPartDefOf.Eye);
				cut(target, BodyPartDefOf.Eye);
				cut(target, BodyPartDefOf.Stomach);
				cut(target, BodyPartDefOf.Liver);
				target.health.AddHediff(HediffDef.Named("RectalCoring"));
			}
			if (life > 240 && tipAlpha <= 0f)
			{
				Destroy();
			}
			life++;
		}

		public void cut(Pawn target, BodyPartDef def)
		{
			BodyPartRecord bodyPartRecord = target.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == def);
			if (bodyPartRecord != null)
			{
				target.health.AddHediff(HediffDefOf.MissingBodyPart, bodyPartRecord);
			}
		}
	}
}
