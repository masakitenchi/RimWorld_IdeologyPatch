using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class WeaponsConsole : Building
	{
		public static float BarrageRadius = 29f;

		public CompBreakdownable breakdown;

		public Dialog_Radar console;

		public bool ConsoleOpen;

		public float currentPPC;

		public Vector3 halfVec = new Vector3(0.5f, 0.5f, 0.5f);

		public CompMannable mannableComp;

		public MapComponent_Rimatomics mapComp;

		private Graphic offGraphic;

		public CompPowerTrader powerComp;

		public float radarSweep;

		public float totalPPC;

		public static int texUpdate = 0;

		public bool Manned
		{
			get
			{
				if (!MP.IsInMultiplayer)
				{
					return mannableComp.MannedNow;
				}
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			mapComp = base.Map.Rimatomics();
			mannableComp = GetComp<CompMannable>();
			powerComp = GetComp<CompPowerTrader>();
			breakdown = GetComp<CompBreakdownable>();
			base.Map.Rimatomics().RegisterConsole(this);
			DubUtils.GetResearch().NotifyResearch();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Map.Rimatomics().DeRegisterConsole(this);
			base.DeSpawn(mode);
		}

		public override void Tick()
		{
			base.Tick();
			radarSweep += 1f;
			if (radarSweep >= 360f)
			{
				radarSweep = 0f;
			}
			if (!this.IsHashIntervalTick(30))
			{
				return;
			}
			List<Building_PPC> list = powerComp.PowerNet?.batteryComps?.Select((CompPowerBattery x) => x.parent).OfType<Building_PPC>().ToList();
			currentPPC = 0f;
			totalPPC = 0f;
			if (list.NullOrEmpty())
			{
				return;
			}
			foreach (Building_PPC item in list)
			{
				currentPPC += item.batt.StoredEnergy;
				totalPPC += item.batt.Props.storedEnergyMax;
			}
		}

		public virtual void DrawUI(Vector3 pos, Vector3 size, Quaternion rot, Material mat)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, rot, size);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		public static void DrawGauge(Vector3 pos, Vector3 size, Quaternion rot, Material guage, Material mask, float fillPercent)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, rot, size);
			Graphics.DrawMesh(MeshPool.plane10, matrix, guage, 0);
			if (fillPercent > 0.001f)
			{
				Vector3 s = new Vector3(size.x, 1f, size.z * fillPercent);
				pos.y += 1f;
				pos.z += size.z * 0.5f;
				pos.z -= 0.5f * size.z * fillPercent;
				matrix = default(Matrix4x4);
				matrix.SetTRS(pos, rot, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, mask, 0);
			}
		}

		public override void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this, 0f);
			if (powerComp.PowerOn)
			{
				if (offGraphic == null)
				{
					offGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, def.graphicData.drawSize, DrawColor, DrawColorTwo);
				}
				offGraphic.Print(layer, this, 0f);
			}
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

		public static void UpdateMapTex(Map map)
		{
		}

		public override void Draw()
		{
			base.Draw();
			if ((int)Find.CameraDriver.CurrentZoom > 1 || !powerComp.PowerOn || !Manned)
			{
				return;
			}
			Quaternion asQuat = base.Rotation.AsQuat;
			Vector3 drawPos = DrawPos;
			drawPos.y += 1f;
			float fillPercent = Mathf.Lerp(1f, 0f, currentPPC / totalPPC);
			Vector3 pos = drawPos + new Vector3(0.73f, 0f, 0f);
			Vector3 size = new Vector3(0.04f, 1f, 1.3f);
			if (totalPPC > 0f)
			{
				DrawGauge(pos, size, default(Quaternion), GraphicsCache.GaugeMat, GraphicsCache.controlPanelBG, fillPercent);
			}
			Vector3 vector = drawPos;
			if (!mapComp.RadarActive)
			{
				return;
			}
			float num = 0.8f;
			foreach (Thing item in mapComp.AllProjectiles())
			{
				vector = drawPos;
				vector.y += 2f;
				vector.x += GenMath.LerpDoubleClamped(0f, base.Map.Size.x, 0f - num, num, item.DrawPos.x);
				vector.z += GenMath.LerpDoubleClamped(0f, base.Map.Size.z, 0f - num, num, item.DrawPos.z);
			}
			foreach (Thing item2 in base.Map.attackTargetsCache.TargetsHostileToColony)
			{
				vector = drawPos;
				vector.y += 2f;
				vector.x += GenMath.LerpDoubleClamped(0f, base.Map.Size.x, 0f - num, num, item2.DrawPos.x);
				vector.z += GenMath.LerpDoubleClamped(0f, base.Map.Size.z, 0f - num, num, item2.DrawPos.z);
				DrawUI(vector, halfVec, asQuat, GraphicsCache.LEDred);
			}
		}

		public void TryOpenTerminal(Pawn negotiator)
		{
			console = new Dialog_Radar(this, negotiator);
			Find.WindowStack.Add(console);
		}

		internal void CloseConsole()
		{
			console.Close();
		}

		[SyncMethod(SyncContext.None)]
		public void scramblePods()
		{
			DubUtils.GetResearch().ScrambleMode = !DubUtils.GetResearch().ScrambleMode;
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		[SyncMethod(SyncContext.None)]
		public void BuggerMe()
		{
			DubUtils.GetResearch().BuggerMe = !DubUtils.GetResearch().BuggerMe;
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (Manned && mapComp.RadarActive)
			{
				yield return new Command_Action
				{
					defaultLabel = "Tracking",
					defaultDesc = "OpenRadar".Translate(),
					icon = GraphicsCache.TrackingScreen,
					action = delegate
					{
						TryOpenTerminal(mannableComp.ManningPawn);
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					},
					hotKey = KeyBindingDefOf.Misc5
				};
			}
			if (mapComp.Radars.Any((Building_Radar x) => x.HasATOM))
			{
				yield return new Command_Toggle
				{
					defaultLabel = "ScrambleDropPods".Translate(),
					defaultDesc = "ScrambleDropPodsDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/Scrambler", reportFailure: false),
					isActive = () => DubUtils.GetResearch().ScrambleMode,
					toggleAction = scramblePods
				};
			}
			yield return new Command_Action
			{
				defaultLabel = (DubUtils.GetResearch().BuggerMe ? "TargetAllShells".Translate() : "TargetEnemyShells".Translate()),
				defaultDesc = "TestFireHELSOnOwnShells".Translate(),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/laserTarget", reportFailure: false),
				action = BuggerMe
			};
		}
	}
}
