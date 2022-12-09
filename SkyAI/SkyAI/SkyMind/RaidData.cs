using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000059 RID: 89
	[StaticConstructorOnStartup]
	public class RaidData : IExposable
	{
		// Token: 0x0600020B RID: 523 RVA: 0x0002D8D4 File Offset: 0x0002BAD4
		public RaidData()
		{
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0002D974 File Offset: 0x0002BB74
		public RaidData(Faction faction, List<Pawn> raidPawns, int raidCount, List<Pawn> raidDoctors, List<Pawn> leaderGuards, Pawn raidLeader, RaidData.RaidStage raidStage, IntVec3 leaderTarget)
		{
			this.raidPawns = raidPawns;
			this.faction = faction;
			this.raidCount = raidCount;
			this.raidDoctors = raidDoctors;
			this.leaderGuards = leaderGuards;
			this.raidLeader = raidLeader;
			this.raidStage = raidStage;
			this.leaderTarget = leaderTarget;
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0002DA50 File Offset: 0x0002BC50
		public void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			bool flag = Scribe.mode == LoadSaveMode.Saving && this.raidPawns.Count<Pawn>() > 0;
			if (flag)
			{
				this.raidPawns.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.raidPawns, "raidPawns", LookMode.Reference, Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.raidCount, "raidCount", 0, false);
			bool flag2 = Scribe.mode == LoadSaveMode.Saving && this.raidDoctors.Count<Pawn>() > 0;
			if (flag2)
			{
				this.raidDoctors.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.raidDoctors, "raidDoctors", LookMode.Reference, Array.Empty<object>());
			bool flag3 = Scribe.mode == LoadSaveMode.Saving && this.leaderGuards.Count<Pawn>() > 0;
			if (flag3)
			{
				this.leaderGuards.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.leaderGuards, "leaderGuards", LookMode.Reference, Array.Empty<object>());
			Scribe_References.Look<Pawn>(ref this.raidLeader, "raidLeader", false);
			Scribe_Values.Look<IntVec3>(ref this.leaderTarget, "leaderTarget", default(IntVec3), false);
			Scribe_Values.Look<RaidData.RaidStage>(ref this.raidStage, "raidStage", RaidData.RaidStage.start, false);
			Scribe_Values.Look<int>(ref this.startAttackingDelay, "startAttackingDelay", 0, false);
			bool flag4 = Scribe.mode == LoadSaveMode.Saving && this.squadCommanders.Count<Pawn>() > 0;
			if (flag4)
			{
				this.squadCommanders.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.squadCommanders, "squadCommanders", LookMode.Reference, Array.Empty<object>());
			Scribe_Collections.Look<IntVec3>(ref this.gatherCells, "gatherCells", LookMode.Undefined, Array.Empty<object>());
			bool flag5 = Scribe.mode == LoadSaveMode.Saving && this.squads.Count<SquadData>() > 0;
			if (flag5)
			{
				this.squads.RemoveAll((SquadData x) => x == null);
			}
			Scribe_Collections.Look<SquadData>(ref this.squads, "squads", LookMode.Deep, Array.Empty<object>());
			bool flag6 = Scribe.mode == LoadSaveMode.Saving && this.exitCells.Count<ExitData>() > 0;
			if (flag6)
			{
				this.exitCells.RemoveAll((ExitData x) => x == null);
			}
			Scribe_Collections.Look<ExitData>(ref this.exitCells, "exitCells", LookMode.Deep, Array.Empty<object>());
			bool flag7 = Scribe.mode == LoadSaveMode.Saving && this.raidOrders.Count<KeyValuePair<Pawn, bool>>() > 0;
			if (flag7)
			{
				this.raidOrders.RemoveAll((KeyValuePair<Pawn, bool> x) => x.Key == null);
			}
			Scribe_Collections.Look<Pawn, bool>(ref this.raidOrders, "raidOrders", LookMode.Reference, LookMode.Value, ref this.reservedRaidOrdersPawnList, ref this.reservedRaidOrdersBoolList);
			bool flag8 = Scribe.mode == LoadSaveMode.Saving && this.squadDefence.Count<KeyValuePair<Lord, bool>>() > 0;
			if (flag8)
			{
				this.squadDefence.RemoveAll((KeyValuePair<Lord, bool> x) => x.Key == null || x.Key.ownedPawns.NullOrEmpty<Pawn>());
			}
			Scribe_Collections.Look<Lord, bool>(ref this.squadDefence, "squadDefence", LookMode.Reference, LookMode.Value, ref this.reservedsquadDefenceLordList, ref this.reservedsquadDefenceBoolList);
			bool flag9 = Scribe.mode == LoadSaveMode.Saving && this.squadDefencePoint.Count<KeyValuePair<Lord, IntVec3>>() > 0;
			if (flag9)
			{
				this.squadDefencePoint.RemoveAll((KeyValuePair<Lord, IntVec3> x) => x.Key == null || x.Key.ownedPawns.NullOrEmpty<Pawn>());
			}
			Scribe_Collections.Look<Lord, IntVec3>(ref this.squadDefencePoint, "squadDefencePoint", LookMode.Reference, LookMode.Value, ref this.reservedsquadDefencePointLordList, ref this.reservedsquadDefencePointIntVecList);
			Scribe_Values.Look<bool>(ref this.raidIsReady, "raidIsReady", false, false);
			Scribe_Values.Look<bool>(ref this.squadsFormed, "squadsFormed", false, false);
		}

		// Token: 0x04000120 RID: 288
		public Faction faction;

		// Token: 0x04000121 RID: 289
		public List<Pawn> raidPawns = new List<Pawn>();

		// Token: 0x04000122 RID: 290
		public int raidCount;

		// Token: 0x04000123 RID: 291
		public List<Pawn> raidDoctors = new List<Pawn>();

		// Token: 0x04000124 RID: 292
		public List<Pawn> leaderGuards = new List<Pawn>();

		// Token: 0x04000125 RID: 293
		public Pawn raidLeader;

		// Token: 0x04000126 RID: 294
		public IntVec3 leaderTarget;

		// Token: 0x04000127 RID: 295
		public RaidData.RaidStage raidStage = RaidData.RaidStage.start;

		// Token: 0x04000128 RID: 296
		public int startAttackingDelay = 3;

		// Token: 0x04000129 RID: 297
		public List<Pawn> squadCommanders = new List<Pawn>();

		// Token: 0x0400012A RID: 298
		public List<SquadData> squads = new List<SquadData>();

		// Token: 0x0400012B RID: 299
		public Dictionary<Pawn, bool> raidOrders = new Dictionary<Pawn, bool>();

		// Token: 0x0400012C RID: 300
		public List<IntVec3> gatherCells = new List<IntVec3>();

		// Token: 0x0400012D RID: 301
		public Dictionary<Lord, bool> squadDefence = new Dictionary<Lord, bool>();

		// Token: 0x0400012E RID: 302
		public Dictionary<Lord, IntVec3> squadDefencePoint = new Dictionary<Lord, IntVec3>();

		// Token: 0x0400012F RID: 303
		public List<ExitData> exitCells = new List<ExitData>();

		// Token: 0x04000130 RID: 304
		public bool raidIsReady = false;

		// Token: 0x04000131 RID: 305
		public bool squadsFormed = false;

		// Token: 0x04000132 RID: 306
		private List<Pawn> reservedRaidOrdersPawnList;

		// Token: 0x04000133 RID: 307
		private List<bool> reservedRaidOrdersBoolList;

		// Token: 0x04000134 RID: 308
		private List<Lord> reservedsquadDefenceLordList;

		// Token: 0x04000135 RID: 309
		private List<bool> reservedsquadDefenceBoolList;

		// Token: 0x04000136 RID: 310
		private List<Lord> reservedsquadDefencePointLordList;

		// Token: 0x04000137 RID: 311
		private List<IntVec3> reservedsquadDefencePointIntVecList;

		// Token: 0x020000F4 RID: 244
		public enum RaidStage
		{
			// Token: 0x040002FE RID: 766
			start,
			// Token: 0x040002FF RID: 767
			siege,
			// Token: 0x04000300 RID: 768
			defending,
			// Token: 0x04000301 RID: 769
			gathering,
			// Token: 0x04000302 RID: 770
			startAttacking,
			// Token: 0x04000303 RID: 771
			attack,
			// Token: 0x04000304 RID: 772
			fleeing
		}
	}
}
