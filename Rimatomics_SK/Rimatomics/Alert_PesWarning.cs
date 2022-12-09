using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Alert_PesWarning : Alert
	{
		private ResearchProjectDef PES_SkyIDS = DefDatabase<ResearchProjectDef>.GetNamed("PES_SkyIDS", errorOnFail: false);

		private ResearchProjectDef PES_SkyIDL = DefDatabase<ResearchProjectDef>.GetNamed("PES_SkyIDL", errorOnFail: false);

		public Alert_PesWarning()
		{
			defaultPriority = AlertPriority.Medium;
		}

		public override string GetLabel()
		{
			return "ThreatDetection".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "MissingPESUpgrades".Translate();
		}

		public override AlertReport GetReport()
		{
			if (HarmonyPatches.Pstrike && !PES_SkyIDS.IsFinished && !PES_SkyIDL.IsFinished)
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].Rimatomics().Radars.Any((Building_Radar x) => x.HasATOM))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
