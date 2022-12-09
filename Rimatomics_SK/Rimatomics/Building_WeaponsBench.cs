using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Building_WeaponsBench : ResearchBuilding
	{
		public CompPowerTrader powerComp;

		public int biff;

		private static List<Graphic> LaserBeams = new List<Graphic>();

		private Effecter effecter;

		public override void Draw()
		{
			base.Draw();
			if (LaserBeams.NullOrEmpty())
			{
				LaserBeams = new List<Graphic>
				{
					GraphicDatabase.Get<Graphic_Multi>(def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, def.graphicData.drawSize, Color.white),
					GraphicDatabase.Get<Graphic_Multi>(def.graphicData.texPath + "_FX1", ShaderDatabase.MoteGlow, def.graphicData.drawSize, Color.white),
					GraphicDatabase.Get<Graphic_Multi>(def.graphicData.texPath + "_FX2", ShaderDatabase.MoteGlow, def.graphicData.drawSize, Color.white)
				};
			}
			if (InUse)
			{
				LaserBeams[biff].Draw(DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (InUse)
			{
				if (effecter == null)
				{
					if (DubDef.RimatomicsEnergyTestEffect == null)
					{
						return;
					}
					effecter = DubDef.RimatomicsEnergyTestEffect.Spawn();
				}
				else
				{
					effecter.EffectTick(this, this);
				}
			}
			else if (effecter != null)
			{
				effecter.Cleanup();
				effecter = null;
			}
			if (this.IsHashIntervalTick(2))
			{
				biff = Rand.Range(0, 3);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
		}
	}
}
