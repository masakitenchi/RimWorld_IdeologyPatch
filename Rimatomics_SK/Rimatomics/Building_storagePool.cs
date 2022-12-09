using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_storagePool : Building, ISlotGroupParent, IStoreSettingsParent, IHaulDestination, IThingHolder, IFuelFilter
	{
		public enum armStatus
		{
			pickupStore,
			dropStore,
			pickupRemove,
			dropRemove,
			idle
		}

		public FloatRange _FuelLifeFilter = new FloatRange(0f, 1f);

		public static Graphic storagePoolWater;

		public static Graphic storagePoolWaterLit;

		public static Graphic loadingBay;

		public Vector3 armPos = Vector3.zero;

		public Vector3 armSize = new Vector3(7f, 1f, 1f);

		public armStatus ArmStatus = armStatus.idle;

		public Vector3 armVeloctiy = Vector3.zero;

		private List<Vector3> cachedPoolSlots;

		public Vector3 graphicSize = new Vector3(7f, 1f, 7f);

		protected ThingOwner innerContainer;

		public bool LoadingEnabled = true;

		public float maxH = 1.296875f;

		public float maxW = 1.546875f;

		public float minH = -1.765625f;

		public float minW = -1.546875f;

		public CompPowerTrader powerComp;

		public SlotGroup slotGroup;

		public int storageCap = 128;

		private StorageSettings storageSettings;

		public Vector3 targetArmPos = Vector3.zero;

		public bool UnLoadingEnabled = true;

		private Sustainer wickSustainer;

		private StringBuilder stringBuilder = new StringBuilder();

		private List<IntVec3> cachedOccupiedCells;

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

		public bool HasAnyContents => innerContainer.Count > 0;

		public Thing ContainedThing
		{
			get
			{
				if (innerContainer.Count == 0)
				{
					return null;
				}
				return innerContainer.First();
			}
		}

		public Vector3 droppingPos
		{
			get
			{
				if (GetDirectlyHeldThings().Count > 25)
				{
					return DrawPos + cachedPoolSlots.RandomElement();
				}
				return DrawPos + cachedPoolSlots[GetDirectlyHeldThings().Count - 1];
			}
		}

		public Vector3 pickupPos
		{
			get
			{
				if (GetDirectlyHeldThings().Count > 25)
				{
					return DrawPos + cachedPoolSlots.RandomElement();
				}
				return DrawPos + cachedPoolSlots[GetDirectlyHeldThings().Count - 1];
			}
		}

		public bool armOnTarget
		{
			get
			{
				if (Mathf.Abs(armPos.x - targetArmPos.x) < 0.01f)
				{
					return Mathf.Abs(armPos.y - targetArmPos.y) < 0.01f;
				}
				return false;
			}
		}

		public IntVec3 unloadSlot
		{
			get
			{
				if (base.Rotation == Rot4.North)
				{
					return IntVec3.FromVector3(DrawPos + new Vector3(1f, 0f, -2f));
				}
				if (base.Rotation == Rot4.South)
				{
					return IntVec3.FromVector3(DrawPos + new Vector3(-1f, 0f, 2f));
				}
				if (base.Rotation == Rot4.East)
				{
					return IntVec3.FromVector3(DrawPos + new Vector3(-2f, 0f, -1f));
				}
				return IntVec3.FromVector3(DrawPos + new Vector3(2f, 0f, 1f));
			}
		}

		public bool StorageTabVisible => true;

		public bool IgnoreStoredThingsBeauty => true;

		public Building_storagePool()
		{
			slotGroup = new SlotGroup(this);
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}

		public bool Accepts(Thing t)
		{
			return storageSettings.AllowedToAccept(t);
		}

		public StorageSettings GetStoreSettings()
		{
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			cachedPoolSlots = poolSlots().ToList();
			armPos = DrawPos;
			targetArmPos = DrawPos;
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
			base.DeSpawn(mode);
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _FuelLifeFilter, "_FuelLifeFilter", new FloatRange(0f, 0.95f));
			Scribe_Values.Look(ref LoadingEnabled, "LoadingEnabled", defaultValue: true);
			Scribe_Values.Look(ref UnLoadingEnabled, "UnLoadingEnabled", defaultValue: true);
			Scribe_Values.Look(ref armVeloctiy, "armVeloctiy");
			Scribe_Values.Look(ref targetArmPos, "targetArmPos");
			Scribe_Values.Look(ref armPos, "armPos");
			Scribe_Values.Look(ref ArmStatus, "ArmStatus", armStatus.idle);
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		}

		public void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			wickSustainer = SoundDef.Named("GeothermalPlant_Ambience").TrySpawnSustainer(info);
		}

		public IEnumerable<Vector3> poolSlots()
		{
			for (int y = 0; y < 5; y++)
			{
				for (int x = 0; x < 5; x++)
				{
					yield return new Vector3(Mathf.Lerp(minW, maxW, 0.25f * (float)x), 0f, Mathf.Lerp(minH, maxH, 0.25f * (float)y) + 0.16f);
				}
			}
		}

		public override void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this, 0f);
			if (storagePoolWater == null)
			{
				storagePoolWater = GraphicDatabase.Get(typeof(Graphic_Single), "Rimatomics/FX/storagePoolWater", ShaderDatabase.Transparent, def.graphicData.drawSize, DrawColor, DrawColorTwo);
			}
			if (storagePoolWaterLit == null)
			{
				storagePoolWaterLit = GraphicDatabase.Get(typeof(Graphic_Single), "Rimatomics/FX/storagePoolWaterLit", ShaderDatabase.MoteGlow, def.graphicData.drawSize, DrawColor, DrawColorTwo);
			}
			if (loadingBay == null)
			{
				loadingBay = GraphicDatabase.Get(typeof(Graphic_Single), "Rimatomics/Things/RimatomicsBuildings/loadingBay", ShaderDatabase.CutoutComplex, def.graphicData.drawSize, DrawColor, DrawColorTwo);
			}
			int count = GetDirectlyHeldThings().Count;
			count = Mathf.Min(count, 25);
			if (count < 25 && (ArmStatus == armStatus.dropRemove || ArmStatus == armStatus.dropStore))
			{
				count--;
			}
			for (int i = 0; i < count; i++)
			{
				Printer_Plane.PrintPlane(layer, DrawPos + new Vector3(0f, 0.1f, 0f) + cachedPoolSlots[i], Vector2.one, GraphicsCache.storagePoolRods);
			}
			if (powerComp.PowerOn)
			{
				Printer_Plane.PrintPlane(layer, DrawPos + new Vector3(0f, 0.3f, 0f), storagePoolWaterLit.drawSize, storagePoolWaterLit.MatSingle);
			}
			else
			{
				Printer_Plane.PrintPlane(layer, DrawPos + new Vector3(0f, 0.3f, 0f), storagePoolWater.drawSize, storagePoolWater.MatSingle);
			}
			Printer_Plane.PrintPlane(layer, DrawPos + new Vector3(0f, 0.36f, 0f), loadingBay.drawSize, loadingBay.MatSingle, base.Rotation.AsAngle);
		}

		public override void ReceiveCompSignal(string signal)
		{
			switch (signal)
			{
			case "PowerTurnedOn":
			case "PowerTurnedOff":
			case "FlickedOn":
			case "FlickedOff":
			case "Refueled":
			case "RanOutOfFuel":
			case "ScheduledOn":
			case "ScheduledOff":
				base.Map?.mapDrawer?.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
				break;
			}
		}

		public override void Draw()
		{
			base.Draw();
			drawPoolBit(base.Rotation.IsHorizontal ? new Vector3(armPos.x, DrawPos.y + 1.6f, DrawPos.z) : new Vector3(DrawPos.x, DrawPos.y + 1.6f, armPos.z), armSize, GraphicsCache.storageArm);
			drawPoolBit(new Vector3(armPos.x, DrawPos.y + 1.7f, armPos.z), Vector3.one, GraphicsCache.storageGrip);
		}

		public virtual void drawPoolBit(Vector3 pos, Vector3 size, Material mat)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, base.Rotation.AsQuat, size);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		public bool UnloadSlotEmpty()
		{
			return !unloadSlot.GetThingList(base.Map).Any((Thing x) => x is Item_NuclearFuel);
		}

		public override void Tick()
		{
			base.Tick();
			armPos = Vector3.SmoothDamp(armPos, targetArmPos, ref armVeloctiy, 0.5f, 5f, 0.0166f);
			if (!powerComp.PowerOn)
			{
				return;
			}
			if (ArmStatus == armStatus.pickupRemove && armOnTarget)
			{
				if (GetDirectlyHeldThings().OfType<Item_NuclearFuel>().Any((Item_NuclearFuel x) => x.markedForRemove) && UnloadSlotEmpty())
				{
					ArmStatus = armStatus.dropRemove;
					_ = GetDirectlyHeldThings().Count;
					_ = 25;
					targetArmPos = unloadSlot.ToVector3Shifted();
				}
				else
				{
					ArmStatus = armStatus.idle;
				}
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			}
			if (ArmStatus == armStatus.dropRemove && armOnTarget)
			{
				if (GetDirectlyHeldThings().OfType<Item_NuclearFuel>().Any((Item_NuclearFuel x) => x.markedForRemove) && UnloadSlotEmpty())
				{
					GetDirectlyHeldThings().TryDrop(GetDirectlyHeldThings().FirstOrDefault((Thing x) => ((Item_NuclearFuel)x).markedForRemove), unloadSlot, base.Map, ThingPlaceMode.Direct, out var lastResultingThing);
					((Item_NuclearFuel)lastResultingThing).markedForRemove = false;
				}
				ArmStatus = armStatus.idle;
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			}
			if (ArmStatus == armStatus.pickupStore && armOnTarget)
			{
				Thing thing = IntVec3.FromVector3(targetArmPos).GetThingList(base.Map).FirstOrDefault((Thing x) => x is Item_NuclearFuel);
				if (thing != null && GetDirectlyHeldThings().Count < storageCap)
				{
					Thing item = thing.SplitOff(1);
					GetDirectlyHeldThings().TryAddOrTransfer(item, canMergeWithExistingStacks: false);
					targetArmPos = droppingPos;
					ArmStatus = armStatus.dropStore;
				}
				else
				{
					ArmStatus = armStatus.idle;
				}
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			}
			if (ArmStatus == armStatus.dropStore && armOnTarget)
			{
				ArmStatus = armStatus.idle;
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			}
			if (ArmStatus == armStatus.idle)
			{
				targetArmPos = DrawPos;
				if (UnLoadingEnabled && GetDirectlyHeldThings().OfType<Item_NuclearFuel>().Any((Item_NuclearFuel x) => x.markedForRemove) && UnloadSlotEmpty())
				{
					targetArmPos = pickupPos;
					ArmStatus = armStatus.pickupRemove;
					return;
				}
				Thing thing2 = GetSlotGroup().HeldThings.FirstOrDefault();
				if (LoadingEnabled && thing2 != null && GetDirectlyHeldThings().Count < storageCap)
				{
					ArmStatus = armStatus.pickupStore;
					targetArmPos = thing2.DrawPos;
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
				}
			}
			else if (wickSustainer == null)
			{
				StartWickSustainer();
			}
			else if (wickSustainer.Ended)
			{
				StartWickSustainer();
			}
			else
			{
				wickSustainer.Maintain();
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (StorageTabVisible)
			{
				foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
				{
					yield return item;
				}
			}
			yield return new Command_Toggle
			{
				defaultLabel = "PoolLoading".Translate(),
				defaultDesc = "PoolLoading".Translate(),
				toggleAction = ToggleLoading,
				isActive = () => LoadingEnabled,
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/Install")
			};
			yield return new Command_Toggle
			{
				defaultLabel = "PoolUnloading".Translate(),
				defaultDesc = "PoolUnloading".Translate(),
				toggleAction = ToggleUnLoading,
				isActive = () => UnLoadingEnabled,
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/Uninstall")
			};
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.Append("critStorageCapacity".Translate(innerContainer.Count, storageCap));
			return stringBuilder.ToString();
		}

		[SyncMethod(SyncContext.None)]
		public void ToggleLoading()
		{
			LoadingEnabled = !LoadingEnabled;
		}

		[SyncMethod(SyncContext.None)]
		public void ToggleUnLoading()
		{
			UnLoadingEnabled = !UnLoadingEnabled;
		}

		[SyncMethod(SyncContext.None)]
		public void ToggleDesignation(Thing storedThing, bool selection)
		{
			((Item_NuclearFuel)storedThing).markedForRemove = selection;
		}

		public virtual IEnumerable<IntVec3> AllSlotCells()
		{
			if (base.Rotation == Rot4.North)
			{
				yield return IntVec3.FromVector3(DrawPos + new Vector3(-1f, 0f, -2f));
				yield return IntVec3.FromVector3(DrawPos + new Vector3(0f, 0f, -2f));
			}
			if (base.Rotation == Rot4.South)
			{
				yield return IntVec3.FromVector3(DrawPos + new Vector3(1f, 0f, 2f));
				yield return IntVec3.FromVector3(DrawPos + new Vector3(0f, 0f, 2f));
			}
			if (base.Rotation == Rot4.East)
			{
				yield return IntVec3.FromVector3(DrawPos + new Vector3(-2f, 0f, 1f));
				yield return IntVec3.FromVector3(DrawPos + new Vector3(-2f, 0f, 0f));
			}
			if (base.Rotation == Rot4.West)
			{
				yield return IntVec3.FromVector3(DrawPos + new Vector3(2f, 0f, 0f));
				yield return IntVec3.FromVector3(DrawPos + new Vector3(2f, 0f, -1f));
			}
		}

		public List<IntVec3> AllSlotCellsList()
		{
			return cachedOccupiedCells ?? (cachedOccupiedCells = AllSlotCells().ToList());
		}

		public void Notify_ReceivedThing(Thing newItem)
		{
		}

		public void Notify_LostThing(Thing newItem)
		{
		}

		public string SlotYielderLabel()
		{
			return LabelCap;
		}

		public SlotGroup GetSlotGroup()
		{
			return slotGroup;
		}

		public void Notify_SettingsChanged()
		{
		}
	}
}
