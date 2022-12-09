using System;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Dialog_ReactorConsole : Window
	{
		private const float InteractivityDelay = 0.5f;

		public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall");

		public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");

		public static float MoxClip = 25f;

		public static float DrawListW = 160f;

		public static float DrawBarsW = 160f;

		public static float DrawRodsW = 270f;

		public static float rowH = 32f;

		public static float rowsTopOffset = 60f;

		public static float gappy = 60f;

		public Color barCold = new Color32(100, 100, 100, byte.MaxValue);

		public Color barHot = new Color32(byte.MaxValue, 55, 0, byte.MaxValue);

		public RodDesignate DesMode = RodDesignate.Fuel;

		private bool minimized;

		private int mouseOverInt = -1;

		public Pawn neg;

		public ReactorControl rc;

		public reactorCore reactor;

		public string rodString = string.Empty;

		public Color screenFillColor = Color.clear;

		private Vector2 scrollPos = Vector2.zero;

		private StringBuilder sb = new StringBuilder();

		public override Vector2 InitialSize => new Vector2(1080f, 470f);

		public override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public Dialog_ReactorConsole(ReactorControl console, Pawn pawn)
		{
			rc = console;
			reactor = console.CoreLink;
			neg = pawn;
			forcePause = false;
			absorbInputAroundWindow = false;
			closeOnCancel = true;
			soundAppear = SoundDefOf.CommsWindow_Open;
			soundClose = SoundDefOf.CommsWindow_Close;
			soundAmbient = SoundDef.Named("Console_Ambience");
			doCloseButton = false;
			doCloseX = false;
			draggable = false;
			drawShadow = true;
			preventCameraMotion = false;
			onlyOneOfTypeAllowed = false;
		}

		public override void Close(bool doCloseSound = true)
		{
			base.Close(doCloseSound);
			if (neg != null)
			{
				ReactorControl.EndJob(neg);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			if (rc == null)
			{
				Close();
			}
			if (Current.Game.CurrentMap == null)
			{
				return;
			}
			Rect position = new Rect(inRect.x, inRect.y, inRect.width - 40f, 20f).ContractedBy(2f);
			GUI.DragWindow(position);
			Widgets.DrawLine(new Vector2(position.x, position.y + position.height * 0.25f), new Vector2(position.xMax, position.y + position.height * 0.25f), Color.gray, 1f);
			Widgets.DrawLine(new Vector2(position.x, position.y + position.height * 0.75f), new Vector2(position.xMax, position.y + position.height * 0.75f), Color.gray, 1f);
			if (Widgets.ButtonImage(new Rect(inRect.xMax - 20f, inRect.y, 20f, 20f).ContractedBy(2f), CloseXSmall))
			{
				Close();
			}
			if (Widgets.ButtonImage(new Rect(inRect.xMax - 40f, inRect.y, 20f, 20f).ContractedBy(2f), Minus))
			{
				minimized = !minimized;
				windowRect.height = (minimized ? 30f : InitialSize.y);
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
			}
			Rect rect = inRect;
			rect.x += 10f;
			rect.height -= 30f;
			rect.y += 20f;
			if (!minimized)
			{
				rect.width = DrawListW;
				DrawList(rect);
				if (reactor != null && reactor.IsBuilt)
				{
					rect.x = rect.xMax + 10f;
					rect.width = rect.height;
					DrawGrid(rect);
					rect.x = rect.xMax + 10f;
					rect.width = DrawBarsW;
					DrawBars(rect);
					rect.x = rect.xMax + 10f;
					rect.width = DrawRodsW;
					DrawRods(rect);
				}
			}
		}

		public void DrawList(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height = rect.height / 2f;
			Widgets.DrawMenuSection(rect2);
			DrawSlotDesignators(rect2.TopPartPixels(20f));
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			if (reactor != null)
			{
				sb.Clear();
				sb.Append("ReactorBreakdown".Translate(greekAlpha.getAlpha(reactor.GreekID), reactor.StatusText, reactor.MaxPowerPossible.ToString("0"), reactor.ConversionRatio.ToString("0.0"), rodString));
				Rect rect3 = rect2;
				rect3.y += 20f;
				rect3.height -= 20f;
				Widgets.Label(rect3.ContractedBy(6f), sb.ToString());
			}
			rect2.y = rect2.yMax + 10f;
			rect2.height -= 10f;
			Widgets.DrawMenuSection(rect2);
			float num = 30f;
			float height = num * (float)rc.LoomNet.Cores.Count;
			Rect viewRect = new Rect(0f, 0f, rect2.width - 16f, height);
			GUI.BeginGroup(rect2);
			Widgets.BeginScrollView(new Rect(0f, 0f, rect2.width, rect2.height), ref scrollPos, viewRect);
			Text.Anchor = TextAnchor.MiddleLeft;
			int num2 = 0;
			foreach (reactorCore core in rc.LoomNet.Cores)
			{
				Rect rect4 = new Rect(0f, num2, rect2.width, num);
				DrawCoreRow(rect4, core);
				num2 += (int)num;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		public void DrawCoreRow(Rect rect, reactorCore core)
		{
			rect = rect.ContractedBy(1f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = Color.grey;
				Widgets.DrawHighlight(rect);
			}
			else if (reactor == core)
			{
				GUI.color = Color.grey;
				Widgets.DrawHighlight(rect);
			}
			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(2f, 0f);
			widgetRow.Label(greekAlpha.getAlpha(core.GreekID), 50f);
			widgetRow.Gap(1f);
			if (core.IsBuilt)
			{
				widgetRow.Icon(GraphicsCache.fuelRodTex);
				widgetRow.Label(core.FuelGrid.Count.ToString() ?? "", 20f);
			}
			else
			{
				widgetRow.Label("CoreNotBuilt".Translate(), 60f);
			}
			if (core.IsOverheating)
			{
				widgetRow.Gap(1f);
				widgetRow.Icon(GraphicsCache.OverheatIcon);
			}
			if (core.Leakage > 0f)
			{
				widgetRow.Gap(1f);
				widgetRow.Icon(GraphicsCache.RadIcon);
			}
			if (Widgets.ButtonText(new Rect(0f, 0f, widgetRow.FinalX, 30f), "", drawBackground: false, doMouseoverSound: true, Color.blue))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				reactor = core;
			}
			GUI.EndGroup();
		}

		public void DrawSlotDesignators(Rect jam)
		{
			jam.width /= 5f;
			Widgets.DrawBoxSolid(jam, Color.grey);
			if (Widgets.ButtonImageFitted(jam, GraphicsCache.Uninstall))
			{
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				for (int i = 0; i < reactor.FuelCells.Count; i++)
				{
					if (reactor.SlotStatus[i] != 0)
					{
						reactor.DesignateSlot(i, RodDesignate.Remove);
					}
				}
			}
			TooltipHandler.TipRegion(jam, "Designateremoveall".Translate());
			jam.x = jam.xMax;
			Widgets.DrawBoxSolid(jam, Color.grey);
			if (Widgets.ButtonImageFitted(jam, GraphicsCache.Uninstall))
			{
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				for (int j = 0; j < reactor.FuelCells.Count; j++)
				{
					if (reactor.SlotStatus[j] == RodStatus.Spent)
					{
						reactor.DesignateSlot(j, RodDesignate.Remove);
					}
				}
			}
			TooltipHandler.TipRegion(jam, "Designateremoveallspent".Translate());
			jam.x = jam.xMax;
			Widgets.DrawBoxSolid(jam, Color.grey);
			if (Widgets.ButtonImageFitted(jam, GraphicsCache.Uninstall))
			{
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				for (int k = 0; k < reactor.FuelCells.Count; k++)
				{
					if (reactor.SlotStatus[k] == RodStatus.Activated)
					{
						reactor.DesignateSlot(k, RodDesignate.Remove);
					}
				}
			}
			TooltipHandler.TipRegion(jam, "Designateremoveallprocessable".Translate());
			jam.x = jam.xMax;
			Widgets.DrawBoxSolid(jam, Color.grey);
			if (Widgets.ButtonImageFitted(jam, GraphicsCache.Install))
			{
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				for (int l = 0; l < reactor.FuelCells.Count; l++)
				{
					if (reactor.SlotStatus[l] == RodStatus.Empty)
					{
						reactor.DesignateSlot(l, DesMode);
					}
				}
			}
			TooltipHandler.TipRegion(jam, "Designateinstallall".Translate());
			jam.x = jam.xMax;
			Widgets.DrawBoxSolid(jam, Color.grey);
			if (Widgets.ButtonImageFitted(jam, GraphicsCache.ClearDes))
			{
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				for (int m = 0; m < reactor.FuelCells.Count; m++)
				{
					reactor.DesignateSlot(m, RodDesignate.None);
				}
			}
			TooltipHandler.TipRegion(jam, "Clearalldesignations".Translate());
		}

		public void DrawGrid(Rect rect)
		{
			rodString = "";
			Rect rect2 = rect;
			Widgets.DrawMenuSection(rect2);
			GUI.BeginGroup(rect2);
			float num = (rect2.width - 80f) / (reactor.radius * 2f);
			float num2 = rect2.width / 2f - num / 2f;
			int count = reactor.FuelCells.Count;
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = reactor.FuelCells[i].ToVector3() * num;
				vector.z *= -1f;
				vector.x += num2;
				vector.z += num2;
				Rect rekt = new Rect(vector.x, vector.z, num, num);
				drawSlot(rekt, i);
			}
			Rect rect3 = new Rect(10f, 10f, 66f, 66f);
			Widgets.DrawMenuSection(rect3);
			if (Widgets.ButtonImageFitted(rect3.ContractedBy(1f), (DesMode == RodDesignate.Fuel) ? GraphicsCache.UIfuelKey : GraphicsCache.UIfuelKeyMOX))
			{
				DesMode = ((DesMode != RodDesignate.Fuel) ? RodDesignate.Fuel : RodDesignate.MOX);
			}
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect3.ContractedBy(4f), (DesMode == RodDesignate.Fuel) ? "Normal" : "MOX");
			rect3.x = rect2.width - 76f;
			Widgets.DrawMenuSection(rect3);
			Widgets.DrawTextureFitted(rect3.ContractedBy(1f), GraphicsCache.UImouseKey, 1f);
			Text.Font = GameFont.Small;
			rect3.y = rect2.width - 76f;
			Widgets.DrawMenuSection(rect3);
			if (Widgets.ButtonImage(rect3.ContractedBy(1f), GraphicsCache.scram))
			{
				if (!reactor.IsShutdown)
				{
					SoundDef.Named("ReactorOff").PlayOneShotOnCamera();
					rc.DisengageAutoThrottle();
					reactor.SCRAM();
				}
				else
				{
					SoundDef.Named("input_failed_clean").PlayOneShotOnCamera();
				}
			}
			Widgets.Label(rect3, "reactorSCRAM".Translate());
			rect3.x = 10f;
			Widgets.DrawMenuSection(rect3);
			Text.Anchor = TextAnchor.UpperCenter;
			if (Widgets.ButtonImage(rect3.ContractedBy(1f), GraphicsCache.startReactor))
			{
				if (reactor.IsShutdown)
				{
					SoundDef.Named("ReactorOn").PlayOneShotOnCamera();
					reactor.Startup();
				}
				else
				{
					SoundDef.Named("input_failed_clean").PlayOneShotOnCamera();
				}
			}
			Widgets.Label(rect3, "reactorSTART".Translate());
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		public void DrawBars(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			GUI.BeginGroup(rect);
			float num = rect.width / 4f;
			Rect outerRect = new Rect(0f, 0f, num, num).ContractedBy(0f);
			Rect outerRect2 = new Rect(num, 0f, num, num).ContractedBy(6f);
			Rect outerRect3 = new Rect(num * 2f, 0f, num, num).ContractedBy(6f);
			Rect outerRect4 = new Rect(num * 3f, 0f, num, num).ContractedBy(6f);
			if (reactor.powerComp.PowerOn)
			{
				if (reactor.coldAndDead)
				{
					Widgets.DrawTextureFitted(outerRect, GraphicsCache.ReactorCold, 1f);
				}
				else if (!reactor.IsCold && reactor.IsShutdown)
				{
					Widgets.DrawTextureFitted(outerRect, GraphicsCache.ReactorShutdown, 1f);
					switch (reactor.shutdownFlicker)
					{
					case 0:
						Widgets.DrawTextureFitted(outerRect2, GraphicsCache.ReactorHot, 0.5f);
						break;
					case 1:
						Widgets.DrawTextureFitted(outerRect3, GraphicsCache.ReactorHot, 0.5f);
						break;
					case 2:
						Widgets.DrawTextureFitted(outerRect4, GraphicsCache.ReactorHot, 0.5f);
						break;
					}
				}
				else
				{
					Widgets.DrawTextureFitted(outerRect, GraphicsCache.ReactorHot, 1f);
					Widgets.DrawTextureFitted(outerRect2, GraphicsCache.ReactorHot, 0.5f);
					Widgets.DrawTextureFitted(outerRect3, GraphicsCache.ReactorHot, 0.5f);
					Widgets.DrawTextureFitted(outerRect4, GraphicsCache.ReactorHot, 0.5f);
				}
			}
			else
			{
				Widgets.DrawTextureFitted(outerRect, GraphicsCache.ReactorDead, 1f);
			}
			float val2 = Mathf.InverseLerp(0f, 600f, reactor.postReturnTemp);
			float coolingCapPct = reactor.coolingCapPct;
			float val3 = reactor.ThermalEnergy / reactor.MaxPowerPossible;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Tiny;
			Rect row = new Rect(2f, 0f, rect.width, rect.height);
			row.y += rowsTopOffset;
			row.height = rowH;
			RowYourBoat(GraphicsCache.UItemp, GraphicsCache.tempGauge, val2, reactor.postReturnTemp.ToStringTemperature("F0"), "TemperatureTip".Translate());
			RowYourBoat(GraphicsCache.UIfuel, GraphicsCache.Gauge, Mathf.InverseLerp(0f, reactor.FuelCells.Count, reactor.FuelCount), $"{reactor.FuelCount} / {reactor.FuelCells.Count}", "FuelTip".Translate());
			RowYourBoat(GraphicsCache.UIcooling, GraphicsCache.Gauge, coolingCapPct, coolingCapPct.ToStringPercent("0.00"), "CoolingTip".Translate());
			RowYourBoat(GraphicsCache.UIturbine, GraphicsCache.Gauge, reactor.totalPowerPct, reactor.totalPowerPct.ToStringPercent("0.00"), "TurbineTip".Translate());
			RowYourBoat(GraphicsCache.UIpower, GraphicsCache.Gauge, val3, $"{reactor.ThermalEnergy / 1000f:0.00} kW", "PowerTip".Translate());
			RowYourBoat(GraphicsCache.UIrad, GraphicsCache.Gauge, reactor.Leakage, $"{reactor.Leakage:0.00} R", "RadiationTip".Translate());
			GUI.EndGroup();
			Rect rect2 = rect.BottomPartPixels(25f).LeftHalf();
			rect2.y -= 4f;
			rect2.x += 4f;
			Widgets.DrawTextureFitted(rect2.LeftPartPixels(25f), GraphicsCache.Support, 1f);
			if (Widgets.ButtonText(rect2.RightPartPixels(rect2.width - 25f), "Wiki", drawBackground: false))
			{
				Application.OpenURL("https://github.com/Dubwise56/Rimatomics/wiki");
			}
			rect2.x += rect2.width;
			Widgets.DrawTextureFitted(rect2.LeftPartPixels(25f), GraphicsCache.disco, 1f);
			if (Widgets.ButtonText(rect2.RightPartPixels(rect2.width - 25f), "Discord", drawBackground: false))
			{
				Application.OpenURL("https://discord.gg/Az5CnDW");
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			void RowYourBoat(Texture2D ico, Texture2D tx, float val, string st, string tip)
			{
				if (val >= 1f)
				{
					tx = GraphicsCache.red;
				}
				TooltipHandler.TipRegion(row, tip);
				row.width = rowH;
				Widgets.DrawTextureFitted(row, ico, 1f);
				row.width = rect.width - rowH;
				row.x = rowH;
				Widgets.DrawBoxSolid(row.ContractedBy(6f), Color.grey);
				Widgets.DrawBoxSolid(row.ContractedBy(7f), new ColorInt(21, 25, 29).ToColor);
				if (val > 0f)
				{
					Rect position = row.ContractedBy(7f);
					position.width *= Mathf.Clamp(val, 0f, 1f);
					GUI.BeginGroup(position);
					GUI.DrawTexture(new Rect(0f, 0f, row.width, row.height), tx, ScaleMode.StretchToFill);
					GUI.EndGroup();
				}
				row.x += 4f;
				Widgets.Label(row.ContractedBy(8f), st);
				row.x = 2f;
				row.y += gappy;
			}
		}

		public void DrawRods(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			try
			{
				_ = MP.IsInMultiplayer;
			}
			catch (Exception)
			{
				Widgets.Label(rect, "Your current version of Multiplayer by Zetrith is out of date and is incompatible with the new Multiplayer API, you must remove the mod or update to the latest version of Multiplayer by Parexy for the UI to work, it can be found on steam and github");
				return;
			}
			GUI.BeginGroup(rect.ContractedBy(1f));
			Rect rect2 = new Rect(0f, 0f, 192f, 448f);
			Widgets.DrawTextureFitted(rect2, GraphicsCache.coldRods, 1f);
			float realTempPct = reactor.RealTempPct;
			if (realTempPct > 0f)
			{
				GUI.color = new Color(1f, 1f, 1f, realTempPct);
				GUI.DrawTexture(rect2, GraphicsCache.warmRods, ScaleMode.StretchToFill, alphaBlend: true);
				GUI.color = new Color(1f, 1f, 1f, 1f);
			}
			Text.Anchor = TextAnchor.LowerCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2.ContractedBy(60f), reactor.postReturnTemp.ToStringTemperature("F0"));
			float num = GenMath.LerpDoubleClamped(0f, 1f, 0f, 0.4f, reactor.RealControlRodPosition);
			rect2.y -= rect2.height * num;
			Widgets.DrawTextureFitted(rect2, GraphicsCache.controlRods, 1f);
			Text.Font = GameFont.Tiny;
			rect2 = new Rect(rect2.xMax, 0f, rect.width - rect2.width - 10f, 48f);
			Widgets.DrawTextureFitted(rect2, GraphicsCache.UIatom, 1f);
			rect2.y = rect2.yMax;
			rect2.height = 20f;
			Widgets.DrawMenuSection(rect2);
			Widgets.Label(rect2, reactor.TargetControlRodTo.ToStringPercent("0.000"));
			Rect rect3 = new Rect(rect2.x + 20f, rect2.yMax + 20f, 10f, rect.height - rect2.yMax - 40f);
			if (MP.IsInMultiplayer)
			{
				MP.WatchBegin();
				MP.Watch(typeof(reactorCore), "TargetControlRodTo", reactor);
			}
			TooltipHandler.TipRegion(rect3, "Dragtoincreasefluxlevel".Translate());
			float targetControlRodTo = reactor.TargetControlRodTo;
			reactor.TargetControlRodTo = DubUtils.VerticalSlider(rect3, reactor.TargetControlRodTo, 1f, 0f);
			if (rc.AutoThrottle && targetControlRodTo != reactor.TargetControlRodTo)
			{
				rc.DisengageAutoThrottle();
			}
			Text.Font = GameFont.Small;
			Rect rect4 = rect3;
			rect4.x -= 20f;
			rect4.height *= 0.6f;
			rect4 = rect4.CenteredOnYIn(rect3);
			Rect rect5 = new Rect(0f, rect4.y - 20f, 20f, 20f).CenteredOnXIn(rect4);
			rect5.y -= 30f;
			Widgets.DrawMenuSection(rect5);
			Widgets.DrawHighlightIfMouseover(rect5);
			if (Widgets.ButtonInvisible(rect5))
			{
				if (rc.AutoThrottle)
				{
					rc.DisengageAutoThrottle();
				}
				else if (!reactor.IsShutdown && reactor.overheating < 10f)
				{
					if (reactor.GreekID == rc.GreekID)
					{
						float num2 = rc.powerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
						if (num2 < 5000f && num2 > -5000f)
						{
							if (rc.LoomNet.Cores.Count((reactorCore x) => x.GreekID == rc.GreekID) > 1)
							{
								Messages.Message("AutothrotIDCount".Translate(greekAlpha.getAlpha(rc.GreekID)), MessageTypeDefOf.NegativeEvent, historical: false);
							}
							else
							{
								rc.EngageAutoThrottle();
							}
						}
						else
						{
							Messages.Message("AutothrotRange".Translate(), MessageTypeDefOf.NegativeEvent, historical: false);
						}
					}
					else
					{
						Messages.Message("CoreIdMismatch".Translate(), MessageTypeDefOf.NegativeEvent, historical: false);
					}
				}
			}
			float num3 = Pulser.PulseBrightness(0.7f, Pulser.PulseBrightness(0.7f, 1f));
			Color color = new Color(num3, num3, num3) * Color.yellow;
			Widgets.DrawBoxSolid(rect5.ContractedBy(4f), rc.AutoThrottle ? color : Color.grey);
			TooltipHandler.TipRegion(rect5, "AutothrotTip".Translate());
			rect5.y -= 20f;
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect5, "A/T");
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect4, "+");
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect4, "-");
			rect4.height -= 40f;
			rect4 = rect4.CenteredOnYIn(rect3);
			targetControlRodTo = reactor.nudgeVal;
			reactor.nudgeVal = DubUtils.VerticalSlider(rect4, reactor.nudgeVal, 0.0001f, -0.0001f);
			if (rc.AutoThrottle && targetControlRodTo != reactor.nudgeVal && rc.AutoThrottle)
			{
				rc.DisengageAutoThrottle();
			}
			Text.Font = GameFont.Tiny;
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
			{
				reactor.nudgeVal = 0f;
			}
			else
			{
				reactor.TargetControlRodTo = Mathf.Clamp01(reactor.TargetControlRodTo + reactor.nudgeVal);
			}
			if (MP.IsInMultiplayer)
			{
				MP.WatchEnd();
			}
			float num4 = rect3.height / 10f;
			for (int i = 0; i < 10; i++)
			{
				if (i != 0 && i != 5)
				{
					Widgets.DrawLineHorizontal(rect3.xMax + 5f, rect3.y + num4 * (float)i, 6f);
				}
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect6 = new Rect(rect3.xMax + 5f, rect3.y - 10f, 60f, 20f);
			Widgets.Label(rect6, "100%");
			rect6.y = rect3.y + rect3.height / 2f - 10f;
			Widgets.Label(rect6, "50%");
			rect6.y = rect3.yMax - 10f;
			Widgets.Label(rect6, "0%");
			sb.Clear();
			sb.AppendLine("critTargetRod".Translate());
			sb.AppendLine(reactor.TargetControlRodTo.ToStringPercent("0.0"));
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		public void drawSlot(Rect rekt, int i)
		{
			Widgets.DrawTextureFitted(rekt.ContractedBy(2f), GraphicsCache.UIbarBG, 1f);
			RodStatus rodStatus = reactor.SlotStatus[i];
			if (!Mouse.IsOver(rekt) && mouseOverInt == i)
			{
				mouseOverInt = -1;
			}
			if (Mouse.IsOver(rekt) && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
			{
				mouseOverInt = -1;
			}
			if (Input.GetMouseButton(0) && Mouse.IsOver(rekt) && mouseOverInt != i)
			{
				mouseOverInt = i;
				if (rodStatus != 0)
				{
					SoundDef.Named("keyok3").PlayOneShotOnCamera();
					reactor.DesignateSlot(i, RodDesignate.Remove);
				}
				if (rodStatus == RodStatus.Empty)
				{
					SoundDef.Named("keyok3").PlayOneShotOnCamera();
					reactor.DesignateSlot(i, DesMode);
				}
			}
			if (Input.GetMouseButton(1) && Mouse.IsOver(rekt) && mouseOverInt != i)
			{
				mouseOverInt = i;
				SoundDef.Named("keyok3").PlayOneShotOnCamera();
				reactor.DesignateSlot(i, RodDesignate.None);
			}
			Item_NuclearFuel item_NuclearFuel = reactor.FuelAt(i);
			if (item_NuclearFuel != null)
			{
				Text.Font = GameFont.Tiny;
				float h = GenMath.LerpDoubleClamped(0f, 5f, 0.18f, 0f, item_NuclearFuel.ChainReaction);
				float s = Mathf.Lerp(0f, 1f, reactor.RealTempPct);
				float v = Mathf.Lerp(0.2f, 0.9f, reactor.RealTempPct);
				Color color = Color.HSVToRGB(h, s, v);
				if (Mouse.IsOver(rekt))
				{
					rodString = "SlotBreakdown".Translate((item_NuclearFuel.BurnupRate > 0f) ? ((int)((float)item_NuclearFuel.LifeSpan * item_NuclearFuel.FuelLevel / item_NuclearFuel.BurnupRate)).ToStringTicksToPeriod() : "", item_NuclearFuel.MaxPowerLevel.ToString("0.00"), item_NuclearFuel.BasePowerLevel.ToString("0.00"), item_NuclearFuel.FuelLevel.ToStringPercent("0.00"), item_NuclearFuel.PuCreated);
					if (item_NuclearFuel.cracked)
					{
						rodString += "crackedRemoveNow".Translate();
					}
				}
				if (Event.current.type != EventType.Repaint)
				{
					Text.Anchor = TextAnchor.UpperLeft;
					return;
				}
				float fuelLevel = item_NuclearFuel.FuelLevel;
				Rect rect = rekt.ContractedBy(6f);
				Rect rect2 = rect.ContractedBy(3f);
				rect2.width = 8f;
				Rect rect3 = rect2.ContractedBy(1f);
				switch (rodStatus)
				{
				case RodStatus.Cracked:
				{
					Widgets.DrawTextureFitted(rekt, GraphicsCache.crackedSlot, 1f);
					float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
					GUI.color = new Color(num, num, num) * Color.red;
					Widgets.DrawBox(rekt.ContractedBy(6f));
					GUI.color = Color.white;
					break;
				}
				case RodStatus.Spent:
				{
					Widgets.DrawBoxSolid(rect, barCold);
					Widgets.DrawBoxSolid(rect2, Color.black);
					Widgets.DrawBoxSolid(rect3, barCold);
					rect3.y += rect3.height - rect3.height * fuelLevel;
					rect3.height *= fuelLevel;
					Widgets.DrawBoxSolid(rect3, Color.black);
					float num2 = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
					GUI.color = new Color(num2, num2, num2) * Color.red;
					Widgets.DrawBox(rekt.ContractedBy(6f));
					GUI.color = Color.white;
					Widgets.DrawTextureFitted(rekt.ContractedBy(6f).RightPartPixels(10f).TopPartPixels(10f), GraphicsCache.activatedTex, 1f);
					break;
				}
				default:
					Widgets.DrawBoxSolid(rect, color);
					Widgets.DrawBoxSolid(rect2, Color.black);
					Widgets.DrawBoxSolid(rect3, color);
					rect3.y += rect3.height - rect3.height * fuelLevel;
					rect3.height *= fuelLevel;
					Widgets.DrawBoxSolid(rect3, Color.black);
					if (rodStatus == RodStatus.Activated)
					{
						Widgets.DrawTextureFitted(rekt.ContractedBy(6f).RightPartPixels(10f).TopPartPixels(10f), GraphicsCache.activatedTex, 1f);
					}
					break;
				}
				if (item_NuclearFuel.MOX)
				{
					Widgets.DrawTextureFitted(rekt.ContractedBy(6f).RightPartPixels(10f).BottomPartPixels(10f), GraphicsCache.moxMtext, 1f);
				}
				Rect rect4 = rekt.ContractedBy(2f);
				float num3 = rect4.width / 4f;
				if (item_NuclearFuel.AdjFuelRefs[0] != null)
				{
					float num4 = item_NuclearFuel.AdjFuelRefs[0].FuelLevel * reactor.FinalFlux;
					if (DebugSettings.godMode)
					{
						Text.Anchor = TextAnchor.UpperCenter;
						Widgets.Label(rect, num4.ToString("0.0"));
					}
					if (num4 > 0f)
					{
						color = Color.HSVToRGB(Mathf.Lerp(0.18f, 0f, num4), s, v);
						Widgets.DrawLine(new Vector2(rect4.x + num3, rect4.y), new Vector2(rect4.xMax - num3, rect4.y), color, 4f);
					}
				}
				if (item_NuclearFuel.AdjFuelRefs[1] != null)
				{
					float num5 = item_NuclearFuel.AdjFuelRefs[1].FuelLevel * reactor.FinalFlux;
					if (DebugSettings.godMode)
					{
						Text.Anchor = TextAnchor.MiddleRight;
						Widgets.Label(rect, num5.ToString("0.0"));
					}
					if (num5 > 0f)
					{
						color = Color.HSVToRGB(Mathf.Lerp(0.18f, 0f, num5), s, v);
						Widgets.DrawLine(new Vector2(rect4.xMax, rect4.y + num3), new Vector2(rect4.xMax, rect4.yMax - num3), color, 4f);
					}
				}
				if (item_NuclearFuel.AdjFuelRefs[2] != null)
				{
					float num6 = item_NuclearFuel.AdjFuelRefs[2].FuelLevel * reactor.FinalFlux;
					if (DebugSettings.godMode)
					{
						Text.Anchor = TextAnchor.LowerCenter;
						Widgets.Label(rect, num6.ToString("0.0"));
					}
					if (num6 > 0f)
					{
						color = Color.HSVToRGB(Mathf.Lerp(0.18f, 0f, num6), s, v);
						Widgets.DrawLine(new Vector2(rect4.x + num3, rect4.yMax), new Vector2(rect4.xMax - num3, rect4.yMax), color, 4f);
					}
				}
				if (item_NuclearFuel.AdjFuelRefs[3] != null)
				{
					float num7 = item_NuclearFuel.AdjFuelRefs[3].FuelLevel * reactor.FinalFlux;
					if (DebugSettings.godMode)
					{
						Text.Anchor = TextAnchor.MiddleLeft;
						Widgets.Label(rect, num7.ToString("0.0"));
					}
					if (num7 > 0f)
					{
						color = Color.HSVToRGB(Mathf.Lerp(0.18f, 0f, num7), s, v);
						Widgets.DrawLine(new Vector2(rect4.x, rect4.y + num3), new Vector2(rect4.x, rect4.yMax - num3), color, 4f);
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (reactor.SlotDesignations[i] == RodDesignate.Fuel)
			{
				Widgets.DrawTextureFitted(rekt, GraphicsCache.Install, 1f);
			}
			if (reactor.SlotDesignations[i] == RodDesignate.MOX)
			{
				Widgets.DrawTextureFitted(rekt, GraphicsCache.Install, 1f);
				Widgets.DrawTextureFitted(rekt.ContractedBy(6f).RightPartPixels(10f).BottomPartPixels(10f), GraphicsCache.moxMtext, 1f);
			}
			if (reactor.SlotDesignations[i] == RodDesignate.Remove)
			{
				Widgets.DrawTextureFitted(rekt, GraphicsCache.Uninstall, 1f);
			}
		}
	}
}
