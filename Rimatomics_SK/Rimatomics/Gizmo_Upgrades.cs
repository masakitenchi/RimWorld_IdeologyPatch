using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Gizmo_Upgrades : Command
	{
		public static readonly Texture2D Greyed = SolidColorMaterials.NewSolidColorTexture(0f, 0f, 0f, 0.7f);

		public static readonly Texture2D Upgrade = ContentFinder<Texture2D>.Get("Rimatomics/UI/Upgrade");

		public Action action;

		public UpgradeState upgradeState;

		public override float GetWidth(float maxWidth)
		{
			return 75f;
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			action();
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms p)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			bool flag = false;
			if (Mouse.IsOver(rect))
			{
				flag = true;
				GUI.color = GenUI.MouseoverColor;
			}
			Texture badTex = icon;
			if (badTex == null)
			{
				badTex = BaseContent.BadTex;
			}
			GUI.DrawTexture(rect, Command.BGTex);
			MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
			GUI.color = IconDrawColor;
			Widgets.DrawTextureFitted(new Rect(rect), badTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
			if (upgradeState == UpgradeState.None)
			{
				Widgets.DrawTextureFitted(new Rect(rect), Greyed, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
			}
			if (upgradeState == UpgradeState.Install)
			{
				Widgets.DrawTextureFitted(new Rect(rect), Greyed, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
				Widgets.DrawTextureFitted(new Rect(rect), Upgrade, iconDrawScale * 0.85f, iconProportions, iconTexCoords);
			}
			GUI.color = Color.white;
			bool flag2 = false;
			KeyCode keyCode = ((hotKey != null) ? hotKey.MainKey : KeyCode.None);
			if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
			{
				Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 18f), keyCode.ToStringReadable());
				GizmoGridDrawer.drawnHotKeys.Add(keyCode);
				if (hotKey.KeyDownEvent)
				{
					flag2 = true;
					Event.current.Use();
				}
			}
			if (Widgets.ButtonInvisible(rect))
			{
				flag2 = true;
			}
			Text.Font = GameFont.Small;
			string labelCap = LabelCap;
			if (!labelCap.NullOrEmpty())
			{
				float num = Text.CalcHeight(labelCap, rect.width);
				Rect rect2 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
				GUI.DrawTexture(rect2, TexUI.GrayTextBG);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect2, labelCap);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			GUI.color = Color.white;
			if (DoTooltip)
			{
				TipSignal tip = Desc;
				if (disabled && !disabledReason.NullOrEmpty())
				{
					string text = tip.text;
					tip.text = string.Concat(text, "\n\n", "DisabledCommand".Translate(), ": ", disabledReason);
				}
				TooltipHandler.TipRegion(rect, tip);
			}
			if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null || !Find.WindowStack.FloatMenu.windowRect.Overlaps(rect)))
			{
				UIHighlighter.HighlightOpportunity(rect, HighlightTag);
			}
			if (flag2)
			{
				if (disabled)
				{
					if (!disabledReason.NullOrEmpty())
					{
						Messages.Message(disabledReason, MessageTypeDefOf.RejectInput);
					}
					return new GizmoResult(GizmoState.Mouseover, null);
				}
				if (!TutorSystem.AllowAction(TutorTagSelect))
				{
					return new GizmoResult(GizmoState.Mouseover, null);
				}
				GizmoResult result = new GizmoResult(GizmoState.Interacted, Event.current);
				TutorSystem.Notify_Event(TutorTagSelect);
				return result;
			}
			if (flag)
			{
				return new GizmoResult(GizmoState.Mouseover, null);
			}
			return new GizmoResult(GizmoState.Clear, null);
		}
	}
}
