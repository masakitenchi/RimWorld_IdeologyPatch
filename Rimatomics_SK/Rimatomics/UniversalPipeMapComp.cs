using System;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using Verse;

namespace Rimatomics
{
	public abstract class UniversalPipeMapComp : MapComponent
	{
		public List<CompPipe> cachedPipes = new List<CompPipe>();

		public bool[] DirtyPipe;

		public int masterID = 1;

		public int[,] PipeGrid;

		public BasePipeNet[] PipeNets = new List<BasePipeNet>().ToArray();

		protected UniversalPipeMapComp(Map map)
			: base(map)
		{
			int length = Enum.GetValues(typeof(PipeType)).Length;
			PipeGrid = new int[length, map.cellIndices.NumGridCells];
			DirtyPipe = new bool[length];
			for (int i = 0; i < DirtyPipe.Length; i++)
			{
				DirtyPipe[i] = true;
			}
		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();
			int num = PipeNets.Length;
			for (int i = 0; i < num; i++)
			{
				PipeNets[i].Tick();
			}
		}

		public override void FinalizeInit()
		{
			base.FinalizeInit();
			RegenGrids();
		}

		public override void MapGenerated()
		{
			base.MapGenerated();
			RegenGrids();
		}

		public void RegenGrids()
		{
			for (int i = 0; i < DirtyPipe.Length; i++)
			{
				if (DirtyPipe[i])
				{
					RebuildPipeGrid(i);
				}
			}
		}

		public bool ZoneAt(IntVec3 pos, PipeType P)
		{
			return PipeGrid[(int)P, map.cellIndices.CellToIndex(pos)] > 0;
		}

		public void RegisterPipe(CompPipe pipe, bool respawningAfterLoad)
		{
			if (!cachedPipes.Contains(pipe))
			{
				cachedPipes.Add(pipe);
			}
			DirtyPipeGrid(pipe.mode);
			if (!respawningAfterLoad && MP.IsInMultiplayer)
			{
				RegenGrids();
			}
		}

		public void DeregisterPipe(CompPipe pipe)
		{
			pipe.net.PipedThings.Remove(pipe.parent);
			if (cachedPipes.Contains(pipe))
			{
				cachedPipes.Remove(pipe);
			}
			foreach (IntVec3 item in pipe.parent.OccupiedRect())
			{
				PipeGrid[(int)pipe.Props.mode, map.cellIndices.CellToIndex(item)] = -1;
			}
			DirtyPipeGrid(pipe.mode);
			if (MP.IsInMultiplayer)
			{
				RegenGrids();
			}
		}

		public void DirtyPipeGrid(PipeType p)
		{
			DirtyPipe[(int)p] = true;
		}

		public void DirtyAllPipeGrids()
		{
			for (int i = 0; i < DirtyPipe.Length; i++)
			{
				DirtyPipe[i] = true;
			}
		}

		public void RebuildPipeGrid(int P)
		{
			DirtyPipe[P] = false;
			for (int i = 0; i < PipeGrid.GetLength(1); i++)
			{
				PipeGrid[P, i] = -1;
			}
			List<BasePipeNet> list = PipeNets.Where((BasePipeNet x) => x.NetType != P).ToList();
			List<CompPipe> list2 = cachedPipes.Where((CompPipe x) => x.mode == (PipeType)P).ToList();
			foreach (CompPipe item in list2)
			{
				item.GridID = -1;
			}
			Dictionary<IntVec3, CompPipe> pipeDic = new Dictionary<IntVec3, CompPipe>();
			foreach (CompPipe item2 in list2.Where((CompPipe x) => !x.closed))
			{
				foreach (IntVec3 item3 in item2.parent.OccupiedRect())
				{
					try
					{
						pipeDic.Add(item3, item2);
					}
					catch (Exception ex)
					{
						Log.Error($"More than 1 pipe comp in the same cell at {item3}, things with pipes cannot overlap!\n" + ex);
					}
				}
			}
			CompPipe compPipe = pipeDic.Values.FirstOrDefault((CompPipe k) => k.mode == (PipeType)P && !k.closed && k.GridID == -1);
			while (compPipe != null)
			{
				BasePipeNet newNet = null;
				if (P == 3)
				{
					newNet = new ColdWaterNet();
				}
				if (P == 2)
				{
					newNet = new SteamNet();
				}
				if (P == 1)
				{
					newNet = new CoolingNet();
				}
				if (P == 0)
				{
					newNet = new HighVoltageNet();
				}
				if (P == 4)
				{
					newNet = new LoomNet();
				}
				newNet.MapComp = map.Rimatomics();
				newNet.NetID = masterID;
				newNet.NetType = P;
				list.Add(newNet);
				map.floodFiller.FloodFill(compPipe.parent.Position, (Predicate<IntVec3>)PassCheck, (Action<IntVec3>)Processor, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
				masterID++;
				compPipe = pipeDic.Values.FirstOrDefault((CompPipe k) => k.mode == (PipeType)P && !k.closed && k.GridID == -1);
				bool PassCheck(IntVec3 c)
				{
					CompPipe compPipe2 = pipeDic.TryGetValue(c);
					if (compPipe2 != null)
					{
						compPipe2.GridID = masterID;
						compPipe2.pipeNetRef = newNet;
						if (!newNet.PipedThings.Contains(compPipe2.parent))
						{
							newNet.PipedThings.Add(compPipe2.parent);
						}
						PipeGrid[P, map.cellIndices.CellToIndex(c)] = masterID;
						return true;
					}
					return false;
				}
			}
			PipeNets = list.ToArray();
			for (int j = 0; j < PipeNets.Length; j++)
			{
				PipeNets[j].InitNet();
			}
			DubUtils.GetResearch().CheckAllStepsAllBenches();
			static void Processor(IntVec3 c)
			{
			}
		}
	}
}
