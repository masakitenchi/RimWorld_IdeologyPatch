using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class IncidentWorker_Unknown : IncidentWorker
	{
		public override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (DubDef.RimatomicsActivate.IsFinished && DubDef.AllReactors(map).Any() && map.skyManager.CurSkyGlow < 0.2f)
			{
				return map.mapPawns.AllPawnsSpawned.Any((Pawn x) => x.def.race.Animal && x.Faction != Faction.OfPlayer && x.RaceProps.baseBodySize > 0.5f && !x.Position.Roofed(x.Map));
			}
			return false;
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			notReal notReal2 = ThingMaker.MakeThing(ThingDef.Named("notReal")) as notReal;
			notReal2.target = map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.def.race.Animal && x.Faction != Faction.OfPlayer && x.RaceProps.baseBodySize > 0.5f && !x.Position.Roofed(x.Map)).RandomElement();
			GenSpawn.Spawn(notReal2, notReal2.target.Position, map);
			return true;
		}
	}
}
