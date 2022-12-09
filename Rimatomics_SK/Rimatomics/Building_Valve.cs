using System.Text;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Building_Valve : Building
	{
		private CompFlickable flickableComp;

		private StringBuilder stringBuilder = new StringBuilder();

		public override Graphic Graphic => flickableComp.CurrentGraphic;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			flickableComp = GetComp<CompFlickable>();
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append(base.GetInspectString());
			if (!FlickUtility.WantsToBeOn(this))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("critValveClosed".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
