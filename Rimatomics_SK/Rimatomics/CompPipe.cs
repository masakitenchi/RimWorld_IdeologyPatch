using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class CompPipe : ThingComp
	{
		public CompFlickable flicker;

		public MapComponent_Rimatomics MapComp;

		public BasePipeNet pipeNetRef;

		private Graphic overlay;

		public BasePipeNet net
		{
			get
			{
				if (pipeNetRef == null)
				{
					MapComp.RegenGrids();
				}
				return pipeNetRef;
			}
		}

		public bool closed
		{
			get
			{
				if (flicker == null || flicker.SwitchIsOn)
				{
					return false;
				}
				return true;
			}
		}

		public int GridID { get; set; } = -1;


		public PipeType mode => Props.mode;

		public CompProperties_Pipe Props => (CompProperties_Pipe)props;

		public override string CompInspectStringExtra()
		{
			if (DebugSettings.godMode)
			{
				return mode.ToString() + "_ID:" + GridID;
			}
			return null;
		}

		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "FlickedOn" || signal == "FlickedOff")
			{
				MapComp.DirtyPipeGrid(mode);
				MapComp.RegenGrids();
			}
		}

		public override void PostDeSpawn(Map map)
		{
			MapComp.DeregisterPipe(this);
			net.DeregisterPipe(parent);
			base.PostDeSpawn(map);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (parent is Building_Valve)
			{
				flicker = parent.GetComp<CompFlickable>();
			}
			MapComp = parent.Map.Rimatomics();
			foreach (IntVec3 item in parent.OccupiedRect())
			{
				foreach (Thing item2 in item.GetThingList(parent.Map).ToList())
				{
					if (item2 != parent && item2.TryGetComp<CompPipe>() != null && item2.TryGetComp<CompPipe>().mode == mode)
					{
						item2.Destroy();
					}
				}
			}
			MapComp.RegisterPipe(this, respawningAfterLoad);
			overlay = GraphicsCache.pipeOverlayDick[(int)mode];
			base.PostSpawnSetup(respawningAfterLoad);
		}

		public void PrintForGrid(SectionLayer layer)
		{
			if (!closed)
			{
				overlay.Print(layer, parent, 0f);
			}
		}
	}
}
