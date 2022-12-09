using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class CoreC : reactorCore
	{
		public override int SlotCount => 89;

		public override bool IsBreeder => true;

		public override ThingDef PoppedCoreDef => ThingDef.Named("PoppedReactorCoreC");

		public override bool IsBuilt => DubDef.AdvReactor3.IsFinished;

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

		public CoreC()
		{
			IDlabel = new Vector3(-3.75f, 1f, 4.265625f);
			base.drawScale = 0.68f;
			base.drawOffset = -0.2f;
			RadLeakIco = new Vector3(-4.15625f, 1f, 3.8125f);
			OverheatIco = new Vector3(-3.828125f, 1f, 3.8125f);
			ledbig = new Vector3(-4.109375f, 1f, 3.203125f);
			led1 = new Vector3(-3.703125f, 1f, 3.359375f);
			led2 = new Vector3(-3.453125f, 1f, 3.203125f);
			led3 = new Vector3(-3.703125f, 1f, 3.015625f);
		}
	}
}
