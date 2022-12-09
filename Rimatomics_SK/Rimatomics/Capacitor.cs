using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Capacitor : CompPowerBattery
	{
		public float DischargeAmount;

		public override void CompTick()
		{
			DrawPower(Mathf.Min(DischargeAmount * CompPower.WattsToWattDaysPerTick, base.storedEnergy));
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Battery compProperties_Battery = base.Props;
			TaggedString taggedString = (string)("PowerBatteryStored".Translate() + ": " + base.storedEnergy.ToString("F0") + " / " + compProperties_Battery.storedEnergyMax.ToString("F0") + " Wd") + ("\n" + "PowerBatteryEfficiency".Translate() + ": " + (compProperties_Battery.efficiency * 100f).ToString("F0") + "%");
			if ((double)base.storedEnergy > 0.0)
			{
				taggedString = (string)(taggedString + ("\n" + "SelfDischarging".Translate() + ": " + DischargeAmount.ToString("F0") + " W"));
			}
			return taggedString + "\n" + base.CompInspectStringExtra();
		}
	}
}
