using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class CoolingTower : CoolingSystem
	{
		private int diddlebit;

		private int sticklebrick = Rand.Range(0, 120);

		private StringBuilder stringBuilder = new StringBuilder();

		public override float coolingCapacity
		{
			get
			{
				float num = 1f;
				if (fuel != null)
				{
					num = fuel.FuelPercentOfMax;
				}
				if (powerComp == null || powerComp.PowerOn)
				{
					if (Stalled)
					{
						return 25000f * num;
					}
					return 250000f * num;
				}
				return 0f;
			}
		}

		public override float WaterUsage => 1f;

		public override void Tick()
		{
			base.Tick();
			if (base.CoolingNet.CoolingLoopRatio > 0f)
			{
				if (sticklebrick == 0)
				{
					sticklebrick = 120;
					FleckDef fleckDef;
					if (diddlebit == 0)
					{
						diddlebit++;
						fleckDef = DubDef.Mote_CoolingTowerSteamB;
					}
					else if (diddlebit == 1)
					{
						diddlebit++;
						fleckDef = DubDef.Mote_CoolingTowerSteamA;
					}
					else
					{
						diddlebit = 0;
						fleckDef = DubDef.Mote_CoolingTowerSteamC;
					}
					FleckCreationData dataStatic = FleckMaker.GetDataStatic(DrawPos, base.Map, fleckDef);
					dataStatic.exactScale = new Vector3(def.graphicData.drawSize.x, 1f, def.graphicData.drawSize.y);
					base.Map.flecks.CreateFleck(dataStatic);
				}
				sticklebrick--;
				if (this.IsHashIntervalTick(50))
				{
					FleckMaker.ThrowSmoke(this.TrueCenter() + new Vector3(0f, 0f, 2f), base.Map, 3f);
				}
			}
			if (this.IsHashIntervalTick(250))
			{
				Stalled = this.OccupiedRect().Cells.Any((IntVec3 x) => x.Roofed(base.Map));
			}
		}

		public override string GetInspectString()
		{
			stringBuilder.Clear();
			stringBuilder.Append(base.GetInspectString());
			if (Stalled)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("RoofedTower".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
