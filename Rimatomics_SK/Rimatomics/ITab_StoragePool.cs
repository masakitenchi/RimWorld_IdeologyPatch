using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class ITab_StoragePool : ITab
	{
		private static readonly Vector2 WinSize = new Vector2(500f, 500f);

		public Building_storagePool pool;

		public Vector2 scrollPos = Vector2.zero;

		public ITab_StoragePool()
		{
			size = WinSize;
			labelKey = "Managefuel";
		}

		public override void FillTab()
		{
			if (Event.current.type != EventType.Layout)
			{
				pool = base.SelThing as Building_storagePool;
				Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(20f);
				Widgets.DrawMenuSection(rect);
				float height = 30f * (float)pool.GetDirectlyHeldThings().Count + 50f;
				Rect rect2 = new Rect(0f, 0f, rect.width - 16f, height);
				Widgets.BeginScrollView(rect, ref scrollPos, rect2);
				Rect position = rect2.ContractedBy(10f);
				GUI.BeginGroup(position);
				int num = 0;
				for (int i = 0; i < pool.GetDirectlyHeldThings().Count(); i++)
				{
					Rect source = new Rect(0f, num, position.width, 30f);
					Rect rect3 = new Rect(source);
					rect3.x += 6f;
					rect3.width -= 6f;
					DoAreaRow(rect3, pool.GetDirectlyHeldThings().ElementAt(i) as Item_NuclearFuel);
					num += 30;
				}
				GUI.EndGroup();
				Widgets.EndScrollView();
			}
		}

		private void DoAreaRow(Rect rect, Item_NuclearFuel rod)
		{
			rect = rect.ContractedBy(4f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = Color.blue;
				Widgets.DrawHighlight(rect);
				GUI.color = Color.white;
			}
			Text.Font = GameFont.Small;
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			if (rod.cracked)
			{
				widgetRow.Icon(GraphicsCache.crackedSlot);
				widgetRow.Label("RimatomCracked".Translate(), 50f);
			}
			else
			{
				widgetRow.Icon(GraphicsCache.fuelRodTex);
				widgetRow.FillableBar(50f, 20f, rod.FuelLevel, "RimatomLife".Translate(), BaseContent.GreyTex, BaseContent.BlackTex);
			}
			widgetRow.Gap(4f);
			widgetRow.Label(rod.LabelCap, 170f);
			widgetRow.Gap(4f);
			Rect rect2 = new Rect(widgetRow.FinalX, 0f, 150f, rect.height);
			float num = rect2.width / 2f;
			Text.WordWrap = false;
			Text.Font = GameFont.Tiny;
			Rect rect3 = new Rect(rect2.x, rect2.y, num, rect2.height);
			DoAreaSelector(rect3, rod, selection: false);
			Rect rect4 = new Rect(rect2.x + num, rect2.y, num, rect2.height);
			DoAreaSelector(rect4, rod, selection: true);
			Text.Font = GameFont.Small;
			Text.WordWrap = true;
			GUI.EndGroup();
		}

		private void DoAreaSelector(Rect rect, Item_NuclearFuel rod, bool selection)
		{
			GUI.DrawTexture(rect, selection ? GraphicsCache.red : GraphicsCache.grey);
			Text.Anchor = TextAnchor.MiddleLeft;
			TaggedString label = (selection ? "RimatomRemove".Translate() : "RimatomStore".Translate());
			Rect rect2 = rect;
			rect2.xMin += 3f;
			rect2.yMin += 2f;
			Widgets.Label(rect2, label);
			if (rod.markedForRemove == selection)
			{
				Widgets.DrawBox(rect, 2);
			}
			if (rod.markedForRemove != selection && Input.GetMouseButton(0) && Event.current.isMouse && Mouse.IsOver(rect))
			{
				pool.ToggleDesignation(rod, selection);
				SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
