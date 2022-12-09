using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public abstract class WorldObject_Missile : WorldObject
	{
		public const float TravelSpeed = 0.00015f;

		public IntVec3 destinationCell = IntVec3.Invalid;

		public int destinationTile = -1;

		public int initialTile = -1;

		public float traveledPct;

		public ThingDef warheadDef;

		public float yield;

		public Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

		public Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

		public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

		public float TraveledPctStepPerTick
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
				return 0.00015f / num;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref yield, "yield", 0f);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
			Scribe_Values.Look(ref destinationCell, "destinationCell");
			Scribe_Values.Look(ref initialTile, "initialTile", 0);
			Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
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

		public virtual void Arrived()
		{
		}
	}
}
