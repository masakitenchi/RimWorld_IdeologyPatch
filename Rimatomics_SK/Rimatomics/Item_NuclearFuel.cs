using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Item_NuclearFuel : Item_RadioactiveThing
	{
		public static readonly Graphic CrackedRod = GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/Resources/fuelRodCracked", ShaderDatabase.DefaultShader, new Vector2(1.2f, 1.2f), Color.white);

		[TweakValue("Rimatomics", 0f, 100f)]
		public static bool OldFuelMode = false;

		public Item_NuclearFuel[] AdjFuelRefs;

		public float BasePowerLevel;

		public float BurnupRate;

		public float BurnupRateForAdjFuel;

		public float ChainReaction;

		public bool cracked;

		public float FuelLevel = 1f;

		public bool markedForRemove;

		public float MaxPowerLevel;

		public float Ratio;

		public int SlotID = -1;

		private NuclearFuel serp;

		private StringBuilder sb = new StringBuilder();

		public int LifeSpan = 144000000;

		public float rate = 6.94444457E-09f;

		private StringBuilder stringBuilder = new StringBuilder();

		public NuclearFuel FuelDef
		{
			get
			{
				if (serp == null)
				{
					serp = (def as RimatomicsThingDef).nuclearFuel;
				}
				return serp;
			}
		}

		public float DU => FuelDef.Du;

		public float Pu => FuelDef.Pu;

		public float PuCreated => Pu * FuelLevel + 20f * Ratio;

		public bool MOX => FuelDef.mox;

		public override float strength => Mathf.Lerp(1f, 0f, FuelLevel);

		public reactorCore GetCore => base.ParentHolder as reactorCore;

		public override Graphic Graphic
		{
			get
			{
				if (cracked)
				{
					return CrackedRod;
				}
				return base.Graphic;
			}
		}

		public bool Reprocessable
		{
			get
			{
				if (cracked)
				{
					return true;
				}
				return FuelLevel < 0.5f;
			}
		}

		public string rodString
		{
			get
			{
				sb.Clear();
				if (FuelLevel > 0f)
				{
					sb.Append(FuelLevel.ToStringPercent("0.0"));
				}
				else
				{
					sb.Append("RimatomSpent".Translate());
				}
				if (Reprocessable)
				{
					sb.Append("-Pu");
				}
				return sb.ToString();
			}
		}

		public override string Label => base.Label + " " + rodString;

		public float Curve
		{
			get
			{
				if (OldFuelMode)
				{
					return FuelLevel;
				}
				if (GetCore.IsBreeder)
				{
					return FuelDef.fastCurve.Evaluate(FuelLevel);
				}
				return FuelDef.thermalCurve.Evaluate(FuelLevel);
			}
		}

		public float ActinidesPerTick
		{
			get
			{
				if (GetCore.FastFuel)
				{
					rate *= 1200f;
				}
				return rate;
			}
		}

		public override bool CanStackWith(Thing other)
		{
			return false;
		}

		public void UpdateFuelRefs(reactorCore core)
		{
			AdjFuelRefs = new Item_NuclearFuel[4];
			for (int i = 0; i < 4; i++)
			{
				int ind = core.FuelCells.IndexOf(core.FuelCells[SlotID] + GenAdj.CardinalDirections[i]);
				if (ind != -1)
				{
					AdjFuelRefs[i] = core.FuelGrid.FirstOrDefault((Item_NuclearFuel f) => f.SlotID == ind);
				}
			}
			UpdateFluxShape(core);
		}

		public void UpdateFluxShape(reactorCore core)
		{
			float curve = Curve;
			BasePowerLevel = 0f;
			MaxPowerLevel = 0f;
			ChainReaction = curve;
			BurnupRate = 0f;
			BurnupRateForAdjFuel = curve;
			for (int i = 0; i < 4; i++)
			{
				if (AdjFuelRefs[i] != null)
				{
					ChainReaction += AdjFuelRefs[i].Curve;
					BurnupRateForAdjFuel += AdjFuelRefs[i].Curve;
					BurnupRate += AdjFuelRefs[i].BurnupRateForAdjFuel;
				}
			}
			BurnupRateForAdjFuel *= core.FinalFlux;
			BurnupRate += BurnupRateForAdjFuel;
			BurnupRate *= RimatomicsMod.Settings.FuelBurnRate;
			MaxPowerLevel = MaxPwerCurveGetter(core) * ChainReaction;
			ChainReaction *= core.FinalFlux;
			BasePowerLevel = PwerCurveGetter(core) * ChainReaction;
		}

		public float MaxPwerCurveGetter(reactorCore core)
		{
			float num = 0f;
			num = ((!core.IsBreeder) ? FuelDef.thermalCurve.Evaluate(1f) : FuelDef.fastCurve.Evaluate(1f));
			if (cracked)
			{
				num *= 0.5f;
			}
			return FuelDef.PowerOutput * num;
		}

		public float PwerCurveGetter(reactorCore core)
		{
			float num = 0f;
			num = ((!core.IsBreeder) ? FuelDef.thermalCurve.Evaluate(FuelLevel) : FuelDef.fastCurve.Evaluate(FuelLevel));
			if (cracked)
			{
				num *= 0.5f;
			}
			return FuelDef.PowerOutput * num;
		}

		public void FissionEvent(reactorCore core)
		{
			if (FuelLevel > 0f)
			{
				float num = ActinidesPerTick * BurnupRate;
				float num2 = num * core.ConversionRatio;
				FuelLevel = Mathf.Clamp01(FuelLevel - num);
				Ratio = Mathf.Min(1.2f, Ratio + num2);
			}
		}

		public override void DrawGUIOverlay()
		{
			base.DrawGUIOverlay();
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
				GenMapUI.DrawThingLabel(this, rodString, defaultThingLabelColor);
			}
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
					defaultLabel = "Debug: Crack",
					action = delegate
					{
						cracked = true;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Debug: Spent Thermal",
					action = delegate
					{
						FuelLevel = 0f;
						Ratio = 0.6f;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Debug: Spicy Bread",
					action = delegate
					{
						FuelLevel = 0.5f;
						Ratio = 1.2f;
					}
				};
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref markedForRemove, "markedForRemove", defaultValue: false, forceSave: true);
			Scribe_Values.Look(ref cracked, "cracked", defaultValue: false, forceSave: true);
			Scribe_Values.Look(ref Ratio, "Plutonium", 0f, forceSave: true);
			Scribe_Values.Look(ref FuelLevel, "Actinides", 1f, forceSave: true);
			Scribe_Values.Look(ref SlotID, "SlotID", -1, forceSave: true);
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append("critFuelRemaining2".Translate(FuelLevel.ToStringPercent("0.00")));
			if (cracked)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("fuelCracked".Translate());
			}
			stringBuilder.AppendLine();
			stringBuilder.Append("PuCreated".Translate(PuCreated));
			if (Reprocessable)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("critEnouPU".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
