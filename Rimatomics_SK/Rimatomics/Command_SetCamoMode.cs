using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Command_SetCamoMode : Command
	{
		public List<string> CamoModes = new List<string> { "base", "woodland", "desert", "snow" };

		public Thing parent;

		public Command_SetCamoMode(Thing p)
		{
			parent = p;
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (string item in CamoModes)
			{
				list.Add(new FloatMenuOption(item, delegate
				{
					int numSelected = Find.Selector.NumSelected;
					List<object> selectedObjects = Find.Selector.SelectedObjects;
					for (int i = 0; i < numSelected; i++)
					{
						if (selectedObjects[i] is ICamoSelect)
						{
							Building_LaunchPad.SetCamoMode(selectedObjects[i] as Thing, item);
						}
					}
					SoundDefOf.Click.PlayOneShotOnCamera();
				}));
			}
			foreach (string item2 in CamoModes)
			{
				list.Add(new FloatMenuOption("All " + item2, delegate
				{
					foreach (ICamoSelect item3 in parent.Map.listerBuildings.allBuildingsColonist.OfType<ICamoSelect>())
					{
						item3.SetMode(item2);
					}
					parent.Map.Rimatomics().MapCamoMode = item2;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
