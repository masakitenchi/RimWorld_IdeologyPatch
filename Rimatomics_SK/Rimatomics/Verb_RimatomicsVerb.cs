using Verse;

namespace Rimatomics
{
	public abstract class Verb_RimatomicsVerb : Verb_LaunchProjectile
	{
		public Building_EnergyWeapon GetWep => caster as Building_EnergyWeapon;

		public override int ShotsPerBurst
		{
			get
			{
				return GetWep.ShotCount;
			}
		}

		public override bool Available()
		{
			Building_EnergyWeapon getWep = GetWep;
			return getWep.HasCharge(getWep.PulseSize);
		}
	}
}
