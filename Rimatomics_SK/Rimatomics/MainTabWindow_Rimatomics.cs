using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Rimatomics : MainTabWindow
	{
		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = Color.red;

		private static readonly Color BlueLight = new ColorInt(65, 84, 100, 255).ToColor;

		private static readonly Color BlueMedium = new ColorInt(37, 55, 70, 255).ToColor;

		private static readonly Color BlueDark = new ColorInt(21, 25, 29, 255).ToColor;

		public static readonly Texture2D ProgBar = SolidColorMaterials.NewSolidColorTexture(new ColorInt(0, 130, 183, 255).ToColor);

		public static int LineSpacing = 5;

		private float floatyHeight;

		public RimatomicsResearch Research;

		public Vector2 scrollPosLeft = Vector2.zero;

		public Vector2 scrollPosRight = Vector2.zero;

		public Building_RimatomicsResearchBench selBench;

		public RimatomicResearchDef SelectedResearch;

		private StringBuilder stringybits = new StringBuilder();

		public override float Margin
		{
			get
			{
				return 5f;
			}
		}

		public override Vector2 RequestedTabSize => new Vector2(1180f, (float)UI.screenHeight * 0.7f);

		public List<RimatomicResearchDef> ProjectsAvailable => Research.AllProjects.Where((RimatomicResearchDef x) => x.PrerequisitesCompleted || (!x.HideOnComplete && x.IsFinished)).ToList();

		public MainTabWindow_Rimatomics()
		{
			soundAppear = null;
			soundClose = null;
			doCloseButton = false;
			doCloseX = false;
			preventCameraMotion = false;
			absorbInputAroundWindow = false;
			closeOnClickedOutside = false;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Research = DubUtils.GetResearch();
		}

		public override void DoWindowContents(Rect inRect)
		{
			selBench = Find.Selector.FirstSelectedObject as Building_RimatomicsResearchBench;
			if (SelectedResearch == null && selBench != null)
			{
				SelectedResearch = selBench.currentProj;
			}
			if (SelectedResearch == null)
			{
				SelectedResearch = Research.AllProjects.FirstOrDefault((RimatomicResearchDef x) => x.CanStartNow);
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect = inRect;
			rect.width = 300f;
			Rect rect2 = inRect;
			rect2.width -= 300f;
			rect2.x = rect.xMax;
			rect = rect.ContractedBy(10f);
			rect2 = rect2.ContractedBy(10f);
			Widgets.DrawMenuSection(rect);
			Widgets.DrawTextureFitted(new Rect(rect.x, rect.y, rect.width, 64f).ContractedBy(2f), GraphicsCache.rimatomicsFlag, 1f);
			rect.y += 64f;
			rect.height -= 64f;
			rect = rect.ContractedBy(4f);
			float num = 30f;
			float height = num * (float)ProjectsAvailable.Count();
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, height);
			GUI.BeginGroup(rect);
			Widgets.BeginScrollView(new Rect(0f, 0f, rect.width, rect.height), ref scrollPosLeft, viewRect);
			int num2 = 0;
			foreach (RimatomicResearchDef item in ProjectsAvailable)
			{
				if (!item.HideOnComplete || !item.IsFinished)
				{
					Rect rect3 = new Rect(0f, num2, rect.width, num);
					DoAreaRow(rect3, item);
					num2 += (int)num;
				}
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.BeginGroup(rect2);
			if (SelectedResearch != null)
			{
				DrawPanel(new Rect(0f, 0f, rect2.width, rect2.height));
			}
			GUI.EndGroup();
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoAreaRow(Rect rect, RimatomicResearchDef proj)
		{
			rect = rect.ContractedBy(1f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = Color.grey;
				Widgets.DrawHighlight(rect);
			}
			else if (SelectedResearch == proj)
			{
				GUI.color = Color.grey;
				Widgets.DrawHighlight(rect);
			}
			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			if (!Research.IsPurchased(proj))
			{
				widgetRow.Icon(ThingDefOf.Silver.uiIcon);
			}
			else if (Research.IsActive(proj))
			{
				widgetRow.Icon(ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", reportFailure: false));
			}
			else if (proj.IsFinished)
			{
				widgetRow.Icon(Widgets.CheckboxOnTex);
			}
			else
			{
				widgetRow.Icon(ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young", reportFailure: false));
			}
			widgetRow.Gap(4f);
			widgetRow.Label(proj.ResearchLabel, 200f);
			if (Widgets.ButtonText(new Rect(0f, 0f, widgetRow.FinalX, 30f), "", drawBackground: false, doMouseoverSound: true, Color.blue))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				SelectedResearch = proj;
			}
			GUI.EndGroup();
		}

		public void DrawPanel(Rect rectum)
		{
			Rect viewRect = rectum;
			float num = viewRect.width - 18f;
			viewRect.height = floatyHeight;
			Widgets.BeginScrollView(rectum, ref scrollPosRight, viewRect);
			Rect rect = new Rect(num - 240f, 0f, 240f, 240f);
			Widgets.DrawShadowAround(rect);
			Widgets.DrawMenuSection(rect);
			if (SelectedResearch.screenshot.NullOrBad())
			{
				Widgets.DrawTextureFitted(rect.ContractedBy(4f), SelectedResearch.PreviewImage, 0.5f);
			}
			else
			{
				Widgets.DrawTextureFitted(rect.ContractedBy(4f), SelectedResearch.screenshot, 1f);
			}
			Rect rect2 = new Rect(0f, 0f, 50f, 50f);
			Widgets.DrawShadowAround(rect2.ContractedBy(2f));
			Widgets.DrawBoxSolid(rect2.ContractedBy(2f), Color.grey);
			Widgets.DrawTextureFitted(rect2.ContractedBy(4f), SelectedResearch.PreviewImage, 1f);
			Rect rect3 = new Rect(55f, 0f, rect.x - 20f, 400f);
			rect3.height = 50f;
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect3, SelectedResearch.ResearchLabel);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(rect3, SelectedResearch.ProjTypeLabel);
			rect3.y = rect3.yMax + 10f;
			rect3.x = 0f;
			rect3.yMax = rect.yMax - 25f;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			stringybits.Clear();
			if (SelectedResearch.PrimaryBuilding != null)
			{
				stringybits.AppendLine(SelectedResearch.PrimaryBuilding.DescriptionDetailed);
			}
			stringybits.AppendLine(SelectedResearch.ResearchDesc);
			if (SelectedResearch.BlueprintUpgrade)
			{
				stringybits.AppendLine();
				stringybits.AppendLine("BlueprintUpgradeDesc".Translate());
			}
			if (SelectedResearch.stats != null)
			{
				stringybits.AppendLine();
				foreach (string stat in SelectedResearch.stats)
				{
					stringybits.AppendLine(stat);
				}
			}
			Widgets.Label(rect3, stringybits.ToString());
			rect3.y = rect3.yMax;
			rect3.height = 24f;
			rect3.width = num;
			Widgets.Label(rect3, "RimatomUnlocks".Translate());
			rect3.y = rect3.yMax;
			rect3.height = 8f;
			Widgets.DrawBoxSolid(rect3, Color.grey);
			rect3.y = rect3.yMax + 3f;
			rect3.height = 40f;
			rect3.width = 40f;
			foreach (ResearchStepDef step in SelectedResearch.Steps)
			{
				foreach (RimatomicsThingDef unlock in step.Unlocks)
				{
					if (unlock.designatorDropdown == null)
					{
						Widgets.DrawHighlightIfMouseover(rect3);
						Widgets.DrawTextureFitted(rect3, unlock.uiIcon, 1f);
						TooltipHandler.TipRegion(rect3, unlock.LabelCap + "\n\n" + unlock.description);
						rect3.x += 40f;
					}
				}
				foreach (RecipeDef item in step.RecipeUnlocks.Where((RecipeDef x) => x.UIIconThing != null))
				{
					Widgets.DrawHighlightIfMouseover(rect3);
					Widgets.DrawTextureFitted(rect3, item.UIIconThing.uiIcon, 1f);
					TooltipHandler.TipRegion(rect3, item.UIIconThing.LabelCap + "\n\n" + item.UIIconThing.description);
					rect3.x += 40f;
				}
			}
			rect3.x = rect.x;
			rect3.width = rect.width / 2f;
			Text.Anchor = TextAnchor.MiddleLeft;
			if (!Research.IsPurchased(SelectedResearch))
			{
				if (SelectedResearch.price > 0)
				{
					GUI.color = Color.green;
					Text.Font = GameFont.Medium;
					Widgets.Label(rect3, "$" + SelectedResearch.price);
					Text.Font = GameFont.Small;
					GUI.color = Color.white;
				}
				else
				{
					Widgets.Label(rect3, "RimatomFree".Translate());
				}
			}
			rect3.x = rect.x + rect.width / 2f;
			rect3.width = rect.width / 2f;
			Text.Font = GameFont.Small;
			if (!SelectedResearch.IsFinished)
			{
				if (Research.IsPurchased(SelectedResearch))
				{
					if (selBench != null)
					{
						if (SelectedResearch != selBench.currentProj)
						{
							if (Widgets.ButtonText(rect3.ContractedBy(3f), "StartProject".Translate()))
							{
								selBench.SetProject(SelectedResearch);
								Messages.Message("ProjectInitiated".Translate(), MessageTypeDefOf.PositiveEvent);
							}
						}
						else if (Widgets.ButtonText(rect3.ContractedBy(3f), "StopProject".Translate()))
						{
							selBench.SetProject(null);
							Messages.Message("ProjectTerminated".Translate(), MessageTypeDefOf.NeutralEvent);
						}
					}
					else if (SelectedResearch.ActiveBenches.Any())
					{
						if (Widgets.ButtonText(rect3.ContractedBy(3f), "StopProject".Translate()))
						{
							foreach (Building_RimatomicsResearchBench activeBench in SelectedResearch.ActiveBenches)
							{
								activeBench.SetProject(null);
							}
							Messages.Message("ProjectTerminated".Translate(), MessageTypeDefOf.NeutralEvent);
						}
					}
					else if (Widgets.ButtonText(rect3.ContractedBy(3f), "StartProject".Translate()))
					{
						selBench = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_RimatomicsResearchBench>().FirstOrDefault();
						if (selBench != null)
						{
							Messages.Message("ProjectInitiated".Translate(), MessageTypeDefOf.PositiveEvent);
							selBench.SetProject(SelectedResearch);
						}
						else
						{
							Messages.Message("NoBenchAvailable".Translate(), MessageTypeDefOf.RejectInput);
						}
					}
				}
				else if (Widgets.ButtonText(rect3.ContractedBy(3f), "RimatomPurchase".Translate()))
				{
					if (TradeUtility.ColonyHasEnoughSilver(Find.CurrentMap, SelectedResearch.price))
					{
						Building_RimatomicsResearchBench.BuyProject(Find.CurrentMap.uniqueID, SelectedResearch);
					}
					else
					{
						Messages.Message("ResearchPriceNeed".Translate(SelectedResearch.price), MessageTypeDefOf.RejectInput);
					}
				}
			}
			rect3.y += 40f;
			rect3.width = 40f;
			rect3.height = 20f;
			if (DebugSettings.godMode && Widgets.ButtonText(rect3, "Fin"))
			{
				Building_RimatomicsResearchBench.DebugFinish(SelectedResearch);
			}
			rect3.x += 50f;
			if (DebugSettings.godMode && Widgets.ButtonText(rect3, "Buy"))
			{
				Building_RimatomicsResearchBench.Purchase(SelectedResearch);
			}
			rect3.x += 50f;
			if (DebugSettings.godMode && Widgets.ButtonText(rect3, "Rst"))
			{
				Building_RimatomicsResearchBench.ResetProj(SelectedResearch);
			}
			float num2 = 28f;
			Rect rect4 = new Rect(0f, 320f, num, num2);
			foreach (ResearchStepDef step2 in SelectedResearch.Steps)
			{
				Text.Anchor = TextAnchor.UpperLeft;
				rect4.height = num2;
				if (step2 == SelectedResearch.CurrentStep && !step2.requiredResearchFacilities.NullOrEmpty())
				{
					rect4.height += num2;
					rect4.height += num2 * (float)step2.requiredResearchFacilities.Count;
				}
				Rect rect5 = new Rect(0f, 0f, 0f, 0f);
				if (step2.IsFinished)
				{
					Widgets.DrawBoxSolid(rect4, BlueMedium);
				}
				else if (step2 == SelectedResearch.CurrentStep)
				{
					float height = Text.CalcHeight(step2.GetStepDesc, rect4.width - 10f);
					rect5.width = rect4.width - 10f;
					rect5.height = height;
					rect5.y = rect4.yMax;
					rect5.x = 5f;
					rect4.height += rect5.height;
					Widgets.DrawMenuSection(rect4);
					Widgets.Label(rect5, step2.GetStepDesc);
				}
				else
				{
					Widgets.DrawBoxSolid(rect4, BlueDark);
				}
				Rect rect6 = rect4;
				rect6.width = num2;
				rect6.height = num2;
				if (step2.IsFinished)
				{
					Widgets.DrawTextureFitted(rect6.ContractedBy(4f), Widgets.CheckboxOnTex, 1f);
				}
				else if (step2 == SelectedResearch.CurrentStep)
				{
					Widgets.DrawTextureFitted(rect6.ContractedBy(4f), ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", reportFailure: false), 1f);
				}
				else
				{
					Widgets.DrawTextureFitted(rect6.ContractedBy(4f), ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young", reportFailure: false), 1f);
				}
				rect6.x = rect6.xMax + 4f;
				rect6.width = num - 110f - rect6.x;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect6, step2.GetStepLabel);
				rect6.x = num - 110f;
				rect6.width = 100f;
				if (step2.PointCost > 0f)
				{
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.FillableBar(rect6.ContractedBy(2f), Research.GetProgress(step2) / step2.PointCost, ProgBar, GraphicsCache.black, doBorder: true);
					Widgets.Label(rect6, "ResearchProg".Translate(Research.GetProgress(step2).ToString("0"), step2.PointCost));
				}
				Text.Anchor = TextAnchor.UpperLeft;
				if (DebugSettings.godMode)
				{
					Text.Font = GameFont.Tiny;
					rect6.x -= 20f;
					rect6.width = 20f;
					if (Widgets.ButtonText(rect6, "Fin"))
					{
						Building_RimatomicsResearchBench.FinStep(step2);
					}
					rect6.x -= 20f;
					rect6.width = 20f;
					if (Widgets.ButtonText(rect6, "Rst"))
					{
						Building_RimatomicsResearchBench.ResetStep(step2);
					}
					Text.Font = GameFont.Small;
				}
				if (step2 == SelectedResearch.CurrentStep && !step2.requiredResearchFacilities.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.MiddleLeft;
					rect6.y = rect6.yMax;
					rect6.width = 800f;
					rect6.x = num2 + 4f;
					Widgets.Label(rect6, "RequiredResearchBenchFacilities".Translate() + ":");
					rect6.y = rect6.yMax;
					foreach (ThingDef fac in step2.requiredResearchFacilities)
					{
						rect6.x = num2 + 4f;
						string text = fac.LabelCap;
						List<Thing> list = Find.Maps.SelectMany((Map map) => map.listerThings.ThingsOfDef(fac)).ToList();
						if (!list.NullOrEmpty() && !list.Any(Predicate))
						{
							text += " (" + "InactiveFacility".Translate() + ")";
						}
						Widgets.HyperlinkWithIcon(rect6, new Dialog_InfoCard.Hyperlink(fac), text);
						rect6.y = rect6.yMax;
					}
					Text.Anchor = TextAnchor.UpperLeft;
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				rect4.y = rect4.yMax + (float)LineSpacing;
				rect4.height = num2;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			floatyHeight = rect4.yMax + 10f;
			Widgets.EndScrollView();
			static bool Predicate(Thing t)
			{
				CompResearchFacility compResearchFacility = t.TryGetComp<CompResearchFacility>();
				if (compResearchFacility != null && compResearchFacility.powerComp.PowerOn)
				{
					return true;
				}
				return false;
			}
		}
	}
}
