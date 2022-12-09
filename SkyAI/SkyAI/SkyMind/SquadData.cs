using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200000E RID: 14
	public class SquadData : IExposable
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00005C58 File Offset: 0x00003E58
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00005CF7 File Offset: 0x00003EF7
		public Map Map
		{
			get
			{
				bool flag = this.map != null;
				Map currentMap;
				if (flag)
				{
					currentMap = this.map;
				}
				else
				{
					bool flag2 = this.squadCommander != null && this.squadCommander.MapHeld != null;
					if (flag2)
					{
						this.map = this.squadCommander.MapHeld;
					}
					bool flag3 = !this.squadPawns.NullOrEmpty<Pawn>() && this.squadPawns[0].MapHeld != null;
					if (flag3)
					{
						this.map = this.squadPawns[0].MapHeld;
					}
					currentMap = Find.CurrentMap;
				}
				return currentMap;
			}
			set
			{
				this.map = value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00005D04 File Offset: 0x00003F04
		public IntVec3 Start
		{
			get
			{
				bool flag = this.squadCommander != null && AdvancedAI.IsActivePawn(this.squadCommander);
				IntVec3 result;
				if (flag)
				{
					result = this.squadCommander.PositionHeld;
				}
				else
				{
					bool flag2 = !this.squadPawns.NullOrEmpty<Pawn>();
					if (flag2)
					{
						IntVec3 positionHeld = this.squadPawns[0].PositionHeld;
						bool flag3 = this.center.IsValid && positionHeld.DistanceTo(this.center) <= 25f;
						if (flag3)
						{
							result = this.center;
						}
						else
						{
							result = positionHeld;
						}
					}
					else
					{
						result = IntVec3.Invalid;
					}
				}
				return result;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00005DA4 File Offset: 0x00003FA4
		public IntVec3 Destination
		{
			get
			{
				bool flag = !this.Start.IsValid;
				IntVec3 result;
				if (flag)
				{
					result = IntVec3.Invalid;
				}
				else
				{
					result = GenAI.RandomRaidDest(this.Start, this.Map);
				}
				return result;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00005DE8 File Offset: 0x00003FE8
		public SquadAttackData Data
		{
			get
			{
				bool flag = this.data == null;
				if (flag)
				{
					this.data = new SquadAttackData(this.Map, this);
				}
				return this.data;
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00005E24 File Offset: 0x00004024
		public List<Thing> FindBuildingsToAttack(Faction searcherFaction, int maxCount)
		{
			bool flag = !this.buildingAttackList.NullOrEmpty<Thing>();
			if (flag)
			{
				for (int i = this.buildingAttackList.Count<Thing>() - 1; i >= 0; i--)
				{
					Building building = (Building)this.buildingAttackList.ElementAt(i);
					bool flag2 = building == null || !building.Spawned || building.Destroyed;
					if (flag2)
					{
						this.shouldUpdateBuildingAttackList = true;
						this.data.squadAttackGrid.Notify_BuildingStateChanged(building);
					}
				}
			}
			else
			{
				this.shouldUpdateBuildingAttackList = true;
			}
			bool flag3 = this.shouldUpdateBuildingAttackList;
			if (flag3)
			{
				this.buildingAttackList = this.FindBuildingTargets(searcherFaction, maxCount);
			}
			return this.buildingAttackList;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00005EE4 File Offset: 0x000040E4
		public List<Thing> FindBuildingTargets(Faction searcherFaction, int maxCount)
		{
			bool flag = this.Data != null;
			List<Thing> result;
			if (flag)
			{
				this.shouldUpdateBuildingAttackList = false;
				result = this.Data.squadAttackGrid.FindBuildingTargets(this.Data, searcherFaction, maxCount);
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00005F27 File Offset: 0x00004127
		public SquadData()
		{
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00005F64 File Offset: 0x00004164
		public SquadData(List<Pawn> squadPawns)
		{
			this.squadPawns = squadPawns;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00005FB4 File Offset: 0x000041B4
		public RaidData PawnRaidData(Pawn squadPawn)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(squadPawn);
			bool flag = mapComponent_SkyAI != null && !mapComponent_SkyAI.raidData.NullOrEmpty<RaidData>();
			if (flag)
			{
				foreach (RaidData raidData in mapComponent_SkyAI.raidData)
				{
					bool flag2 = raidData != null && !raidData.squads.NullOrEmpty<SquadData>();
					if (flag2)
					{
						foreach (SquadData squadData in from sq in raidData.squads
						where !sq.squadPawns.NullOrEmpty<Pawn>()
						select sq)
						{
							bool flag3 = squadData.squadPawns.Contains(squadPawn);
							if (flag3)
							{
								return raidData;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000060D0 File Offset: 0x000042D0
		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.id, "id", 0, false);
			Scribe_Values.Look<string>(ref this.squadName, "squadName", null, false);
			Scribe_References.Look<Pawn>(ref this.squadCommander, "squadCommander", false);
			Scribe_Values.Look<IntVec3>(ref this.gatherSpot, "gatherSpot", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.squadTarget, "squadTarget", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.center, "center", default(IntVec3), false);
			Scribe_Values.Look<bool>(ref this.squadEnteredSiegeCombat, "squadEnteredSiegeCombat", false, false);
			Scribe_Values.Look<bool>(ref this.isReserved, "isReserved", false, false);
			Scribe_Values.Look<bool>(ref this.isReady, "isReady", false, false);
			bool flag = Scribe.mode == LoadSaveMode.Saving && this.squadPawns.Count > 0;
			if (flag)
			{
				this.squadPawns.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.squadPawns, "squadPawns", LookMode.Reference, Array.Empty<object>());
			bool flag2 = Scribe.mode == LoadSaveMode.Saving && this.buildingAttackList.Count > 0;
			if (flag2)
			{
				this.buildingAttackList.RemoveAll((Thing x) => x == null);
			}
			Scribe_Collections.Look<Thing>(ref this.buildingAttackList, "buildingAttackList", LookMode.Reference, Array.Empty<object>());
		}

		// Token: 0x0400001D RID: 29
		public int id;

		// Token: 0x0400001E RID: 30
		public string squadName;

		// Token: 0x0400001F RID: 31
		public Pawn squadCommander;

		// Token: 0x04000020 RID: 32
		public IntVec3 gatherSpot;

		// Token: 0x04000021 RID: 33
		public IntVec3 squadTarget;

		// Token: 0x04000022 RID: 34
		public IntVec3 center;

		// Token: 0x04000023 RID: 35
		public bool isReserved = false;

		// Token: 0x04000024 RID: 36
		public bool isReady = false;

		// Token: 0x04000025 RID: 37
		public bool squadEnteredSiegeCombat = false;

		// Token: 0x04000026 RID: 38
		public List<Pawn> squadPawns = new List<Pawn>();

		// Token: 0x04000027 RID: 39
		public List<Thing> buildingAttackList = new List<Thing>();

		// Token: 0x04000028 RID: 40
		public bool shouldUpdateBuildingAttackList = false;

		// Token: 0x04000029 RID: 41
		public Map map;

		// Token: 0x0400002A RID: 42
		public SquadAttackData data;
	}
}
