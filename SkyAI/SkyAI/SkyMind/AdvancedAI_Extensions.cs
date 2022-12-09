using System;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x0200002C RID: 44
	internal static class AdvancedAI_Extensions
	{
		// Token: 0x06000165 RID: 357 RVA: 0x00020244 File Offset: 0x0001E444
		public static void SkyAiDebug(this Pawn pawn, bool debug, string debugLog)
		{
			bool flag = debug && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag)
			{
				Log.Message(debugLog ?? "");
			}
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00020274 File Offset: 0x0001E474
		public static void Slider(this Listing_Standard list, ref int value, int min, int max, Func<string> label, int roundToAsInt, float gap = 10f)
		{
			float num = (float)value;
			float gapHeight = AdvancedAI_Extensions.HorizontalSlider(list.GetRect(gap), ref num, (float)min, (float)max, (label == null) ? null : label(), 1f);
			value = num.RoundToAsInt(roundToAsInt);
			list.Gap(gapHeight);
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000202C0 File Offset: 0x0001E4C0
		public static void Slider(this Listing_Standard list, ref float value, float min, float max, Func<string> label, float roundTo = -1f, float gap = 10f)
		{
			Rect rect = list.GetRect(gap);
			float gapHeight = AdvancedAI_Extensions.HorizontalSlider(rect, ref value, min, max, (label == null) ? null : label(), roundTo);
			list.Gap(gapHeight);
		}

		// Token: 0x06000168 RID: 360 RVA: 0x000202FC File Offset: 0x0001E4FC
		public static float HorizontalSlider(Rect rect, ref float value, float leftValue, float rightValue, string label = null, float roundTo = -1f)
		{
			bool flag = label != null;
			if (flag)
			{
				TextAnchor anchor = Text.Anchor;
				GameFont font = Text.Font;
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.Label(rect, label);
				Text.Anchor = anchor;
				Text.Font = font;
				rect.y += 8f;
			}
			value = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
			bool flag2 = roundTo > 0f;
			if (flag2)
			{
				value = (float)Mathf.RoundToInt(value / roundTo) * roundTo;
			}
			return (4f.ToString() + label != null) ? 8f : 0f;
		}

		// Token: 0x06000169 RID: 361 RVA: 0x000203AC File Offset: 0x0001E5AC
		public static int RoundToAsInt(this float num, int factor)
		{
			return (int)(Math.Round((double)num / (double)factor, 0) * (double)factor);
		}
	}
}
