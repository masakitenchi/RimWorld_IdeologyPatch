using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class DefExtensions : Def
	{
		public static List<ThingDef> ProjectileDefs = new List<ThingDef>();

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			StatDefOf.MarketValue.parts.Add(new FuelValueStat());
			ProjectileDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.projectile != null && x.projectile.flyOverhead).ToList();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (item?.race?.lifeStageWorkSettings != null && !item.race.lifeStageWorkSettings.NullOrEmpty())
				{
					LifeStageWorkSettings lifeStageWorkSettings = new LifeStageWorkSettings();
					lifeStageWorkSettings.minAge = 14;
					lifeStageWorkSettings.workType = DefDatabase<WorkTypeDef>.GetNamed("NuclearWork");
					item?.race?.lifeStageWorkSettings.Add(lifeStageWorkSettings);
				}
			}
		}
	}
}
