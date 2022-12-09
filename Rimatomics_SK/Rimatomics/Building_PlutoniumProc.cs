using System.Collections.Generic;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_PlutoniumProc : Building, IFuelFilter, IStoreSettingsParent
	{
		public const int MaxCapacity = 12;

		public static readonly Vector2 BarSize = new Vector2(1f, 5f / 32f);

		public static readonly Material FuelMat = SolidColorMaterials.SimpleSolidColorMaterial(new ColorInt(57, 181, 74, 255).ToColor);

		public static readonly Material ChemMat = SolidColorMaterials.SimpleSolidColorMaterial(new ColorInt(236, 0, 140, 255).ToColor);

		public static readonly Material ProductMat = SolidColorMaterials.SimpleSolidColorMaterial(new ColorInt(0, 191, 243, 255).ToColor);

		public static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

		public float autoRefuelPercent = 0.7f;

		public Vector3 bar1 = new Vector3(-1.109375f, 1f, 1.453125f);

		public Vector3 bar2 = new Vector3(-1.109375f, 1f, 1.265625f);

		public Vector3 bar3 = new Vector3(-1.109375f, 1f, 1.078125f);

		public float Chemfuel;

		public bool FastMode;

		private CompFlickable flickComp;

		public Vector3 led1 = new Vector3(-1.71875f, 1f, 1.453125f);

		public Vector3 led2 = new Vector3(-1.71875f, 1f, 1.265625f);

		public Vector3 led3 = new Vector3(-1.71875f, 1f, 1.078125f);

		public float Plutonium;

		private CompPowerTrader powerComp;

		public float progressInt;

		public int rodcount;

		public float TargetFuelLevel = 250f;

		public float Uranium;

		public FloatRange _FuelLifeFilter = new FloatRange(0f, 1f);

		private StorageSettings storageSettings;

		private StringBuilder stringBuilder = new StringBuilder();

		public bool IsFull => TargetFuelLevel - Chemfuel < 1f;

		public bool ShouldAutoRefuelNow
		{
			get
			{
				if (FuelPercentOfTarget <= autoRefuelPercent && !IsFull && !this.IsBurning() && (flickComp == null || flickComp.SwitchIsOn) && base.Map.designationManager.DesignationOn(this, DesignationDefOf.Flick) == null)
				{
					return base.Map.designationManager.DesignationOn(this, DesignationDefOf.Deconstruct) == null;
				}
				return false;
			}
		}

		public float FuelPercentOfTarget => Chemfuel / TargetFuelLevel;

		public float Progress
		{
			get
			{
				return progressInt;
			}
			set
			{
				if (value != progressInt)
				{
					progressInt = value;
				}
			}
		}

		public int SpaceLeftForWort
		{
			get
			{
				if (Fermented)
				{
					return 0;
				}
				return 12 - rodcount;
			}
		}

		private bool Empty => rodcount <= 0;

		public bool Fermented
		{
			get
			{
				if (!Empty)
				{
					return Progress >= 1f;
				}
				return false;
			}
		}

		private float ProgressPerTickAtCurrentTemp
		{
			get
			{
				if (FastMode)
				{
					return 0.0004f;
				}
				return 1.11111115E-06f;
			}
		}

		private int EstimatedTicksLeft => Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

		public FloatRange FuelLifeFilter
		{
			get
			{
				return _FuelLifeFilter;
			}
			set
			{
				_FuelLifeFilter = value;
			}
		}

		public bool StorageTabVisible => true;

		public int GetFuelCountToFullyRefuel()
		{
			return Mathf.Max(Mathf.CeilToInt(TargetFuelLevel - Chemfuel), 1);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			flickComp = GetComp<CompFlickable>();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (rodcount > 0)
			{
				Thing thing = ThingMaker.MakeThing(DubDef.NuclearWaste);
				thing.stackCount = rodcount * 4;
				GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
			}
			base.DeSpawn(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref rodcount, "wortCount", 0);
			Scribe_Values.Look(ref progressInt, "progress", 0f);
			Scribe_Values.Look(ref Plutonium, "Plutonium", 0f);
			Scribe_Values.Look(ref Uranium, "Uranium", 0f);
			Scribe_Values.Look(ref Chemfuel, "Chemfuel", 0f);
			Scribe_Values.Look(ref _FuelLifeFilter, "_FuelLifeFilter", new FloatRange(0f, 0.5f));
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		}

		public override void Tick()
		{
			base.Tick();
			if (powerComp.PowerOn && !this.IsBrokenDown())
			{
				powerComp.PowerOutput = -175f;
				if (!Empty && Chemfuel > 0f && Progress < 1f)
				{
					powerComp.PowerOutput = -3000f;
					Chemfuel = Mathf.Max(Chemfuel - ProgressPerTickAtCurrentTemp * 250f, 0f);
					Progress = Mathf.Min(Progress + ProgressPerTickAtCurrentTemp, 1f);
				}
			}
		}

		private void Reset()
		{
			rodcount = 0;
			Progress = 0f;
			Uranium = 0f;
			Plutonium = 0f;
		}

		public void AddFuel(Thing thing)
		{
			if (thing is Item_NuclearFuel item_NuclearFuel)
			{
				Uranium += item_NuclearFuel.DU;
				Plutonium += item_NuclearFuel.PuCreated;
				if (Fermented)
				{
					return;
				}
				Progress = GenMath.WeightedAverage(0f, 1f, Progress, rodcount);
				rodcount++;
			}
			else
			{
				Chemfuel = Mathf.Min(Chemfuel + (float)thing.stackCount, TargetFuelLevel);
			}
			thing.Destroy();
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.AppendLine(base.GetInspectString());
			if (Fermented)
			{
				stringBuilder.AppendLine("critRemovePlutonium".Translate());
			}
			else
			{
				stringBuilder.AppendLine("ContainsFuel".Translate(rodcount, 12));
				stringBuilder.AppendLine("ContainsChemfuel".Translate(Chemfuel, TargetFuelLevel));
				stringBuilder.AppendLine("ProcessingProgress".Translate(Progress.ToStringPercent(), EstimatedTicksLeft.ToStringTicksToPeriodVerbose()));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public Thing TakeOutFuel()
		{
			if (!Fermented)
			{
				return null;
			}
			if (Mathf.CeilToInt(Uranium) > 0)
			{
				Thing thing = ThingMaker.MakeThing(DubDef.DepletedUraniumPellets);
				thing.stackCount = Mathf.CeilToInt(Uranium);
				GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
			}
			if (Mathf.CeilToInt(Plutonium) > 0)
			{
				Thing thing2 = ThingMaker.MakeThing(DubDef.Plutonium);
				thing2.stackCount = Mathf.CeilToInt(Plutonium);
				GenPlace.TryPlaceThing(thing2, base.Position, base.Map, ThingPlaceMode.Near);
			}
			Thing thing3 = ThingMaker.MakeThing(DubDef.NuclearWaste);
			thing3.stackCount = rodcount;
			Reset();
			return thing3;
		}

		public void DrawBar(float f, Vector3 bar, Material mat)
		{
			base.Draw();
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = DrawPos + bar;
			r.size = BarSize;
			r.fillPercent = f;
			r.filledMat = mat;
			r.unfilledMat = BatteryBarUnfilledMat;
			r.margin = 0.05f;
			Rot4 rot = (r.rotation = base.Rotation);
			GenDraw.DrawFillableBar(r);
		}

		public override void Draw()
		{
			base.Draw();
			DrawBar((float)rodcount / 12f, bar1, FuelMat);
			DrawBar(FuelPercentOfTarget, bar2, ChemMat);
			DrawBar(Progress, bar3, ProductMat);
			if (powerComp == null || !powerComp.PowerOn)
			{
				return;
			}
			Quaternion asQuat = base.Rotation.AsQuat;
			if (Fermented)
			{
				DubUtils.drawLED(DrawPos + led1, asQuat, GraphicsCache.LEDorange);
				DubUtils.drawLED(DrawPos + led2, asQuat, GraphicsCache.LEDorange);
				DubUtils.drawLED(DrawPos + led3, asQuat, GraphicsCache.LEDorange);
				return;
			}
			if (Empty)
			{
				DubUtils.drawLED(DrawPos + led1, asQuat, GraphicsCache.LEDred);
			}
			else
			{
				DubUtils.drawLED(DrawPos + led1, asQuat, GraphicsCache.LEDgreen);
			}
			if (FuelPercentOfTarget > 0f)
			{
				DubUtils.drawLED(DrawPos + led2, asQuat, GraphicsCache.LEDgreen);
			}
			else
			{
				DubUtils.drawLED(DrawPos + led2, asQuat, GraphicsCache.LEDred);
			}
			if (Progress > 0f)
			{
				DubUtils.drawLED(DrawPos + led3, asQuat, GraphicsCache.LEDgreen);
			}
		}

		[SyncMethod(SyncContext.None)]
		public void faster()
		{
			FastMode = !FastMode;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "FastMode",
					action = faster
				};
			}
		}

		public StorageSettings GetStoreSettings()
		{
			if (storageSettings == null)
			{
				storageSettings = new StorageSettings(this);
				if (def.building.defaultStorageSettings != null)
				{
					storageSettings.CopyFrom(def.building.defaultStorageSettings);
				}
			}
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public void Notify_SettingsChanged()
		{
		}
	}
}
