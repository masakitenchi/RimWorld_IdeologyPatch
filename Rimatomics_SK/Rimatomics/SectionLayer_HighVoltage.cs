using Verse;

namespace Rimatomics
{
	internal class SectionLayer_HighVoltage : SectionLayer_OverlayPipe
	{
		public SectionLayer_HighVoltage(Section section)
			: base(section)
		{
			mode = PipeType.HighVoltage;
		}
	}
}
