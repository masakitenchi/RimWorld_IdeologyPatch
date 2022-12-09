using RimWorld;
using Verse;

namespace Rimatomics
{
	public class CreudMinxident : IExposable
	{
		public const int RetryIntervalTicks = 833;

		public bool attackMethodKnown;

		public string detection = "";

		public bool factionKnown;

		public int fireTick = -1;

		private FiringIncident firingInc;

		private int retryDurationTicks;

		public int TickFound = -1;

		private bool triedToFire;

		public bool verboseTime;

		private bool pods
		{
			get
			{
				if (FiringIncident.parms.raidArrivalMode != PawnsArrivalModeDefOf.CenterDrop)
				{
					return FiringIncident.parms.raidArrivalMode == PawnsArrivalModeDefOf.EdgeDrop;
				}
				return true;
			}
		}

		public int FireTick => fireTick;

		public FiringIncident FiringIncident => firingInc;

		public int RetryDurationTicks => retryDurationTicks;

		public bool TriedToFire => triedToFire;

		public CreudMinxident()
		{
		}

		public CreudMinxident(FiringIncident firingInc, int fireTick, int retryDurationTicks = 0)
		{
			this.firingInc = firingInc;
			this.fireTick = fireTick;
			this.retryDurationTicks = retryDurationTicks;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref firingInc, "firingInc");
			Scribe_Values.Look(ref fireTick, "fireTick", 0);
			Scribe_Values.Look(ref retryDurationTicks, "retryDurationTicks", 0);
			Scribe_Values.Look(ref triedToFire, "triedToFire", defaultValue: false);
			Scribe_Values.Look(ref TickFound, "TickFound", 0);
			Scribe_Values.Look(ref verboseTime, "factionKnown", defaultValue: false);
			Scribe_Values.Look(ref factionKnown, "factionKnown", defaultValue: false);
			Scribe_Values.Look(ref attackMethodKnown, "attackMethodKnown", defaultValue: false);
			Scribe_Values.Look(ref detection, "detection", "");
		}

		public string ReportThreat()
		{
			TaggedString taggedString = "AnUnknownForce".Translate();
			if (factionKnown || DebugSettings.godMode)
			{
				taggedString = FiringIncident.parms.faction.Name;
			}
			string text = ((!verboseTime && !pods && !DebugSettings.godMode) ? (fireTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVague() : (fireTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose());
			string text2 = ((!pods) ? ((string)"ATOMattackGround".Translate()) : ((string)"ATOMattackPods".Translate()));
			if (attackMethodKnown)
			{
				_ = (string)FiringIncident.parms.raidStrategy.LabelCap;
			}
			return "ATOMdetectionDesc".Translate(taggedString, text2, text);
		}

		public void Notify_TriedToFire()
		{
			triedToFire = true;
		}

		public override string ToString()
		{
			return fireTick + "->" + firingInc;
		}
	}
}
