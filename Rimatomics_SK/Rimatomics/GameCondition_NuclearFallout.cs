using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class GameCondition_NuclearFallout : GameCondition
	{
		private List<SkyOverlay> overlays;

		private SkyColorSet VolcanicWinterColors;

		public GameCondition_NuclearFallout()
		{
			VolcanicWinterColors = new SkyColorSet(new ColorInt(0, 0, 0).ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.65f);
			overlays = new List<SkyOverlay>
			{
				new WeatherOverlay_Fallout(),
				new WeatherOverlay_Fog()
			};
		}

		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		public override void GameConditionTick()
		{
			if (Find.TickManager.TicksGame % 3451 == 0)
			{
				List<Pawn> allPawnsSpawned = base.SingleMap.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn = allPawnsSpawned[i];
					if (!pawn.Position.Roofed(base.SingleMap) && pawn.def.race.IsFlesh)
					{
						float num = 0.028758334f;
						num *= pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance);
						if (Math.Abs(num) > 1E-06f)
						{
							float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x46EDC5D));
							num *= num2;
							HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
						}
					}
				}
			}
			for (int j = 0; j < overlays.Count; j++)
			{
				overlays[j].TickOverlay(base.SingleMap);
			}
		}

		public override void DoCellSteadyEffects(IntVec3 c, Map map)
		{
			if (!c.Roofed(map))
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is Plant)
					{
						if (Rand.Value < 0.0065f)
						{
							thing.Kill();
						}
					}
					else if (thing.def.category == ThingCategory.Item)
					{
						CompRottable compRottable = thing.TryGetComp<CompRottable>();
						if (compRottable != null && (int)compRottable.Stage < 2)
						{
							compRottable.RotProgress += 3000f;
						}
					}
				}
			}
			if (c.UsesOutdoorTemperature(map))
			{
				Pawn firstPawn = c.GetFirstPawn(map);
				if (firstPawn != null && firstPawn.def.race.IsFlesh)
				{
					DubUtils.applyRads(firstPawn, 3f);
				}
			}
		}

		public override void GameConditionDraw(Map map)
		{
			for (int i = 0; i < overlays.Count; i++)
			{
				overlays[i].DrawOverlay(map);
			}
		}

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(base.TicksPassed, base.TicksLeft, 5000f, 0.5f);
		}

		public override SkyTarget? SkyTarget(Map map)
		{
			return new SkyTarget(0.55f, VolcanicWinterColors, 1f, 1f);
		}

		public override float AnimalDensityFactor(Map map)
		{
			return 0f;
		}

		public override float PlantDensityFactor(Map map)
		{
			return 0f;
		}

		public override bool AllowEnjoyableOutsideNow(Map map)
		{
			return false;
		}

		public override List<SkyOverlay> SkyOverlays(Map map)
		{
			return overlays;
		}
	}
}
