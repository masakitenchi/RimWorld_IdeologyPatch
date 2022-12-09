using RimWorld;
using Verse;

namespace Rimatomics
{
	internal class MainButtonWorker_Rimatomics : MainButtonWorker_ToggleTab
	{
		public override bool Disabled
		{
			get
			{
				def.buttonVisible = RimatomicsMod.Settings.ShowResearchButton;
				if (Find.CurrentMap != null || (def.validWithoutMap && def != MainButtonDefOf.World))
				{
					if (Find.WorldRoutePlanner.Active && Find.WorldRoutePlanner.FormingCaravan)
					{
						if (def.validWithoutMap)
						{
							return def == MainButtonDefOf.World;
						}
						return true;
					}
					return false;
				}
				return true;
			}
		}
	}
}
