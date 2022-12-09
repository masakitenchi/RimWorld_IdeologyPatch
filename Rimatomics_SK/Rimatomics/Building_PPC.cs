using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_PPC : Building
	{
		public const float MinEnergyToExplode = 500f;

		public const float EnergyToLoseWhenExplode = 400f;

		public const float ExplodeChancePerDamage = 0.05f;

		public static readonly Vector2 BarSize = new Vector2(1.7f, 0.2f);

		public static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.1f, 0.2f));

		public static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

		public float BarTo;

		public CompPowerBattery batt;

		public int ticksToExplode;

		public Sustainer wickSustainer;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToExplode, "ticksToExplode", 0);
			Scribe_Values.Look(ref BarTo, "BarTo", 0f);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!map.Rimatomics().PPCs.Contains(this))
			{
				map.Rimatomics().PPCs.Add(this);
			}
			batt = GetComp<CompPowerBattery>();
			DubUtils.GetResearch().NotifyResearch();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (base.Map.Rimatomics().PPCs.Contains(this))
			{
				base.Map.Rimatomics().PPCs.Remove(this);
			}
			base.DeSpawn(mode);
		}

		public override void Draw()
		{
			base.Draw();
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = DrawPos + Vector3.up * 0.1f;
			r.size = BarSize;
			r.fillPercent = BarTo;
			r.filledMat = BatteryBarFilledMat;
			r.unfilledMat = BatteryBarUnfilledMat;
			r.margin = 0.15f;
			Rot4 rotation = base.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
			if (ticksToExplode > 0 && base.Spawned)
			{
				base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
			}
		}

		public override void Tick()
		{
			base.Tick();
			BarTo = DubUtils.FInterpTo(BarTo, batt.StoredEnergy / batt.Props.storedEnergyMax, 0.16f, 1f);
			if (ticksToExplode > 0)
			{
				if (wickSustainer == null)
				{
					StartWickSustainer();
				}
				else
				{
					wickSustainer.Maintain();
				}
				ticksToExplode--;
				if (ticksToExplode == 0)
				{
					GenExplosion.DoExplosion(this.OccupiedRect().RandomCell, radius: Rand.Range(0.5f, 1f) * 6f, map: base.Map, damType: DamageDefOf.Flame, instigator: null);
					GetComp<CompPowerBattery>().SetStoredEnergyPct(0f);
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			if (!base.Destroyed && ticksToExplode == 0 && dinfo.Def == DamageDefOf.Flame && Rand.Value < 0.05f && GetComp<CompPowerBattery>().StoredEnergy > 500f)
			{
				ticksToExplode = Rand.Range(70, 150);
				StartWickSustainer();
			}
		}

		private void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}
	}
}
