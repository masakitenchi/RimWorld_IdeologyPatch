using Verse;

namespace Rimatomics
{
	internal class SectionLayer_CoolingPipe : SectionLayer_OverlayPipe
	{
		public SectionLayer_CoolingPipe(Section section)
			: base(section)
		{
			mode = PipeType.Cooling;
		}
	}
}
