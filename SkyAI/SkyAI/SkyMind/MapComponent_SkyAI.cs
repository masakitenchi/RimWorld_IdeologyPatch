using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using SK;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200005C RID: 92
	[StaticConstructorOnStartup]
	public class MapComponent_SkyAI : MapComponent
	{
		// Token: 0x060002DB RID: 731 RVA: 0x0003B0D0 File Offset: 0x000392D0
		public MapComponent_SkyAI(Map map) : base(map)
		{
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0003B19C File Offset: 0x0003939C
		public override void FinalizeInit()
		{
			base.FinalizeInit();
			bool flag = this.lords == null;
			if (flag)
			{
				this.lords = new List<Lord>();
			}
			bool flag2 = this.raidData == null;
			if (flag2)
			{
				this.raidData = new List<RaidData>();
			}
			bool flag3 = this.focusCells == null;
			if (flag3)
			{
				this.focusCells = new Dictionary<Pawn, IntVec3>();
			}
			bool flag4 = this.coverJobs == null;
			if (flag4)
			{
				this.coverJobs = new Dictionary<Pawn, IntVec3>();
			}
			bool flag5 = this.lordtoils == null;
			if (flag5)
			{
				this.lordtoils = new Dictionary<Pawn, LordToil>();
			}
			bool flag6 = this.destroyersExclusions == null;
			if (flag6)
			{
				this.destroyersExclusions = new List<Pawn>();
			}
			bool flag7 = this.mainBlowCells == null;
			if (flag7)
			{
				this.mainBlowCells = new List<IntVec3>();
			}
			bool flag8 = this.activeCover == null;
			if (flag8)
			{
				this.activeCover = new Dictionary<Pawn, Thing>();
			}
			bool flag9 = this.dangerousCells == null;
			if (flag9)
			{
				this.dangerousCells = new List<IntVec3>();
			}
			bool flag10 = this.pawnThings == null;
			if (flag10)
			{
				this.pawnThings = new List<PawnThingsOwner>();
			}
			bool flag11 = this.lostThings == null;
			if (flag11)
			{
				this.lostThings = new Dictionary<ThingCountClass, Faction>();
			}
			bool flag12 = this.exitCounter == null;
			if (flag12)
			{
				this.exitCounter = new Dictionary<Pawn, int>();
			}
			bool flag13 = this.boughtThings == null;
			if (flag13)
			{
				this.boughtThings = new Dictionary<Thing, Faction>();
			}
			MapComponent_SkyAI.GetMapPawnsNumbers(this.map, out this.mapPawnCount, out this.mapRangedPawnCount);
			foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
			{
				bool flag14 = pawn != null;
				if (flag14)
				{
					bool flag15 = pawn.health != null;
					if (flag15)
					{
						pawn.health.hediffSet.hediffs.RemoveAll((Hediff h) => h.def.HasModExtension<RaidLeaderAuraExtension>() || h.def.HasModExtension<RaidLeaderExtension>());
					}
					CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
					bool flag16 = compDoctorRole != null;
					if (flag16)
					{
						pawn.AllComps.Remove(compDoctorRole);
					}
					CompLeaderRole compLeaderRole = pawn.TryGetComp<CompLeaderRole>();
					bool flag17 = compLeaderRole != null;
					if (flag17)
					{
						pawn.AllComps.Remove(compLeaderRole);
					}
					CompSquadCommanderRole compSquadCommanderRole = pawn.TryGetComp<CompSquadCommanderRole>();
					bool flag18 = compSquadCommanderRole != null;
					if (flag18)
					{
						pawn.AllComps.Remove(compSquadCommanderRole);
					}
					CompGuardRole compGuardRole = pawn.TryGetComp<CompGuardRole>();
					bool flag19 = compGuardRole != null;
					if (flag19)
					{
						pawn.AllComps.Remove(compGuardRole);
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				bool flag20 = map != null;
				if (flag20)
				{
					foreach (Corpse corpse in map.listerThings.AllThings.OfType<Corpse>())
					{
						Pawn innerPawn = corpse.InnerPawn;
						bool flag21 = innerPawn != null;
						if (flag21)
						{
							bool flag22 = innerPawn.health != null;
							if (flag22)
							{
								innerPawn.health.hediffSet.hediffs.RemoveAll((Hediff h) => h.def.HasModExtension<RaidLeaderAuraExtension>() || h.def.HasModExtension<RaidLeaderExtension>());
							}
							CompDoctorRole compDoctorRole2 = innerPawn.TryGetComp<CompDoctorRole>();
							bool flag23 = compDoctorRole2 != null;
							if (flag23)
							{
								innerPawn.AllComps.Remove(compDoctorRole2);
							}
							CompLeaderRole compLeaderRole2 = innerPawn.TryGetComp<CompLeaderRole>();
							bool flag24 = compLeaderRole2 != null;
							if (flag24)
							{
								innerPawn.AllComps.Remove(compLeaderRole2);
							}
							CompSquadCommanderRole compSquadCommanderRole2 = innerPawn.TryGetComp<CompSquadCommanderRole>();
							bool flag25 = compSquadCommanderRole2 != null;
							if (flag25)
							{
								innerPawn.AllComps.Remove(compSquadCommanderRole2);
							}
							CompGuardRole compGuardRole2 = innerPawn.TryGetComp<CompGuardRole>();
							bool flag26 = compGuardRole2 != null;
							if (flag26)
							{
								innerPawn.AllComps.Remove(compGuardRole2);
							}
						}
					}
					foreach (Pawn pawn2 in map.mapPawns.AllPawns)
					{
						bool flag27 = pawn2 != null;
						if (flag27)
						{
							bool flag28 = pawn2.health != null;
							if (flag28)
							{
								pawn2.health.hediffSet.hediffs.RemoveAll((Hediff h) => h.def.HasModExtension<RaidLeaderAuraExtension>() || h.def.HasModExtension<RaidLeaderExtension>());
							}
							bool flag29 = !this.raidData.NullOrEmpty<RaidData>();
							if (flag29)
							{
								foreach (RaidData raidData in this.raidData)
								{
									bool flag30 = raidData.raidDoctors == null || (raidData.raidDoctors != null && !raidData.raidDoctors.Contains(pawn2));
									if (flag30)
									{
										CompDoctorRole compDoctorRole3 = pawn2.TryGetComp<CompDoctorRole>();
										bool flag31 = compDoctorRole3 != null;
										if (flag31)
										{
											pawn2.AllComps.Remove(compDoctorRole3);
										}
									}
									bool flag32 = raidData.raidLeader == null || (raidData.raidLeader != null && raidData.raidLeader != pawn2);
									if (flag32)
									{
										CompLeaderRole compLeaderRole3 = pawn2.TryGetComp<CompLeaderRole>();
										bool flag33 = compLeaderRole3 != null;
										if (flag33)
										{
											pawn2.AllComps.Remove(compLeaderRole3);
										}
									}
									bool flag34 = raidData.squadCommanders == null || (raidData.squadCommanders != null && (!raidData.squadCommanders.Contains(pawn2) || pawn2 == raidData.raidLeader));
									if (flag34)
									{
										CompSquadCommanderRole compSquadCommanderRole3 = pawn2.TryGetComp<CompSquadCommanderRole>();
										bool flag35 = compSquadCommanderRole3 != null;
										if (flag35)
										{
											pawn2.AllComps.Remove(compSquadCommanderRole3);
										}
									}
									bool flag36 = raidData.leaderGuards == null || (raidData.leaderGuards != null && !raidData.leaderGuards.Contains(pawn2));
									if (flag36)
									{
										CompGuardRole compGuardRole3 = pawn2.TryGetComp<CompGuardRole>();
										bool flag37 = compGuardRole3 != null;
										if (flag37)
										{
											pawn2.AllComps.Remove(compGuardRole3);
										}
									}
								}
							}
							else
							{
								CompDoctorRole compDoctorRole4 = pawn2.TryGetComp<CompDoctorRole>();
								bool flag38 = compDoctorRole4 != null;
								if (flag38)
								{
									pawn2.AllComps.Remove(compDoctorRole4);
								}
								CompLeaderRole compLeaderRole4 = pawn2.TryGetComp<CompLeaderRole>();
								bool flag39 = compLeaderRole4 != null;
								if (flag39)
								{
									pawn2.AllComps.Remove(compLeaderRole4);
								}
								CompSquadCommanderRole compSquadCommanderRole4 = pawn2.TryGetComp<CompSquadCommanderRole>();
								bool flag40 = compSquadCommanderRole4 != null;
								if (flag40)
								{
									pawn2.AllComps.Remove(compSquadCommanderRole4);
								}
								CompGuardRole compGuardRole4 = pawn2.TryGetComp<CompGuardRole>();
								bool flag41 = compGuardRole4 != null;
								if (flag41)
								{
									pawn2.AllComps.Remove(compGuardRole4);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0003B944 File Offset: 0x00039B44
		public void ClearData()
		{
			this.lords = new List<Lord>();
			this.raidData = new List<RaidData>();
			this.focusCells = new Dictionary<Pawn, IntVec3>();
			this.coverJobs = new Dictionary<Pawn, IntVec3>();
			this.lordtoils = new Dictionary<Pawn, LordToil>();
			this.destroyersExclusions = new List<Pawn>();
			this.mainBlowCells = new List<IntVec3>();
			this.activeCover = new Dictionary<Pawn, Thing>();
			this.dangerousCells = new List<IntVec3>();
			this.pawnThings = new List<PawnThingsOwner>();
			this.lostThings = new Dictionary<ThingCountClass, Faction>();
			this.exitCounter = new Dictionary<Pawn, int>();
			this.boughtThings = new Dictionary<Thing, Faction>();
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0003B9E4 File Offset: 0x00039BE4
		public static void ClearEnemyMapPawns()
		{
			Map map = Find.CurrentMap;
			bool flag = map != null;
			if (flag)
			{
				List<Pawn> list = (from p in map.mapPawns.AllPawns
				where p.RaceProps.intelligence >= Intelligence.Humanlike && !p.RaceProps.Animal && !map.fogGrid.IsFogged(p.Position) && p.Faction != null && !p.IsPrisoner && !p.IsSlave && p.Faction != Faction.OfPlayer
				select p).ToList<Pawn>();
				bool flag2 = !list.EnumerableNullOrEmpty<Pawn>();
				if (flag2)
				{
					int num = list.Count<Pawn>() - 1;
					while (num >= 0 && !list.NullOrEmpty<Pawn>())
					{
						list[num].Destroy(DestroyMode.Vanish);
						num--;
					}
				}
			}
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0003BA84 File Offset: 0x00039C84
		public override void ExposeData()
		{
			base.ExposeData();
			bool flag = Scribe.mode == LoadSaveMode.Saving && this.saveGameModVersion == "";
			if (flag)
			{
				this.saveGameModVersion = SkyAiCore.modVersion;
			}
			bool flag2 = Scribe.mode == LoadSaveMode.PostLoadInit && SkyAiCore.shouldClearData;
			if (flag2)
			{
				this.ClearData();
				this.shouldClearMapPawns = true;
				SkyAiCore.shouldClearData = false;
				Log.Message("HSK SkyAI: settings modVersion was outdated. Cleared data. Updated version.");
			}
			bool flag3 = Scribe.mode == LoadSaveMode.PostLoadInit && SkyAiCore.ShouldUpdateModVersion(this.saveGameModVersion);
			if (flag3)
			{
				this.ClearData();
				this.shouldClearMapPawns = true;
				this.saveGameModVersion = SkyAiCore.modVersion;
				Log.Message("HSK SkyAI: savegame modVersion was outdated. Cleared savegame data. Updated version.");
			}
			Scribe_Values.Look<string>(ref this.saveGameModVersion, "saveGameModVersion", "", true);
			bool flag4 = Scribe.mode == LoadSaveMode.Saving && this.raidData.Count<RaidData>() > 0;
			if (flag4)
			{
				this.raidData.RemoveAll((RaidData x) => x == null || x.raidPawns.NullOrEmpty<Pawn>());
			}
			Scribe_Collections.Look<RaidData>(ref this.raidData, "raidData", LookMode.Deep, Array.Empty<object>());
			bool flag5 = Scribe.mode == LoadSaveMode.PostLoadInit;
			if (flag5)
			{
				bool debugRaidData = SkyAiCore.Settings.debugRaidData;
				if (debugRaidData)
				{
					bool flag6 = this.raidData.NullOrEmpty<RaidData>();
					if (flag6)
					{
						Log.Message("MapComponent: ExposeData. List raidData NullOrEmpty.");
					}
					bool flag7 = this.raidData != null && !this.raidData.NullOrEmpty<RaidData>();
					if (flag7)
					{
						foreach (RaidData raidData in this.raidData)
						{
							bool flag8 = raidData != null;
							if (flag8)
							{
								Log.Message(string.Format("MapComponent: ExposeData. Saved raidData. leader: {0} pawns: {1} doctors: {2} faction: {3} of def: {4} leaderTarget: {5} raidStage: {6}", new object[]
								{
									raidData.raidLeader,
									raidData.raidPawns.Count<Pawn>(),
									raidData.raidDoctors.Count<Pawn>(),
									raidData.faction,
									raidData.faction.def,
									raidData.leaderTarget,
									raidData.raidStage
								}));
							}
							else
							{
								Log.Message("MapComponent: ExposeData. raidData null.");
							}
						}
					}
				}
			}
			bool flag9 = Scribe.mode == LoadSaveMode.Saving && this.focusCells.Count<KeyValuePair<Pawn, IntVec3>>() > 0;
			if (flag9)
			{
				this.focusCells.RemoveAll((KeyValuePair<Pawn, IntVec3> x) => x.Key == null);
			}
			Scribe_Collections.Look<Pawn, IntVec3>(ref this.focusCells, "focusCells", LookMode.Reference, LookMode.Undefined, ref this.reservedFocusCellPawnList, ref this.reservedFocusCellIntVecList);
			bool flag10 = Scribe.mode == LoadSaveMode.Saving && this.coverJobs.Count<KeyValuePair<Pawn, IntVec3>>() > 0;
			if (flag10)
			{
				this.coverJobs.RemoveAll((KeyValuePair<Pawn, IntVec3> x) => x.Key == null);
			}
			Scribe_Collections.Look<Pawn, IntVec3>(ref this.coverJobs, "coverJobs", LookMode.Reference, LookMode.Undefined, ref this.reservedCoverJobPawnList, ref this.reservedCoverJobIntVecList);
			bool flag11 = Scribe.mode == LoadSaveMode.Saving && this.destroyersExclusions.Count<Pawn>() > 0;
			if (flag11)
			{
				this.destroyersExclusions.RemoveAll((Pawn x) => x == null);
			}
			Scribe_Collections.Look<Pawn>(ref this.destroyersExclusions, "destroyersExclusions", LookMode.Deep, Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.mainBlowDelay, "mainBlowDelay", 0, false);
			Scribe_Collections.Look<IntVec3>(ref this.mainBlowCells, "mainBlow", LookMode.Undefined, Array.Empty<object>());
			bool flag12 = Scribe.mode == LoadSaveMode.Saving && this.lordtoils.Count<KeyValuePair<Pawn, LordToil>>() > 0;
			if (flag12)
			{
				this.lordtoils.RemoveAll((KeyValuePair<Pawn, LordToil> x) => x.Key == null);
			}
			Scribe_Collections.Look<Pawn, LordToil>(ref this.lordtoils, "lordtoils", LookMode.Reference, LookMode.Value, ref this.reservedLordtoilsPawnList, ref this.reservedLordtoilsLordToilList);
			bool flag13 = Scribe.mode == LoadSaveMode.Saving && this.activeCover.Count<KeyValuePair<Pawn, Thing>>() > 0;
			if (flag13)
			{
				this.activeCover.RemoveAll((KeyValuePair<Pawn, Thing> x) => x.Key == null || x.Value == null);
			}
			Scribe_Collections.Look<Pawn, Thing>(ref this.activeCover, "activeCover", LookMode.Reference, LookMode.Reference, ref this.reservedActiveCoverPawnList, ref this.reservedActiveCoverEnemyList);
			Scribe_Collections.Look<IntVec3>(ref this.dangerousCells, "dangerousCells", LookMode.Undefined, Array.Empty<object>());
			bool flag14 = Scribe.mode == LoadSaveMode.Saving && this.pawnThings.Count<PawnThingsOwner>() > 0;
			if (flag14)
			{
				this.pawnThings.RemoveAll((PawnThingsOwner x) => x == null || x.owner == null || x.thingCount == null);
			}
			Scribe_Collections.Look<PawnThingsOwner>(ref this.pawnThings, "pawnThings", LookMode.Deep, Array.Empty<object>());
			bool flag15 = Scribe.mode == LoadSaveMode.Saving && this.lostThings.Count<KeyValuePair<ThingCountClass, Faction>>() > 0;
			if (flag15)
			{
				this.lostThings.RemoveAll((KeyValuePair<ThingCountClass, Faction> x) => x.Key == null || x.Value == null);
			}
			Scribe_Collections.Look<ThingCountClass, Faction>(ref this.lostThings, "lostThings", LookMode.Deep, LookMode.Reference, ref this.reservedLostThingsThingCountList, ref this.reservedLostThingsFactionList);
			bool flag16 = Scribe.mode == LoadSaveMode.Saving && this.exitCounter.Count<KeyValuePair<Pawn, int>>() > 0;
			if (flag16)
			{
				this.exitCounter.RemoveAll((KeyValuePair<Pawn, int> x) => x.Key == null);
			}
			Scribe_Collections.Look<Pawn, int>(ref this.exitCounter, "exitCounter", LookMode.Reference, LookMode.Value, ref this.reservedExitCounterPawnList, ref this.reservedExitCounterIntList);
			bool flag17 = Scribe.mode == LoadSaveMode.Saving && this.boughtThings.Count<KeyValuePair<Thing, Faction>>() > 0;
			if (flag17)
			{
				this.boughtThings.RemoveAll((KeyValuePair<Thing, Faction> x) => x.Key == null || x.Value == null);
			}
			Scribe_Collections.Look<Thing, Faction>(ref this.boughtThings, "boughtThings", LookMode.Reference, LookMode.Reference, ref this.reservedBoughtThingsThingList, ref this.reservedBoughtThingsFactionList);
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x0003C0FC File Offset: 0x0003A2FC
		// (set) Token: 0x060002E1 RID: 737 RVA: 0x0003C114 File Offset: 0x0003A314
		public bool Generated
		{
			get
			{
				return this.generated;
			}
			set
			{
				this.generated = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x0003C11E File Offset: 0x0003A31E
		public int Delay
		{
			get
			{
				return this.Generated ? 60 : 2000;
			}
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0003C134 File Offset: 0x0003A334
		public override void MapComponentTick()
		{
			base.MapComponentTick();
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				bool flag = Find.TickManager.TicksGame % 60 == 0 && this.shouldClearMapPawns;
				if (flag)
				{
					MapComponent_SkyAI.ClearEnemyMapPawns();
					this.shouldClearMapPawns = false;
				}
				bool flag2 = Find.TickManager.TicksGame % this.Delay == 0;
				if (flag2)
				{
					this.mainBlowDelay--;
					try
					{
						bool flag3 = !this.lords.NullOrEmpty<Lord>();
						if (flag3)
						{
							int num = this.lords.Count - 1;
							while (num >= 0 && !this.lords.NullOrEmpty<Lord>())
							{
								Lord lord5 = this.lords[num];
								bool flag4 = lord5 == null;
								if (flag4)
								{
									this.lords.Remove(lord5);
								}
								bool flag5 = lord5 != null;
								if (flag5)
								{
									bool flag6 = lord5.faction == null;
									if (!flag6)
									{
										bool flag7 = this.raidData == null;
										if (flag7)
										{
											this.raidData = new List<RaidData>();
										}
										bool flag8 = lord5.ownedPawns.NullOrEmpty<Pawn>() && !this.raidData.NullOrEmpty<RaidData>();
										if (flag8)
										{
											bool debugRaidData = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData)
											{
												Log.Message(string.Format("MapComponent: Remove lord with empty pawn list: {0} with id: {1}", lord5.LordJob, lord5.loadID));
											}
											bool flag9 = lord5.CurLordToil != null;
											if (flag9)
											{
												lord5.CurLordToil.AddFailCondition(() => true);
											}
											this.lords.Remove(lord5);
										}
										bool flag10 = !lord5.ownedPawns.NullOrEmpty<Pawn>() && (this.raidData.NullOrEmpty<RaidData>() || !MapComponent_SkyAI.RaidDatasPawnsHasLord(lord5, this.raidData));
										if (flag10)
										{
											List<Pawn> list = new List<Pawn>();
											bool flag11 = !lord5.ownedPawns.NullOrEmpty<Pawn>();
											if (flag11)
											{
												foreach (Pawn pawn in lord5.ownedPawns)
												{
													RaidData raidData = AdvancedAI.PawnRaidData(pawn);
													bool flag12 = raidData != null;
													if (flag12)
													{
														bool debugRaidData2 = SkyAiCore.Settings.debugRaidData;
														if (debugRaidData2)
														{
															Log.Message(string.Format("MapComponent: Add pawn: {0} tried to add pawn to generate new RaidData when already part of another raidData. Passing...", pawn));
														}
													}
													else
													{
														bool debugRaidData3 = SkyAiCore.Settings.debugRaidData;
														if (debugRaidData3)
														{
															Log.Message(string.Format("MapComponent: Add pawn: {0} to raidList to generate new RaidData with lord: {1} with id: {2}", pawn, lord5.LordJob, lord5.loadID));
														}
														list.Add(pawn);
														bool enablePawnThingOwnerData = SkyAiCore.Settings.enablePawnThingOwnerData;
														if (enablePawnThingOwnerData)
														{
															this.pawnThings.Add(AdvancedAI_CaravanUtility.PawnOwnerThingsList(pawn));
														}
													}
												}
											}
											else
											{
												bool debugRaidData4 = SkyAiCore.Settings.debugRaidData;
												if (debugRaidData4)
												{
													Log.Message(string.Format("MapComponent: Can't generated raid pawns, bcs of empty lord pawn list: {0} with id: {1}", lord5.LordJob, lord5.loadID));
												}
											}
											bool debugRaidData5 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData5)
											{
												bool flag13 = this.raidData.NullOrEmpty<RaidData>();
												if (flag13)
												{
													Log.Message("MapComponent: Generated new RaidData bcs of raidData null or empty.");
												}
												bool flag14 = !this.raidData.NullOrEmpty<RaidData>() && !MapComponent_SkyAI.RaidDatasPawnsHasLord(lord5, this.raidData);
												if (flag14)
												{
													Log.Message(string.Format("MapComponent: Generated new RaidData bcs of raidData missing lord {0} with id: {1}", lord5.LordJob, lord5.loadID));
												}
											}
											bool debugLog = SkyAiCore.Settings.debugLog;
											if (debugLog)
											{
												Log.Message(string.Format("MapComponent: Generated raid with lord (id: {0}) info:. Lord count: {1} LordJob: {2} Toil: {3} Faction: {4} of def: {5}", new object[]
												{
													lord5.loadID,
													lord5.ownedPawns.Count<Pawn>(),
													lord5.LordJob,
													lord5.CurLordToil,
													lord5.faction,
													lord5.faction.def
												}));
											}
											int count = lord5.ownedPawns.Count;
											Pawn trader = TraderCaravanUtility.FindTrader(lord5);
											List<Pawn> list2 = new List<Pawn>();
											Pawn pawn2 = (from p in lord5.ownedPawns
											where p.RaceProps.Humanlike && AdvancedAI.IsHumanlikeOnly(p)
											select p).FirstOrDefault<Pawn>();
											bool flag15 = !pawn2.Faction.HostileTo(Faction.OfPlayer) && !AdvancedAI.DutyHasAttackSubNodes(pawn2, false) && trader == null;
											Pawn pawn3 = null;
											bool flag16 = lord5.ownedPawns.Count >= SkyAiCore.Settings.minRaidCountForLeader && pawn2 != null && trader == null && !flag15;
											if (flag16)
											{
												pawn3 = (from leader in lord5.ownedPawns
												where AdvancedAI.IsGoodLeader(leader)
												select leader into p1
												orderby AdvancedAI.ShooterSkill(p1) descending
												select p1).ThenByDescending((Pawn p2) => AdvancedAI.MostExperienced(p2)).FirstOrDefault<Pawn>();
												bool flag17 = pawn3 != null;
												if (flag17)
												{
													SkillRecord skill = pawn3.skills.GetSkill(SkillDefOf.Social);
													bool flag18 = skill != null;
													if (flag18)
													{
														int level = skill.Level;
														foreach (SkillRecord skillRecord in pawn3.skills.skills)
														{
															bool flag19 = skillRecord.def == SkillDefOf.Social;
															if (flag19)
															{
																float f = ((float)pawn3.Faction.def.techLevel * 0.1f + Rand.Value + 1f) * (float)Mathf.Max(1, level);
																skillRecord.Level = Mathf.Clamp(Mathf.RoundToInt(f), 5, 18);
																bool flag20 = skillRecord.Level >= 14;
																if (flag20)
																{
																	skillRecord.passion = Passion.Major;
																}
																else
																{
																	bool flag21 = skillRecord.passion < Passion.Minor;
																	if (flag21)
																	{
																		skillRecord.passion = Passion.Minor;
																	}
																}
																bool debugLog2 = SkyAiCore.Settings.debugLog;
																if (debugLog2)
																{
																	Log.Message(string.Format("{0} {1}: Increased raid leader social skill from {2} to {3}", new object[]
																	{
																		pawn3,
																		pawn3.Position,
																		level,
																		skillRecord.Level
																	}));
																}
																break;
															}
														}
													}
													CompLeaderRole compLeaderRole = pawn3.TryGetComp<CompLeaderRole>();
													bool flag22 = compLeaderRole == null;
													if (flag22)
													{
														compLeaderRole = (CompLeaderRole)Activator.CreateInstance(typeof(CompLeaderRole));
														compLeaderRole.parent = pawn3;
														pawn3.AllComps.Add(compLeaderRole);
														compLeaderRole.Initialize(compLeaderRole.Props);
													}
													bool debugLog3 = SkyAiCore.Settings.debugLog;
													if (debugLog3)
													{
														Log.Message(string.Format("{0} {1}: is Raid Leader", pawn3, pawn3.Position));
													}
												}
											}
											bool flag23 = trader != null;
											if (flag23)
											{
												SkyAiCore.Settings.minRaidCountForDoctor = 3;
											}
											bool flag24 = lord5.ownedPawns.Count >= SkyAiCore.Settings.minRaidCountForDoctor && pawn2 != null;
											if (flag24)
											{
												IOrderedEnumerable<Pawn> orderedEnumerable = from doc in lord5.ownedPawns
												where AdvancedAI.IsHumanlikeOnly(doc) && !doc.IsPrisoner && !doc.IsSlave && !doc.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) && !AdvancedAI.PrimaryIsSiegeWeapon(doc) && AdvancedAI.LightWeapon(doc) && base.<MapComponentTick>g__isNotTrader|6(doc) && !SK_Utility.isMechanical(doc)
												select doc into p
												orderby MapComponent_SkyAI.MedicineSkill(p) descending
												select p;
												int num2 = Mathf.Max(1, lord5.ownedPawns.Count<Pawn>() / SkyAiCore.Settings.pawnBecomeDoctorEvery);
												bool flag25 = !orderedEnumerable.EnumerableNullOrEmpty<Pawn>();
												if (flag25)
												{
													ThingDef thingDef = AdvancedAI_TendUtility.MedicineDef(lord5.faction);
													int num3 = 0;
													foreach (Pawn pawn4 in orderedEnumerable)
													{
														bool flag26 = num2 > 0 && !list2.Contains(pawn4) && thingDef != null && !AdvancedAI.PawnIsLeader(pawn4) && !AdvancedAI.PawnIsDoctor(pawn4) && !AdvancedAI.PawnIsGuard(pawn4);
														if (flag26)
														{
															list2.Add(pawn4);
															AdvancedAI_TendUtility.AddMedicineToInventory(pawn4, thingDef, new IntRange(Mathf.Max(1, SkyAiCore.Settings.medicineSpawnCount - 1), SkyAiCore.Settings.medicineSpawnCount + 1));
															CompDoctorRole compDoctorRole = pawn4.TryGetComp<CompDoctorRole>();
															bool flag27 = compDoctorRole == null;
															if (flag27)
															{
																compDoctorRole = (CompDoctorRole)Activator.CreateInstance(typeof(CompDoctorRole));
																compDoctorRole.parent = pawn4;
																pawn4.AllComps.Add(compDoctorRole);
																compDoctorRole.Initialize(compDoctorRole.Props);
															}
															num3++;
														}
														bool flag28 = num3 >= num2;
														if (flag28)
														{
															break;
														}
													}
												}
											}
											bool debugLog4 = SkyAiCore.Settings.debugLog;
											if (debugLog4)
											{
												foreach (Pawn pawn5 in list2)
												{
													Log.Message(string.Format("{0} {1}: is Doctor", pawn5, pawn5.Position));
												}
											}
											IEnumerable<Pawn> enumerable = from p in lord5.ownedPawns
											where p != null && p.mindState != null && p.mindState.duty != null && p.mindState.duty.def == DutyDefOf.Breaching
											select p;
											bool flag29 = !enumerable.EnumerableNullOrEmpty<Pawn>();
											if (flag29)
											{
												bool debugLog5 = SkyAiCore.Settings.debugLog;
												if (debugLog5)
												{
													Log.Message("BreachingAI. Try to distribute additional siege equipment.");
												}
												AdvancedAI_BreachingUtility.TryDistributeAdditionalSiegeWeaponForRaidPawns(lord5.ownedPawns);
											}
											List<Pawn> leaderGuards = new List<Pawn>();
											RaidData item = new RaidData(lord5.faction, list, count, list2, leaderGuards, pawn3, RaidData.RaidStage.start, IntVec3.Invalid);
											this.raidData.Add(item);
											bool debugRaidData6 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData6)
											{
												Log.Message(string.Format("MapComponent: Added new raidData by Leader: {0} count: {1} Doctors count: {2} with faction: {3}", new object[]
												{
													pawn3,
													list.Count,
													list2.Count<Pawn>(),
													lord5.faction
												}));
											}
											AdvancedAI_SquadUtility.MakeSimpleSquad(pawn3, item, false);
										}
									}
								}
								IL_A87:
								num--;
								continue;
								goto IL_A87;
							}
						}
					}
					catch (Exception arg)
					{
						Log.Error(string.Format("MapComponent_SkyAI: RaidData Add part exception: {0}", arg));
					}
					try
					{
						bool flag30 = !this.raidData.NullOrEmpty<RaidData>();
						if (flag30)
						{
							bool debugRaidData7 = SkyAiCore.Settings.debugRaidData;
							if (debugRaidData7)
							{
								for (int i = 0; i < this.raidData.Count; i++)
								{
									Log.Message(string.Format("MapComponent: Active raidData {0}: With leader: {1} count: {2} doctors: {3} faction: {4} of def: {5}", new object[]
									{
										i,
										this.raidData[i].raidLeader,
										this.raidData[i].raidPawns.Count<Pawn>(),
										this.raidData[i].raidDoctors.Count<Pawn>(),
										this.raidData[i].faction,
										this.raidData[i].faction.def
									}));
								}
							}
							for (int j = this.raidData.Count - 1; j >= 0; j--)
							{
								RaidData raidData2 = this.raidData[j];
								bool flag31 = raidData2 != null;
								if (flag31)
								{
									List<SquadData> squads = raidData2.squads;
									bool flag32 = squads.NullOrEmpty<SquadData>();
									if (flag32)
									{
										bool flag33 = raidData2.raidLeader != null;
										if (flag33)
										{
											AdvancedAI_SquadUtility.MakeSimpleSquad(raidData2.raidLeader, raidData2, false);
											bool debugRaidData8 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData8)
											{
												Log.Message(string.Format("MapComponent: MakeSimpleSquad. with leader: {0}", raidData2.raidLeader));
											}
										}
										else
										{
											raidData2.raidStage = RaidData.RaidStage.fleeing;
										}
									}
									else
									{
										for (int k = squads.Count - 1; k >= 0; k--)
										{
											SquadData squad = squads[k];
											bool flag34 = squad != null;
											if (flag34)
											{
												List<Map> maps = Find.Maps;
												Func<Lord, bool> <>9__9;
												for (int l = 0; l < maps.Count; l++)
												{
													IEnumerable<Lord> source = maps[l].lordManager.lords;
													Func<Lord, bool> predicate;
													if ((predicate = <>9__9) == null)
													{
														predicate = (<>9__9 = ((Lord x) => x.loadID.Equals(squad.id)));
													}
													Lord lord2 = source.Where(predicate).FirstOrDefault<Lord>();
													bool flag35 = lord2 != null;
													if (flag35)
													{
														bool flag36 = !lord2.ownedPawns.NullOrEmpty<Pawn>();
														if (flag36)
														{
															bool flag37 = !lord2.ownedPawns.SequenceEqual(squad.squadPawns);
															if (flag37)
															{
																squad.squadPawns = new List<Pawn>(lord2.ownedPawns);
																bool debugRaidData9 = SkyAiCore.Settings.debugRaidData;
																if (debugRaidData9)
																{
																	Log.Message(string.Format("MapComponent: Update squad. Sync1: {0} id: {1} Commander: {2} Pawns: {3} lord id: {4} lord pawns: {5}", new object[]
																	{
																		squad.squadName,
																		squad.id,
																		squad.squadCommander,
																		squad.squadPawns.Count,
																		lord2.loadID,
																		lord2.ownedPawns.Count
																	}));
																}
															}
														}
													}
													else
													{
														bool flag38 = !squad.squadPawns.NullOrEmpty<Pawn>();
														if (flag38)
														{
															bool flag39 = squad.squadCommander != null;
															if (flag39)
															{
																Lord lord3 = squad.squadCommander.GetLord();
																bool flag40 = lord3 != null;
																if (flag40)
																{
																	squad.id = lord3.loadID;
																	bool debugRaidData10 = SkyAiCore.Settings.debugRaidData;
																	if (debugRaidData10)
																	{
																		Log.Message(string.Format("MapComponent: Update squad. Sync2: {0} id: {1} Commander: {2} Pawns: {3}", new object[]
																		{
																			squad.squadName,
																			squad.id,
																			squad.squadCommander,
																			squad.squadPawns.Count
																		}));
																	}
																}
																else
																{
																	Pawn pawn6 = (from p in squad.squadPawns
																	where AdvancedAI.IsActivePawn(p)
																	select p).FirstOrDefault<Pawn>();
																	bool flag41 = pawn6 != null;
																	if (flag41)
																	{
																		Lord lord4 = pawn6.GetLord();
																		bool flag42 = lord4 != null;
																		if (flag42)
																		{
																			squad.id = lord4.loadID;
																			bool debugRaidData11 = SkyAiCore.Settings.debugRaidData;
																			if (debugRaidData11)
																			{
																				Log.Message(string.Format("MapComponent: Update squad. Sync3: {0} id: {1} FirstPawn: {2} Pawns: {3}", new object[]
																				{
																					squad.squadName,
																					squad.id,
																					pawn6,
																					squad.squadPawns.Count
																				}));
																			}
																		}
																	}
																}
															}
														}
														else
														{
															bool debugRaidData12 = SkyAiCore.Settings.debugRaidData;
															if (debugRaidData12)
															{
																Log.Message(string.Format("MapComponent: Squad removed: {0} id: {1} Pawns: {2}", squad.squadName, squad.id, squad.squadPawns.Count));
															}
															squads.Remove(squad);
														}
													}
												}
											}
										}
									}
									bool flag43 = !raidData2.raidPawns.NullOrEmpty<Pawn>();
									if (flag43)
									{
										Pawn pawn7 = AdvancedAI.FirstRaidPawnAtFast(raidData2);
										bool flag44 = pawn7 != null && !AdvancedAI.HasDefendBaseDuty(pawn7) && !AdvancedAI_SquadUtility.RaidIsCapableOfFighting(raidData2, 0.4f);
										if (flag44)
										{
											raidData2.raidStage = RaidData.RaidStage.fleeing;
										}
										bool flag45 = !SkyAiCore.Settings.enableRaidLeaderGathersTroopsNearColony && raidData2.raidStage == RaidData.RaidStage.start;
										bool flag46 = !AdvancedAI.RaidLeaderIsActive(raidData2) && (AdvancedAI_SquadUtility.IsStartingRaidStage(raidData2) || flag45);
										if (flag46)
										{
											bool flag47;
											if (raidData2.startAttackingDelay > 0)
											{
												flag47 = raidData2.squadDefence.Any((KeyValuePair<Lord, bool> lord) => lord.Value);
											}
											else
											{
												flag47 = false;
											}
											bool flag48 = flag47;
											if (flag48)
											{
												raidData2.startAttackingDelay--;
												bool debugRaidData13 = SkyAiCore.Settings.debugRaidData;
												if (debugRaidData13)
												{
													Log.Message(string.Format("MapComponent: Raid stage delay: {0}", raidData2.startAttackingDelay));
												}
											}
											bool flag49 = raidData2.startAttackingDelay <= 0;
											if (flag49)
											{
												bool debugRaidData14 = SkyAiCore.Settings.debugRaidData;
												if (debugRaidData14)
												{
													Log.Message(string.Format("MapComponent: Raid stage changed to {0}", RaidData.RaidStage.attack));
												}
												AdvancedAI_SquadUtility.UpdateStageForSiegeAI(raidData2);
												raidData2.raidStage = RaidData.RaidStage.attack;
											}
										}
										try
										{
											int num4 = raidData2.raidPawns.Count - 1;
											while (num4 >= 0 && !raidData2.raidPawns.NullOrEmpty<Pawn>())
											{
												Pawn pawn8 = raidData2.raidPawns[num4];
												bool flag50 = pawn8 == null || MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(pawn8);
												if (flag50)
												{
													raidData2.raidPawns.Remove(pawn8);
												}
												num4--;
											}
										}
										catch (Exception arg2)
										{
											Log.Error(string.Format("MapComponent_SkyAI: RaidData check. RaidPawns duty. part exception: {0}", arg2));
										}
									}
									bool flag51 = !raidData2.raidDoctors.NullOrEmpty<Pawn>();
									if (flag51)
									{
										int num5 = raidData2.raidDoctors.Count - 1;
										while (num5 >= 0 && !raidData2.raidDoctors.NullOrEmpty<Pawn>())
										{
											Pawn pawn9 = raidData2.raidDoctors[num5];
											bool flag52 = pawn9 != null;
											if (flag52)
											{
												bool flag53 = !raidData2.raidPawns.NullOrEmpty<Pawn>();
												if (flag53)
												{
													bool flag54 = !raidData2.raidPawns.Contains(pawn9);
													if (flag54)
													{
														bool debugRaidData15 = SkyAiCore.Settings.debugRaidData;
														if (debugRaidData15)
														{
															Log.Message(string.Format("MapComponent: Removed doctor pawn: {0}, bcs of pawn missing in raidPawn list", pawn9));
														}
														AdvancedAI.ClearRoleComps(pawn9);
														raidData2.raidDoctors.Remove(pawn9);
													}
												}
												bool flag55 = MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(pawn9);
												if (flag55)
												{
													bool debugRaidData16 = SkyAiCore.Settings.debugRaidData;
													if (debugRaidData16)
													{
														Log.Message(string.Format("MapComponent: Removed doctor pawn {0} from doctor list. Reason: ShouldBeRemovedFromRaidDataPawns.", pawn9));
													}
													AdvancedAI.ClearRoleComps(pawn9);
													raidData2.raidDoctors.Remove(pawn9);
												}
												bool flag56 = pawn9.Faction == null || pawn9.IsPrisoner || pawn9.IsSlave;
												if (flag56)
												{
													bool debugRaidData17 = SkyAiCore.Settings.debugRaidData;
													if (debugRaidData17)
													{
														Log.Message(string.Format("MapComponent: Removed doctor pawn {0} from doctor list. Reason: Pawn faction null or is prisoner.", pawn9));
													}
													AdvancedAI.ClearRoleComps(pawn9);
													raidData2.raidDoctors.Remove(pawn9);
												}
												bool flag57 = pawn9.Faction != null && pawn9.Faction == Faction.OfPlayer;
												if (flag57)
												{
													bool debugRaidData18 = SkyAiCore.Settings.debugRaidData;
													if (debugRaidData18)
													{
														Log.Message(string.Format("MapComponent: Removed doctor pawn {0} from doctor list. Reason: Pawn has player faction.", pawn9));
													}
													AdvancedAI.ClearRoleComps(pawn9);
													raidData2.raidDoctors.Remove(pawn9);
												}
											}
											num5--;
										}
									}
									Pawn raidLeader = raidData2.raidLeader;
									bool flag58 = raidLeader != null;
									if (flag58)
									{
										bool flag59 = !raidData2.raidPawns.NullOrEmpty<Pawn>();
										if (flag59)
										{
											bool flag60 = !raidData2.raidPawns.Contains(raidLeader);
											if (flag60)
											{
												AdvancedAI.ClearRoleComps(raidLeader);
												raidData2.raidLeader = null;
											}
										}
										bool flag61 = MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(raidLeader);
										if (flag61)
										{
											bool debugRaidData19 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData19)
											{
												Log.Message(string.Format("MapComponent: Removed leader pawn {0} from raid leader duty. Reason: ShouldBeRemovedFromRaidDataPawns.", raidLeader));
											}
											AdvancedAI.ClearRoleComps(raidLeader);
											raidData2.raidLeader = null;
										}
										bool flag62 = raidLeader.Faction == null || raidLeader.IsPrisoner || raidLeader.IsSlave;
										if (flag62)
										{
											bool debugRaidData20 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData20)
											{
												Log.Message(string.Format("MapComponent: Removed leader pawn {0} from raid leader duty. Reason: Pawn faction null or is prisoner.", raidLeader));
											}
											AdvancedAI.ClearRoleComps(raidLeader);
											raidData2.raidLeader = null;
										}
										bool flag63 = raidLeader.Faction != null && (raidLeader.Faction == null || raidLeader.Faction == Faction.OfPlayer);
										if (flag63)
										{
											bool debugRaidData21 = SkyAiCore.Settings.debugRaidData;
											if (debugRaidData21)
											{
												Log.Message(string.Format("MapComponent: Removed leader pawn {0} from raid leader duty. Reason: Pawn has player faction.", raidLeader));
											}
											AdvancedAI.ClearRoleComps(raidLeader);
											raidData2.raidLeader = null;
										}
									}
									bool flag64 = raidData2.raidPawns.NullOrEmpty<Pawn>();
									if (flag64)
									{
										bool debugRaidData22 = SkyAiCore.Settings.debugRaidData;
										if (debugRaidData22)
										{
											Log.Message(string.Format("MapComponent: Remove raid data. Reason: RaidData missing leader, doctors. Raid pawn count: {0}", raidData2.raidPawns.Count<Pawn>()));
										}
										this.raidData.Remove(raidData2);
									}
									try
									{
										bool flag65 = !raidData2.squadCommanders.NullOrEmpty<Pawn>();
										if (flag65)
										{
											int num6 = raidData2.squadCommanders.Count - 1;
											while (num6 >= 0 && !raidData2.squadCommanders.NullOrEmpty<Pawn>())
											{
												Pawn pawn10 = raidData2.squadCommanders[num6];
												bool flag66 = pawn10 != null && MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(pawn10);
												if (flag66)
												{
													AdvancedAI.ClearRoleComps(pawn10);
													raidData2.squadCommanders.Remove(pawn10);
												}
												num6--;
											}
										}
										bool flag67 = !raidData2.raidOrders.EnumerableNullOrEmpty<KeyValuePair<Pawn, bool>>();
										if (flag67)
										{
											List<Pawn> list3 = AdvancedAI_SquadUtility.RaidSquadCommanders(raidData2, true);
											bool flag68 = !list3.NullOrEmpty<Pawn>();
											if (flag68)
											{
												List<Pawn> list4 = new List<Pawn>();
												bool flag69 = raidData2.raidOrders.Count > 0;
												if (flag69)
												{
													int num7 = raidData2.raidOrders.Count - 1;
													while (num7 >= 0 && !raidData2.raidOrders.EnumerableNullOrEmpty<KeyValuePair<Pawn, bool>>())
													{
														KeyValuePair<Pawn, bool> keyValuePair = raidData2.raidOrders.ElementAt(num7);
														Pawn key = keyValuePair.Key;
														bool flag70 = key == null || !AdvancedAI.IsActivePawn(key);
														if (flag70)
														{
															raidData2.raidOrders.Remove(keyValuePair.Key);
														}
														else
														{
															bool value = keyValuePair.Value;
															if (value)
															{
																bool flag71 = !list4.Contains(keyValuePair.Key);
																if (flag71)
																{
																	list4.Add(keyValuePair.Key);
																}
															}
														}
														num7--;
													}
												}
												bool flag72 = list4.Count >= list3.Count;
												if (flag72)
												{
													raidData2.raidIsReady = true;
												}
												bool debugLog6 = SkyAiCore.Settings.debugLog;
												if (debugLog6)
												{
													Log.Message(string.Format("MapComponent: Readiness: {0}/{1}. Check squad Commanders: {2}", list4.Count, list3.Count, GeneralExtensions.Join<Pawn>(list3, null, ", ").ToString()));
												}
												list4.Clear();
											}
										}
									}
									catch (Exception arg3)
									{
										Log.Error(string.Format("MapComponent_SkyAI: RaidData check squadCommanders: {0}", arg3));
									}
									bool flag73 = !raidData2.squadDefence.EnumerableNullOrEmpty<KeyValuePair<Lord, bool>>();
									if (flag73)
									{
										int num8 = raidData2.squadDefence.Count - 1;
										while (num8 >= 0 && !raidData2.squadDefence.EnumerableNullOrEmpty<KeyValuePair<Lord, bool>>())
										{
											KeyValuePair<Lord, bool> keyValuePair2 = raidData2.squadDefence.ElementAt(num8);
											bool flag74 = keyValuePair2.Key == null;
											if (flag74)
											{
												raidData2.squadDefence.Remove(keyValuePair2.Key);
											}
											num8--;
										}
									}
									bool flag75 = !raidData2.squadDefencePoint.EnumerableNullOrEmpty<KeyValuePair<Lord, IntVec3>>();
									if (flag75)
									{
										int num9 = raidData2.squadDefencePoint.Count - 1;
										while (num9 >= 0 && !raidData2.squadDefencePoint.EnumerableNullOrEmpty<KeyValuePair<Lord, IntVec3>>())
										{
											KeyValuePair<Lord, IntVec3> keyValuePair3 = raidData2.squadDefencePoint.ElementAt(num9);
											bool flag76 = keyValuePair3.Key == null;
											if (flag76)
											{
												raidData2.squadDefencePoint.Remove(keyValuePair3.Key);
											}
											num9--;
										}
									}
									List<ExitData> exitCells = raidData2.exitCells;
									bool flag77 = !exitCells.EnumerableNullOrEmpty<ExitData>();
									if (flag77)
									{
										int num10 = exitCells.Count - 1;
										while (num10 >= 0 && !exitCells.EnumerableNullOrEmpty<ExitData>())
										{
											ExitData exitData = exitCells.ElementAt(num10);
											bool flag78 = exitData != null;
											if (flag78)
											{
												int num11 = Find.TickManager.TicksGame - exitData.dirtyTime;
												bool flag79 = num11 >= 4000;
												if (flag79)
												{
													exitCells.Remove(exitData);
												}
											}
											num10--;
										}
									}
								}
							}
						}
					}
					catch (Exception arg4)
					{
						Log.Error(string.Format("MapComponent_SkyAI: RaidData check part exception: {0}", arg4));
					}
					bool flag80 = !this.focusCells.EnumerableNullOrEmpty<KeyValuePair<Pawn, IntVec3>>();
					if (flag80)
					{
						int num12 = this.focusCells.Count - 1;
						while (num12 >= 0 && !this.focusCells.EnumerableNullOrEmpty<KeyValuePair<Pawn, IntVec3>>())
						{
							Pawn key2 = this.focusCells.ElementAt(num12).Key;
							bool flag81 = key2 != null;
							if (flag81)
							{
								bool flag82 = MapComponent_SkyAI.ShouldBeRemoved(key2);
								if (flag82)
								{
									this.focusCells.Remove(key2);
								}
							}
							num12--;
						}
					}
					bool flag83 = !this.coverJobs.EnumerableNullOrEmpty<KeyValuePair<Pawn, IntVec3>>();
					if (flag83)
					{
						int num13 = this.coverJobs.Count - 1;
						while (num13 >= 0 && !this.coverJobs.EnumerableNullOrEmpty<KeyValuePair<Pawn, IntVec3>>())
						{
							Pawn key3 = this.coverJobs.ElementAt(num13).Key;
							bool flag84 = key3 != null;
							if (flag84)
							{
								bool flag85 = MapComponent_SkyAI.ShouldBeRemoved(key3);
								if (flag85)
								{
									this.coverJobs.Remove(key3);
								}
							}
							num13--;
						}
					}
					bool flag86 = !this.destroyersExclusions.NullOrEmpty<Pawn>();
					if (flag86)
					{
						int num14 = this.destroyersExclusions.Count - 1;
						while (num14 >= 0 && !this.destroyersExclusions.NullOrEmpty<Pawn>())
						{
							Pawn pawn11 = this.destroyersExclusions[num14];
							bool flag87 = MapComponent_SkyAI.ShouldBeRemoved(pawn11);
							if (flag87)
							{
								this.destroyersExclusions.Remove(pawn11);
							}
							num14--;
						}
					}
					try
					{
						bool enableMainBlowSiegeTactic = SkyAiCore.Settings.enableMainBlowSiegeTactic;
						if (enableMainBlowSiegeTactic)
						{
							bool flag88 = this.mainBlowCells.NullOrEmpty<IntVec3>() && !this.raidData.NullOrEmpty<RaidData>();
							if (flag88)
							{
								RaidData raidData3 = (from r in this.raidData
								where r.raidLeader != null && r.raidLeader.Spawned && AdvancedAI.IsActivePawn(r.raidLeader) && r.raidLeader.TryGetComp<CompLeaderRole>() != null && AdvancedAI.IsValidLoc(r.raidLeader.TryGetComp<CompLeaderRole>().BlockerCell)
								select r).FirstOrDefault<RaidData>();
								bool flag89 = raidData3 != null;
								if (flag89)
								{
									Pawn raidLeader2 = raidData3.raidLeader;
									IntVec3 intVec = IntVec3.Invalid;
									IntVec3 intVec2 = IntVec3.Invalid;
									CompLeaderRole compLeaderRole2 = raidLeader2.TryGetComp<CompLeaderRole>();
									bool flag90 = compLeaderRole2 != null;
									if (flag90)
									{
										bool flag91 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
										if (flag91)
										{
											Log.Message(string.Format("{0} {1}: Received from comp focus cell: {2} enemyTarget: {3}", new object[]
											{
												raidLeader2,
												raidLeader2.Position,
												compLeaderRole2.BlockerCell,
												compLeaderRole2.LeaderEnemyTarget
											}));
										}
										intVec2 = compLeaderRole2.LeaderEnemyTarget;
										intVec = compLeaderRole2.BlockerCell;
									}
									else
									{
										bool flag92 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
										if (flag92)
										{
											Log.Message(string.Format("{0} {1}: Comp focus cell data empty.", raidLeader2, raidLeader2.Position));
										}
									}
									bool flag93 = intVec.IsValid && raidLeader2.Position.DistanceTo(intVec) < 120f;
									if (flag93)
									{
										bool flag94 = SkyAiCore.Settings.debugLog && SkyAiCore.Settings.enableMainBlowSiegeTactic;
										if (flag94)
										{
											Log.Message(string.Format("{0} {1}: Exec MainBlow tactic with focus cell: {2} enemyTarget: {3}", new object[]
											{
												raidLeader2,
												raidLeader2.Position,
												intVec,
												intVec2
											}));
										}
										AdvancedAI.AddCellsToMainBlow(raidLeader2, intVec, intVec2, 6f, true);
									}
								}
							}
						}
					}
					catch (Exception arg5)
					{
						Log.Error(string.Format("MapComponent_SkyAI: Mainblow part exception: {0}", arg5));
					}
					this.Generated = false;
				}
				bool flag95 = Find.TickManager.TicksGame % 600 == 0;
				if (flag95)
				{
					bool flag96 = !this.exitCounter.NullOrEmpty<Pawn, int>();
					if (flag96)
					{
						int num15 = this.exitCounter.Count - 1;
						while (num15 >= 0 && !this.exitCounter.NullOrEmpty<Pawn, int>())
						{
							Pawn key4 = this.exitCounter.ElementAt(num15).Key;
							bool flag97 = key4 != null;
							if (flag97)
							{
								bool flag98 = MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(key4);
								if (flag98)
								{
									this.exitCounter.Remove(key4);
								}
								else
								{
									int value2 = this.exitCounter.ElementAt(num15).Value;
									this.exitCounter.Remove(key4);
									this.exitCounter.Add(key4, value2 + 600);
								}
							}
							num15--;
						}
					}
				}
				bool flag99 = Find.TickManager.TicksGame % 900 == 0;
				if (flag99)
				{
					this.dangerousCells.Clear();
				}
				bool flag100 = Find.TickManager.TicksGame % 1800 == 0;
				if (flag100)
				{
					MapComponent_SkyAI.GetMapPawnsNumbers(this.map, out this.mapPawnCount, out this.mapRangedPawnCount);
					bool flag101 = !this.pawnThings.NullOrEmpty<PawnThingsOwner>();
					if (flag101)
					{
						for (int m = this.pawnThings.Count - 1; m >= 0; m--)
						{
							PawnThingsOwner pawnThingsOwner = this.pawnThings.ElementAt(m);
							bool flag102 = pawnThingsOwner != null;
							if (flag102)
							{
								Pawn owner = pawnThingsOwner.owner;
								bool flag103 = owner == null || MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(owner);
								if (flag103)
								{
									bool debugPawnThingsOwner = SkyAiCore.Settings.debugPawnThingsOwner;
									if (debugPawnThingsOwner)
									{
										Log.Message(string.Format("MapComponent. Removed pawnThingOwner: {0} bcs of pawn null or should be removed.", pawnThingsOwner));
									}
									this.pawnThings.Remove(pawnThingsOwner);
								}
							}
						}
					}
					bool flag104 = !this.lostThings.EnumerableNullOrEmpty<KeyValuePair<ThingCountClass, Faction>>();
					if (flag104)
					{
						for (int n = this.lostThings.Count - 1; n >= 0; n--)
						{
							ThingCountClass key5 = this.lostThings.ElementAt(n).Key;
							bool flag105 = key5 != null && (key5.thing == null || AdvancedAI_CaravanUtility.CurrentOwnerOf(key5.thing) != null);
							if (flag105)
							{
								bool debugPawnThingsOwner2 = SkyAiCore.Settings.debugPawnThingsOwner;
								if (debugPawnThingsOwner2)
								{
									Log.Message(string.Format("MapComponent. Removed lostThings item: {0} bcs of item owner changed or item null.", key5));
								}
								this.lostThings.Remove(key5);
							}
						}
					}
				}
				bool flag106 = Find.TickManager.TicksGame % 5000 == 0;
				if (flag106)
				{
					bool flag107 = !this.boughtThings.EnumerableNullOrEmpty<KeyValuePair<Thing, Faction>>();
					if (flag107)
					{
						for (int num16 = this.boughtThings.Count - 1; num16 >= 0; num16--)
						{
							Thing key6 = this.boughtThings.ElementAt(num16).Key;
							Map map = this.map;
							bool flag108 = key6 == null || !key6.Position.InBounds(map) || map.areaManager.Home[key6.Position];
							if (flag108)
							{
								this.boughtThings.Remove(key6);
							}
						}
					}
				}
				bool flag109 = Find.TickManager.TicksGame % 12000 == 0;
				if (flag109)
				{
					bool flag110 = !this.mainBlowCells.NullOrEmpty<IntVec3>();
					if (flag110)
					{
						int num17 = this.mainBlowCells.Count - 1;
						while (num17 >= 0 && !this.mainBlowCells.NullOrEmpty<IntVec3>())
						{
							this.mainBlowCells.Remove(this.mainBlowCells[num17]);
							num17--;
						}
					}
				}
				bool flag111 = Find.TickManager.TicksGame % 60000 == 0;
				if (flag111)
				{
					bool flag112 = !this.lordtoils.EnumerableNullOrEmpty<KeyValuePair<Pawn, LordToil>>();
					if (flag112)
					{
						int num18 = this.lordtoils.Count - 1;
						while (num18 >= 0 && !this.lordtoils.EnumerableNullOrEmpty<KeyValuePair<Pawn, LordToil>>())
						{
							Pawn key7 = this.lordtoils.ElementAt(num18).Key;
							bool flag113 = key7 != null;
							if (flag113)
							{
								bool flag114 = MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(key7);
								if (flag114)
								{
									this.lordtoils.Remove(key7);
								}
							}
							num18--;
						}
					}
				}
			}
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0003E4D8 File Offset: 0x0003C6D8
		public static bool RaidDatasPawnsHasLord(Lord lord, List<RaidData> raidDatas)
		{
			bool flag = !raidDatas.NullOrEmpty<RaidData>();
			if (flag)
			{
				for (int i = 0; i < raidDatas.Count; i++)
				{
					RaidData raidData = raidDatas[i];
					bool flag2 = MapComponent_SkyAI.RaidDataPawnsHasLord(lord, raidData);
					if (flag2)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0003E530 File Offset: 0x0003C730
		public static bool RaidDataPawnsHasLord(Lord lord, RaidData raidData)
		{
			for (int i = 0; i < raidData.raidPawns.Count; i++)
			{
				Pawn pawn = raidData.raidPawns[i];
				bool flag = pawn != null && pawn.Spawned;
				if (flag)
				{
					Lord lord2 = pawn.GetLord();
					bool flag2 = lord2 == lord;
					if (flag2)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0003E59C File Offset: 0x0003C79C
		public static int MedicineSkill(Pawn pawn)
		{
			bool flag = pawn.skills != null && !pawn.skills.skills.NullOrEmpty<SkillRecord>();
			if (flag)
			{
				foreach (SkillRecord skillRecord in pawn.skills.skills)
				{
					bool flag2 = skillRecord.def == SkillDefOf.Medicine;
					if (flag2)
					{
						return skillRecord.Level;
					}
				}
			}
			return 0;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0003E63C File Offset: 0x0003C83C
		public static bool ShouldBeRemovedFromRaidDataPawns(Pawn pawn)
		{
			bool flag = pawn == null;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = !pawn.Spawned || pawn.Dead || pawn.IsPrisoner || pawn.IsSlave || pawn.IsColonist;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = pawn.Faction != null && pawn.Faction == Faction.OfPlayer;
					result = flag3;
				}
			}
			return result;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0003E6B0 File Offset: 0x0003C8B0
		public static bool ShouldBeRemoved(Pawn pawn)
		{
			bool flag = MapComponent_SkyAI.ShouldBeRemovedFromRaidDataPawns(pawn);
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool downed = pawn.Downed;
				result = downed;
			}
			return result;
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0003E6E4 File Offset: 0x0003C8E4
		public static void GetMapPawnsNumbers(Map map, out int pawnsCount, out int rangedPawnsCount)
		{
			int num = 0;
			int num2 = 0;
			IEnumerable<Pawn> source = from p in map.mapPawns.AllPawnsSpawned
			where AdvancedAI.IsHumanlikeOnly(p)
			select p;
			for (int i = 0; i < source.Count<Pawn>(); i++)
			{
				Pawn pawn = source.ElementAt(i);
				bool flag = pawn.Downed || pawn.Dead;
				if (!flag)
				{
					bool flag2 = !pawn.IsColonist && !pawn.IsPrisoner && !pawn.IsSlave;
					if (flag2)
					{
						num++;
					}
					bool flag3 = !AdvancedAI.IsMeleeVerb(pawn, null);
					if (flag3)
					{
						num2++;
					}
				}
			}
			pawnsCount = num;
			rangedPawnsCount = num2;
		}

		// Token: 0x0400013B RID: 315
		public string saveGameModVersion = "";

		// Token: 0x0400013C RID: 316
		public int mainBlowDelay = 0;

		// Token: 0x0400013D RID: 317
		public bool shouldClearMapPawns = false;

		// Token: 0x0400013E RID: 318
		public List<Lord> lords = new List<Lord>();

		// Token: 0x0400013F RID: 319
		public List<RaidData> raidData = new List<RaidData>();

		// Token: 0x04000140 RID: 320
		public Dictionary<Pawn, IntVec3> focusCells = new Dictionary<Pawn, IntVec3>();

		// Token: 0x04000141 RID: 321
		public Dictionary<Pawn, IntVec3> coverJobs = new Dictionary<Pawn, IntVec3>();

		// Token: 0x04000142 RID: 322
		public List<Pawn> destroyersExclusions = new List<Pawn>();

		// Token: 0x04000143 RID: 323
		public List<IntVec3> mainBlowCells = new List<IntVec3>();

		// Token: 0x04000144 RID: 324
		public Dictionary<Pawn, LordToil> lordtoils = new Dictionary<Pawn, LordToil>();

		// Token: 0x04000145 RID: 325
		public Dictionary<Pawn, Thing> activeCover = new Dictionary<Pawn, Thing>();

		// Token: 0x04000146 RID: 326
		public List<IntVec3> dangerousCells = new List<IntVec3>();

		// Token: 0x04000147 RID: 327
		public int mapPawnCount = 0;

		// Token: 0x04000148 RID: 328
		public int mapRangedPawnCount = 0;

		// Token: 0x04000149 RID: 329
		public List<PawnThingsOwner> pawnThings = new List<PawnThingsOwner>();

		// Token: 0x0400014A RID: 330
		public Dictionary<ThingCountClass, Faction> lostThings = new Dictionary<ThingCountClass, Faction>();

		// Token: 0x0400014B RID: 331
		public Dictionary<Thing, Faction> boughtThings = new Dictionary<Thing, Faction>();

		// Token: 0x0400014C RID: 332
		public Dictionary<Pawn, int> exitCounter = new Dictionary<Pawn, int>();

		// Token: 0x0400014D RID: 333
		private List<Pawn> reservedFocusCellPawnList;

		// Token: 0x0400014E RID: 334
		private List<IntVec3> reservedFocusCellIntVecList;

		// Token: 0x0400014F RID: 335
		private List<Pawn> reservedCoverJobPawnList;

		// Token: 0x04000150 RID: 336
		private List<IntVec3> reservedCoverJobIntVecList;

		// Token: 0x04000151 RID: 337
		private List<Pawn> reservedLordtoilsPawnList;

		// Token: 0x04000152 RID: 338
		private List<LordToil> reservedLordtoilsLordToilList;

		// Token: 0x04000153 RID: 339
		private List<Pawn> reservedActiveCoverPawnList;

		// Token: 0x04000154 RID: 340
		private List<Thing> reservedActiveCoverEnemyList;

		// Token: 0x04000155 RID: 341
		private List<ThingCountClass> reservedLostThingsThingCountList;

		// Token: 0x04000156 RID: 342
		private List<Faction> reservedLostThingsFactionList;

		// Token: 0x04000157 RID: 343
		private List<Pawn> reservedExitCounterPawnList;

		// Token: 0x04000158 RID: 344
		private List<int> reservedExitCounterIntList;

		// Token: 0x04000159 RID: 345
		private List<Thing> reservedBoughtThingsThingList;

		// Token: 0x0400015A RID: 346
		private List<Faction> reservedBoughtThingsFactionList;

		// Token: 0x0400015B RID: 347
		private bool generated;
	}
}
