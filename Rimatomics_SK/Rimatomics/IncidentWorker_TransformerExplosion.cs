using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class IncidentWorker_TransformerExplosion : IncidentWorker
	{
		public override bool CanFireNowSub(IncidentParms parms)
		{
			return ((Map)parms.target).Rimatomics().PipeNets.Any((BasePipeNet z) => z.PipedThings.OfType<Transformer>().Any((Transformer x) => x.CanPop));
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Transformer transformer = map.Rimatomics().PipeNets.SelectMany((BasePipeNet z) => from x in z.PipedThings.OfType<Transformer>()
				where x.CanPop
				select x).RandomElement();
			if (transformer == null)
			{
				return false;
			}
			transformer.Overcurrent = true;
			Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef, new TargetInfo(transformer.Position, map));
			return true;
		}
	}
}
