using System;
using Verse;

namespace Rimatomics
{
	public class CompProperties_Pipe : CompProperties
	{
		public PipeType mode;

		public Type PipeNetClass = typeof(BasePipeNet);

		public bool stuffed;

		public CompProperties_Pipe()
		{
			compClass = typeof(CompPipe);
		}
	}
}
