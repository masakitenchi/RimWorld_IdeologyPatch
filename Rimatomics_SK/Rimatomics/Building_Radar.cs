using System.Collections.Generic;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_Radar : Building, ICamoSelect
	{
		public Graphic baseGraphic;

		public string camoMode = "base";

		private float curRotationInt;

		private float curScanRotationInt;

		public bool HasATOM;

		public CompPowerTrader powerComp;

		public Graphic scannerMatCache;

		public Graphic turretMatCache;

		private static string toppath = "Rimatomics/Things/RimatomicsBuildings/RadarDish";

		public Graphic scannerMat
		{
			get
			{
				if (scannerMatCache == null)
				{
					_ = Graphic;
				}
				return scannerMatCache;
			}
		}

		public Graphic turretMat
		{
			get
			{
				if (turretMatCache == null)
				{
					_ = Graphic;
				}
				return turretMatCache;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				Vector2 drawSize = new Vector2(def.building.turretTopDrawSize, def.building.turretTopDrawSize);
				if (camoMode == "base")
				{
					turretMatCache = GraphicDatabase.Get<Graphic_Single>(toppath, ShaderDatabase.DefaultShader, drawSize, DrawColor);
					scannerMatCache = GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/scanner", ShaderDatabase.DefaultShader, drawSize, DrawColor);
					return base.Graphic;
				}
				if (baseGraphic != null)
				{
					return baseGraphic;
				}
				turretMatCache = GraphicDatabase.Get<Graphic_Single>(toppath + GetCamoMode, ShaderDatabase.DefaultShader, drawSize, Color.white);
				scannerMatCache = GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/scanner" + GetCamoMode, ShaderDatabase.DefaultShader, drawSize, Color.white);
				return baseGraphic = GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath + GetCamoMode, ShaderDatabase.DefaultShader, def.graphicData.drawSize, Color.white);
			}
		}

		public string GetCamoMode
		{
			get
			{
				if (!(camoMode == "base"))
				{
					return "-" + camoMode;
				}
				return "";
			}
		}

		public bool Working
		{
			get
			{
				if (powerComp.PowerOn)
				{
					return !base.Position.Roofed(base.Map);
				}
				return false;
			}
		}

		private float CurRotation
		{
			get
			{
				return curRotationInt;
			}
			set
			{
				curRotationInt = value;
				if (curRotationInt > 360f)
				{
					curRotationInt -= 360f;
				}
				if (curRotationInt < 0f)
				{
					curRotationInt += 360f;
				}
			}
		}

		private float CurScanRotation
		{
			get
			{
				return curScanRotationInt;
			}
			set
			{
				curScanRotationInt = value;
				if (curScanRotationInt > 360f)
				{
					curScanRotationInt -= 360f;
				}
				if (curScanRotationInt < 0f)
				{
					curScanRotationInt += 360f;
				}
			}
		}

		public void SpawnedCamo()
		{
		}

		public void SetMode(string mode)
		{
			camoMode = mode;
			baseGraphic = null;
			DirtyMapMesh(base.Map);
		}

		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "upgraded")
			{
				HasATOM = GetComp<CompUpgradable>().HasUpgrade(DubDef.ATOM);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			base.Map.Rimatomics().RegisterRadar(this);
			HasATOM = GetComp<CompUpgradable>().HasUpgrade(DubDef.ATOM);
			DubUtils.GetResearch().NotifyResearch();
			SpawnedCamo();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref camoMode, "camoMode", "base");
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Map.Rimatomics().DeRegisterRadar(this);
			base.DeSpawn(mode);
		}

		public override void Tick()
		{
			base.Tick();
			if (HasATOM)
			{
				powerComp.PowerOutput = (0f - powerComp.Props.basePowerConsumption) * 7f;
			}
			else
			{
				powerComp.PowerOutput = 0f - powerComp.Props.basePowerConsumption;
			}
			if (Working)
			{
				CurRotation += 0.1f;
				CurScanRotation += 1f;
			}
		}

		[SyncMethod(SyncContext.None)]
		public void scramblePods()
		{
			DubUtils.GetResearch().ScrambleMode = !DubUtils.GetResearch().ScrambleMode;
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		[SyncMethod(SyncContext.None)]
		public void threatDetect()
		{
			DubUtils.GetResearch().ThreatDetectionMode = !DubUtils.GetResearch().ThreatDetectionMode;
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.Map.Rimatomics().Radars.Any((Building_Radar x) => x.HasATOM))
			{
				yield return new Command_Toggle
				{
					defaultLabel = "ScrambleDropPods".Translate(),
					defaultDesc = "ScrambleDropPodsDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/Scrambler", reportFailure: false),
					isActive = () => DubUtils.GetResearch().ScrambleMode,
					toggleAction = scramblePods
				};
				yield return new Command_Toggle
				{
					defaultLabel = "ThreatDetectionToggle".Translate(),
					defaultDesc = "ThreatDetectionToggleDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/ThreatDetect", reportFailure: false),
					isActive = () => DubUtils.GetResearch().ThreatDetectionMode,
					toggleAction = threatDetect
				};
				if (DebugSettings.godMode)
				{
					yield return new Command_Action
					{
						defaultLabel = "Debug: Force raid",
						defaultDesc = "Debug: Force raid",
						action = delegate
						{
							DubUtils.GetResearch().ForceRaids();
						}
					};
				}
			}
			yield return new Command_SetCamoMode(this)
			{
				defaultLabel = "SetCamoMode".Translate(),
				defaultDesc = "SetCamoMode".Translate(),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/CamoMode")
			};
		}

		public override void Draw()
		{
			base.Draw();
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 drawPos = DrawPos;
			drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
			matrix.SetTRS(drawPos, CurRotation.ToQuat(), new Vector3(5f, 1f, 5f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, turretMat.MatSingle, 0);
			Vector3 v = new Vector3(0f, 0f, 1f);
			v = v.RotatedBy(CurRotation);
			drawPos += v;
			drawPos.y += 1f;
			matrix.SetTRS(drawPos, CurScanRotation.ToQuat(), new Vector3(5f, 1f, 5f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, scannerMat.MatSingle, 0);
		}
	}
}
