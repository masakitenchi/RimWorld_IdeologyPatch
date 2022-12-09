using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class WorldObject_Fallout : WorldObject
	{
		public int lifeSpan = 900000;

		public int destinationTile = -1;

		private int initialTile = -1;

		private float traveledPct;

		public int period = 30000;

		public bool wandering = true;

		public bool big;

		public string conditionDef = "RadioactiveFallout";

		private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

		private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

		public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

		public override void PostAdd()
		{
			base.PostAdd();
			initialTile = base.Tile;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref conditionDef, "conditionDef", "RadioactiveFallout");
			Scribe_Values.Look(ref big, "big", defaultValue: false);
			Scribe_Values.Look(ref wandering, "wandering", defaultValue: true);
			Scribe_Values.Look(ref lifeSpan, "lifeSpan", 900000);
			Scribe_Values.Look(ref period, "period", 0);
			Scribe_Values.Look(ref initialTile, "initialTile", 0);
			Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Tile == -1)
			{
				Find.WorldObjects.Remove(this);
			}
			traveledPct += 1f / (float)period;
			if (traveledPct >= 1f)
			{
				traveledPct = 1f;
				Arrived();
			}
			lifeSpan--;
			if (lifeSpan == 0)
			{
				Find.WorldObjects.Remove(this);
			}
		}

		private void Arrived()
		{
			base.Tile = destinationTile;
			initialTile = base.Tile;
			if (!TileFinder.TryFindPassableTileWithTraversalDistance(initialTile, 1, 2, out destinationTile))
			{
				Find.WorldObjects.Remove(this);
			}
			traveledPct = 0f;
			Map map = Current.Game.FindMap(initialTile);
			if (map != null && !map.GameConditionManager.ConditionIsActive(GameConditionDef.Named(conditionDef)))
			{
				int duration = Mathf.RoundToInt(Rand.Range(3f, 8f) * 60000f);
				GameCondition cond = GameConditionMaker.MakeCondition(GameConditionDef.Named(conditionDef), duration);
				map.GameConditionManager.RegisterCondition(cond);
			}
		}
	}
}
