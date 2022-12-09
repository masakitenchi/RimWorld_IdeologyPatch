using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class CoreB : reactorCore
	{
		public override int SlotCount => 37;

		public override bool IsBreeder => true;

		public override ThingDef PoppedCoreDef => ThingDef.Named("PoppedReactorCoreB");

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

		public CoreB()
		{
			IDlabel = new Vector3(-2.265625f, 1f, 2.765625f);
			base.drawScale = 0.66f;
			base.drawOffset = -0.17f;
			RadLeakIco = new Vector3(-2.671875f, 1f, 2.3125f);
			OverheatIco = new Vector3(-2.328125f, 1f, 2.3125f);
			ledbig = new Vector3(-2.5f, 1f, -2.5f);
			led1 = new Vector3(-2.5f, 1f, -1.859375f);
			led2 = new Vector3(-2.359375f, 1f, -2.109375f);
			led3 = new Vector3(-2.640625f, 1f, -2.109375f);
		}
	}
}
