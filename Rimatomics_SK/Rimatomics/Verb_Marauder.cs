using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Verb_Marauder : Verb_RimatomicsVerb
	{
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Building_EnergyWeapon getWep = base.GetWep;
			getWep.GatherData("PPCWeapon", 5f);
			getWep.GatherData("PPCMarauder", 5f);
			getWep.PrototypeBang(getWep.GunProps.EnergyWep.PrototypeFailureChance);
		}

		public override bool TryCastShot()
		{
			Building_EnergyWeapon getWep = base.GetWep;
			if (!getWep.top.TargetInSights)
			{
				return false;
			}
			bool num = base.TryCastShot();
			if (num)
			{
				getWep.DissipateCharge(getWep.PulseSize);
				getWep.MuzzleFlash();
				Find.CameraDriver.shaker.SetMinShake(0.05f);
			}
			return num;
		}
	}
}
