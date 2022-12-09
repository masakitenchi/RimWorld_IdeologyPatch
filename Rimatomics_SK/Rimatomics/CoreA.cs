using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class CoreA : reactorCore
	{
		public override int SlotCount => 21;

		public override ThingDef PoppedCoreDef => ThingDef.Named("PoppedReactorCoreA");

		public override bool IsBuilt => DubDef.BuildReactorCore.IsFinished;

		public override Graphic Graphic
		{
			get
			{
				if (!IsBuilt)
				{
					DrawFX = false;
					return GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath + "_construct", ShaderDatabase.DefaultShader, def.graphicData.drawSize, Color.white);
				}
				DrawFX = true;
				return base.Graphic;
			}
		}

		public CoreA()
		{
			IDlabel = new Vector3(-1.8125f, 1f, 2.265625f);
			base.drawScale = 0.69f;
			base.drawOffset = -0.12f;
			RadLeakIco = new Vector3(-2.171875f, 1f, 1.8125f);
			OverheatIco = new Vector3(-1.84375f, 1f, 1.8125f);
			ledbig = new Vector3(-2f, 1f, -2f);
			led1 = new Vector3(-2f, 1f, -1.5f);
			led2 = new Vector3(-1.75f, 1f, -1.75f);
			led3 = new Vector3(-1.5f, 1f, -2f);
		}
	}
}
