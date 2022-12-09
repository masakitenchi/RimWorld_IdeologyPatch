using System;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x02000034 RID: 52
	public class SkyAiCore : Mod
	{
		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600019D RID: 413 RVA: 0x00025284 File Offset: 0x00023484
		// (set) Token: 0x0600019E RID: 414 RVA: 0x0002528B File Offset: 0x0002348B
		public static SkyAiCore Instance { get; private set; }

		// Token: 0x0600019F RID: 415 RVA: 0x00025294 File Offset: 0x00023494
		public SkyAiCore(ModContentPack content) : base(content)
		{
			SkyAiCore.Instance = this;
			DateTime now = DateTime.Now;
			SkyAiCore.Settings = base.GetSettings<Settings>();
			HarmonyBase.InitPatches();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				TexturesLoader.InitTextures();
			});
			bool flag = SkyAiCore.ShouldUpdateModVersion(SkyAiCore.Settings.modVersion);
			if (flag)
			{
				SkyAiCore.shouldClearData = true;
				SkyAiCore.Settings.modVersion = SkyAiCore.modVersion;
				SkyAiCore.Settings.Write();
			}
			bool flag2 = ModLister.GetActiveModWithIdentifier("skyarkhangel.darkestnight") != null;
			if (flag2)
			{
				SkyAiCore.Settings.DarknestNightEnabled = true;
			}
			bool flag3 = ModLister.GetActiveModWithIdentifier("skyarkhangel.HSK") == null;
			if (flag3)
			{
				Log.ErrorOnce("HSK SkyAI: Core SK is not found. This will lead to game errors. ", 616233263);
			}
			bool flag4 = ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") == null;
			if (flag4)
			{
				Log.ErrorOnce("HSK SkyAI: Combat Extended is not found. This will lead to game errors.", 616233264);
			}
			Log.Message(string.Format("HSK SkyAI Project loaded :: Finished patching for {0} ms", (DateTime.Now - now).TotalMilliseconds));
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x000253B4 File Offset: 0x000235B4
		public static bool ShouldUpdateModVersion(string version)
		{
			return !SkyAiCore.modVersion.Equals(version);
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x000253DE File Offset: 0x000235DE
		public override void WriteSettings()
		{
			base.WriteSettings();
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x000253E8 File Offset: 0x000235E8
		public static bool SelectedPawnDebug(Pawn pawn)
		{
			Pawn pawn2 = Find.Selector.SingleSelectedThing as Pawn;
			return pawn2 != null && pawn2 == pawn;
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00025420 File Offset: 0x00023620
		public override string SettingsCategory()
		{
			return "SkyAI Mod";
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00025437 File Offset: 0x00023637
		public override void DoSettingsWindowContents(Rect inRect)
		{
			SkyAiCore.Settings.DoWindowContents(inRect);
		}

		// Token: 0x04000109 RID: 265
		public static string modVersion = "1.7";

		// Token: 0x0400010A RID: 266
		public static bool shouldClearData = false;

		// Token: 0x0400010B RID: 267
		public static Settings Settings;
	}
}
