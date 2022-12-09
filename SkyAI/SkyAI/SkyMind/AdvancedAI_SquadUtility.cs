using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x0200001C RID: 28
	public static class AdvancedAI_SquadUtility
	{
		// Token: 0x060000EA RID: 234 RVA: 0x00014E44 File Offset: 0x00013044
		public static bool IsStartingRaidStage(Pawn pawn)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null;
			return flag && (raidData.raidStage == RaidData.RaidStage.startAttacking || raidData.raidStage == RaidData.RaidStage.gathering || raidData.raidStage == RaidData.RaidStage.start);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00014E88 File Offset: 0x00013088
		public static bool IsStartingRaidStage(RaidData raidData)
		{
			bool flag = raidData != null;
			return flag && (raidData.raidStage == RaidData.RaidStage.startAttacking || raidData.raidStage == RaidData.RaidStage.gathering || raidData.raidStage == RaidData.RaidStage.start);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00014EC8 File Offset: 0x000130C8
		public static bool ShouldWaitCoverJob(Pawn pawn, IntVec3 focusCell, RaidData raidData)
		{
			bool flag = !SkyAiCore.Settings.enabledWaitSquad || !AdvancedAI.IsValidLoc(focusCell) || raidData == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Pawn pawn2;
				bool flag2 = AdvancedAI_SquadUtility.TryPawnSquadLeader(pawn, raidData, out pawn2) && pawn2 != null;
				if (flag2)
				{
					SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn2);
					bool flag3 = squadData != null && !squadData.squadEnteredSiegeCombat;
					if (flag3)
					{
						IntVec3 intVec;
						bool flag4 = !AdvancedAI_SquadUtility.IsNearCenterOfSquad(pawn, raidData, out intVec) && AdvancedAI.IsValidLoc(intVec) && pawn.Position.DistanceTo(focusCell) < intVec.DistanceTo(focusCell);
						if (flag4)
						{
							return true;
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00014F74 File Offset: 0x00013174
		public static bool IsNearCenterOfSquad(Pawn pawn, RaidData raidData, out IntVec3 center)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord == null;
			bool result;
			if (flag)
			{
				center = IntVec3.Invalid;
				result = false;
			}
			else
			{
				int num = Mathf.FloorToInt(AdvancedAI_SquadUtility.DistanceToSquadCenterBySquadCountCurve.Evaluate((float)lord.ownedPawns.Count));
				float num2;
				bool flag2 = AdvancedAI_SquadUtility.TryPawnDistanceToCenterSquad(pawn, raidData, out num2, out center);
				if (flag2)
				{
					bool flag3 = num2 <= (float)num;
					if (flag3)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00014FEC File Offset: 0x000131EC
		public static bool TryPawnDistanceToCenterSquad(Pawn pawn, RaidData raidData, out float distance, out IntVec3 center)
		{
			bool flag = raidData != null;
			if (flag)
			{
				Pawn pawn2;
				bool flag2 = AdvancedAI_SquadUtility.TryPawnSquadLeader(pawn, raidData, out pawn2);
				if (flag2)
				{
					center = pawn2.Position;
					bool flag3 = pawn == pawn2;
					if (flag3)
					{
						distance = 0f;
						return true;
					}
					distance = pawn.Position.DistanceTo(pawn2.Position);
					return true;
				}
				else
				{
					bool flag4 = AdvancedAI_SquadUtility.TryPawnLordCenterPosition(pawn, out center);
					if (flag4)
					{
						distance = pawn.Position.DistanceTo(center);
						return true;
					}
				}
			}
			center = IntVec3.Invalid;
			distance = 99f;
			return false;
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0001508C File Offset: 0x0001328C
		public static bool TryPawnLordCenterPosition(Pawn pawn, out IntVec3 center)
		{
			AdvancedAI_SquadUtility.<>c__DisplayClass7_0 CS$<>8__locals1 = new AdvancedAI_SquadUtility.<>c__DisplayClass7_0();
			CS$<>8__locals1.pawn = pawn;
			center = IntVec3.Invalid;
			Lord lord = CS$<>8__locals1.pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				IEnumerable<IntVec3> enumerable = from p in lord.ownedPawns
				where p != CS$<>8__locals1.pawn && p.Position.InBounds(CS$<>8__locals1.pawn.Map)
				select p into p1
				select p1.Position;
				bool flag2 = !enumerable.EnumerableNullOrEmpty<IntVec3>();
				if (flag2)
				{
					IntVec3 averageLoc = new IntVec3(Mathf.RoundToInt((float)enumerable.Average((IntVec3 loc) => loc.x)), 0, Mathf.RoundToInt((float)enumerable.Average((IntVec3 loc) => loc.z)));
					float averageDistanceToLoc = enumerable.Average((IntVec3 loc) => loc.DistanceTo(averageLoc));
					IEnumerable<IntVec3> enumerable2 = from loc in enumerable
					where loc.DistanceTo(averageLoc) <= averageDistanceToLoc
					select loc;
					bool flag3 = !enumerable2.EnumerableNullOrEmpty<IntVec3>();
					if (flag3)
					{
						IntVec3 intVec = new IntVec3(Mathf.RoundToInt((float)enumerable2.Average((IntVec3 loc) => loc.x)), 0, Mathf.RoundToInt((float)enumerable2.Average((IntVec3 loc) => loc.z)));
						bool flag4 = !CS$<>8__locals1.<TryPawnLordCenterPosition>g__goodLoc|2(intVec);
						if (flag4)
						{
							intVec = CellFinder.RandomClosewalkCellNear(intVec, CS$<>8__locals1.pawn.Map, 15, (IntVec3 validator) => base.<TryPawnLordCenterPosition>g__goodLoc|2(validator));
						}
						bool flag5 = AdvancedAI.IsValidLoc(intVec);
						if (flag5)
						{
							center = intVec;
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00015284 File Offset: 0x00013484
		public static List<Pawn> ActiveLeadersList(RaidData raidData)
		{
			List<Pawn> list = new List<Pawn>();
			bool flag = raidData != null;
			if (flag)
			{
				bool flag2 = AdvancedAI.RaidLeaderIsActive(raidData);
				if (flag2)
				{
					list.Add(raidData.raidLeader);
				}
				foreach (Pawn pawn in raidData.squadCommanders)
				{
					bool flag3 = AdvancedAI.IsActivePawn(pawn) && !list.Contains(pawn);
					if (flag3)
					{
						bool flag4 = raidData.raidStage != RaidData.RaidStage.fleeing && AdvancedAI.HasExitJob(pawn);
						if (!flag4)
						{
							list.Add(pawn);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00015350 File Offset: 0x00013550
		public static bool TryPawnSquadLeader(Pawn pawn, RaidData raidData, out Pawn squadLeader)
		{
			List<Pawn> list = new List<Pawn>();
			bool flag = raidData != null;
			if (flag)
			{
				bool flag2 = AdvancedAI.RaidLeaderIsActive(raidData);
				if (flag2)
				{
					bool flag3 = pawn == raidData.raidLeader;
					if (flag3)
					{
						squadLeader = pawn;
						return true;
					}
					list.Add(raidData.raidLeader);
				}
				foreach (Pawn pawn2 in raidData.squadCommanders)
				{
					bool flag4 = AdvancedAI.IsActivePawn(pawn2) && !list.Contains(pawn2);
					if (flag4)
					{
						bool flag5 = raidData.raidStage != RaidData.RaidStage.fleeing && AdvancedAI.HasExitJob(pawn2);
						if (!flag5)
						{
							bool flag6 = pawn == pawn2;
							if (flag6)
							{
								squadLeader = pawn;
								return true;
							}
							list.Add(pawn2);
						}
					}
				}
			}
			Lord lord = pawn.GetLord();
			bool flag7 = lord != null;
			if (flag7)
			{
				foreach (Pawn pawn3 in lord.ownedPawns)
				{
					bool flag8 = list.Contains(pawn3);
					if (flag8)
					{
						squadLeader = pawn3;
						return true;
					}
				}
			}
			squadLeader = null;
			return false;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x000154BC File Offset: 0x000136BC
		public static bool PawnIsSiegedPlayerPawn(Pawn source, Thing enemy)
		{
			return enemy == null || enemy.Faction == null || enemy.Faction != Faction.OfPlayer || source.Map == null || !source.Map.IsPlayerHome || source.Map.areaManager.Home[enemy.Position];
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0001551C File Offset: 0x0001371C
		public static bool SquadEnteredSiegeCombat(Pawn pawn, Lord lord)
		{
			bool flag = lord != null;
			if (flag)
			{
				int num = (from p in lord.ownedPawns
				where AdvancedAI.PrimaryWeapon(p) != null && !AdvancedAI.PawnIsDoctor(p)
				select p).Count<Pawn>();
				int num2 = 0;
				int num3 = Mathf.RoundToInt((float)num * 0.33f);
				for (int i = num - 1; i >= 0; i--)
				{
					Pawn pawn2 = lord.ownedPawns[i];
					bool flag2 = pawn2.mindState.enemyTarget != null;
					if (flag2)
					{
						bool flag3 = AdvancedAI_SquadUtility.PawnIsSiegedPlayerPawn(pawn, pawn2.mindState.enemyTarget);
						if (flag3)
						{
							num2++;
							bool flag4 = num2 >= num3;
							if (flag4)
							{
								bool debugLog = SkyAiCore.Settings.debugLog;
								if (debugLog)
								{
									Log.Message(string.Format("{0} {1}: I'am commander. Squad entered in the siege combat: {2}/{3}={4}", new object[]
									{
										pawn,
										pawn.Position,
										num2,
										num3,
										num2 >= num3
									}));
								}
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00015658 File Offset: 0x00013858
		public static List<Pawn> RaidReservedSquads(RaidData raidData)
		{
			List<Pawn> list = new List<Pawn>();
			bool flag = raidData != null;
			if (flag)
			{
				bool flag2 = !raidData.squadCommanders.NullOrEmpty<Pawn>();
				if (flag2)
				{
					foreach (Pawn pawn in raidData.squadCommanders)
					{
						bool flag3 = pawn != null;
						if (flag3)
						{
							SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
							bool flag4 = squadData != null && squadData.isReserved && !list.Contains(pawn);
							if (flag4)
							{
								list.Add(pawn);
							}
						}
					}
				}
				bool flag5 = raidData.raidLeader != null;
				if (flag5)
				{
					SquadData squadData2 = AdvancedAI_SquadUtility.PawnSquadData(raidData.raidLeader);
					bool flag6 = squadData2 != null && squadData2.isReserved && !list.Contains(raidData.raidLeader);
					if (flag6)
					{
						list.Add(raidData.raidLeader);
					}
				}
			}
			return list;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0001576C File Offset: 0x0001396C
		public static bool IsSquadInDefence(Lord squad, RaidData raidData, out IntVec3 point)
		{
			bool flag = raidData.squadDefence == null;
			if (flag)
			{
				raidData.squadDefence = new Dictionary<Lord, bool>();
			}
			bool flag2 = raidData.squadDefencePoint == null;
			if (flag2)
			{
				raidData.squadDefencePoint = new Dictionary<Lord, IntVec3>();
			}
			bool flag3 = raidData != null && squad != null;
			if (flag3)
			{
				foreach (KeyValuePair<Lord, bool> keyValuePair in raidData.squadDefence)
				{
					bool flag4 = keyValuePair.Key != null && keyValuePair.Key == squad;
					if (flag4)
					{
						foreach (KeyValuePair<Lord, IntVec3> keyValuePair2 in raidData.squadDefencePoint)
						{
							bool flag5 = keyValuePair2.Key != null && keyValuePair2.Key == squad;
							if (flag5)
							{
								point = keyValuePair2.Value;
								return keyValuePair.Value;
							}
						}
					}
				}
			}
			point = IntVec3.Invalid;
			return false;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x000158B4 File Offset: 0x00013AB4
		public static void ChangeSquadDefenceStatus(IntVec3 point, Lord squad, RaidData raidData, bool InDefence)
		{
			bool flag = raidData != null && squad != null;
			if (flag)
			{
				bool flag2 = raidData.squadDefence == null;
				if (flag2)
				{
					raidData.squadDefence = new Dictionary<Lord, bool>();
				}
				bool flag3 = !raidData.squadDefence.ContainsKey(squad);
				if (flag3)
				{
					raidData.squadDefence.Add(squad, InDefence);
				}
				else
				{
					foreach (KeyValuePair<Lord, bool> keyValuePair in raidData.squadDefence)
					{
						bool flag4 = keyValuePair.Key != null && keyValuePair.Key == squad;
						if (flag4)
						{
							raidData.squadDefence.Remove(squad);
							break;
						}
					}
					raidData.squadDefence.Add(squad, InDefence);
				}
				AdvancedAI_SquadUtility.SetSquadDefencePoint(squad, raidData, point);
			}
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x000159A0 File Offset: 0x00013BA0
		public static void SetSquadDefencePoint(Lord squad, RaidData raidData, IntVec3 point)
		{
			bool flag = raidData != null && squad != null;
			if (flag)
			{
				bool flag2 = raidData.squadDefencePoint == null;
				if (flag2)
				{
					raidData.squadDefencePoint = new Dictionary<Lord, IntVec3>();
				}
				bool flag3 = !raidData.squadDefencePoint.ContainsKey(squad);
				if (flag3)
				{
					raidData.squadDefencePoint.Add(squad, point);
				}
				else
				{
					foreach (KeyValuePair<Lord, IntVec3> keyValuePair in raidData.squadDefencePoint)
					{
						bool flag4 = keyValuePair.Key != null && keyValuePair.Key == squad;
						if (flag4)
						{
							raidData.squadDefencePoint.Remove(squad);
							break;
						}
					}
					raidData.squadDefencePoint.Add(squad, point);
				}
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00015A80 File Offset: 0x00013C80
		public static int CalculateRaidSquads(Pawn pawn, int minPawnsInSquad = 15, int maxUnitCount = 4)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			int result;
			if (flag)
			{
				IEnumerable<Pawn> source = from p in lord.ownedPawns
				where p != null && !AdvancedAI.PawnIsGuard(p)
				select p;
				int value = Mathf.FloorToInt((float)(source.Count<Pawn>() / minPawnsInSquad));
				IEnumerable<Pawn> source2 = from p in lord.ownedPawns
				where p != null && AdvancedAI.PawnIsDoctor(p)
				select p;
				result = Mathf.Clamp(value, 1, Math.Min(source2.Count<Pawn>(), maxUnitCount));
			}
			else
			{
				result = 1;
			}
			return result;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00015B24 File Offset: 0x00013D24
		public static bool ShouldUseReserveSquad(int count, int initCount, int currentRaidCount, int initRaidCount)
		{
			bool flag = initCount <= 0;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				float num = (float)currentRaidCount / (float)initRaidCount;
				int num2 = Mathf.FloorToInt(AdvancedAI_SquadUtility.ReservedSquadsToUseByRaidCountCurve.Evaluate(num));
				bool flag2 = num2 <= 0;
				if (flag2)
				{
					result = false;
				}
				else
				{
					float num3 = Mathf.Lerp(0f, (float)num2, (float)(count / initCount));
					bool debugLog = SkyAiCore.Settings.debugLog;
					if (debugLog)
					{
						Log.Message(string.Format("ShouldUseReserveSquad. countRatio: {0} result: {1}. Other data: requiredSquadsCount: {2} currentCount: {3} init count: {4} currentRaidCount: {5} initRaidCount: {6}", new object[]
						{
							num,
							num3,
							num2,
							count,
							initCount,
							currentRaidCount,
							initRaidCount
						}));
					}
					result = (num3 >= 0.99f);
				}
			}
			return result;
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00015BFC File Offset: 0x00013DFC
		public static void RaidSquadCommandersReservation(Pawn pawn, RaidData raidData, bool skipStage, int minCount = 2)
		{
			List<Pawn> squadCommanders = raidData.squadCommanders;
			bool flag = !squadCommanders.Contains(raidData.raidLeader);
			if (flag)
			{
				squadCommanders.Add(raidData.raidLeader);
			}
			int num = squadCommanders.Count<Pawn>();
			bool flag2 = num > minCount;
			if (flag2)
			{
				Dictionary<IntVec3, float> dictionary = new Dictionary<IntVec3, float>();
				foreach (Pawn pawn2 in squadCommanders)
				{
					SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn2);
					IntVec3 gatherSpot = squadData.gatherSpot;
					string squadName = squadData.squadName;
					bool flag3 = AdvancedAI.IsValidLoc(gatherSpot) && !dictionary.ContainsKey(gatherSpot);
					if (flag3)
					{
						dictionary.Add(gatherSpot, pawn.Position.DistanceTo(gatherSpot));
						bool debugLog = SkyAiCore.Settings.debugLog;
						if (debugLog)
						{
							Log.Message(string.Format("{0} {1}: SquadCommandersReservation. commander: {2} spot: {3} distance: {4}", new object[]
							{
								pawn,
								pawn.Position,
								pawn2,
								gatherSpot,
								pawn.Position.DistanceTo(gatherSpot)
							}));
						}
					}
				}
				int num2 = Mathf.Clamp(num - minCount, 0, 10);
				int num3 = 0;
				bool flag4 = SkyAiCore.Settings.keepLeaderInReserveIfPossible && num2 > 1;
				if (flag4)
				{
					IntVec3 raidLeaderSpot = AdvancedAI_SquadUtility.PawnSquadData(raidData.raidLeader).gatherSpot;
					KeyValuePair<IntVec3, float> element = (from k in dictionary
					where k.Key == raidLeaderSpot
					select k).FirstOrDefault<KeyValuePair<IntVec3, float>>();
					from wdist in dictionary
					orderby wdist.Value descending
					select wdist;
					dictionary.Prepend(element);
				}
				else
				{
					from wdist in dictionary
					orderby wdist.Value descending
					select wdist;
				}
				foreach (KeyValuePair<IntVec3, float> keyValuePair in dictionary)
				{
					foreach (Pawn pawn3 in squadCommanders)
					{
						bool flag5 = pawn3 != null;
						if (flag5)
						{
							bool flag6 = num3 >= num2;
							if (flag6)
							{
								break;
							}
							SquadData squadData2 = AdvancedAI_SquadUtility.PawnSquadData(pawn3);
							IntVec3 gatherSpot2 = squadData2.gatherSpot;
							string squadName2 = squadData2.squadName;
							List<Pawn> squadPawns = squadData2.squadPawns;
							bool flag7 = gatherSpot2 == keyValuePair.Key;
							if (flag7)
							{
								squadData2.isReserved = true;
								bool debugLog2 = SkyAiCore.Settings.debugLog;
								if (debugLog2)
								{
									Log.Message(string.Format("{0} {1}: as Leader, ordered to move to the reserve squad: {2} with commander: {3}. Commander distance to spot: {4}", new object[]
									{
										pawn,
										pawn.Position,
										squadName2,
										pawn3,
										keyValuePair.Value
									}));
								}
								KeyValuePair<IntVec3, float> keyValuePair2 = dictionary.MinBy((KeyValuePair<IntVec3, float> min) => min.Value);
								float num4 = keyValuePair.Value - keyValuePair2.Value;
								bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag8)
								{
									Log.Message(string.Format("SquadCommandersReservation. distanceFromClosestSpot: {0} is too high?: {1} spotCommander.Value: {2} closestSpot: {3}", new object[]
									{
										num4,
										num4 >= 100f,
										keyValuePair.Value,
										keyValuePair2
									}));
								}
								bool flag9 = num4 >= 100f;
								if (flag9)
								{
									List<IntVec3> list = AdvancedAI_SquadUtility.RaidVacatedSpots(pawn3, raidData);
									bool flag10 = !list.NullOrEmpty<IntVec3>();
									if (flag10)
									{
										IntVec3 intVec = list.FirstOrDefault<IntVec3>();
										bool flag11 = AdvancedAI.IsValidLoc(intVec);
										if (flag11)
										{
											AdvancedAI_SquadUtility.MakeNewStageAttackLordJob(pawn3, squadPawns, intVec, new IntRange(6000, 8000), skipStage);
										}
									}
								}
								Pawn raidLeader = raidData.raidLeader;
								bool flag12 = raidLeader != null;
								if (flag12)
								{
									CompLeaderRole compLeaderRole = raidLeader.TryGetComp<CompLeaderRole>();
									bool flag13 = compLeaderRole != null;
									if (flag13)
									{
										compLeaderRole.usedReservation = true;
									}
								}
								num3++;
							}
						}
						else
						{
							raidData.squadCommanders.Remove(pawn3);
						}
					}
					bool flag14 = num3 >= num2;
					if (flag14)
					{
						break;
					}
				}
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x000160D0 File Offset: 0x000142D0
		public static void MakeNewStageAttackLordJob(Pawn sqCommander, List<Pawn> squad, IntVec3 newSpot, IntRange intRange, bool skipStage)
		{
			Lord oldLord = sqCommander.GetLord();
			int count = squad.Count;
			List<Pawn> list = new List<Pawn>();
			int num = 0;
			int num2 = squad.Count - 1;
			while (num2 >= 0 && !squad.NullOrEmpty<Pawn>())
			{
				Pawn pawn = squad[num2];
				bool flag = pawn != null && !AdvancedAI.PawnIsGuard(pawn);
				if (flag)
				{
					num++;
					list.Add(pawn);
					AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
					pawn.jobs.StopAll(false, true);
					pawn.jobs.ClearQueuedJobs(true);
				}
				num2--;
			}
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(sqCommander);
			List<Lord> list2 = (mapComponent_SkyAI != null) ? mapComponent_SkyAI.lords : null;
			bool flag2 = list2 != null;
			if (flag2)
			{
				Lord lord = (from lr in list2
				where lr == oldLord
				select lr).FirstOrDefault<Lord>();
				bool flag3 = lord != null;
				if (flag3)
				{
					list2.Remove(lord);
				}
			}
			LordJob_StageAttack lordJob_StageAttack = new LordJob_StageAttack(sqCommander, sqCommander.Faction, newSpot, intRange, skipStage, 1);
			Lord lord2 = LordMaker.MakeNewLord(sqCommander.Faction, lordJob_StageAttack, sqCommander.Map, list);
			SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(sqCommander);
			squadData.id = lord2.loadID;
			bool debugLog = SkyAiCore.Settings.debugLog;
			if (debugLog)
			{
				Log.Message(string.Format("{0} {1}: MakeNewStageAttackLordJob. Make new Lord: {2} LordJob: {3} with spot: {4}. Squad pawns successful changed lord: {5}/{6}", new object[]
				{
					sqCommander,
					sqCommander,
					lord2,
					lordJob_StageAttack,
					newSpot,
					num,
					count
				}));
			}
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00016268 File Offset: 0x00014468
		public static List<IntVec3> RaidVacatedSpots(Pawn pawn, RaidData raidData)
		{
			List<IntVec3> list = new List<IntVec3>();
			List<IntVec3> gatherCells = raidData.gatherCells;
			bool flag = !gatherCells.EnumerableNullOrEmpty<IntVec3>();
			if (flag)
			{
				foreach (IntVec3 a in gatherCells)
				{
					List<Pawn> list2 = AdvancedAI_SquadUtility.RaidSquadCommanders(raidData, false);
					bool flag2 = !list2.NullOrEmpty<Pawn>();
					if (flag2)
					{
						foreach (Pawn pawn2 in list2)
						{
							SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn2);
							CompSquadCommanderRole comp = pawn2.GetComp<CompSquadCommanderRole>();
							bool flag3 = comp != null && squadData != null && !squadData.isReserved && a == squadData.gatherSpot;
							if (flag3)
							{
								list.Add(squadData.gatherSpot);
							}
						}
					}
				}
				bool flag4 = raidData.raidLeader != null;
				if (flag4)
				{
					SquadData squadData2 = AdvancedAI_SquadUtility.PawnSquadData(raidData.raidLeader);
					CompLeaderRole compLeaderRole = raidData.raidLeader.TryGetComp<CompLeaderRole>();
					bool flag5 = compLeaderRole != null && squadData2 != null;
					if (flag5)
					{
						list.Add(squadData2.gatherSpot);
					}
				}
			}
			return list;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x000163D0 File Offset: 0x000145D0
		public static List<Pawn> RaidSquadCommanders(RaidData raidData, bool checkReserved)
		{
			List<Pawn> list = new List<Pawn>();
			bool flag = raidData != null;
			if (flag)
			{
				bool flag2 = raidData.raidLeader != null;
				if (flag2)
				{
					SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(raidData.raidLeader);
					bool flag3 = squadData != null;
					if (flag3)
					{
						if (checkReserved)
						{
							SquadData squadData2 = AdvancedAI_SquadUtility.PawnSquadData(raidData.raidLeader);
							bool flag4 = squadData2 != null && !squadData2.isReserved;
							if (flag4)
							{
								list.Add(raidData.raidLeader);
							}
						}
						else
						{
							list.Add(raidData.raidLeader);
						}
					}
				}
				bool flag5 = !raidData.squadCommanders.NullOrEmpty<Pawn>();
				if (flag5)
				{
					IEnumerable<Pawn> source = from p in raidData.squadCommanders
					where p != raidData.raidLeader
					select p;
					for (int i = 0; i < source.Count<Pawn>(); i++)
					{
						Pawn pawn = raidData.squadCommanders[i];
						bool flag6 = pawn != null && !list.Contains(pawn);
						if (flag6)
						{
							if (checkReserved)
							{
								SquadData squadData3 = AdvancedAI_SquadUtility.PawnSquadData(pawn);
								bool flag7 = squadData3 != null && !squadData3.isReserved;
								if (flag7)
								{
									list.Add(pawn);
								}
							}
							else
							{
								list.Add(pawn);
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00016568 File Offset: 0x00014768
		public static float CalculateMaxDistanceToCenter(Pawn pawn, IntVec3 center, IntVec3 cell, IntRange range)
		{
			float num = pawn.Position.DistanceTo(center);
			float num2 = pawn.Position.DistanceTo(cell);
			float num3 = num2 / num;
			return Mathf.Clamp(num * num3, (float)range.min, (float)range.max);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x000165B0 File Offset: 0x000147B0
		public static List<IntVec3> GenerateRaidGatherPositions(Pawn pawn, RaidData raidData, IntVec3 center, int positionCount, IntRange range)
		{
			List<IntVec3> resultCells = new List<IntVec3>();
			IntVec3 intVec = IntVec3.Invalid;
			IEnumerable<IntVec3> enumerable = from c in GenRadial.RadialCellsAround(center, (float)range.min, (float)range.max).InRandomOrder(null)
			where c.InBounds(pawn.Map) && c.Standable(pawn.Map) && !c.Fogged(Find.CurrentMap) && pawn.Position.DistanceTo(c) <= pawn.Position.DistanceTo(center) * SkyAiCore.Settings.gatherPositionRangeMultiplier
			select c;
			bool flag = false;
			IEnumerable<Building> enumerable2 = from turret in pawn.Map.listerBuildings.allBuildingsColonist
			where turret is Building_TurretGunCE || turret is Building_TurretGun
			select turret;
			bool flag2 = !enumerable2.EnumerableNullOrEmpty<Building>();
			if (flag2)
			{
				flag = true;
			}
			List<IntVec3> gatherCells = raidData.gatherCells;
			Func<IntVec3, float> <>9__3;
			for (int i = 0; i < positionCount; i++)
			{
				bool flag3 = resultCells.NullOrEmpty<IntVec3>();
				IEnumerable<IntVec3> enumerable3;
				if (flag3)
				{
					enumerable3 = enumerable;
				}
				else
				{
					IEnumerable<IntVec3> source = enumerable;
					Func<IntVec3, float> keySelector;
					if ((keySelector = <>9__3) == null)
					{
						keySelector = (<>9__3 = ((IntVec3 c) => base.<GenerateRaidGatherPositions>g__minDistanceToCellInList|2(c)));
					}
					enumerable3 = source.OrderByDescending(keySelector);
				}
				foreach (IntVec3 intVec2 in enumerable3)
				{
					bool flag4 = pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn) && intVec2.Standable(pawn.Map) && !resultCells.Contains(intVec2);
					if (flag4)
					{
						bool flag5 = intVec2.DistanceTo(center) < AdvancedAI_SquadUtility.CalculateMaxDistanceToCenter(pawn, center, intVec2, range);
						if (!flag5)
						{
							bool flag6 = flag;
							if (flag6)
							{
								foreach (Building building in enumerable2)
								{
									bool flag7 = GenSight.LineOfSight(building.Position, intVec2, pawn.Map, false, null, 0, 0);
									if (flag7)
									{
										bool flag8 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag8)
										{
											pawn.Map.debugDrawer.FlashCell(intVec2, 0.7f, "Z", 5000);
										}
									}
									else
									{
										foreach (IntVec3 intVec3 in GenRadial.RadialCellsAround(intVec2, 5f, true))
										{
											bool flag9 = GenSight.LineOfSight(building.Position, intVec3, pawn.Map, false, null, 0, 0);
											if (flag9)
											{
												bool flag10 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
												if (flag10)
												{
													pawn.Map.debugDrawer.FlashCell(intVec3, 0.7f, "Z", 5000);
												}
											}
										}
									}
								}
							}
							int num = 0;
							foreach (IntVec3 intVec4 in GenRadial.RadialCellsAround(intVec2, 6f, true))
							{
								bool flag11 = AdvancedAI.IsWater(intVec4, pawn.Map);
								if (flag11)
								{
									bool flag12 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
									if (flag12)
									{
										pawn.Map.debugDrawer.FlashCell(intVec4, 0.7f, "Z", 5000);
									}
									num++;
								}
							}
							bool flag13 = num > 2;
							if (!flag13)
							{
								intVec = intVec2;
								break;
							}
						}
					}
				}
				bool flag14 = !AdvancedAI.IsValidLoc(intVec);
				if (flag14)
				{
					foreach (IntVec3 intVec5 in enumerable3)
					{
						bool flag15 = pawn.CanReach(intVec5, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn) && intVec5.Standable(pawn.Map) && !resultCells.Contains(intVec5);
						if (flag15)
						{
							bool flag16 = intVec5.DistanceTo(center) < AdvancedAI_SquadUtility.CalculateMaxDistanceToCenter(pawn, center, intVec5, range);
							if (!flag16)
							{
								int num2 = 0;
								foreach (IntVec3 intVec6 in GenRadial.RadialCellsAround(intVec5, 6f, true))
								{
									bool flag17 = AdvancedAI.IsWater(intVec6, pawn.Map);
									if (flag17)
									{
										bool flag18 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
										if (flag18)
										{
											pawn.Map.debugDrawer.FlashCell(intVec6, 0.7f, "Z", 5000);
										}
										num2++;
									}
								}
								bool flag19 = num2 > 2;
								if (!flag19)
								{
									intVec = intVec5;
									break;
								}
							}
						}
					}
				}
				bool flag20 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag20)
				{
					foreach (IntVec3 c3 in enumerable3)
					{
						pawn.Map.debugDrawer.FlashCell(c3, 0.7f, null, 5000);
					}
				}
				bool flag21 = AdvancedAI.IsValidLoc(intVec);
				if (flag21)
				{
					resultCells.Add(intVec);
				}
			}
			bool flag22 = SkyAiCore.Settings.debugPath && SkyAiCore.SelectedPawnDebug(pawn);
			if (flag22)
			{
				for (int j = 0; j < resultCells.Count<IntVec3>(); j++)
				{
					float colorPct = (float)j * 0.1f + 0.1f;
					foreach (IntVec3 c2 in GenRadial.RadialCellsAround(resultCells[j], 20f, true))
					{
						pawn.Map.debugDrawer.FlashCell(c2, colorPct, j.ToString(), 5000);
					}
				}
			}
			return resultCells;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00016D40 File Offset: 0x00014F40
		public static string SquadName(int id)
		{
			string result;
			switch (id)
			{
			case 0:
				result = "Alpha";
				break;
			case 1:
				result = "Bravo";
				break;
			case 2:
				result = "Charly";
				break;
			case 3:
				result = "Delta";
				break;
			case 4:
				result = "Echo";
				break;
			case 5:
				result = "Foxtrott";
				break;
			case 6:
				result = "India";
				break;
			case 7:
				result = "Juliett";
				break;
			case 8:
				result = "November";
				break;
			case 9:
				result = "Oscar";
				break;
			default:
				result = "Zeta";
				break;
			}
			return result;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00016DDC File Offset: 0x00014FDC
		public static bool IsRaidLeaderSquad(List<Pawn> squad)
		{
			return squad.Any((Pawn p) => AdvancedAI.PawnIsLeader(p));
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00016E14 File Offset: 0x00015014
		public static Lord QuickLordFromRaidPawn(RaidData raidData)
		{
			Pawn raidLeader = raidData.raidLeader;
			bool flag = raidLeader != null && AdvancedAI.RaidLeaderIsActive(raidData);
			if (flag)
			{
				Lord lord = raidLeader.GetLord();
				bool flag2 = lord != null;
				if (flag2)
				{
					return lord;
				}
			}
			else
			{
				foreach (Pawn pawn in raidData.squadCommanders)
				{
					bool flag3 = pawn != null && pawn.GetLord() != null;
					if (flag3)
					{
						return pawn.GetLord();
					}
				}
				foreach (List<Pawn> list in from squadData in raidData.squads
				select squadData.squadPawns)
				{
					foreach (Pawn p in list)
					{
						bool flag4 = p.GetLord() != null;
						if (flag4)
						{
							return p.GetLord();
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00016F88 File Offset: 0x00015188
		public static List<Pawn> RaidLordPawns(RaidData raidData)
		{
			List<Pawn> list = new List<Pawn>();
			foreach (Lord lord in AdvancedAI_LordUtility.RaidLords(raidData))
			{
				list.AddRange(lord.ownedPawns);
			}
			return list;
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00016FF4 File Offset: 0x000151F4
		public static int MissingCommanderSquadsCount(RaidData raidData)
		{
			bool flag = raidData.squads.NullOrEmpty<SquadData>();
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int num = 0;
				foreach (SquadData squadData2 in raidData.squads)
				{
					bool flag2 = squadData2.squadCommander != null && squadData2.squadCommander != raidData.raidLeader && AdvancedAI.IsActivePawn(squadData2.squadCommander);
					if (flag2)
					{
						num++;
					}
				}
				int num2 = (from squadData in raidData.squads
				where squadData.squadCommander != null && squadData.squadCommander != raidData.raidLeader
				select squadData).Count<SquadData>();
				result = Mathf.Max(num2 - num, 0);
			}
			return result;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x000170E4 File Offset: 0x000152E4
		public static float FactionFleeValue(RaidData raidData)
		{
			Lord lord = AdvancedAI_SquadUtility.QuickLordFromRaidPawn(raidData);
			bool flag = lord != null;
			float result;
			if (flag)
			{
				float num = 0f;
				num += (float)AdvancedAI_SquadUtility.MissingCommanderSquadsCount(raidData) * SkyAiCore.Settings.spiritLossMultiplierSquadCommanderLost;
				bool flag2 = !AdvancedAI.RaidLeaderIsActive(raidData);
				if (flag2)
				{
					num += SkyAiCore.Settings.spiritLossMultiplierRaidLeaderLost;
				}
				result = Mathf.Clamp01(num + SkyAiCore.Settings.raidFleeMultiplier + (1f - raidData.faction.def.attackersDownPercentageRangeForAutoFlee.RandomInRangeSeeded(lord.loadID)));
			}
			else
			{
				result = 0f;
			}
			return result;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x0001717C File Offset: 0x0001537C
		public static bool RaidIsCapableOfFighting(RaidData raidData, float ratio)
		{
			Faction faction = raidData.faction;
			bool flag = faction != null && !faction.IsPlayer && !faction.def.autoFlee && faction.neverFlee;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				List<Pawn> source = AdvancedAI_SquadUtility.RaidLordPawns(raidData);
				float num = (float)source.Count<Pawn>() / (float)raidData.raidCount;
				float num2 = AdvancedAI_SquadUtility.FactionFleeValue(raidData);
				bool debugRaidData = SkyAiCore.Settings.debugRaidData;
				if (debugRaidData)
				{
					Log.Message(string.Format("RaidIsCapableOfFighting: Checking the status of the raid. cur/initial=raidRatio/FleeRatio: {0}/{1}={2}/{3} result: {4}", new object[]
					{
						source.Count<Pawn>(),
						raidData.raidCount,
						num,
						num2,
						num2 >= num
					}));
				}
				bool flag2 = num2 >= num;
				if (flag2)
				{
					bool debugRaidData2 = SkyAiCore.Settings.debugRaidData;
					if (debugRaidData2)
					{
						Log.Message("RaidIsCapableOfFighting: Raid is start fleeing!");
					}
					result = false;
				}
				else
				{
					int num3 = 0;
					List<Pawn> list = new List<Pawn>();
					IEnumerable<Pawn> enumerable = from p in source
					where p != raidData.raidLeader && !raidData.squadCommanders.Contains(p) && !raidData.raidDoctors.Contains(p)
					select p;
					foreach (Pawn pawn in enumerable)
					{
						bool flag3 = !AdvancedAI.IsBioHumanlikeOnly(pawn);
						if (flag3)
						{
							num3++;
						}
						else
						{
							ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(pawn);
							bool flag4 = thingWithComps != null;
							if (flag4)
							{
								CompInventory compInventory = pawn.TryGetComp<CompInventory>();
								bool flag5 = thingWithComps.def.IsMeleeWeapon || AdvancedAI.HasAmmoForWeaponInInventory(thingWithComps, compInventory);
								if (flag5)
								{
									num3++;
								}
								else
								{
									list.Add(pawn);
								}
							}
						}
					}
					float num4 = (float)num3 / (float)enumerable.Count<Pawn>();
					bool flag6 = num4 <= ratio;
					bool debugRaidData3 = SkyAiCore.Settings.debugRaidData;
					if (debugRaidData3)
					{
						Log.Message(string.Format("RaidIsCapableOfFighting: Checking the combat capability of the raid. armed/overall: {0}/{1}={2} data: {3}/{4} result: {5} Problem pawns: {6}", new object[]
						{
							num3,
							enumerable.Count<Pawn>(),
							num4,
							num4,
							ratio,
							flag6,
							GeneralExtensions.Join<Pawn>(list, null, ", ")
						}));
						bool flag7 = flag6;
						if (flag7)
						{
							Log.Message("RaidIsCapableOfFighting: Raid is start fleeing!");
						}
					}
					result = !flag6;
				}
			}
			return result;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00017420 File Offset: 0x00015620
		public static void SetGatherSpotsToSquads(Pawn pawn, RaidData raidData, int initLordCount, List<IntVec3> gatherSpots)
		{
			List<SquadData> squads = raidData.squads;
			int num = 0;
			IntVec3 intVec = gatherSpots.MinBy((IntVec3 closestSpot) => pawn.Position.DistanceTo(closestSpot));
			gatherSpots.Remove(intVec);
			for (int i = 0; i < squads.Count<SquadData>(); i++)
			{
				SquadData squadData = squads[i];
				List<Pawn> squadPawns = squadData.squadPawns;
				int num2 = squadPawns.Count<Pawn>();
				bool flag = AdvancedAI_SquadUtility.IsRaidLeaderSquad(squadPawns);
				IntVec3 intVec2;
				if (flag)
				{
					intVec2 = intVec;
				}
				else
				{
					intVec2 = gatherSpots.RandomElement<IntVec3>();
					gatherSpots.Remove(intVec2);
				}
				List<Pawn> list = new List<Pawn>();
				int num3 = squadPawns.Count - 1;
				while (num3 >= 0 && !squadPawns.NullOrEmpty<Pawn>())
				{
					bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag2)
					{
						Log.Message(string.Format("SetGatherSpotsToSquads LOG: Element: {0}  pawn: {1}  count: {2} to spot: {3}", new object[]
						{
							num3,
							squadPawns[num3],
							squadPawns.Count,
							intVec2
						}));
					}
					Pawn pawn2 = squadPawns[num3];
					bool flag3 = pawn2 != null && !AdvancedAI.PawnIsGuard(pawn2);
					if (flag3)
					{
						num++;
						list.Add(pawn2);
						AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn2);
					}
					num3--;
				}
				foreach (Pawn pawn3 in squadPawns)
				{
					CompSquadCommanderRole comp = pawn3.GetComp<CompSquadCommanderRole>();
					CompLeaderRole comp2 = pawn3.GetComp<CompLeaderRole>();
					bool flag4 = comp2 != null || comp != null;
					if (flag4)
					{
						bool flag5 = comp2 != null;
						if (flag5)
						{
							squadData.gatherSpot = intVec2;
						}
						bool flag6 = comp != null;
						if (flag6)
						{
							squadData.gatherSpot = intVec2;
						}
						raidData.squadsFormed = true;
						bool flag7 = raidData != null && raidData.raidStage != RaidData.RaidStage.gathering;
						if (flag7)
						{
							raidData.raidStage = RaidData.RaidStage.gathering;
						}
						bool debugLog = SkyAiCore.Settings.debugLog;
						if (debugLog)
						{
							bool flag8 = comp != null;
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: SetGatherSpotsToSquads. {2} is leader and created squad: ({3}, squad leader: {4}) for assault. Gather spot on {5}. Success squad for new lord: {6}/{7} Overall lord stat: {8}/{9} squadsFormed: {10}", new object[]
								{
									pawn,
									pawn.Position,
									pawn,
									squadData.squadName,
									pawn3,
									intVec2,
									list.Count,
									num2,
									num,
									initLordCount - raidData.leaderGuards.Count,
									raidData.squadsFormed
								}));
							}
							bool flag9 = comp2 != null;
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: SetGatherSpotsToSquads. {2} is leader and created own squad: ({3}) for assault. Gather spot on {4}. Success squad for new lord: {5}/{6} Overall lord stat: {7}/{8} squadsFormed: {9}", new object[]
								{
									pawn,
									pawn.Position,
									pawn,
									squadData.squadName,
									intVec2,
									list.Count,
									num2,
									num,
									initLordCount - raidData.leaderGuards.Count,
									raidData.squadsFormed
								}));
							}
						}
					}
				}
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x000177DC File Offset: 0x000159DC
		public static SquadData PawnSquadData(Pawn pawn)
		{
			MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(pawn);
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = mapComponent_SkyAI != null && !mapComponent_SkyAI.raidData.NullOrEmpty<RaidData>();
				if (flag2)
				{
					foreach (List<SquadData> list in from raidData in mapComponent_SkyAI.raidData
					select raidData.squads)
					{
						foreach (SquadData squadData in list)
						{
							bool flag3 = squadData.id.Equals(lord.loadID);
							if (flag3)
							{
								return squadData;
							}
						}
					}
				}
			}
			bool flag4 = mapComponent_SkyAI != null && !mapComponent_SkyAI.raidData.NullOrEmpty<RaidData>();
			if (flag4)
			{
				IEnumerable<RaidData> raidData2 = mapComponent_SkyAI.raidData;
				Func<RaidData, bool> <>9__1;
				Func<RaidData, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((RaidData raiData) => raiData.faction == pawn.Faction));
				}
				Func<SquadData, bool> <>9__3;
				foreach (List<SquadData> list2 in from raidData in raidData2.Where(predicate)
				select raidData.squads)
				{
					IEnumerable<SquadData> source = list2;
					Func<SquadData, bool> predicate2;
					if ((predicate2 = <>9__3) == null)
					{
						predicate2 = (<>9__3 = ((SquadData p) => p.squadPawns.Contains(pawn)));
					}
					using (IEnumerator<SquadData> enumerator4 = source.Where(predicate2).GetEnumerator())
					{
						if (enumerator4.MoveNext())
						{
							return enumerator4.Current;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00017A18 File Offset: 0x00015C18
		public static bool TryCreateSquads(Pawn leader, RaidData raidData, int initLordCount, int minPawnsInSquad = 15, int maxSquadCount = 4)
		{
			List<SquadData> squads = raidData.squads;
			bool flag = !squads.NullOrEmpty<SquadData>();
			if (flag)
			{
				for (int i = squads.Count - 1; i >= 0; i--)
				{
					SquadData squadData = squads[i];
					bool flag2 = squadData != null;
					if (flag2)
					{
						bool debugRaidData = SkyAiCore.Settings.debugRaidData;
						if (debugRaidData)
						{
							Log.Message(string.Format("CreateSquads. Found simple squad. Remove: {0} with commander: {1}", squadData, squadData.squadCommander));
						}
						squads.Remove(squadData);
					}
				}
			}
			int num = 0;
			List<List<Pawn>> list = new List<List<Pawn>>();
			Lord lord = leader.GetLord();
			bool flag3 = lord != null;
			if (flag3)
			{
				num = AdvancedAI_SquadUtility.CalculateRaidSquads(leader, minPawnsInSquad, maxSquadCount);
				List<Pawn> list2 = (from p in lord.ownedPawns
				where p != null && !AdvancedAI.PawnIsGuard(p)
				select p).ToList<Pawn>();
				bool debugRaidData2 = SkyAiCore.Settings.debugRaidData;
				if (debugRaidData2)
				{
					Log.Message(string.Format("CreateSquads. Squad count: {0} Overall pawns count: {1}", num, list2.Count<Pawn>()));
				}
				for (int j = 0; j < num; j++)
				{
					List<Pawn> list3 = new List<Pawn>();
					int num2 = 0;
					bool flag4 = !list3.NullOrEmpty<Pawn>();
					if (flag4)
					{
						for (int k = 0; k < list3.Count<Pawn>(); k++)
						{
							bool flag5 = AdvancedAI.PawnIsDoctor(list3[k]);
							if (flag5)
							{
								num2++;
							}
						}
					}
					bool flag6 = num2 < 1;
					if (flag6)
					{
						int num3 = list2.Count<Pawn>() - 1;
						while (num3 >= 0 && !list2.NullOrEmpty<Pawn>())
						{
							Pawn pawn = list2[num3];
							bool flag7 = AdvancedAI.PawnIsDoctor(pawn);
							if (flag7)
							{
								list3.Add(pawn);
								list2.Remove(pawn);
								bool debugRaidData3 = SkyAiCore.Settings.debugRaidData;
								if (debugRaidData3)
								{
									Log.Message(string.Format("CreateSquads. First step. Add doctor: ({0}) {1} to squad: {2} squad count: {3} pawns count: {4}", new object[]
									{
										num3,
										pawn,
										AdvancedAI_SquadUtility.SquadName(j),
										list3.Count,
										list2.Count<Pawn>()
									}));
								}
								break;
							}
							num3--;
						}
					}
					int num4 = list2.Count<Pawn>() - 1;
					while (num4 >= 0 && !list2.NullOrEmpty<Pawn>())
					{
						Pawn pawn2 = list2[num4];
						bool flag8 = !AdvancedAI.PawnIsDoctor(pawn2);
						if (flag8)
						{
							list3.Add(pawn2);
							list2.Remove(pawn2);
							bool debugRaidData4 = SkyAiCore.Settings.debugRaidData;
							if (debugRaidData4)
							{
								Log.Message(string.Format("CreateSquads. First step. Add pawn: ({0}) {1} to squad: {2} squad count: {3} pawns count remain: {4}", new object[]
								{
									num4,
									pawn2,
									AdvancedAI_SquadUtility.SquadName(j),
									list3.Count,
									list2.Count<Pawn>()
								}));
							}
						}
						bool flag9 = list3.Count >= minPawnsInSquad && maxSquadCount > 1;
						if (flag9)
						{
							break;
						}
						num4--;
					}
					list.Add(list3);
				}
				bool flag10 = !list2.NullOrEmpty<Pawn>() && !list.NullOrEmpty<List<Pawn>>();
				if (flag10)
				{
					int num5 = Mathf.RoundToInt((float)(list2.Count<Pawn>() / list.Count<List<Pawn>>()));
					int num6 = list2.Count<Pawn>() - 1;
					while (num6 >= 0 && !list2.NullOrEmpty<Pawn>())
					{
						using (IEnumerator<List<Pawn>> enumerator = list.InRandomOrder(null).GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								List<Pawn> list4 = enumerator.Current;
								Pawn pawn3 = list2[num6];
								list4.Add(pawn3);
								list2.Remove(pawn3);
								bool debugRaidData5 = SkyAiCore.Settings.debugRaidData;
								if (debugRaidData5)
								{
									int id = list.IndexOf(list4);
									Log.Message(string.Format("CreateSquads. Second step: Add pawn: ({0}) {1} to squad: {2} squad count: {3} pawns count remain: {4}", new object[]
									{
										num6,
										pawn3,
										AdvancedAI_SquadUtility.SquadName(id),
										list4.Count,
										list2.Count<Pawn>()
									}));
								}
							}
						}
						num6--;
					}
				}
				bool flag11 = !list2.NullOrEmpty<Pawn>() && !list.NullOrEmpty<List<Pawn>>();
				if (flag11)
				{
					int num7 = list2.Count<Pawn>() - 1;
					while (num7 >= 0 && !list2.NullOrEmpty<Pawn>())
					{
						Pawn pawn4 = list2[num7];
						List<Pawn> list5 = list.RandomElement<List<Pawn>>();
						bool flag12 = !list5.NullOrEmpty<Pawn>();
						if (flag12)
						{
							list5.Add(pawn4);
							list2.Remove(pawn4);
							bool debugRaidData6 = SkyAiCore.Settings.debugRaidData;
							if (debugRaidData6)
							{
								Log.Message(string.Format("CreateSquads. Third step: Add pawn: ({0}) {1} to squad count: {2} pawns count remain: {3}", new object[]
								{
									num7,
									pawn4,
									list5.Count,
									list2.Count<Pawn>()
								}));
							}
						}
						num7--;
					}
				}
				foreach (List<Pawn> squadPawns in list)
				{
					SquadData item = new SquadData(squadPawns);
					bool flag13 = !raidData.squads.Contains(item);
					if (flag13)
					{
						raidData.squads.Add(item);
					}
				}
			}
			bool flag14 = !list.NullOrEmpty<List<Pawn>>();
			bool result;
			if (flag14)
			{
				bool debugLog = SkyAiCore.Settings.debugLog;
				if (debugLog)
				{
					Log.Message(string.Format("{0} {1}: CreateSquads. Squads generated. Overall squads count: {2} Raid count (Guards + Pawns) : {3} + {4} = {5}", new object[]
					{
						leader,
						leader.Position,
						num,
						raidData.leaderGuards.Count,
						initLordCount - raidData.leaderGuards.Count,
						initLordCount
					}));
					foreach (List<Pawn> list6 in list)
					{
						Log.Message(string.Format("{0} {1}: CreateSquads. Generating squads. Squad count: {2} Overall squads: {3} Raid count (Guards + Pawns) : {4} + {5} = {6}", new object[]
						{
							leader,
							leader.Position,
							list6.Count,
							list.Count,
							raidData.leaderGuards.Count,
							initLordCount - raidData.leaderGuards.Count,
							initLordCount
						}));
					}
				}
				result = true;
			}
			else
			{
				Log.Error("CreateSquads. Squads list is empty. This shouldn't been happen. Failed to created squads.");
				result = false;
			}
			return result;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00018148 File Offset: 0x00016348
		public static void GenerateLeadersForSquads(Pawn leader, RaidData raidData)
		{
			List<SquadData> squads = raidData.squads;
			bool flag = !squads.NullOrEmpty<SquadData>();
			if (flag)
			{
				foreach (SquadData squadData in squads)
				{
					bool flag2 = !squadData.squadPawns.NullOrEmpty<Pawn>();
					if (flag2)
					{
						int id = squads.IndexOf(squadData);
						Pawn raidLeader = (from p in squadData.squadPawns
						where AdvancedAI.PawnIsLeader(p)
						select p).FirstOrDefault<Pawn>();
						bool flag3 = raidLeader != null;
						if (flag3)
						{
							squadData.squadCommander = raidLeader;
							squadData.squadName = AdvancedAI_SquadUtility.SquadName(id);
							bool debugLog = SkyAiCore.Settings.debugLog;
							if (debugLog)
							{
								Log.Message(string.Format("{0} {1}: GenerateLeadersForSquads. Creating squadCommanders. Squad: {2} already has raid leader. Passing...", leader, leader.Position, AdvancedAI_SquadUtility.SquadName(id)));
							}
						}
						else
						{
							Pawn pawn = (from p in squadData.squadPawns
							where p != raidLeader && AdvancedAI.IsGoodLeader(p)
							select p into p1
							orderby AdvancedAI.ShooterSkill(p1) descending
							select p1).ThenByDescending((Pawn p2) => AdvancedAI.MostExperienced(p2)).FirstOrDefault<Pawn>();
							RaidData raidData2 = AdvancedAI.PawnRaidData(pawn);
							bool flag4 = raidData2 != null && pawn != null;
							if (flag4)
							{
								CompSquadCommanderRole compSquadCommanderRole = pawn.TryGetComp<CompSquadCommanderRole>();
								bool flag5 = compSquadCommanderRole == null;
								if (flag5)
								{
									compSquadCommanderRole = (CompSquadCommanderRole)Activator.CreateInstance(typeof(CompSquadCommanderRole));
									compSquadCommanderRole.parent = pawn;
									pawn.AllComps.Add(compSquadCommanderRole);
									squadData.squadName = AdvancedAI_SquadUtility.SquadName(id);
								}
								bool flag6 = raidData2.squadCommanders == null;
								if (flag6)
								{
									raidData2.squadCommanders = new List<Pawn>();
								}
								squadData.squadCommander = pawn;
								raidData2.squadCommanders.Add(pawn);
								bool debugLog2 = SkyAiCore.Settings.debugLog;
								if (debugLog2)
								{
									Log.Message(string.Format("{0} {1}: GenerateLeadersForSquads. I'm new squad commander for {2}", pawn, pawn.Position, AdvancedAI_SquadUtility.SquadName(id)));
								}
							}
							else
							{
								bool debugLog3 = SkyAiCore.Settings.debugLog;
								if (debugLog3)
								{
									Log.Message(string.Format("{0} {1}: GenerateLeadersForSquads. Creating squadCommanders. Failed to find any person for squad leader.", leader, leader.Position));
								}
							}
						}
					}
					else
					{
						Log.Error("GenerateLeadersForSquads. Squad list is empty. This shouldn't been happen.");
					}
				}
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x0001840C File Offset: 0x0001660C
		public static bool TryGenerateGatherSpots(Pawn leader, RaidData raidData, IntVec3 leaderFocusCell, out List<IntVec3> generatedGatherSpots)
		{
			List<SquadData> squads = raidData.squads;
			int count = squads.Count;
			List<IntVec3> gatherCells = raidData.gatherCells;
			IntRange range = new IntRange(80, 119);
			generatedGatherSpots = AdvancedAI_SquadUtility.GenerateRaidGatherPositions(leader, raidData, leaderFocusCell, count, range);
			for (int i = 0; i < generatedGatherSpots.Count<IntVec3>(); i++)
			{
				gatherCells.Add(generatedGatherSpots[i]);
				bool debugLog = SkyAiCore.Settings.debugLog;
				if (debugLog)
				{
					leader.Map.debugDrawer.FlashCell(generatedGatherSpots[i], 0.96f, "SPOT", 5000);
					Log.Message(string.Format("{0} {1}: GenerateGatherSpots. Selected gatherSpot (k) : {2} squads count: {3}", new object[]
					{
						leader,
						leader.Position,
						generatedGatherSpots[i],
						count
					}));
				}
			}
			bool flag = generatedGatherSpots.NullOrEmpty<IntVec3>() || generatedGatherSpots.Count < count;
			if (flag)
			{
				int num = Mathf.Abs(count - generatedGatherSpots.Count);
				for (int j = 0; j < num; j++)
				{
					IntVec3 intVec;
					bool flag2 = RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(leader.Position, leader.Map, 80f, out intVec);
					if (flag2)
					{
						bool debugLog2 = SkyAiCore.Settings.debugLog;
						if (debugLog2)
						{
							Log.Message(string.Format("{0} {1}: GenerateGatherSpots. Not enough gatherspots generated. Using TryFindRandomCellOutsideColonyNearTheCenterOfTheMap generated spot {2}", leader, leader.Position, intVec));
						}
						generatedGatherSpots.Add(intVec);
					}
				}
			}
			bool flag3 = !generatedGatherSpots.NullOrEmpty<IntVec3>();
			bool result;
			if (flag3)
			{
				result = true;
			}
			else
			{
				AdvancedAI_SquadUtility.UpdateStageForSiegeAI(leader);
				switch (raidData.raidStage)
				{
				case RaidData.RaidStage.start:
					raidData.raidStage = RaidData.RaidStage.startAttacking;
					goto IL_1FB;
				case RaidData.RaidStage.defending:
					raidData.raidStage = RaidData.RaidStage.attack;
					goto IL_1FB;
				case RaidData.RaidStage.gathering:
					raidData.raidStage = RaidData.RaidStage.startAttacking;
					goto IL_1FB;
				case RaidData.RaidStage.startAttacking:
					raidData.raidStage = RaidData.RaidStage.attack;
					goto IL_1FB;
				}
				raidData.raidStage = RaidData.RaidStage.attack;
				IL_1FB:
				Log.Error(string.Format("GenerateGatherSpots. Gatherspot list is empty. This shouldn't been happen. Changed raidStage to: {0}", raidData.raidStage));
				result = false;
			}
			return result;
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00018638 File Offset: 0x00016838
		public static void MakeSimpleSquad(Pawn leader, RaidData raidData, bool enableSquadFormed)
		{
			Lord lord = leader.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				SquadData squadData = new SquadData(lord.ownedPawns);
				bool flag2 = raidData != null && !raidData.squads.Contains(squadData);
				if (flag2)
				{
					raidData.squads.Add(squadData);
					squadData.id = lord.loadID;
					squadData.squadName = AdvancedAI_SquadUtility.SquadName(0);
					bool flag3 = raidData.raidLeader != null;
					if (flag3)
					{
						squadData.squadCommander = raidData.raidLeader;
					}
					else
					{
						squadData.squadCommander = (from p in lord.ownedPawns
						orderby AdvancedAI.ShooterSkill(p) descending, AdvancedAI.MostExperienced(p) descending
						select p).FirstOrDefault<Pawn>();
					}
					bool debugLog = SkyAiCore.Settings.debugLog;
					if (debugLog)
					{
						Log.Message(string.Format("{0} {1}: MakeSimpleSquad. Generated simple squad: {2} with id: {3} commander: {4} count: {5}", new object[]
						{
							leader,
							leader.Position,
							squadData.squadName,
							squadData.id,
							squadData.squadCommander,
							squadData.squadPawns.Count<Pawn>()
						}));
					}
					if (enableSquadFormed)
					{
						raidData.squadsFormed = true;
					}
				}
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x000187A0 File Offset: 0x000169A0
		public static void FormingSquadsAndGatherPoints(Pawn leader, IntVec3 leaderFocusCell, RaidData raidData)
		{
			Lord lord = leader.GetLord();
			bool flag = lord != null && raidData != null && lord.LordJob is LordJob_AssaultColony && !raidData.squadsFormed;
			if (flag)
			{
				int count = lord.ownedPawns.Count;
				bool flag2 = lord.ownedPawns.Any((Pawn p) => p.CanTradeNow) || TraderCaravanUtility.FindTrader(lord) != null;
				if (flag2)
				{
					bool debugLog = SkyAiCore.Settings.debugLog;
					if (debugLog)
					{
						Log.Message(string.Format("{0} {1}: FormingSquadsAndGatherPoints. Forming squads failed. Found trader in lord.", leader, leader.Position));
					}
				}
				else
				{
					bool debugLog2 = SkyAiCore.Settings.debugLog;
					if (debugLog2)
					{
						Log.Message(string.Format("{0} {1}: FormingSquadsAndGatherPoints. Start to change LordJob_AssaultColony to LordJob_GatherBeforeAttack", leader, leader.Position));
					}
					bool flag3 = AdvancedAI_SquadUtility.TryCreateSquads(leader, raidData, count, SkyAiCore.Settings.minPawnsSquadAmount, SkyAiCore.Settings.maxSquadUnitsCount);
					if (flag3)
					{
						AdvancedAI_SquadUtility.GenerateLeadersForSquads(leader, raidData);
						List<IntVec3> gatherSpots;
						bool flag4 = AdvancedAI_SquadUtility.TryGenerateGatherSpots(leader, raidData, leaderFocusCell, out gatherSpots);
						if (flag4)
						{
							AdvancedAI_SquadUtility.SetGatherSpotsToSquads(leader, raidData, count, gatherSpots);
						}
					}
				}
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x000188D0 File Offset: 0x00016AD0
		public static void UpdateStageForSiegeAI(Pawn pawn)
		{
			SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
			bool flag = squadData != null;
			if (flag)
			{
				SquadAttackComponent component = pawn.Map.GetComponent<SquadAttackComponent>();
				bool flag2 = component != null;
				if (flag2)
				{
					component.Notify_StageChanged(squadData);
				}
			}
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00018910 File Offset: 0x00016B10
		public static void UpdateStageForSiegeAI(RaidData raidData)
		{
			List<SquadData> squads = raidData.squads;
			bool flag = !squads.NullOrEmpty<SquadData>();
			if (flag)
			{
				foreach (SquadData squadData in squads)
				{
					SquadAttackComponent component = squadData.Map.GetComponent<SquadAttackComponent>();
					bool flag2 = component != null;
					if (flag2)
					{
						component.Notify_StageChanged(squadData);
					}
				}
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00018998 File Offset: 0x00016B98
		public static void UpdateSquadID(Pawn pawn, SquadData squadData)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				squadData.id = lord.loadID;
			}
		}

		// Token: 0x04000059 RID: 89
		public static SimpleCurve ReservedSquadsToUseByRaidCountCurve = new SimpleCurve
		{
			{
				new CurvePoint(1f, 0f),
				true
			},
			{
				new CurvePoint(0.86f, 0f),
				true
			},
			{
				new CurvePoint(0.85f, 1f),
				true
			},
			{
				new CurvePoint(0.7f, 2f),
				true
			},
			{
				new CurvePoint(0.55f, 3f),
				true
			},
			{
				new CurvePoint(0f, 100f),
				true
			}
		};

		// Token: 0x0400005A RID: 90
		public static SimpleCurve DistanceToSquadCenterBySquadCountCurve = new SimpleCurve
		{
			{
				new CurvePoint(1f, 11f),
				true
			},
			{
				new CurvePoint(25f, 19f),
				true
			},
			{
				new CurvePoint(50f, 26f),
				true
			},
			{
				new CurvePoint(75f, 33f),
				true
			}
		};
	}
}
