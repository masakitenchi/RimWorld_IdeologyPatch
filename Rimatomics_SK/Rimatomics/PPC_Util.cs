using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	internal static class PPC_Util
	{
		public static bool HasCharge(this PowerNet PowerNet, float charge)
		{
			List<CompPowerBattery> list = (from x in PowerNet.Map.Rimatomics().PPCs
				where x.PowerComp.PowerNet == PowerNet
				select x.batt into x
				where x.StoredEnergy > 0f
				select x).ToList();
			if (list.NullOrEmpty())
			{
				return false;
			}
			if (list.Sum((CompPowerBattery x) => x.StoredEnergy) > charge)
			{
				return true;
			}
			return false;
		}

		public static bool DissipateCharge(this PowerNet PowerNet, float charge)
		{
			List<CompPowerBattery> list = (from x in PowerNet.Map.Rimatomics().PPCs
				where x.PowerComp.PowerNet == PowerNet
				select x.batt into x
				where x.StoredEnergy > 0f
				select x).ToList();
			if (list.NullOrEmpty())
			{
				return false;
			}
			if (list.Sum((CompPowerBattery x) => x.StoredEnergy) < charge)
			{
				return false;
			}
			float num = charge;
			int num2 = 0;
			while (num > 0f)
			{
				num2++;
				list.RemoveAll((CompPowerBattery x) => x.StoredEnergy <= 0f);
				if (list.NullOrEmpty() || list.Sum((CompPowerBattery x) => x.StoredEnergy) < num)
				{
					return false;
				}
				float a = num / (float)list.Count;
				float b = list.Min((CompPowerBattery x) => x.StoredEnergy);
				float num3 = Mathf.Min(a, b);
				foreach (CompPowerBattery item in list)
				{
					item.DrawPower(num3);
					num -= num3;
				}
				if (num2 > 5000)
				{
					return false;
				}
			}
			if (DebugSettings.godMode)
			{
				Log.Warning(num2 + "Pulse loops (god mode on)");
			}
			return true;
		}
	}
}
