using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000016 RID: 22
	public static class AdvancedAI_LordUtility
	{
		// Token: 0x06000092 RID: 146 RVA: 0x0000B8C0 File Offset: 0x00009AC0
		public static List<Lord> RaidLords(RaidData raidData)
		{
			List<Lord> list = new List<Lord>();
			bool flag = raidData != null && !raidData.raidPawns.NullOrEmpty<Pawn>();
			if (flag)
			{
				foreach (Pawn pawn in raidData.raidPawns)
				{
					bool flag2 = pawn != null && pawn.Spawned;
					if (flag2)
					{
						Lord lord = pawn.GetLord();
						bool flag3 = lord != null && !list.Contains(lord);
						if (flag3)
						{
							list.Add(lord);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000093 RID: 147 RVA: 0x0000B978 File Offset: 0x00009B78
		public static bool InStageAttack(Pawn pawn, Lord lord)
		{
			CompGuardRole comp = pawn.GetComp<CompGuardRole>();
			bool flag = comp != null;
			if (flag)
			{
				Pawn escortee = comp.escortee;
				bool flag2 = escortee != null;
				if (flag2)
				{
					Lord lord2 = escortee.GetLord();
					bool flag3 = lord2 != null;
					if (flag3)
					{
						return lord2.LordJob is LordJob_StageAttack && lord2.CurLordToil != null && lord2.CurLordToil is LordToil_Stage;
					}
				}
			}
			else
			{
				bool flag4 = lord != null;
				if (flag4)
				{
					return lord.LordJob is LordJob_StageAttack && lord.CurLordToil != null && lord.CurLordToil is LordToil_Stage;
				}
			}
			return false;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x0000BA2C File Offset: 0x00009C2C
		public static void RemovePawnFromCurrentLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				lord.ownedPawns.Remove(pawn);
				bool flag2 = pawn.mindState != null;
				if (flag2)
				{
					pawn.mindState.duty = null;
				}
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
		}

		// Token: 0x06000095 RID: 149 RVA: 0x0000BA88 File Offset: 0x00009C88
		public static void PawnAddPanicFleeOrExitLordToil(Pawn pawn, LocomotionUrgency locomotion, bool usePanicFlee)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = !usePanicFlee || (lord.faction != null && !lord.faction.def.autoFlee);
				if (flag2)
				{
					LordToil lordToil = (from st in lord.Graph.lordToils
					where st is LordToil_ExitMap
					select st).FirstOrDefault<LordToil>();
					bool flag3 = lordToil != null;
					if (flag3)
					{
						lordToil.lord = lord;
						lord.GotoToil(lordToil);
						pawn.Map.attackTargetsCache.UpdateTarget(pawn);
						bool flag4 = pawn.jobs != null && pawn.jobs.curJob != null;
						if (flag4)
						{
							pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
						}
					}
					else
					{
						LordToil lordToil2 = new LordToil_ExitMap(locomotion, false, true);
						lordToil2.lord = lord;
						lord.Graph.lordToils.Add(lordToil2);
						lord.GotoToil(lordToil2);
						pawn.Map.attackTargetsCache.UpdateTarget(pawn);
						bool flag5 = pawn.jobs != null && pawn.jobs.curJob != null;
						if (flag5)
						{
							pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
						}
					}
				}
				else
				{
					bool flag6 = lord.faction != null && lord.faction.def.autoFlee;
					if (flag6)
					{
						LordToil lordToil3 = lord.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_PanicFlee);
						bool flag7 = lordToil3 != null;
						if (flag7)
						{
							lordToil3.lord = lord;
							lord.Graph.lordToils.Add(lordToil3);
							lord.GotoToil(lordToil3);
							pawn.Map.attackTargetsCache.UpdateTarget(pawn);
							bool flag8 = pawn.jobs != null && pawn.jobs.curJob != null;
							if (flag8)
							{
								pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000096 RID: 150 RVA: 0x0000BCB0 File Offset: 0x00009EB0
		public static void PawnAddExitLord(Pawn pawn, bool panicFlee)
		{
			bool flag = !panicFlee || (pawn.Faction != null && !pawn.Faction.def.autoFlee);
			if (flag)
			{
				AdvancedAI_LordUtility.PawnAddExitMapBestLord(pawn);
			}
			else
			{
				AdvancedAI_LordUtility.PawnAddExitPanicFleeLord(pawn);
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x0000BCFC File Offset: 0x00009EFC
		public static void PawnAddExitPanicFleeLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = lord.LordJob is LordJob_ExitMapPanicFlee;
				if (flag2)
				{
					return;
				}
				AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
			}
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag3 = raidData != null;
			if (flag3)
			{
				List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
				bool flag4 = !list.NullOrEmpty<Lord>();
				if (flag4)
				{
					foreach (Lord lord2 in list)
					{
						bool flag5 = lord2.LordJob is LordJob_ExitMapPanicFlee;
						if (flag5)
						{
							lord2.AddPawn(pawn);
							pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapRandom);
							bool flag6 = pawn.mindState != null && pawn.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.PanicFlee;
							if (flag6)
							{
								pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, false, false, null, false, false, false);
							}
							return;
						}
					}
				}
			}
			IEnumerable<Pawn> enumerable = AdvancedAI.PawnsOfFactionOnMap(pawn);
			bool flag7 = !enumerable.EnumerableNullOrEmpty<Pawn>();
			if (flag7)
			{
				foreach (Pawn p in enumerable)
				{
					Lord lord3 = p.GetLord();
					bool flag8 = lord3 != null && lord3.LordJob is LordJob_ExitMapPanicFlee;
					if (flag8)
					{
						lord3.AddPawn(pawn);
						pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapRandom);
						bool flag9 = pawn.mindState != null && pawn.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.PanicFlee;
						if (flag9)
						{
							pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, false, false, null, false, false, false);
						}
						return;
					}
				}
			}
			LordJob_ExitMapPanicFlee lordJob = new LordJob_ExitMapPanicFlee();
			LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, new List<Pawn>
			{
				pawn
			});
			pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapRandom);
			bool flag10 = pawn.mindState != null && pawn.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.PanicFlee;
			if (flag10)
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, false, false, null, false, false, false);
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x0000BFB0 File Offset: 0x0000A1B0
		public static void PawnAddExitMapBestLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = lord.LordJob is LordJob_ExitMapBest;
				if (flag2)
				{
					return;
				}
				AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
			}
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag3 = raidData != null;
			if (flag3)
			{
				List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
				bool flag4 = !list.NullOrEmpty<Lord>();
				if (flag4)
				{
					foreach (Lord lord2 in list)
					{
						bool flag5 = lord2.LordJob is LordJob_ExitMapBest;
						if (flag5)
						{
							lord2.AddPawn(pawn);
							pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
							return;
						}
					}
				}
			}
			IEnumerable<Pawn> enumerable = AdvancedAI.PawnsOfFactionOnMap(pawn);
			bool flag6 = !enumerable.EnumerableNullOrEmpty<Pawn>();
			if (flag6)
			{
				foreach (Pawn p in enumerable)
				{
					Lord lord3 = p.GetLord();
					bool flag7 = lord3 != null && lord3.LordJob is LordJob_ExitMapBest;
					if (flag7)
					{
						lord3.AddPawn(pawn);
						pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
						return;
					}
				}
			}
			LocomotionUrgency locomotion = SkyAiCore.Settings.boostEnemyDashSpeed ? LocomotionUrgency.Sprint : LocomotionUrgency.Jog;
			LordJob_ExitMapBest lordJob = new LordJob_ExitMapBest(locomotion, false, false);
			LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, new List<Pawn>
			{
				pawn
			});
			pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x0000C188 File Offset: 0x0000A388
		public static void PawnAddAssaultColonyLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = lord.LordJob is LordJob_AssaultColony;
				if (flag2)
				{
					return;
				}
				AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
			}
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag3 = raidData != null;
			if (flag3)
			{
				List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
				bool flag4 = !list.NullOrEmpty<Lord>();
				if (flag4)
				{
					foreach (Lord lord2 in list)
					{
						bool flag5 = lord2.LordJob is LordJob_AssaultColony;
						if (flag5)
						{
							lord2.AddPawn(pawn);
							pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
							return;
						}
					}
				}
			}
			IEnumerable<Pawn> enumerable = AdvancedAI.PawnsOfFactionOnMap(pawn);
			bool flag6 = !enumerable.EnumerableNullOrEmpty<Pawn>();
			if (flag6)
			{
				foreach (Pawn p in enumerable)
				{
					Lord lord3 = p.GetLord();
					bool flag7 = lord3 != null && lord3.LordJob is LordJob_AssaultColony;
					if (flag7)
					{
						lord3.AddPawn(pawn);
						pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
						return;
					}
				}
			}
			LordJob_AssaultColony lordJob = new LordJob_AssaultColony(pawn.Faction, true, true, true, false, true, false, false);
			LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, new List<Pawn>
			{
				pawn
			});
			pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x0000C354 File Offset: 0x0000A554
		public static void PawnAddBodyGuardLord(Pawn pawn, Pawn escortee, bool checkLordOnMap)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = lord.LordJob is LordJob_Bodyguard;
				if (flag2)
				{
					return;
				}
				AdvancedAI_LordUtility.RemovePawnFromCurrentLord(pawn);
			}
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag3 = raidData != null;
			if (flag3)
			{
				List<Lord> list = AdvancedAI_LordUtility.RaidLords(raidData);
				bool flag4 = !list.NullOrEmpty<Lord>();
				if (flag4)
				{
					foreach (Lord lord2 in list)
					{
						bool flag5 = lord2.LordJob is LordJob_Bodyguard;
						if (flag5)
						{
							lord2.AddPawn(pawn);
							return;
						}
					}
				}
			}
			if (checkLordOnMap)
			{
				IEnumerable<Pawn> enumerable = AdvancedAI.PawnsOfFactionOnMap(pawn);
				bool flag6 = !enumerable.EnumerableNullOrEmpty<Pawn>();
				if (flag6)
				{
					foreach (Pawn p in enumerable)
					{
						Lord lord3 = p.GetLord();
						bool flag7 = lord3 != null && lord3.LordJob is LordJob_Bodyguard;
						if (flag7)
						{
							lord3.AddPawn(pawn);
							return;
						}
					}
				}
			}
			LordJob_Bodyguard lordJob = new LordJob_Bodyguard(pawn, escortee);
			LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, new List<Pawn>
			{
				pawn
			});
		}

		// Token: 0x0600009B RID: 155 RVA: 0x0000C4E0 File Offset: 0x0000A6E0
		public static void PawnAddDefendLordToil(Pawn pawn, IntVec3 loc, float radius)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				LordToil lordToil = lord.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_DefendPoint && st.FlagLoc == loc);
				bool flag2 = lordToil != null;
				if (flag2)
				{
					lordToil.lord = lord;
					lord.GotoToil(lordToil);
				}
				else
				{
					LordToil lordToil2 = new LordToil_DefendPoint(loc, radius, null);
					lordToil2.lord = lord;
					lord.Graph.lordToils.Add(lordToil2);
					lord.GotoToil(lordToil2);
				}
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
				bool flag3 = pawn.jobs != null && pawn.jobs.curJob != null;
				if (flag3)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
				}
			}
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000C5CC File Offset: 0x0000A7CC
		public static void PawnRemoveDefendLordToil(Pawn pawn, LordToil previousLordToil)
		{
			bool flag = false;
			bool flag2 = previousLordToil != null;
			if (flag2)
			{
				Lord lord = pawn.GetLord();
				bool flag3 = lord != null;
				if (flag3)
				{
					LordToil lordToil = lord.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_DefendPoint);
					bool flag4 = lordToil != null;
					if (flag4)
					{
						lord.Graph.lordToils.Remove(lordToil);
						flag = true;
					}
					LordToil lordToil2 = (from st in lord.Graph.lordToils
					where st == previousLordToil
					select st).FirstOrDefault<LordToil>();
					bool flag5 = lordToil2 != null;
					if (flag5)
					{
						lordToil2.lord = lord;
						lord.GotoToil(lordToil2);
					}
					bool flag6 = flag;
					if (flag6)
					{
						foreach (Pawn pawn2 in lord.ownedPawns)
						{
							pawn2.Map.attackTargetsCache.UpdateTarget(pawn2);
							pawn2.jobs.StopAll(false, true);
							pawn2.jobs.ClearQueuedJobs(true);
						}
					}
				}
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000C724 File Offset: 0x0000A924
		public static void AddLordToil(Pawn pawnGetter, LordToil lordToilGetter)
		{
			bool flag = lordToilGetter != null;
			if (flag)
			{
				Lord lord = pawnGetter.GetLord();
				bool flag2 = lord != null;
				if (flag2)
				{
					LordToil lordToil = lord.Graph.lordToils.FirstOrDefault((LordToil newToil) => newToil.GetType() == lordToilGetter.GetType());
					bool flag3 = lordToil != null;
					if (flag3)
					{
						lordToil.lord = lord;
						lord.Graph.lordToils.Add(lordToil);
						lord.GotoToil(lordToil);
						int num = lord.ownedPawns.Count - 1;
						while (num >= 0 && !lord.ownedPawns.NullOrEmpty<Pawn>())
						{
							Pawn pawn = lord.ownedPawns[num];
							pawn.Map.attackTargetsCache.UpdateTarget(pawn);
							bool flag4 = pawn != null && pawn.jobs != null && pawnGetter.jobs.curJob != null;
							if (flag4)
							{
								pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
							}
							num--;
						}
					}
				}
			}
		}

		// Token: 0x04000057 RID: 87
		public static List<Type> EndAssaultColonyLordToils = new List<Type>
		{
			typeof(LordToil_ExitMap),
			typeof(LordToil_KidnapCover),
			typeof(LordToil_StealCover)
		};
	}
}
