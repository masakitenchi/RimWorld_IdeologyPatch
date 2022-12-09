using RimWorld;
using Verse;

namespace Rimatomics
{
	public class Building_RimatomicsWorkbench : Building_WorkTable
	{
		private Graphic offGraphic;

		public override void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this, 0f);
			if (base.powerComp.PowerOn)
			{
				if (offGraphic == null)
				{
					offGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, def.graphicData.drawSize, DrawColor, DrawColorTwo);
				}
				offGraphic.Print(layer, this, 0f);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			switch (signal)
			{
			case "PowerTurnedOn":
			case "PowerTurnedOff":
			case "FlickedOn":
			case "FlickedOff":
			case "Refueled":
			case "RanOutOfFuel":
			case "ScheduledOn":
			case "ScheduledOff":
				base.Map?.mapDrawer?.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
				break;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			base.powerComp = GetComp<CompPowerTrader>();
		}
	}
}
