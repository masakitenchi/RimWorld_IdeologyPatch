using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class ReactorControl : Building, IAssignableGreek
	{
		public Dialog_ReactorConsole console;

		public reactorCore CoreLink;

		public bool fillFlick;

		public float integrityTimer;

		public int IntGreekID;

		public CompPipe Loom;

		public float meltdownTimer;

		private Graphic offGraphic;

		public float overheatTimer;

		public CompPowerTrader powerComp;

		public bool shutdownFlick;

		public bool AutoThrottle;

		private List<Material> slotmats = new List<Material>();

		private Vector3[] fuelGrid;

		public int SlotToUpdate;

		private StringBuilder sb = new StringBuilder();

		public LoomNet LoomNet => Loom.net as LoomNet;

		public bool CanUseConsole
		{
			get
			{
				if (base.Spawned)
				{
					return powerComp.PowerOn;
				}
				return false;
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
				CoreLink = null;
			}
		}

		[SyncMethod(SyncContext.None)]
		public static void EndJob(Pawn neg)
		{
			neg.jobs.EndCurrentJob(JobCondition.Succeeded);
		}

		[SyncMethod(SyncContext.None)]
		public void EngageAutoThrottle()
		{
			Messages.Message("AutothrotOn".Translate(greekAlpha.getAlpha(GreekID)), this, MessageTypeDefOf.PositiveEvent, historical: false);
			AutoThrottle = true;
		}

		[SyncMethod(SyncContext.None)]
		public void DisengageAutoThrottle()
		{
			AutoThrottle = false;
			Messages.Message("AutothrotOff".Translate(greekAlpha.getAlpha(GreekID)), this, MessageTypeDefOf.NegativeEvent, historical: false);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Loom = GetComps<CompPipe>().FirstOrDefault((CompPipe p) => p.mode == PipeType.Loom);
			powerComp = GetComp<CompPowerTrader>();
			DubUtils.GetResearch().NotifyResearch();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref AutoThrottle, "AutoThrottle", defaultValue: false);
			Scribe_Values.Look(ref IntGreekID, "IntGreekID", 0);
			Scribe_Values.Look(ref shutdownFlick, "shutdownFlick", defaultValue: false);
			Scribe_Values.Look(ref fillFlick, "fillFlick", defaultValue: false);
		}

		public void TryOpenTerminal()
		{
			TryOpenTerminal(null);
		}

		public void TryOpenTerminal(Pawn negotiator)
		{
			if (console == null || !console.IsOpen)
			{
				console = new Dialog_ReactorConsole(this, negotiator);
				Find.WindowStack.Add(console);
			}
		}

		public void TryCloseTerminal()
		{
			if (console != null && console.IsOpen)
			{
				console.Close();
			}
			console = null;
		}

		public virtual void drawUI(Vector3 pos, Vector3 size, Quaternion rot, Material mat)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, rot, size);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		public static void DrawGuage(Vector3 pos, Vector3 size, Quaternion rot, Material guage, Material mask, float fillPercent)
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

		public void UpdateGraphicsCache()
		{
			SlotToUpdate = 0;
			slotmats.Clear();
			for (int i = 0; i < CoreLink.FuelCells.Count; i++)
			{
				slotmats.Add(GraphicsCache.LEDblack);
			}
			fuelGrid = CoreLink.FuelCells.Select((IntVec3 x) => x.ToVector3() * 0.1f).ToArray();
		}

		public void updateSlotMatAt(int i)
		{
			switch (CoreLink.SlotStatus[i])
			{
			case RodStatus.Cracked:
				slotmats[i] = GraphicsCache.LEDred;
				break;
			case RodStatus.Empty:
				slotmats[i] = GraphicsCache.LEDblack;
				break;
			case RodStatus.New:
				slotmats[i] = GraphicsCache.LEDblue;
				break;
			case RodStatus.Activated:
				slotmats[i] = GraphicsCache.LEDgreen;
				break;
			case RodStatus.Spent:
				slotmats[i] = GraphicsCache.LEDwhite;
				break;
			}
		}

		public void drawSlot(Vector3 pos, int i)
		{
			DubUtils.drawLED(pos, default(Quaternion), slotmats[i]);
		}

		public override void Draw()
		{
			base.Draw();
			if (!powerComp.PowerOn)
			{
				return;
			}
			Vector3 vector = DrawPos + new Vector3(0f, 1f, 0f);
			if (CoreLink == null)
			{
				Graphics.DrawMesh(GraphicsCache.Disconnected.MeshAt(base.Rotation), vector, default(Quaternion), GraphicsCache.Disconnected.MatAt(base.Rotation), 0);
				return;
			}
			updateSlotMatAt(SlotToUpdate);
			SlotToUpdate++;
			if (SlotToUpdate >= CoreLink.SlotCount)
			{
				SlotToUpdate = 0;
			}
			Quaternion quaternion = default(Quaternion);
			if (CoreLink.IsCoreMelt)
			{
				if (Time.realtimeSinceStartup > meltdownTimer - 1f)
				{
					Graphics.DrawMesh(GraphicsCache.coremeltscreen1.MeshAt(base.Rotation), vector, quaternion, GraphicsCache.coremeltscreen1.MatAt(base.Rotation), 0);
				}
				else
				{
					Graphics.DrawMesh(GraphicsCache.coremeltscreen2.MeshAt(base.Rotation), vector, quaternion, GraphicsCache.coremeltscreen2.MatAt(base.Rotation), 0);
				}
				return;
			}
			float fillPercent = Mathf.InverseLerp(600f, 0f, CoreLink.postReturnTemp);
			float fillPercent2 = Mathf.Lerp(1f, 0f, CoreLink.RealControlRodPosition);
			Vector3 pos = vector + new Vector3(-0.75f, 0f, 0f);
			Vector3 pos2 = vector + new Vector3(0.75f, 0f, 0f);
			Vector3 size = new Vector3(0.1f, 1f, 1.6f);
			DrawGuage(pos, size, quaternion, GraphicsCache.GaugeMat, GraphicsCache.controlPanelBG, fillPercent);
			DrawGuage(pos2, size, quaternion, GraphicsCache.GaugeMat, GraphicsCache.controlPanelBG, fillPercent2);
			for (int i = 0; i < fuelGrid.Length; i++)
			{
				drawSlot(vector + fuelGrid[i], i);
			}
			if ((!CoreLink.IsCold || !CoreLink.IsShutdown) && (!CoreLink.IsCold || CoreLink.IsShutdown))
			{
				if (!CoreLink.IsCold && CoreLink.IsShutdown)
				{
					if (CoreLink.shutdownFlicker == 0)
					{
						DubUtils.drawLED(vector + new Vector3(0.4f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
					}
					if (CoreLink.shutdownFlicker == 1)
					{
						DubUtils.drawLED(vector + new Vector3(0.5f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
					}
					if (CoreLink.shutdownFlicker == 2)
					{
						DubUtils.drawLED(vector + new Vector3(0.6f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
					}
				}
				else if (CoreLink.Leakage <= 0f)
				{
					DubUtils.drawLED(vector + new Vector3(0.4f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
					DubUtils.drawLED(vector + new Vector3(0.5f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
					DubUtils.drawLED(vector + new Vector3(0.6f, 0f, -0.6f), quaternion, GraphicsCache.miniRedLed);
				}
			}
			if (CoreLink.Leakage > 0.25f)
			{
				DubUtils.drawLED(vector + new Vector3(0f, 0f, 0.7f), quaternion, GraphicsCache.radiation);
			}
			if (CoreLink.postReturnTemp > 500f)
			{
				DubUtils.drawLED(vector + new Vector3(-0.5f, 0f, 0.7f), quaternion, GraphicsCache.overheat);
			}
			if (CoreLink.CrackedFuel)
			{
				DubUtils.drawLED(vector + new Vector3(0.5f, 0f, 0.7f), new Vector3(0.5f, 0.5f, 0.5f), quaternion, GraphicsCache.damage);
			}
		}

		public override void DrawGUIOverlay()
		{
			base.DrawGUIOverlay();
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				GenMapUI.DrawThingLabel(LabelDrawPosFor(), greekAlpha.getAlpha(GreekID), Color.yellow);
			}
		}

		public Vector2 LabelDrawPosFor()
		{
			Vector3 drawPos = DrawPos;
			drawPos.z += -0.68f;
			Vector2 result = Find.Camera.WorldToScreenPoint(drawPos) / Prefs.UIScale;
			result.y = (float)UI.screenHeight - result.y;
			return result;
		}

		public override void Tick()
		{
			base.Tick();
			if (CoreLink == null)
			{
				CoreLink = LoomNet.Cores.FirstOrDefault((reactorCore z) => z.GreekID == GreekID);
				if (CoreLink != null)
				{
					UpdateGraphicsCache();
				}
			}
			if (CoreLink != null && !CoreLink.Spawned)
			{
				AutoThrottle = false;
				CoreLink = null;
			}
			if (CoreLink != null && CoreLink.LoomNet != LoomNet)
			{
				AutoThrottle = false;
				CoreLink = null;
			}
			if (CoreLink == null || !powerComp.PowerOn)
			{
				return;
			}
			bool num = CoreLink.postReturnTemp > 2000f;
			bool num2 = CoreLink.postReturnTemp > 500f;
			bool flag = CoreLink.coreIntegrity < 0.25f;
			if (num2 && Time.realtimeSinceStartup > overheatTimer)
			{
				SoundDef.Named("OverheatSiren").PlayOneShot(new TargetInfo(base.Position, base.Map));
				overheatTimer = Time.realtimeSinceStartup + 2.35f;
			}
			if (num && Time.realtimeSinceStartup > meltdownTimer)
			{
				SoundDef.Named("MeltdownSiren").PlayOneShot(new TargetInfo(base.Position, base.Map));
				meltdownTimer = Time.realtimeSinceStartup + 1.9f;
			}
			if (flag && Time.realtimeSinceStartup > integrityTimer)
			{
				SoundDef.Named("IntegritySiren").PlayOneShot(new TargetInfo(base.Position, base.Map));
				integrityTimer = Time.realtimeSinceStartup + 5f;
			}
			if (AutoThrottle && (CoreLink.IsShutdown || !CoreLink.powerComp.PowerOn || !powerComp.PowerOn))
			{
				AutoThrottle = false;
				CoreLink.TargetControlRodTo = CoreLink.TargetControlRodPosition;
			}
			if (!AutoThrottle || !this.IsHashIntervalTick(10) || !(Math.Abs(CoreLink.TargetControlRodTo - CoreLink.TargetControlRodPosition) <= 0.001f))
			{
				return;
			}
			if (CoreLink.overheating > 0f)
			{
				CoreLink.TargetControlRodTo -= 0.01f;
			}
			else
			{
				float num3 = powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
				if (num3 > 5000f)
				{
					if (num3 > 10000f)
					{
						CoreLink.TargetControlRodTo -= 0.01f;
					}
					else
					{
						CoreLink.TargetControlRodTo -= 0.001f;
					}
				}
				else if (num3 < 0f)
				{
					if (num3 < -5000f)
					{
						CoreLink.TargetControlRodTo += 0.01f;
					}
					else
					{
						CoreLink.TargetControlRodTo += 0.001f;
					}
				}
				else
				{
					CoreLink.TargetControlRodTo = CoreLink.TargetControlRodPosition;
				}
			}
			CoreLink.TargetControlRodTo = Mathf.Clamp01(CoreLink.TargetControlRodTo);
		}

		public override string GetInspectString()
		{
			sb.Clear();
			sb.Append(base.GetInspectString());
			if (CoreLink != null && powerComp.PowerOn)
			{
				sb.AppendLine();
				sb.Append((CoreLink.ThermalEnergy / 1000f).ToString("0.00") + "kW");
				sb.Append(" | ");
				sb.Append("critTemp".Translate());
				sb.Append(":");
				sb.Append(CoreLink.postReturnTemp.ToStringTemperature("F0"));
				sb.Append(" | ");
				sb.Append("critCooling".Translate());
				sb.Append(":");
				sb.Append(CoreLink.coolingCapPct.ToStringPercent("0.00"));
				sb.AppendLine();
				sb.Append("critTurbine".Translate());
				sb.Append(":");
				sb.Append(CoreLink.totalPowerPct.ToStringPercent("0.00"));
				sb.Append(" | ");
				sb.Append("ControlRod".Translate());
				sb.Append(":");
				sb.Append(CoreLink.RealControlRodPosition.ToStringPercent("0.00"));
			}
			return sb.ToString();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_SetModeulID(this)
			{
				defaultLabel = greekAlpha.getAlpha(GreekID),
				defaultDesc = "critsetid".Translate(),
				hotKey = KeyBindingDefOf.Misc4,
				icon = GraphicsCache.setID
			};
			if (!RimatomicsMod.Settings.MannedReactor)
			{
				yield return new Command_Action
				{
					defaultLabel = "critManageReactor".Translate(),
					defaultDesc = "critManageReactor".Translate(),
					hotKey = KeyBindingDefOf.Misc5,
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/OpenUI"),
					action = TryOpenTerminal
				};
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "force ship",
					defaultDesc = "",
					action = forceship
				};
				yield return new Command_Action
				{
					defaultLabel = "end cooldown",
					defaultDesc = "",
					action = endcooldown
				};
			}
			yield return new Command_Action
			{
				defaultLabel = "TradeShipPrice".Translate(RimatomicsResearch.SilverForShip),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/logo"),
				defaultDesc = "TradeShipPrice".Translate(RimatomicsResearch.SilverForShip),
				action = TradeShipReq
			};
		}

		[SyncMethod(SyncContext.None)]
		public void TradeShipReq()
		{
			if (DubUtils.GetResearch().TraderCooldown > 0)
			{
				Messages.Message("TradeShipWait".Translate(DubUtils.GetResearch().TraderCooldown.ToStringTicksToPeriod()), MessageTypeDefOf.RejectInput);
				return;
			}
			if (!TradeUtility.ColonyHasEnoughSilver(base.MapHeld, RimatomicsResearch.SilverForShip))
			{
				Messages.Message("TradeShipPriceNeed".Translate(RimatomicsResearch.SilverForShip), MessageTypeDefOf.RejectInput);
				return;
			}
			TradeUtility.LaunchSilver(base.MapHeld, RimatomicsResearch.SilverForShip);
			DubUtils.GetResearch().TicksToShipArrive = 120000;
			DubUtils.GetResearch().TraderCooldown = (int)(60000f * RimatomicsMod.Settings.RimatomicsTraderCooldown);
			DiaNode diaNode = new DiaNode("TradeShipReq".Translate());
			diaNode.options.Add(DiaOption.DefaultOK);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
		}

		[SyncMethod(SyncContext.None)]
		public void forceship()
		{
			DubUtils.GetResearch().TicksToShipArrive = 1;
		}

		[SyncMethod(SyncContext.None)]
		public void endcooldown()
		{
			DubUtils.GetResearch().TraderCooldown = 1;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
			{
				yield return floatMenuOption2;
			}
			_ = RimatomicsMod.Settings.MannedReactor;
			FloatMenuOption floatMenuOption = new FloatMenuOption("critManageReactor".Translate(), delegate
			{
				Job job = new Job(DubDef.UseReactorConsole, this);
				myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
			if (!myPawn.CanReserve(this))
			{
				floatMenuOption = new FloatMenuOption("CannotUseReserved".Translate(), null);
			}
			if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
			{
				floatMenuOption = new FloatMenuOption("CannotUseNoPath".Translate(), null);
			}
			if (!powerComp.PowerOn)
			{
				floatMenuOption = new FloatMenuOption("CannotUseNoPower".Translate(), null);
			}
			if (myPawn.skills != null && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level < 8)
			{
				floatMenuOption = new FloatMenuOption("CantUseReactorConsole".Translate(), null);
			}
			yield return floatMenuOption;
		}
	}
}
