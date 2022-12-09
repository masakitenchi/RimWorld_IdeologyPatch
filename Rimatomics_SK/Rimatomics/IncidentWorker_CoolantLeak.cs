using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class IncidentWorker_CoolantLeak : IncidentWorker
	{
		public override bool CanFireNowSub(IncidentParms parms)
		{
			return ((Map)parms.target).listerBuildings.AllBuildingsColonistOfClass<Building_Pipe>().Any((Building_Pipe x) => x.pipe.mode == PipeType.Cooling);
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!map.listerBuildings.AllBuildingsColonistOfClass<reactorCore>().Any())
			{
				return false;
			}
			reactorCore reactorCore2 = map.listerBuildings.AllBuildingsColonistOfClass<reactorCore>().RandomElement();
			SendStandardLetter(parms, reactorCore2);
			return true;
		}
	}
}
