using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Building_RadDetector : Building
	{
		public float LastDetection;

		public bool Mute = true;

		public CompPowerTrader powerComp;

		public int TicksSinceDetection;

		public override void Draw()
		{
			base.Draw();
			if (TicksSinceDetection > 0)
			{
				GraphicsCache.RadDetector.Draw(DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this);
			}
		}

		public override void Tick()
		{
			base.Tick();
			TicksSinceDetection = Mathf.Max(TicksSinceDetection - 1, 0);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
		}

		public void DetectRads(float strength)
		{
			if (powerComp != null && powerComp.PowerOn)
			{
				LastDetection = strength;
				TicksSinceDetection = 1500;
				if (Mute)
				{
					DubDef.geigerTick.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
			}
		}

		public override void DrawGUIOverlay()
		{
			base.DrawGUIOverlay();
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TicksSinceDetection > 0)
			{
				GenMapUI.DrawThingLabel(LabelDrawPosFor(), LastDetection.ToString("0.0"), Color.green);
			}
		}

		public Vector2 LabelDrawPosFor()
		{
			Vector3 drawPos = DrawPos;
			Vector2 result = Find.Camera.WorldToScreenPoint(drawPos) / Prefs.UIScale;
			result.y = (float)UI.screenHeight - result.y;
			return result;
		}
	}
}
