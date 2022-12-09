using System.Xml;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class RimatomicsMod : Mod
	{
		public static class H_DefFromNode
		{
			public static bool Prefix(XmlNode node, ref Def __result)
			{
				if (node.NodeType != XmlNodeType.Element)
				{
					return true;
				}
				if (node.Attributes?["RimatomicsDLC"] != null)
				{
					__result = null;
					return false;
				}
				return true;
			}
		}

		public const int StartDate = 6266;

		public static Settings Settings;

		public static string Version;

		public RimatomicsMod(ModContentPack content)
			: base(content)
		{
			Settings = GetSettings<Settings>();
			string text = "Rimatomics 1.7.2500";
			Log.Message(text);
			_ = text != Settings.LastVersion;
			if (!ModsConfig.RoyaltyActive)
			{
				new Harmony("Dubwise.Rimatomics").Patch(AccessTools.Method(typeof(DirectXmlLoader), "DefFromNode"), new HarmonyMethod(typeof(H_DefFromNode), "Prefix"));
			}
			Settings.LastVersion = text;
			Settings.Write();
		}

		public override string SettingsCategory()
		{
			return "Dubs Rimatomics";
		}

		public override void DoSettingsWindowContents(Rect canvas)
		{
			Settings.DoWindowContents(canvas);
		}
	}
}
