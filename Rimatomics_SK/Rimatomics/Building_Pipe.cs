using Verse;

namespace Rimatomics
{
	public class Building_Pipe : Building
	{
		public CompPipe pipe;

		private Graphic_LinkedPipe cached;

		public Graphic_LinkedPipe Graphicc
		{
			get
			{
				if (cached == null)
				{
					if (pipe.Props.stuffed)
					{
						cached = GraphicsCache.pipeDick[(int)pipe.mode].GetColoredVersion(GraphicsCache.pipeDick[(int)pipe.mode].Shader, DrawColor, DrawColorTwo) as Graphic_LinkedPipe;
					}
					else
					{
						cached = GraphicsCache.pipeDick[(int)pipe.mode];
					}
				}
				return cached;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			pipe = GetComp<CompPipe>();
		}

		public void PrintForGrid(SectionLayer layer)
		{
			Graphicc.PrintForGrid(layer, this);
		}

		public override void Print(SectionLayer layer)
		{
		}
	}
}
