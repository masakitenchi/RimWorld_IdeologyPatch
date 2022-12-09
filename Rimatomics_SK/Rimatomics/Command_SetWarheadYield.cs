using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Command_SetWarheadYield : Command
	{
		public MissileSilo refuelable;

		private List<MissileSilo> refuelables;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			if (refuelables == null)
			{
				refuelables = new List<MissileSilo>();
			}
			if (!refuelables.Contains(refuelable))
			{
				refuelables.Add(refuelable);
			}
			int num = int.MaxValue;
			for (int i = 0; i < refuelables.Count; i++)
			{
				if ((int)refuelables[i].MaxYield < num)
				{
					num = (int)refuelables[i].MaxYield;
				}
			}
			int num2 = 0;
			for (int j = 0; j < refuelables.Count; j++)
			{
				if ((int)refuelables[j].MinYield > num2)
				{
					num2 = (int)refuelables[j].MinYield;
				}
			}
			int startingValue = num;
			for (int k = 0; k < refuelables.Count; k++)
			{
				if ((int)refuelables[k].WarheadYield <= num)
				{
					startingValue = (int)refuelables[k].WarheadYield;
					break;
				}
			}
			Dialog_Slider window = new Dialog_Slider((int x) => "SetWarheadYield".Translate(x), num2, num, delegate(int value)
			{
				for (int l = 0; l < refuelables.Count; l++)
				{
					refuelables[l].WarheadYield = value;
				}
			}, startingValue);
			Find.WindowStack.Add(window);
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			if (refuelables == null)
			{
				refuelables = new List<MissileSilo>();
			}
			refuelables.Add(((Command_SetWarheadYield)other).refuelable);
			return false;
		}
	}
}
