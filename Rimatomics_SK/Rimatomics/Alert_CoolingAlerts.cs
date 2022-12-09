using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Alert_CoolingAlerts : Alert
	{
		private IEnumerable<CoolingSystem> coolers
		{
			get
			{
				List<Map> maps = Find.Maps;
				foreach (Map item in maps)
				{
					foreach (CoolingSystem item2 in item.Rimatomics().PipeNets.OfType<CoolingNet>().SelectMany((CoolingNet z) => z.Coolers.Where((CoolingSystem x) => x.coolingCapacity < x.CoolingCapacityWatts * 0.5f)))
					{
						yield return item2;
					}
				}
			}
		}

		public override Color BGColor
		{
			get
			{
				float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
				return new Color(num, num, num) * Color.red;
			}
		}

		public Alert_CoolingAlerts()
		{
			defaultPriority = AlertPriority.High;
		}

		public override string GetLabel()
		{
			return "Rimatomics.LowCoolingCapacity".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "Rimatomics.LowCoolingCapacityDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return coolers.FirstOrDefault();
		}
	}
}
