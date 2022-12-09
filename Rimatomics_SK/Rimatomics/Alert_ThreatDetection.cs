using System.Text;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Alert_ThreatDetection : Alert_Critical
	{
		private StringBuilder sb = new StringBuilder();

		public Alert_ThreatDetection()
		{
			defaultPriority = AlertPriority.Critical;
		}

		public override string GetLabel()
		{
			return "ThreatDetection".Translate();
		}

		public override TaggedString GetExplanation()
		{
			sb.Clear();
			foreach (CreudMinxident queuedIncident in DubUtils.GetResearch().queuedIncidents)
			{
				sb.AppendLine(queuedIncident.ReportThreat());
				sb.AppendLine();
			}
			return sb.ToString();
		}

		public override AlertReport GetReport()
		{
			if (HarmonyPatches.Pstrike)
			{
				return false;
			}
			return !DubUtils.GetResearch().queuedIncidents.NullOrEmpty();
		}
	}
}
