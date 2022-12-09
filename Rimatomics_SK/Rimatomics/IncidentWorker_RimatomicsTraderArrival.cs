using RimWorld;
using Verse;

namespace Rimatomics
{
	public class IncidentWorker_RimatomicsTraderArrival : IncidentWorker
	{
		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.passingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
			TradeShip tradeShip = new TradeShip(DefDatabase<TraderKindDef>.GetNamed("Orbital_Nuclear"));
			tradeShip.name = "Rimatomics";
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
			{
				Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label), LetterDefOf.PositiveEvent);
			}
			map.passingShipManager.AddShip(tradeShip);
			tradeShip.GenerateThings();
			return true;
		}
	}
}
