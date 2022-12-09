using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Settings : ModSettings
	{
		public bool MannedReactor = true;

		public bool EnableGiblets = true;

		public bool ShowVetPatches = true;

		public string LastVersion;

		private Listing_Standard row = new Listing_Standard();

		public int PipeVisibility = 2;

		internal bool ShowResearchButton;

		private int FuelBurnRateInt = 4;

		private int PulseSizeScalingInt = 4;

		private int RadiationStrengthInt = 4;

		private int RimatomicsTraderCooldownInt = 4;

		public float FuelBurnRate => DubDef.FuelBurnRate.options[FuelBurnRateInt].value;

		public float PulseSizeScaling => DubDef.PulseSizeScaling.options[PulseSizeScalingInt].value;

		public float RadiationStrength => DubDef.RadiationStrength.options[RadiationStrengthInt].value;

		public float RimatomicsTraderCooldown => DubDef.RimatomicsTraderCooldown.options[RimatomicsTraderCooldownInt].value;

		public void ResetSettings()
		{
			PipeVisibility = 2;
			FuelBurnRateInt = 4;
		}

		private void DoRow(DubsModOptions D, ref int X)
		{
			row.LabelDouble(D.label, D.options[X].label);
			X = Mathf.RoundToInt(row.Slider(X, 0f, D.options.Count - 1));
			row.GapLine();
		}

		public void DoWindowContents(Rect canvas)
		{
			Rect rect = canvas;
			row.ColumnWidth = (rect.width - 40f) / 2f;
			row.Begin(canvas);
			row.Label(RimatomicsMod.Version);
			Rect rect2 = row.GetRect(24f);
			Widgets.DrawTextureFitted(rect2.LeftPartPixels(40f), GraphicsCache.Support, 1f);
			if (Widgets.ButtonText(rect2.RightPartPixels(rect2.width - 40f), "Rimatomics Wiki", drawBackground: false))
			{
				Application.OpenURL("https://github.com/Dubwise56/Rimatomics/wiki");
			}
			rect2 = row.GetRect(24f);
			Widgets.DrawTextureFitted(rect2.LeftPartPixels(40f), GraphicsCache.disco, 1f);
			if (Widgets.ButtonText(rect2.RightPartPixels(rect2.width - 40f), "Dubs Mods Discord", drawBackground: false))
			{
				Application.OpenURL("https://discord.gg/Az5CnDW");
			}
			row.GapLine();
			row.Label(DubDef.ReactorPipeVisibility.label);
			int num = 0;
			foreach (DubsModOption option in DubDef.ReactorPipeVisibility.options)
			{
				if (row.RadioButton(option.label, PipeVisibility == num, 30f))
				{
					PipeVisibility = num;
					if (Current.ProgramState == ProgramState.Playing)
					{
						Current.Game.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlag.Buildings);
					}
				}
				num++;
			}
			row.GapLine();
			DoRow(DubDef.FuelBurnRate, ref FuelBurnRateInt);
			DoRow(DubDef.PulseSizeScaling, ref PulseSizeScalingInt);
			DoRow(DubDef.RadiationStrength, ref RadiationStrengthInt);
			DoRow(DubDef.RimatomicsTraderCooldown, ref RimatomicsTraderCooldownInt);
			row.CheckboxLabeled("Weaponrankicons".Translate(), ref ShowVetPatches, "WeaponrankiconsDesc".Translate());
			row.CheckboxLabeled("Showresearchbutton".Translate(), ref ShowResearchButton, "ShowresearchbuttonDesc".Translate());
			row.CheckboxLabeled("Mannedreactorconsole".Translate(), ref MannedReactor, "MannedreactorconsoleDesc".Translate());
			row.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref MannedReactor, "MannedReactor", defaultValue: true, forceSave: true);
			Scribe_Values.Look(ref LastVersion, "LastVersion");
			Scribe_Values.Look(ref FuelBurnRateInt, "FuelBurnRateInt", 4, forceSave: true);
			Scribe_Values.Look(ref PulseSizeScalingInt, "PulseSizeScalingInt", 4, forceSave: true);
			Scribe_Values.Look(ref RadiationStrengthInt, "RadiationStrengthInt", 4, forceSave: true);
			Scribe_Values.Look(ref RimatomicsTraderCooldownInt, "RimatomicsTraderCooldownInt", 4, forceSave: true);
			Scribe_Values.Look(ref PipeVisibility, "PipeVisibility", 2, forceSave: true);
			Scribe_Values.Look(ref ShowVetPatches, "ShowVetPatches", defaultValue: true, forceSave: true);
			Scribe_Values.Look(ref ShowResearchButton, "ShowResearchButton", defaultValue: true, forceSave: true);
		}
	}
}
