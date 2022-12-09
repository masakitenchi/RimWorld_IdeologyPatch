using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class Building_ResearchReactor : ResearchBuilding
	{
		public float inout;

		public CompPowerTrader powerComp;

		private Material researchReactorGlow;

		public float Target = 1f;

		public override void Draw()
		{
			base.Draw();
			if (researchReactorGlow == null)
			{
				researchReactorGlow = MaterialPool.MatFrom(def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow);
			}
			if (inout > 0f)
			{
				DubUtils.drawLEDfade(DrawPos + new Vector3(0f, 1f, 0f), new Vector3(5f, 1f, 5f), base.Rotation.AsQuat, researchReactorGlow, inout);
			}
			if (powerComp.PowerOn)
			{
				DubUtils.drawLED(DrawPos + new Vector3(-1.5f, 1f, -1.5f), base.Rotation.AsQuat, GraphicsCache.BigBlueLed);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (InUse)
			{
				if (this.IsHashIntervalTick(1000))
				{
					Target = Rand.Value;
				}
			}
			else
			{
				Target = 0f;
			}
			if (powerComp != null && powerComp.PowerOn)
			{
				if (inout < Target)
				{
					inout += 0.001f;
				}
				else if (inout > 0f)
				{
					inout -= 0.001f;
				}
			}
			else if (inout > 0f)
			{
				inout -= 0.001f;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
		}
	}
}
