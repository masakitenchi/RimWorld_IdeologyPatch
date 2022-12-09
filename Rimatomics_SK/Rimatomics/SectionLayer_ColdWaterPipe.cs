using Verse;

namespace Rimatomics
{
	internal class SectionLayer_ColdWaterPipe : SectionLayer_OverlayPipe
	{
		public SectionLayer_ColdWaterPipe(Section section)
			: base(section)
		{
			mode = PipeType.ColdWater;
		}
	}
}
