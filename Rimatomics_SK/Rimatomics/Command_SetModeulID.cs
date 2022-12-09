using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Command_SetModeulID : Command
	{
		public Building parent;

		public Command_SetModeulID(Building p)
		{
			parent = p;
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			for (int i = 0; i < greekAlpha.alphas.Count; i++)
			{
				int li = i;
				list.Add(new FloatMenuOption(greekAlpha.getAlphaString(i, parent.Map), delegate
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					if (parent is IAssignableGreek)
					{
						reactorCore.SetGreekID(parent, li);
					}
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
