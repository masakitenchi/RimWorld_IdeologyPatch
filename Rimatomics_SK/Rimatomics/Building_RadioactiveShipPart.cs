using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace Rimatomics
{
	public class Building_RadioactiveShipPart : Building
	{
		private const float MechanoidsDefendRadius = 21f;

		private const int SnowExpandInterval = 500;

		private const float SnowAddAmount = 0.12f;

		private const float SnowMaxRadius = 55f;

		private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();

		protected int age;

		private Lord lord;

		public float pointsLeft;

		private float snowRadius;

		private int ticksToPlantHarm;

		private StringBuilder stringBuilder = new StringBuilder();

		protected virtual float PlantHarmRange => 0f;

		protected virtual int PlantHarmInterval => 4;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref pointsLeft, "mechanoidPointsLeft", 0f);
			Scribe_Values.Look(ref age, "age", 0);
			Scribe_Values.Look(ref snowRadius, "snowRadius", 0f);
			Scribe_References.Look(ref lord, "defenseLord");
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.AppendLine("AwokeDaysAgo".Translate(age.TicksToDays().ToString("F1")));
			return stringBuilder.ToString();
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			IntVec3 position = base.Position;
			base.Destroy(mode);
			if (mode != DestroyMode.KillFinalize)
			{
				return;
			}
			DamageDefOf.Bomb.soundExplosion.PlayOneShot(new TargetInfo(position, map));
			Find.CameraDriver.shaker.DoShake(1f);
			IntVec3 intVec = this.OccupiedRect().EdgeCells.RandomElement();
			_ = IntVec3.Invalid;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec2 = intVec + GenAdj.CardinalDirections[i];
				if (!intVec2.GetThingList(base.Map).Contains(this))
				{
					Effecter effecter = DefDatabase<EffecterDef>.GetNamed("ConstructMetal").Spawn();
					effecter.Trigger(new TargetInfo(intVec, map), new TargetInfo(intVec2, map));
					effecter.Cleanup();
				}
			}
			FleckMaker.ThrowSmoke(DrawPos, map, 3f);
			FleckMaker.ThrowSmoke(DrawPos, map, 4f);
			throwSlag(position, map);
			throwSlag(position, map);
			throwSlag(position, map);
			throwSlag(position, map);
			GenExplosion.DoExplosion(position, map, 12f, DubDef.CoreBlast, null);
		}

		public void throwSlag(IntVec3 pos, Map map)
		{
			CellFinder.TryFindRandomCellNear(pos, map, 20, null, out var result);
			if (!DropCellFinder.TryFindDropSpotNear(result, map, out var result2, allowFogged: true, canRoofPunch: true))
			{
				result2 = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map), map);
			}
			ThrownSlag obj = (ThrownSlag)ThingMaker.MakeThing(DubDef.ThrownSlag);
			obj.startPoint = pos.ToVector3ShiftedWithAltitude(AltitudeLayer.Projectile);
			GenSpawn.Spawn(obj, result2, map);
		}

		public override void Tick()
		{
			base.Tick();
			age++;
			ticksToPlantHarm--;
			if (ticksToPlantHarm <= 0)
			{
				HarmPlant();
			}
			if (this.IsHashIntervalTick(70))
			{
				FleckMaker.ThrowMicroSparks(DrawPos + new Vector3(Rand.Range(-2f, 2f), 0f, Rand.Range(-2f, 2f)), base.Map);
			}
			if (this.IsHashIntervalTick(100))
			{
				FleckMaker.ThrowHeatGlow(base.Position, base.Map, 1f);
			}
			if (this.IsHashIntervalTick(60))
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(Rand.Range(-2f, 2f), 0f, Rand.Range(-2f, 2f)), base.Map, 1f);
			}
			if (this.IsHashIntervalTick(20))
			{
				DubUtils.emitRadiation(base.Position, 5f, 6.9f, base.Map);
			}
			this.IsHashIntervalTick(2500);
		}

		private void HarmPlant()
		{
			if (PlantHarmRange < 0.0001f)
			{
				return;
			}
			float angle = Rand.Range(0f, 360f);
			float num = Rand.Range(0f, PlantHarmRange);
			num = Mathf.Sqrt(num / PlantHarmRange) * PlantHarmRange;
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
			Vector3 vector = Vector3.forward * num;
			IntVec3 intVec = IntVec3.FromVector3(quaternion * vector);
			IntVec3 c = base.Position + intVec;
			if (c.InBounds(base.Map))
			{
				Plant plant = c.GetPlant(base.Map);
				if (plant != null)
				{
					if (Rand.Value < 0.2f)
					{
						plant.Destroy(DestroyMode.KillFinalize);
					}
					else
					{
						plant.MakeLeafless(Plant.LeaflessCause.Poison);
					}
				}
			}
			ticksToPlantHarm = PlantHarmInterval;
		}
	}
}
