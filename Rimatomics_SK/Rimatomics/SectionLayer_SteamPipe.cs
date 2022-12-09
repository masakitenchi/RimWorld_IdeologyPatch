using Verse;

namespace Rimatomics
{
	internal class SectionLayer_SteamPipe : SectionLayer_OverlayPipe
	{
		public SectionLayer_SteamPipe(Section section)
			: base(section)
		{
			mode = PipeType.Steam;
		}
	}
}
