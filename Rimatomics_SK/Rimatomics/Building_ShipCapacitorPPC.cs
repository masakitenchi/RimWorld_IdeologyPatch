using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	internal class Building_ShipCapacitorPPC : Building_PPC
	{
		private static Graphic barGraphic;

		public override void Tick()
		{
			base.Tick();
			BarTo = 0f;
		}

		public override void Draw()
		{
			base.Draw();
			Color color;
			if (this.TryGetComp<CompPowerBattery>().StoredEnergyPct < 0.25f)
			{
				color = new Color(0.25f + this.TryGetComp<CompPowerBattery>().StoredEnergyPct * 3f, 0f, 0f);
			}
			else
			{
				float f = (this.TryGetComp<CompPowerBattery>().StoredEnergyPct - 0.25f) * 2f * (float)Math.PI / 3f;
				color = new Color(Mathf.Cos(f), Mathf.Sin(f), 0f);
			}
			if (barGraphic == null)
			{
				barGraphic = GraphicDatabase.Get(typeof(Graphic_Multi), "Things/Building/Ship/CapacitorBar", ShaderDatabase.Cutout, new Vector2(3f, 5f), Color.white, Color.white);
			}
			barGraphic.GetColoredVersion(ShaderDatabase.Cutout, color, color).Draw(new Vector3(DrawPos.x, DrawPos.y + 1f, DrawPos.z), base.Rotation, this);
		}
	}
}
