using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class reactorCore : Building, IAssignableGreek, IThingHolder, IStoreSettingsParent, IFuelFilter
	{
		public bool AutoScram = true;

		public bool FlareScram = true;

		public bool BreederHotLoad;

		public CompPipe ColdWater;

		public bool ControlRodActuator;

		public ThingDef ControlRodActuatorDef = ThingDef.Named("ControlRodActuator");

		public float coolingCapPct;

		public float coolingCapPctTo;

		public bool DrawFX = true;

		public bool FastFuel;

		public CompFlickable flickableComp;

		public List<IntVec3> FuelCells;

		public int FuelCount;

		public List<Item_NuclearFuel> FuelGrid = new List<Item_NuclearFuel>();

		public ThingOwner FuelGridOwner;

		public Vector3 IDlabel = new Vector3(-1.8125f, 1f, 2.265625f);

		public int IntGreekID;

		public bool IsShutdown = true;

		public Vector3 led1 = new Vector3(-2f, 1f, -1.5f);

		public Vector3 led2 = new Vector3(-1.75f, 1f, -1.75f);

		public Vector3 led3 = new Vector3(-1.5f, 1f, -2f);

		public Vector3 ledbig = new Vector3(-2f, 1f, -2f);

		public CompPipe Loom;

		public float MaxPowerForFuelLevel;

		public float MaxPowerPossible;

		public float MaxSafeTemp = 600f;

		public float nudgeVal;

		public Vector3 OverheatIco = new Vector3(-1.75f, 1f, 2.109375f);

		public float overheating;

		public float overheatingSpeed;

		public float overheatingTarget;

		public float overheatingVelocity;

		public float postReturnTemp;

		public CompPowerTrader powerComp;

		public float radius;

		public Vector3 RadLeakIco = new Vector3(-1.75f, 1f, 2.109375f);

		public int RadLightTimer = 1;

		public float RealControlRodPosition;

		public float RealControlRodVelocity;

		public float RealTemp;

		public float RealTempPct;

		public float RealTempVelocity;

		public bool SCRAMTest;

		public int shutdownFlicker;

		public List<RodDesignate> SlotDesignations;

		public CompPipe Steam;

		public float TargetControlRodPosition;

		[SyncField(SyncContext.None)]
		public float TargetControlRodTo;

		private float TargetControlRodToOld;

		public float TargetControlRodVelocity;

		public float TargetTempVelocty;

		public float ThermalEnergy;

		public float totalPowerPct;

		public float totalPowerPctTo;

		public int Uptime;

		public Dictionary<int, Item_NuclearFuel> fuelscache = new Dictionary<int, Item_NuclearFuel>();

		public FloatRange _FuelLifeFilter = new FloatRange(0f, 1f);

		public Dictionary<int, RodStatus> SlotStatus = new Dictionary<int, RodStatus>();

		private List<Material> slotmats = new List<Material>();

		private int SlotToUpdate;

		private int ShapeRod;

		private StringBuilder stringBuilder = new StringBuilder();

		private StorageSettings storageSettings;

		public virtual bool IsBuilt => DubDef.BuildReactorCore.IsFinished;

		public virtual ThingDef PoppedCoreDef => ThingDef.Named("PoppedReactorCoreA");

		public SteamNet SteamNet => Steam.net as SteamNet;

		public ColdWaterNet ColdWaterNet => ColdWater.net as ColdWaterNet;

		public LoomNet LoomNet => Loom.net as LoomNet;

		public CompUpgradable UG => GetComp<CompUpgradable>();

		public bool CrackedFuel => FuelGrid.Any((Item_NuclearFuel x) => x.cracked);

		public bool IsCoreMelt => postReturnTemp > 2000f;

		public virtual int SlotCount => 21;

		public float drawScale { get; set; }

		public float drawOffset { get; set; }

		public virtual bool IsBreeder => false;

		public bool coldAndDead
		{
			get
			{
				if (IsCold)
				{
					return IsShutdown;
				}
				return false;
			}
		}

		public bool IsCold => postReturnTemp < 50f;

		public bool IsOverheating => postReturnTemp > 500f;

		public float StandardTemp
		{
			get
			{
				if (!IsBreeder)
				{
					return 315f;
				}
				return 375f;
			}
		}

		public float coreIntegrity => Mathf.InverseLerp(0f, base.MaxHitPoints, HitPoints);

		public float FinalFlux => RealControlRodPosition;

		public string StatusText
		{
			get
			{
				if (IsCold && IsShutdown)
				{
					return "ShutdownCold".Translate();
				}
				if (IsCold && !IsShutdown)
				{
					return "StartingUp".Translate();
				}
				if (!IsCold && IsShutdown)
				{
					return "ShuttingDown".Translate();
				}
				return "RimatomActive".Translate();
			}
		}

		public float ControlRodSpeed
		{
			get
			{
				float num = 0.3f;
				if (ControlRodActuator)
				{
					num = 0.6f;
				}
				return num * GenMath.LerpDoubleClamped(500f, 1500f, 1f, 0f, postReturnTemp);
			}
		}

		public float ConversionRatio
		{
			get
			{
				if (IsBreeder)
				{
					return 1.2f;
				}
				return 0.6f;
			}
		}

		public float Leakage
		{
			get
			{
				float num = Mathf.InverseLerp(0f, SlotCount, FuelCount);
				float num2 = Mathf.InverseLerp(base.MaxHitPoints, 0f, HitPoints);
				return 5f * num * num2;
			}
		}

		public float CoreTempRequest
		{
			get
			{
				if (FinalFlux > 0.001f && FuelCount > 0)
				{
					return StandardTemp;
				}
				return 0f;
			}
		}

		public float RodPositionRequest
		{
			get
			{
				if (!powerComp.PowerOn || IsShutdown || FuelCount == 0)
				{
					return 0f;
				}
				return TargetControlRodPosition;
			}
		}

		public int GreekID
		{
			get
			{
				return IntGreekID;
			}
			set
			{
				IntGreekID = value;
			}
		}

		public bool StorageTabVisible => true;

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

		public void Notify_SettingsChanged()
		{
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return FuelGridOwner;
		}

		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "upgraded")
			{
				CheckUpgrades();
			}
		}

		public void CheckUpgrades()
		{
			ControlRodActuator = UG.HasUpgrade(ControlRodActuatorDef);
		}

		public AcceptanceReport CanUpgradeNow()
		{
			if (!coldAndDead)
			{
				return new AcceptanceReport("coreUpgradeFail");
			}
			return true;
		}

		[SyncMethod(SyncContext.None)]
		public static void SetGreekID(Thing t, int li)
		{
			((IAssignableGreek)t).GreekID = li;
		}

		public void RefreshFuelGrid()
		{
			FuelGrid.Clear();
			fuelscache.Clear();
			SlotStatus.Clear();
			FuelGrid = FuelGridOwner.Select((Thing x) => x as Item_NuclearFuel).ToList();
			FuelCount = FuelGrid.Count;
			foreach (Item_NuclearFuel item in FuelGrid)
			{
				item.UpdateFuelRefs(this);
			}
			UpdateGraphicsCache();
		}

		public Item_NuclearFuel FuelAt(int i)
		{
			if (fuelscache.ContainsKey(i))
			{
				return fuelscache[i];
			}
			Item_NuclearFuel item_NuclearFuel = FuelGridOwner.FirstOrDefault((Thing x) => ((Item_NuclearFuel)x).SlotID == i) as Item_NuclearFuel;
			fuelscache.Add(i, item_NuclearFuel);
			return item_NuclearFuel;
		}

		public override void PostMake()
		{
			base.PostMake();
			storageSettings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				storageSettings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			ColdWater = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.ColdWater);
			Steam = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Steam);
			Loom = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Loom);
			powerComp = GetComp<CompPowerTrader>();
			flickableComp = GetComp<CompFlickable>();
			CheckUpgrades();
			RefreshFuelGrid();
			DubUtils.GetResearch().NotifyResearch();
			if (TargetControlRodToOld > 0f)
			{
				if (IsBreeder)
				{
					TargetControlRodTo = TargetControlRodToOld / 3f;
				}
				else
				{
					TargetControlRodTo = TargetControlRodToOld / 2f;
				}
				TargetControlRodToOld = -1f;
				RealTemp = 0f;
				postReturnTemp = 0f;
				Find.LetterStack.ReceiveLetter("Rimatomics Update!", "Rimatomics has been updated, the power settings of this reactor have been adjusted for the changes and may require attention", LetterDefOf.NeutralEvent, this);
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode == DestroyMode.Deconstruct || mode == DestroyMode.Refund)
			{
				FuelGridOwner.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
			}
			base.DeSpawn(mode);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref FuelGridOwner, "FuelGridOwner", this);
			Scribe_Collections.Look(ref SlotDesignations, "SlotDesignations", LookMode.Value);
			Scribe_Values.Look(ref IntGreekID, "IntGreekID", 0);
			Scribe_Values.Look(ref _FuelLifeFilter, "_FuelLifeFilter", new FloatRange(0.01f, 1f));
			Scribe_Values.Look(ref ThermalEnergy, "thermalEnergy", 0f);
			Scribe_Values.Look(ref RealControlRodVelocity, "RealControlRodVelocity", 0f);
			Scribe_Values.Look(ref RealTempVelocity, "RealTempVelocity", 0f);
			Scribe_Values.Look(ref TargetControlRodVelocity, "TargetControlRodVelocity", 0f);
			Scribe_Values.Look(ref TargetTempVelocty, "TargetTempVelocty", 0f);
			Scribe_Values.Look(ref coolingCapPctTo, "coolingCapPctTo", 0f);
			Scribe_Values.Look(ref TargetControlRodTo, "TargetControlRodToNewModel", 0f);
			Scribe_Values.Look(ref TargetControlRodToOld, "TargetControlRodTo", 0f);
			Scribe_Values.Look(ref TargetControlRodPosition, "TargetControlRodPosition", 0f);
			Scribe_Values.Look(ref RealControlRodPosition, "RealControlRodPosition", 0f);
			Scribe_Values.Look(ref RealTemp, "RealTemp", 0f);
			Scribe_Values.Look(ref postReturnTemp, "postReturnTemp", 0f);
			Scribe_Values.Look(ref AutoScram, "AutoScram", defaultValue: true);
			Scribe_Values.Look(ref FlareScram, "FlareScram", defaultValue: true);
			Scribe_Values.Look(ref BreederHotLoad, "BreederHotLoad", defaultValue: false);
			Scribe_Values.Look(ref IsShutdown, "shutdown", defaultValue: true);
			Scribe_Values.Look(ref SCRAMTest, "SCRAMTest", defaultValue: true);
			Scribe_Values.Look(ref Uptime, "Uptime", 0);
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		}

		public override void DrawGUIOverlay()
		{
			base.DrawGUIOverlay();
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				Vector2 screenPos = Find.Camera.WorldToScreenPoint(DrawPos + IDlabel) / Prefs.UIScale;
				screenPos.y = (float)UI.screenHeight - screenPos.y;
				GenMapUI.DrawThingLabel(screenPos, greekAlpha.getAlpha(GreekID), Color.yellow);
			}
		}

		[SyncMethod(SyncContext.None)]
		public void Startup()
		{
			IsShutdown = false;
		}

		[SyncMethod(SyncContext.None)]
		public void SCRAM()
		{
			if (FinalFlux > 0.98f && FuelCount >= SlotCount)
			{
				SCRAMTest = true;
			}
			IsShutdown = true;
		}

		public void InsertFuel(Item_NuclearFuel rod)
		{
			if (!rod.MOX)
			{
				rod.SlotID = SlotDesignations.FirstIndexOf((RodDesignate x) => x == RodDesignate.Fuel);
			}
			if (rod.MOX)
			{
				rod.SlotID = SlotDesignations.FirstIndexOf((RodDesignate x) => x == RodDesignate.MOX);
			}
			if (rod.SlotID != -1)
			{
				SlotDesignations[rod.SlotID] = RodDesignate.None;
				FuelGridOwner.TryAddOrTransfer(rod, canMergeWithExistingStacks: false);
				RefreshFuelGrid();
			}
		}

		public Thing RemoveFuel()
		{
			int fuelID = SlotDesignations.FirstIndexOf((RodDesignate x) => x == RodDesignate.Remove);
			if (fuelID != -1)
			{
				SlotDesignations[fuelID] = RodDesignate.None;
				Item_NuclearFuel item_NuclearFuel = FuelGrid.FirstOrDefault((Item_NuclearFuel x) => x.SlotID == fuelID);
				if (item_NuclearFuel != null)
				{
					item_NuclearFuel.SlotID = -1;
					FuelGridOwner.TryDrop(item_NuclearFuel, ThingPlaceMode.Near, out var lastResultingThing);
					RefreshFuelGrid();
					return lastResultingThing;
				}
			}
			return null;
		}

		private void UpdateStatus(int i)
		{
			if (!SlotStatus.ContainsKey(i))
			{
				SlotStatus.Add(i, RodStatus.Empty);
			}
			Item_NuclearFuel item_NuclearFuel = FuelAt(i);
			if (item_NuclearFuel == null)
			{
				SlotStatus[i] = RodStatus.Empty;
			}
			else if (item_NuclearFuel.cracked)
			{
				SlotStatus[i] = RodStatus.Cracked;
			}
			else if (Math.Abs(item_NuclearFuel.FuelLevel) < 0.01f)
			{
				SlotStatus[i] = RodStatus.Spent;
			}
			else if (item_NuclearFuel.Reprocessable)
			{
				SlotStatus[i] = RodStatus.Activated;
			}
			else
			{
				SlotStatus[i] = RodStatus.New;
			}
		}

		private void UpdateGraphicsCache()
		{
			slotmats.Clear();
			for (int i = 0; i < FuelCells.Count; i++)
			{
				slotmats.Add(GraphicsCache.LEDoff);
			}
			for (int j = 0; j < slotmats.Count; j++)
			{
				UpdateStatus(j);
			}
			for (int k = 0; k < slotmats.Count; k++)
			{
				updateSlotMatAt(k);
			}
		}

		private void updateSlotMatAt(int i)
		{
			if (powerComp.PowerOn)
			{
				switch (SlotStatus[i])
				{
				case RodStatus.Cracked:
					slotmats[i] = GraphicsCache.LEDred;
					break;
				case RodStatus.Empty:
					slotmats[i] = GraphicsCache.LEDoff;
					break;
				case RodStatus.New:
					slotmats[i] = GraphicsCache.LEDblue;
					break;
				case RodStatus.Activated:
					slotmats[i] = GraphicsCache.LEDgreen;
					break;
				case RodStatus.Spent:
					slotmats[i] = GraphicsCache.LEDoff;
					break;
				}
			}
			else
			{
				slotmats[i] = GraphicsCache.LEDoff;
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (!DrawFX)
			{
				return;
			}
			Quaternion asQuat = base.Rotation.AsQuat;
			for (int i = 0; i < FuelCells.Count; i++)
			{
				Vector3 p = FuelCells[i].ToVector3() * drawScale;
				p.y += 1f;
				p += DrawPos;
				p.x += drawOffset;
				p.z -= drawOffset;
				DubUtils.drawLED(p, asQuat, slotmats[i]);
			}
			if (!powerComp.PowerOn)
			{
				return;
			}
			if (coldAndDead)
			{
				DubUtils.drawLED(DrawPos + ledbig, asQuat, GraphicsCache.BigBlueLed);
			}
			else if (!IsCold && IsShutdown)
			{
				DubUtils.drawLED(DrawPos + ledbig, asQuat, GraphicsCache.BigAmberLed);
				switch (shutdownFlicker)
				{
				case 0:
					DubUtils.drawLED(DrawPos + led1, asQuat, GraphicsCache.miniRedLed);
					break;
				case 1:
					DubUtils.drawLED(DrawPos + led2, asQuat, GraphicsCache.miniRedLed);
					break;
				case 2:
					DubUtils.drawLED(DrawPos + led3, asQuat, GraphicsCache.miniRedLed);
					break;
				}
			}
			else
			{
				DubUtils.drawLED(DrawPos + ledbig, asQuat, GraphicsCache.BigRedLed);
				DubUtils.drawLED(DrawPos + led1, asQuat, GraphicsCache.miniRedLed);
				DubUtils.drawLED(DrawPos + led2, asQuat, GraphicsCache.miniRedLed);
				DubUtils.drawLED(DrawPos + led3, asQuat, GraphicsCache.miniRedLed);
			}
			if (Leakage > 0f)
			{
				DubUtils.drawLED(DrawPos + RadLeakIco, asQuat, GraphicsCache.radiation);
			}
			if (IsOverheating)
			{
				DubUtils.drawLED(DrawPos + OverheatIco, asQuat, GraphicsCache.overheat);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (!IsBuilt)
			{
				return;
			}
			ShapeRod++;
			if (ShapeRod > FuelCount)
			{
				ShapeRod = 0;
			}
			for (int i = 0; i < FuelCount; i++)
			{
				Item_NuclearFuel item_NuclearFuel = FuelGrid[i];
				if (ShapeRod == i)
				{
					item_NuclearFuel.UpdateFluxShape(this);
				}
				item_NuclearFuel.FissionEvent(this);
			}
			if (this.IsHashIntervalTick(10))
			{
				overheatingSpeed = GenMath.LerpDoubleClamped(1f, 100f, 30f, 250f, FuelCount);
				MaxPowerPossible = FuelGrid.Sum((Item_NuclearFuel x) => x.MaxPowerLevel);
				MaxPowerForFuelLevel = FuelGrid.Sum((Item_NuclearFuel x) => x.BasePowerLevel);
				ThermalEnergy = MaxPowerForFuelLevel * RealTempPct;
				if (Leakage > 0f)
				{
					DubUtils.emitRadiation(base.Position, Leakage, 12f, base.Map);
				}
			}
			DataGathering();
			Interps();
			if (RealTemp > 100f)
			{
				Uptime++;
			}
			else
			{
				Uptime = 0;
			}
			if (this.IsHashIntervalTick(240))
			{
				if (FlareScram && base.Map.gameConditionManager.ElectricityDisabled && !IsShutdown)
				{
					IsShutdown = true;
					Find.LetterStack.ReceiveLetter("reactorSCRAM".Translate(), "FlareSCRAMletterText".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(this));
				}
				if (AutoScram && postReturnTemp > 800f && !IsShutdown)
				{
					IsShutdown = true;
					Find.LetterStack.ReceiveLetter("reactorSCRAM".Translate(), "SCRAMletterText".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(this));
				}
			}
			if (this.IsHashIntervalTick(30))
			{
				if (HitPoints < base.MaxHitPoints && postReturnTemp > 150f)
				{
					IntegrityEffects();
				}
				if (Leakage > 1f)
				{
					GenTemperature.PushHeat(base.Position, base.MapHeld, Leakage * 5f);
				}
				shutdownFlicker++;
				RadLightTimer++;
				if (RadLightTimer > 3)
				{
					RadLightTimer = 0;
				}
				if (shutdownFlicker > 3)
				{
					shutdownFlicker = 0;
				}
				if (postReturnTemp > 400f)
				{
					Rand.PushState();
					if (Rand.MTBEventOccurs(0.35f, 60000f, 30f))
					{
						TakeDamage(new DamageInfo(DamageDefOf.Flame, Mathf.RoundToInt(100f)));
					}
					if (Rand.MTBEventOccurs(10f, 60000f, 30f) && FuelGrid.Where((Item_NuclearFuel x) => !x.cracked).TryRandomElement(out var result))
					{
						result.cracked = true;
					}
					Rand.PopState();
				}
				if (postReturnTemp > MaxSafeTemp)
				{
					if (Rand.Chance(GenMath.LerpDoubleClamped(1000f, 5000f, 0f, 1f, postReturnTemp)) && FuelGrid.Where((Item_NuclearFuel x) => !x.cracked).TryRandomElement(out var result2))
					{
						result2.cracked = true;
					}
					float f = GenMath.LerpDoubleClamped(MaxSafeTemp, 10000f, 0f, 40f, postReturnTemp);
					TakeDamage(new DamageInfo(DamageDefOf.Flame, Mathf.RoundToInt(f)));
				}
				if (postReturnTemp > 1600f && Rand.Chance(GenMath.LerpDoubleClamped(2000f, 3000f, 0f, 0.1f, postReturnTemp)))
				{
					Destroy(DestroyMode.KillFinalize);
				}
			}
			UpdateStatus(SlotToUpdate);
			updateSlotMatAt(SlotToUpdate);
			SlotToUpdate++;
			if (SlotToUpdate >= SlotCount)
			{
				SlotToUpdate = 0;
			}
		}

		public void DataGathering()
		{
			if (SCRAMTest && FinalFlux < 0.01f)
			{
				SCRAMTest = false;
				DubUtils.GetResearch().GatherData("SCRAMTest", 1f);
			}
			if (this.IsHashIntervalTick(250))
			{
				if (FinalFlux > 0.08f)
				{
					DubUtils.GetResearch().GatherData("PrototypeFluxTest", 1f);
				}
				if (FuelCount >= SlotCount && FinalFlux > 0.08f)
				{
					DubUtils.GetResearch().GatherData("fluxTest2", 1f);
				}
				if (FuelCount >= SlotCount && FinalFlux > 0.1f && FinalFlux < 0.2f)
				{
					DubUtils.GetResearch().GatherData("fluxTest3", 1f);
				}
				if (ThermalEnergy > 0f && IsBreeder)
				{
					DubUtils.GetResearch().GatherData("PrototypeBreederTest", 1f);
				}
				if (Leakage > 1f)
				{
					DubUtils.GetResearch().GatherData("RadiationTest", 1f);
				}
			}
		}

		public void Interps()
		{
			TargetControlRodPosition = DubUtils.FInterpTo(TargetControlRodPosition, TargetControlRodTo, 1f, 0.02f);
			RealControlRodPosition = Mathf.SmoothDamp(RealControlRodPosition, RodPositionRequest, ref RealControlRodVelocity, 1f, ControlRodSpeed, 0.016f);
			if (RealControlRodPosition < 0.0001f)
			{
				RealControlRodPosition = 0f;
			}
			RealTemp = Mathf.SmoothDamp(RealTemp, CoreTempRequest, ref RealTempVelocity, 5f, 20f, 0.016f);
			if (RealTemp < 0.0001f)
			{
				RealTemp = 0f;
			}
			if (ColdWaterNet != null && ColdWaterNet.Turbines.Any())
			{
				overheatingTarget = Mathf.Clamp(ColdWaterNet.ColdWaterFeedReturn / 100f, 0f, MaxPowerForFuelLevel);
			}
			else
			{
				overheatingTarget = MaxPowerForFuelLevel / 100f;
			}
			overheating = Mathf.SmoothDamp(overheating, overheatingTarget, ref overheatingVelocity, (overheatingTarget > overheating) ? 5f : 15f, overheatingSpeed, 0.016f);
			postReturnTemp = RealTemp + overheating;
			RealTempPct = Mathf.InverseLerp(0f, StandardTemp, postReturnTemp);
			if (SteamNet != null && SteamNet.Turbines.Any())
			{
				coolingCapPctTo = SteamNet.Turbines.Min((Turbine x) => x.CoolingNet.CoolingLoopRatio);
				totalPowerPctTo = SteamNet.SteamLoopRatio;
			}
			else
			{
				coolingCapPctTo = 1f;
				totalPowerPctTo = 0f;
			}
			coolingCapPct = DubUtils.FInterpTo(coolingCapPct, coolingCapPctTo, 1f, 0.01f);
			totalPowerPct = DubUtils.FInterpTo(totalPowerPct, totalPowerPctTo, 1f, 0.01f);
		}

		public void IntegrityEffects()
		{
			float num = coreIntegrity;
			if (num < 0.8f)
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(2.1f, 0f, 1.53f), base.Map, Mathf.InverseLerp(5f, 2f, num));
			}
			if (num < 0.6f)
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(2.4f, 0f, -2.4f), base.Map, Mathf.InverseLerp(6f, 3f, num));
			}
			if (num < 0.4f)
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(0f, 0f, 0f), base.Map, Mathf.InverseLerp(7f, 4f, num));
				FleckMaker.ThrowMicroSparks(DrawPos + new Vector3(2.1f, 0f, -2f), base.Map);
			}
			if (num < 0.2f)
			{
				FleckMaker.ThrowSmoke(DrawPos + new Vector3(-2.4f, 0f, 2f), base.Map, 1f);
				FleckMaker.ThrowMicroSparks(DrawPos + new Vector3(-1.2f, 0f, 1f), base.Map);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			IntVec3 position = base.Position;
			base.Destroy(mode);
			if (postReturnTemp > 100f)
			{
				(GenSpawn.Spawn(PoppedCoreDef, position, map) as Building_PoppedCore)?.GoBang(FuelCount, map);
			}
		}

		[SyncMethod(SyncContext.None)]
		public void DesignateSlot(int i, RodDesignate des)
		{
			SlotDesignations[i] = des;
		}

		[SyncMethod(SyncContext.None)]
		public void togScram()
		{
			AutoScram = !AutoScram;
		}

		[SyncMethod(SyncContext.None)]
		public void togFlareScram()
		{
			FlareScram = !FlareScram;
		}

		[SyncMethod(SyncContext.None)]
		public void togBreederHotLoad()
		{
			BreederHotLoad = !BreederHotLoad;
		}

		[SyncMethod(SyncContext.None)]
		public void punchit()
		{
			IsShutdown = false;
			TargetControlRodTo = 1f;
		}

		[SyncMethod(SyncContext.None)]
		public void maxfuel()
		{
			for (int i = 0; i < SlotDesignations.Count; i++)
			{
				if (SlotStatus[i] == RodStatus.Empty)
				{
					SlotDesignations[i] = RodDesignate.None;
					if (ThingMaker.MakeThing(DubDef.FuelRods) is Item_NuclearFuel item_NuclearFuel)
					{
						item_NuclearFuel.SlotID = i;
						FuelGridOwner.TryAddOrTransfer(item_NuclearFuel, canMergeWithExistingStacks: false);
					}
				}
			}
			RefreshFuelGrid();
		}

		[SyncMethod(SyncContext.None)]
		public void maxMOXfuel()
		{
			for (int i = 0; i < SlotDesignations.Count; i++)
			{
				if (SlotStatus[i] == RodStatus.Empty)
				{
					SlotDesignations[i] = RodDesignate.None;
					if (ThingMaker.MakeThing(DubDef.FuelRodsMOX) is Item_NuclearFuel item_NuclearFuel)
					{
						item_NuclearFuel.SlotID = i;
						FuelGridOwner.TryAddOrTransfer(item_NuclearFuel, canMergeWithExistingStacks: false);
					}
				}
			}
			RefreshFuelGrid();
		}

		[SyncMethod(SyncContext.None)]
		public void fastfuel()
		{
			FastFuel = !FastFuel;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_Toggle
			{
				defaultLabel = "AutoScram".Translate(),
				defaultDesc = "AutoScramDesc".Translate(),
				isActive = () => AutoScram,
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/scram"),
				toggleAction = togScram
			};
			yield return new Command_Toggle
			{
				defaultLabel = "FlareScram".Translate(),
				defaultDesc = "FlareScramDesc".Translate(),
				isActive = () => FlareScram,
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/scram"),
				toggleAction = togFlareScram
			};
			if (IsBreeder)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "BreederHotLoading".Translate(),
					defaultDesc = "BreederHotLoadingDesc".Translate(),
					isActive = () => BreederHotLoad,
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/fuel"),
					toggleAction = togBreederHotLoad
				};
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "punchit",
					defaultDesc = "",
					action = punchit
				};
				yield return new Command_Action
				{
					defaultLabel = "maxfuel",
					defaultDesc = (SlotDesignations.Count.ToString() ?? ""),
					action = maxfuel
				};
				yield return new Command_Action
				{
					defaultLabel = "maxMOXfuel",
					defaultDesc = (SlotDesignations.Count.ToString() ?? ""),
					action = maxMOXfuel
				};
				yield return new Command_Action
				{
					defaultLabel = "fastfuel",
					defaultDesc = "",
					action = fastfuel
				};
			}
			yield return new Command_SetModeulID(this)
			{
				defaultLabel = greekAlpha.getAlpha(GreekID),
				defaultDesc = "critsetid".Translate(),
				hotKey = KeyBindingDefOf.Misc1,
				icon = GraphicsCache.setID
			};
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine();
			stringBuilder.Append("ReactorUptime".Translate(Uptime.ToStringTicksToPeriod()));
			if (DebugSettings.godMode)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("ThermalEnergy: " + ThermalEnergy);
				stringBuilder.AppendLine();
				stringBuilder.Append("ColdWaterFeedReturn: " + ColdWaterNet.ColdWaterFeedReturn);
			}
			return stringBuilder.ToString().TrimEndNewlines();
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

		public reactorCore()
		{
			radius = GenRadial.RadiusOfNumCells(SlotCount - 1);
			FuelCells = GenRadial.RadialPatternInRadius(radius).ToList();
			SlotDesignations = new List<RodDesignate>(new RodDesignate[SlotCount]);
			FuelGridOwner = new ThingOwner<Thing>(this, oneStackOnly: false);
		}
	}
}
