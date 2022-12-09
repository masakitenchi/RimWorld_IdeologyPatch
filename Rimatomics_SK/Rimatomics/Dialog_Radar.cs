using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Dialog_Radar : Window
	{
		private const float InteractivityDelay = 0.5f;

		public Action closeAction;

		public Vector2[] lights;

		public Color screenFillColor = Color.clear;

		public WeaponsConsole wc;

		public override Vector2 InitialSize => new Vector2(256f, 256f);

		public override float Margin
		{
			get
            {
                return 20f;
            }
		}

		public Dialog_Radar(WeaponsConsole console, Pawn pawn)
		{
			wc = console;
			wc.ConsoleOpen = true;
			forcePause = false;
			absorbInputAroundWindow = false;
			closeOnCancel = true;
			soundAppear = SoundDefOf.CommsWindow_Open;
			soundClose = SoundDefOf.CommsWindow_Close;
			doCloseButton = false;
			doCloseX = true;
			draggable = true;
			drawShadow = true;
			preventCameraMotion = false;
			onlyOneOfTypeAllowed = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (wc == null || !wc.Manned)
			{
				Close();
			}
			if (WorldRendererUtility.WorldRenderedNow)
			{
				return;
			}
			Rect rect = inRect;
			Widgets.DrawTextureFitted(rect, GraphicsCache.TrackingScreen, 1f);
			Vector2 center = default(Vector2);
			GUI.BeginGroup(rect);
			foreach (Pawn freeColonist in wc.Map.mapPawns.FreeColonists)
			{
				center.x = GenMath.LerpDoubleClamped(0f, wc.Map.Size.x, 0f, rect.width, freeColonist.DrawPos.x);
				center.y = GenMath.LerpDoubleClamped(0f, wc.Map.Size.z, rect.height, 0f, freeColonist.DrawPos.z);
				Widgets.DrawTextureRotated(center, GraphicsCache.DotColonist, 0f, 0.5f);
			}
			foreach (Pawn item in wc.Map.mapPawns.PrisonersOfColony)
			{
				center.x = GenMath.LerpDoubleClamped(0f, wc.Map.Size.x, 0f, rect.width, item.DrawPos.x);
				center.y = GenMath.LerpDoubleClamped(0f, wc.Map.Size.z, rect.height, 0f, item.DrawPos.z);
				Widgets.DrawTextureRotated(center, GraphicsCache.DotFriendly, 0f, 0.5f);
			}
			foreach (Thing item2 in wc.Map.attackTargetsCache.TargetsHostileToColony)
			{
				center.x = GenMath.LerpDoubleClamped(0f, wc.Map.Size.x, 0f, rect.width, item2.DrawPos.x);
				center.y = GenMath.LerpDoubleClamped(0f, wc.Map.Size.z, rect.height, 0f, item2.DrawPos.z);
				Widgets.DrawTextureRotated(center, GraphicsCache.DotEnemy, 0f, 0.5f);
			}
			foreach (Thing item3 in wc.mapComp.AllProjectiles())
			{
				center.x = GenMath.LerpDoubleClamped(0f, wc.Map.Size.x, 0f, rect.width, item3.DrawPos.x);
				center.y = GenMath.LerpDoubleClamped(0f, wc.Map.Size.z, rect.height, 0f, item3.DrawPos.z);
				Widgets.DrawTextureRotated(center, GraphicsCache.DotProjectile, 0f);
			}
			GUI.EndGroup();
		}
	}
}
