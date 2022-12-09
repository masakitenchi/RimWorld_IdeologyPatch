using Verse;

namespace Rimatomics
{
	internal class SectionLayer_Loom : SectionLayer_OverlayPipe
	{
		public SectionLayer_Loom(Section section)
			: base(section)
		{
			mode = PipeType.Loom;
		}
	}
}
