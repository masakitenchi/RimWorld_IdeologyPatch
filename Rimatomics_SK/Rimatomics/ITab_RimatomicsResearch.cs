using RimWorld;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class ITab_RimatomicsResearch : ITab
	{
		public ITab_RimatomicsResearch()
		{
			labelKey = "TabResearch";
		}

		public override void OnOpen()
		{
			Find.MainTabsRoot.ToggleTab(DubDef.Rimatomics);
		}

		public override void FillTab()
		{
			if (IsVisible)
			{
				CloseTab();
			}
		}
	}
}
