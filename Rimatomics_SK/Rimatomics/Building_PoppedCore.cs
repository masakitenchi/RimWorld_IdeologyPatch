using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Building_PoppedCore : Building
	{
		public bool contained = true;

		private Graphic offGraphic;

		public bool exposed
		{
			get
			{
				if (base.Position.UsesOutdoorTemperature(base.Map))
				{
					return !contained;
				}
				return false;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Command item in DubDef.BilderbergCommands(def, visible: false))
			{
				yield return item;
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "delete me",
					action = delegate
					{
						DeSpawn();
					}
				};
			}
		}

		public static void ExplosionDamageTerrain(IntVec3 c, Map map)
		{
			if (map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				map.terrainGrid.Notify_TerrainDestroyed(c);
			}
		}

		public void GoBang(int FuelCount, Map Map)
		{
			foreach (IntVec3 cell2 in this.OccupiedRect().Cells)
			{
				if (base.Position.GetRoof(Map) != RoofDefOf.RoofRockThick)
				{
					Map.roofGrid.SetRoof(cell2, null);
				}
			}
			int num = Mathf.RoundToInt(GenMath.LerpDoubleClamped(0f, 100f, 15f, 50f, FuelCount));
			float num2 = 8f;
			RoofDef roof = base.Position.GetRoof(Map);
			if (roof == RoofDefOf.RoofRockThick)
			{
				num = Mathf.Min(55, num * 3);
				num2 = Mathf.Min(55f, num2 * 3f);
			}
			if (roof == null)
			{
				for (int i = 0; i < FuelCount; i++)
				{
					CellFinderLoose.TryFindSkyfallerCell(DubDef.ThrownSlag, Map, out var cell, 10, default(IntVec3), 999);
					SkyfallerMaker.SpawnSkyfaller(DubDef.ThrownSlag, DubDef.ChunkRadioactiveSlag, cell, Map);
				}
			}
			GenExplosion.DoExplosion(base.Position, Map, num, DubDef.CoreBlast, null, -1, -1f, DubDef.CoreBlast.soundExplosion, null, null, null, ThingDefOf.Filth_Ash, 1f, 1, null, applyDamageToExplosionCellsNeighbors: true);
			Rand.PushState();
			MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_MeltdownFlash"));
			obj.Scale = 100f;
			obj.exactPosition = base.Position.ToVector3();
			GenSpawn.Spawn(obj, base.Position, Map);
			Rand.PopState();
			Effecter effecter = DefDatabase<EffecterDef>.GetNamed("GiantExplosion").Spawn();
			effecter.Trigger(new TargetInfo(base.Position, Map), new TargetInfo(base.Position, Map));
			effecter.Cleanup();
		}

		public override void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this, 0f);
			if (offGraphic == null)
			{
				offGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, def.graphicData.drawSize, DrawColor, DrawColorTwo);
			}
			offGraphic.Print(layer, this, 0f);
		}

		public override void Tick()
		{
			base.Tick();
			float strengthIN = 5f;
			if (this.IsHashIntervalTick(1000))
			{
				if (base.Position.GetFirstThing(base.Map, DubDef.ReactorSacrophagus) != null)
				{
					contained = true;
				}
				else
				{
					contained = false;
				}
			}
			new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(0f, 2f));
			if (contained)
			{
				return;
			}
			if (this.IsHashIntervalTick(70))
			{
				FleckMaker.ThrowMicroSparks(DrawPos + new Vector3(Rand.Range(-2f, 2f), 0f, Rand.Range(-2f, 2f)), base.Map);
			}
			if (this.IsHashIntervalTick(30))
			{
				FleckMaker.ThrowHeatGlow(base.Position + new IntVec3(Rand.Range(-2, 2), 0, Rand.Range(-2, 2)), base.Map, 2f);
			}
			if (this.IsHashIntervalTick(60))
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(Rand.Range(-2f, 2f), 0f, Rand.Range(-2f, 2f)), base.Map, 1f);
			}
			if (this.IsHashIntervalTick(15))
			{
				DubUtils.emitRadiation(base.Position, strengthIN, 6.9f, base.Map);
			}
			if (this.IsHashIntervalTick(2500))
			{
				GameCondition activeCondition = base.Map.GameConditionManager.GetActiveCondition(DubDef.RadioactiveFallout);
				if (activeCondition != null)
				{
					activeCondition.Duration += 2500;
				}
				if (activeCondition == null && base.Position.UsesOutdoorTemperature(base.Map))
				{
					GameCondition cond = GameConditionMaker.MakeCondition(DubDef.RadioactiveFallout, 900000);
					base.Map.GameConditionManager.RegisterCondition(cond);
				}
			}
		}
	}
}
