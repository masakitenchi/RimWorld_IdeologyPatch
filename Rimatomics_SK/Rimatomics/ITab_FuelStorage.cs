using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class ITab_FuelStorage : ITab_Storage
	{
		public override void FillTab()
		{
			IStoreSettingsParent storeSettingsParent = SelStoreSettingsParent;
			StorageSettings settings = storeSettingsParent.GetStoreSettings();
			Rect position = new Rect(0f, 0f, ITab_Storage.WinSize.x, ITab_Storage.WinSize.y).ContractedBy(10f);
			GUI.BeginGroup(position);
			if (IsPrioritySettingVisible)
			{
				Text.Font = GameFont.Small;
				Rect rect = new Rect(0f, 0f, 160f, base.TopAreaHeight - 6f);
				if (Widgets.ButtonText(rect, "Priority".Translate() + ": " + settings.Priority.Label().CapitalizeFirst()))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (StoragePriority value in Enum.GetValues(typeof(StoragePriority)))
					{
						if (value != 0)
						{
							StoragePriority localPr = value;
							list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
							{
								settings.Priority = localPr;
							}));
						}
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				UIHighlighter.HighlightOpportunity(rect, "StoragePriority");
			}
			ThingFilter parentFilter = null;
			if (storeSettingsParent.GetParentStoreSettings() != null)
			{
				parentFilter = storeSettingsParent.GetParentStoreSettings().filter;
			}
			Rect rect2 = new Rect(0f, base.TopAreaHeight, position.width, position.height - base.TopAreaHeight);
			Bill[] first = (from b in BillUtility.GlobalBills()
				where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
				select b).ToArray();
			DoThingFilterConfigWindow(rect2, base.thingFilterState, settings.filter, parentFilter, 8);
			Bill[] second = (from b in BillUtility.GlobalBills()
				where b is Bill_Production && b.GetStoreZone() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStoreInStockpile((Bill_Production)b, b.GetStoreZone())
				select b).ToArray();
			foreach (Bill item in first.Except(second))
			{
				Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(item.LabelCap, item.billStack.billGiver.LabelShort.CapitalizeFirst(), item.GetStoreZone().label), item.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
			GUI.EndGroup();
		}

		public void DoThingFilterConfigWindow(Rect rect, ThingFilterUI.UIState state, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, bool forceHideHitPointsConfig = false, List<ThingDef> suppressSmallVolumeTags = null, Map map = null)
		{
			Widgets.DrawMenuSection(rect);
			Text.Font = GameFont.Tiny;
			float num = rect.width - 2f;
			Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
			if (Widgets.ButtonText(rect2, "ClearAll".Translate()))
			{
				filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			if (Widgets.ButtonText(new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f), "AllowAll".Translate()))
			{
				filter.SetAllowAll(parentFilter);
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			Text.Font = GameFont.Small;
			rect.yMin = rect2.yMax;
			int num2 = 1;
			Rect rect3 = new Rect(rect.x + 1f, rect.y + 1f + (float)num2, rect.width - 2f, 24f);
			state.quickSearch.OnGUI(rect3);
			rect.yMin = rect3.yMax;
			TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
			bool flag = true;
			bool flag2 = true;
			if (parentFilter != null)
			{
				node = parentFilter.DisplayRootCategory;
				flag = parentFilter.allowedHitPointsConfigurable;
				flag2 = parentFilter.allowedQualitiesConfigurable;
			}
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, ThingFilterUI.viewHeight);
			Rect visibleRect = new Rect(0f, 0f, rect.width, rect.height);
			visibleRect.position += state.scrollPosition;
			Widgets.BeginScrollView(rect, ref state.scrollPosition, viewRect);
			float y = 2f;
			if (flag && !forceHideHitPointsConfig)
			{
				ThingFilterUI.DrawHitPointsFilterConfig(ref y, viewRect.width, filter);
			}
			if (flag2)
			{
				ThingFilterUI.DrawQualityFilterConfig(ref y, viewRect.width, filter);
			}
			if (base.SelThing is IFuelFilter filter2)
			{
				FuelLifeFilter(ref y, viewRect.width, filter2);
			}
			float num3 = y;
			Rect rect4 = new Rect(0f, y, viewRect.width, 9999f);
			visibleRect.position -= rect4.position;
			Listing_TreeThingFilter listing_TreeThingFilter = new Listing_TreeThingFilter(filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags, state.quickSearch.filter);
			listing_TreeThingFilter.Begin(rect4);
			listing_TreeThingFilter.ListCategoryChildren(node, openMask, map, visibleRect);
			listing_TreeThingFilter.End();
			state.quickSearch.noResultsMatched = listing_TreeThingFilter.matchCount == 0;
			if (Event.current.type == EventType.Layout)
			{
				ThingFilterUI.viewHeight = num3 + listing_TreeThingFilter.CurHeight + 90f;
			}
			Widgets.EndScrollView();
		}

		private static void FuelLifeFilter(ref float y, float width, IFuelFilter filter)
		{
			Rect rect = new Rect(20f, y, width - 20f, 28f);
			FloatRange range = filter.FuelLifeFilter;
			Widgets.FloatRange(rect, 66677734, ref range, 0f, 1f, "FuelLifeRange", ToStringStyle.PercentZero);
			filter.FuelLifeFilter = range;
			y += 28f;
			y += 5f;
			Text.Font = GameFont.Small;
		}
	}
}
