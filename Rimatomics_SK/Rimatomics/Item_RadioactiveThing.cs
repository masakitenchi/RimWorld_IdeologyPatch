using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Item_RadioactiveThing : ThingWithComps
	{
		public virtual float strength => 3f;

		public virtual float radius => 7f;

		public virtual bool smolders => false;

		private IntVec3 loc
		{
			get
			{
				Pawn_CarryTracker pawn_CarryTracker = null;
				if (holdingOwner != null)
				{
					pawn_CarryTracker = holdingOwner.Owner as Pawn_CarryTracker;
				}
				return pawn_CarryTracker?.pawn.Position ?? base.Position;
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (this.IsHashIntervalTick(15) && strength > 0.01f)
			{
				DubUtils.emitRadiation(loc, strength, radius, base.MapHeld);
			}
			if (smolders)
			{
				if (this.IsHashIntervalTick(70))
				{
					FleckMaker.ThrowMicroSparks(loc.ToVector3(), base.MapHeld);
				}
				if (this.IsHashIntervalTick(400))
				{
					FleckMaker.ThrowHeatGlow(loc, base.MapHeld, 1f);
				}
				if (this.IsHashIntervalTick(120))
				{
					FleckMaker.ThrowSmoke(loc.ToVector3(), base.MapHeld, 1f);
				}
			}
		}
	}
}
