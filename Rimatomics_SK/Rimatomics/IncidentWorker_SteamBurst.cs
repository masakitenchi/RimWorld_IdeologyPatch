using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class IncidentWorker_SteamBurst : IncidentWorker
	{
		public override bool CanFireNowSub(IncidentParms parms)
		{
			return ((Map)parms.target).Rimatomics().PipeNets.Any((BasePipeNet z) => z.PipedThings.OfType<reactorCore>().Any((reactorCore x) => x.postReturnTemp > 400f));
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			reactorCore reactorCore2 = map.Rimatomics().PipeNets.SelectMany((BasePipeNet z) => from x in z.PipedThings.OfType<reactorCore>()
				where x.postReturnTemp > 400f
				select x).RandomElement();
			IntVec3 center = ((!reactorCore2.Steam.net.Pipes.NullOrEmpty()) ? reactorCore2.Steam.net.Pipes.RandomElement().parent.Position : reactorCore2.OccupiedRect().EdgeCells.RandomElement());
			GenExplosion.DoExplosion(center, map, 8f, DamageDefOf.Bomb, null);
			Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef, new TargetInfo(reactorCore2));
			return true;
		}
	}
}
