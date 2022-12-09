using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class WorldObject_Sabot : WorldObject
	{
		private const float TravelSpeed = 0.0001f;

		private bool arrived;

		public IntVec3 destinationCell = IntVec3.Invalid;

		public int destinationTile = -1;

		private int initialTile = -1;

		public ThingDef Projectile;

		public Thing railgun;

		public int spread = 1;

		private float traveledPct;

		private Vector3 Start
		{
			get
			{
				Vector3 result = Find.WorldGrid.GetTileCenter(initialTile);
				if (HarmonyPatches.SoS)
				{
					foreach (WorldObject item in Find.World.worldObjects.AllWorldObjects.Where((WorldObject o) => o.def.defName.Equals("ShipOrbiting")))
					{
						if (item.Tile == initialTile)
						{
							result = item.DrawPos;
						}
					}
					{
						foreach (WorldObject item2 in Find.World.worldObjects.AllWorldObjects.Where((WorldObject o) => o.def.defName.Equals("SiteSpace")))
						{
							if (item2.Tile == initialTile)
							{
								result = item2.DrawPos;
							}
						}
						return result;
					}
				}
				return result;
			}
		}

		private Vector3 End
		{
			get
			{
				Vector3 result = Find.WorldGrid.GetTileCenter(destinationTile);
				if (HarmonyPatches.SoS)
				{
					foreach (WorldObject item in Find.World.worldObjects.AllWorldObjects.Where((WorldObject o) => o.def.defName.Equals("ShipOrbiting")))
					{
						if (item.Tile == destinationTile)
						{
							result = item.DrawPos;
						}
					}
					{
						foreach (WorldObject item2 in Find.World.worldObjects.AllWorldObjects.Where((WorldObject o) => o.def.defName.Equals("SiteSpace")))
						{
							if (item2.Tile == destinationTile)
							{
								result = item2.DrawPos;
							}
						}
						return result;
					}
				}
				return result;
			}
		}

		public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = Start;
				Vector3 end = End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return 0.0001f / num;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
			Scribe_Values.Look(ref destinationCell, "destinationCell");
			Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
			Scribe_Values.Look(ref initialTile, "initialTile", 0);
			Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
			Scribe_Defs.Look(ref Projectile, "Projectile");
		}

		public override void PostAdd()
		{
			base.PostAdd();
			initialTile = base.Tile;
		}

		public override void Tick()
		{
			base.Tick();
			traveledPct += TraveledPctStepPerTick;
			if (traveledPct >= 1f)
			{
				traveledPct = 1f;
				Arrived();
			}
		}

		private void Arrived()
		{
			if (!arrived)
			{
				arrived = true;
				Map map = Current.Game.FindMap(destinationTile);
				if (map != null)
				{
					IntVec3 loc = new IntVec3(CellRect.WholeMap(map).Width / 2, 0, CellRect.WholeMap(map).maxZ);
					Projectile obj = (Projectile)GenSpawn.Spawn(Projectile, loc, map);
					CellFinder.TryFindRandomCellNear(destinationCell, map, spread, null, out var result);
					obj.Launch(railgun, result, result, ProjectileHitFlags.IntendedTarget);
				}
				Find.WorldObjects.Remove(this);
			}
		}
	}
}
