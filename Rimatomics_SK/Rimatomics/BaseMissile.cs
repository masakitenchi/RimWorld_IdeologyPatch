using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class BaseMissile : ThingWithComps
	{
		public enum LaunchPhase
		{
			countdown,
			clearTower,
			stage1,
			complete
		}

		public Vector3 Acceleration = new Vector3(0f, 0f, 0.3f);

		public int clock;

		public Vector3 crashCell = Vector3.zero;

		public bool crashing;

		public float crashrot = 180f;

		public IntVec3 destinationCell;

		public int destinationTile;

		public Vector3 ExactPosition = Vector3.zero;

		public Quaternion ExactRotation;

		public bool failure;

		public Vector3 icbmSize = new Vector3(1.5f, 1f, 6f);

		public LaunchPhase launchPhase;

		public Material MAT_ENGINES;

		public Material MAT_ICBM;

		public float realRot;

		public float rotceleration;

		public GlobalTargetInfo target;

		private static float textureOffset;

		private int timoutticks = 7f.SecondsToTicks();

		public int upgradeStage;

		public float Yield;

		public virtual ThingDef warheadDef => DubDef.FissionWarhead;

		public virtual WorldObjectDef worldObjDef => DubDef.ICBMfission;

		public int MaxLaunchDistance
		{
			get
			{
				int num = 100;
				if (!ResearchStepDef.Named("ICBM6").IsFinished)
				{
					num -= 50;
				}
				if (ResearchStepDef.Named("GuidenceSystem3").IsFinished)
				{
					num += 400;
				}
				return num;
			}
		}

		public override Vector3 DrawPos => ExactPosition;

		public void TryLaunch(GlobalTargetInfo T, Vector3 pos, float yield)
		{
			target = T;
			pos.y = AltitudeLayer.MapDataOverlay.AltitudeFor();
			ExactPosition = pos;
			launchPhase = LaunchPhase.clearTower;
			textureOffset = 0.37f;
			Yield = yield;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			MAT_ICBM = new Material(GraphicsCache.ICBM_MasterMAT);
			MAT_ICBM.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
			MAT_ENGINES = new Material(GraphicsCache.Engines_MasterMAT);
			MAT_ENGINES.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref Yield, "Yield", 0f);
			Scribe_Values.Look(ref Acceleration, "Acceleration", Vector3.one);
			Scribe_Values.Look(ref ExactPosition, "ExactPosition", Vector3.one);
		}

		public override void Draw()
		{
			MAT_ICBM.SetTextureOffset("_MainTex", new Vector2(0.25f, textureOffset));
			MAT_ENGINES.SetTextureOffset("_MainTex", new Vector2(0.25f, textureOffset));
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(ExactPosition, ExactRotation, icbmSize);
			Graphics.DrawMesh(MeshPool.plane10, matrix, MAT_ICBM, 0);
			Graphics.DrawMesh(MeshPool.plane10, matrix, MAT_ENGINES, 0);
		}

		public float EaseIn(float start, float end, float val, float speed)
		{
			float num = Mathf.InverseLerp(start, end, val);
			float num2 = num * num;
			return speed * num2;
		}

		public override void Tick()
		{
			if (launchPhase == LaunchPhase.clearTower)
			{
				textureOffset = Mathf.Max(textureOffset - 0.03f, 0f);
				if (textureOffset == 0f)
				{
					launchPhase = LaunchPhase.stage1;
				}
			}
			if (launchPhase == LaunchPhase.stage1)
			{
				Vector3 exactPosition = ExactPosition;
				ExactPosition += Acceleration;
				Acceleration = Acceleration.RotatedBy(0.025f);
				ExactRotation = Quaternion.LookRotation(ExactPosition - exactPosition);
				clock++;
				if (Find.TickManager.TicksGame % 4 == 0 && base.Position.InBounds(base.Map))
				{
					Rand.PushState();
					DubUtils.ThrowSmoke(ExactPosition, base.Map, 1f);
					Rand.PopState();
					FleckMaker.ThrowFireGlow(ExactPosition, base.Map, 1f);
				}
				if (!ExactPosition.ToIntVec3().InBounds(base.Map) || clock > timoutticks)
				{
					WorldObject_Missile worldObject_Missile = (WorldObject_Missile)WorldObjectMaker.MakeWorldObject(worldObjDef);
					worldObject_Missile.Tile = base.Map.Tile;
					worldObject_Missile.warheadDef = warheadDef;
					worldObject_Missile.destinationTile = target.Tile;
					worldObject_Missile.destinationCell = target.Cell;
					worldObject_Missile.yield = Yield;
					Find.WorldObjects.Add(worldObject_Missile);
					DubUtils.GetResearch().GatherData("ICBMfission", 30f);
					DubUtils.GetResearch().NukeLaunches++;
					launchPhase = LaunchPhase.complete;
					Destroy();
				}
			}
			base.Position = ExactPosition.ToIntVec3();
		}
	}
}
