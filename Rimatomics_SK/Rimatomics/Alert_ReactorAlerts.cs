using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Alert_ReactorAlerts : Alert
	{
		private StringBuilder sb = new StringBuilder();

		private IEnumerable<reactorCore> cores
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (reactorCore item in maps[i].Rimatomics().PipeNets.OfType<LoomNet>().SelectMany((LoomNet z) => z.Cores.Where((reactorCore x) => x.CrackedFuel || x.IsOverheating || x.IsCoreMelt)))
					{
						yield return item;
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

		public Alert_ReactorAlerts()
		{
			defaultPriority = AlertPriority.High;
		}

		public override string GetLabel()
		{
			return "ReactorAlert".Translate();
		}

		public override TaggedString GetExplanation()
		{
			sb.Clear();
			sb.AppendLine("ReactorAlerts".Translate());
			foreach (reactorCore core in cores)
			{
				sb.AppendLine();
				sb.AppendLine(core.Label + "  :");
				if (core.Leakage > 1f)
				{
					sb.AppendLine("RADIATIONLEAK".Translate());
				}
				if (core.IsCoreMelt)
				{
					sb.AppendLine("CONTROLRODJAM".Translate());
					sb.AppendLine("COREMELT".Translate());
					sb.AppendLine("EVACUATE".Translate());
				}
				if (core.IsOverheating)
				{
					sb.AppendLine("OVERHEATING".Translate(core.postReturnTemp.ToStringTemperature()));
				}
				if (core.SteamNet.Turbines.Any((Turbine x) => x.CoolingNet.Coolers.Count == 0) && !core.IsCold)
				{
					sb.AppendLine("NOCOOLING".Translate());
				}
				if (core.CrackedFuel)
				{
					sb.AppendLine("CRACKEDFUEL".Translate());
					sb.AppendLine("RADIATIONLEAKSYSTEM".Translate());
				}
			}
			return sb.ToString();
		}

		public override AlertReport GetReport()
		{
			return cores.FirstOrDefault();
		}
	}
}
