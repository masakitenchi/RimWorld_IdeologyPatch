using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000017 RID: 23
	public static class AdvancedAI_TendUtility
	{
		// Token: 0x0600009F RID: 159 RVA: 0x0000C894 File Offset: 0x0000AA94
		public static bool HasHediffsNeedingTend(Pawn pawn)
		{
			bool flag = pawn.IsBurning();
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !AdvancedAI.IsBioHumanlikeOnly(pawn);
				if (flag2)
				{
					result = false;
				}
				else
				{
					Pawn_HealthTracker health = pawn.health;
					bool flag3 = health != null && health.HasHediffsNeedingTend(false);
					if (flag3)
					{
						bool flag4 = !pawn.Downed;
						result = (!flag4 || health.hediffSet.BleedRateTotal > 0f);
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0000C90C File Offset: 0x0000AB0C
		public static bool TreatmentImmpossible(Pawn pawn)
		{
			return AdvancedAI.InDangerousCombat(pawn, 40f);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000C92C File Offset: 0x0000AB2C
		public static bool DoctorOnSelfTend(Pawn pawn)
		{
			bool flag = pawn != null && pawn.CurJob != null && pawn.CurJob.targetA != null;
			return flag && pawn.CurJob.def == JobDefOfAI.AITend && pawn.CurJob.targetA == pawn;
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x0000C998 File Offset: 0x0000AB98
		public static Pawn ClosestDoctor(Pawn pawn)
		{
			List<Pawn> list = AdvancedAI_TendUtility.RaidDoctors(pawn);
			bool flag = !list.NullOrEmpty<Pawn>();
			if (flag)
			{
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} : Doctors count: {1}", pawn, list.Count<Pawn>()));
				}
				IEnumerable<Pawn> enumerable = list.Where(delegate(Pawn doc)
				{
					if (doc != pawn)
					{
						CompDoctorRole comp = doc.GetComp<CompDoctorRole>();
						if (comp != null && comp != null && (!comp.Reserved || comp.Patient == pawn) && comp.DoctorHasMedicine && !AdvancedAI_TendUtility.RequireTreatment(doc) && !AdvancedAI_TendUtility.TreatmentImmpossible(doc) && !AdvancedAI_TendUtility.DoctorOnSelfTend(doc) && !AdvancedAI.HasExitJob(doc))
						{
							return base.<ClosestDoctor>g__maxDistanceToDoctor|0(doc);
						}
					}
					return false;
				});
				bool flag3 = !enumerable.EnumerableNullOrEmpty<Pawn>();
				if (flag3)
				{
					bool flag4 = SkyAiCore.Settings.debugLog && list != null && !list.NullOrEmpty<Pawn>() && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} : Free doctors: {1} count: {2}", pawn, GeneralExtensions.Join<Pawn>(list, null, ", ").ToString(), list.Count<Pawn>()));
					}
					return enumerable.MinBy((Pawn doctor) => pawn.Position.DistanceTo(doctor.Position));
				}
			}
			return null;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x0000CAB4 File Offset: 0x0000ACB4
		public static Pawn InjuriedAllyPawn(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				CompDoctorRole comp = pawn.GetComp<CompDoctorRole>();
				bool flag2 = comp != null && comp.Patient != null;
				bool flag3 = AdvancedAI_TendUtility.TreatmentImmpossible(pawn);
				if (flag3)
				{
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} {1} InjuriedAllyPawn. I'am in dangerous close combat, can't tend now.", pawn, pawn.Position));
					}
					bool flag5 = flag2;
					if (flag5)
					{
						bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag6)
						{
							Log.Message(string.Format("{0} {1} InjuriedAllyPawn. Patient goes to null.", pawn, pawn.Position));
						}
						comp.patient = null;
					}
					return null;
				}
				bool flag7 = flag2;
				if (flag7)
				{
					bool flag8 = !AdvancedAI_TendUtility.TreatmentImmpossible(comp.Patient) && AdvancedAI.IsActivePawn(comp.Patient);
					if (flag8)
					{
						bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
						if (flag9)
						{
							Log.Message(string.Format("{0} {1} InjuriedAllyPawn. Selected injuired pawn: {2}", pawn, pawn.Position, comp.Patient));
						}
						return comp.Patient;
					}
					bool flag10 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag10)
					{
						Log.Message(string.Format("{0} {1}: InjuriedAllyPawn. Selected {2} is injuried, but is in dangerous close combat, can't tend now. Patient goes to null.", pawn, pawn.Position, comp.Patient));
					}
					comp.patient = null;
				}
				IEnumerable<Pawn> source = from p in lord.ownedPawns
				where p != null && AdvancedAI_TendUtility.RequireTreatment(p) && AdvancedAI_TendUtility.<InjuriedAllyPawn>g__canHealPlayerPawn|6_0(p) && base.<InjuriedAllyPawn>g__canHealEnemyFactionPawn|1(p) && AdvancedAI.IsActivePawn(p) && !AdvancedAI.HasExitJob(p)
				select p;
				IEnumerable<Pawn> enumerable = from p in source
				where !AdvancedAI.InDangerousCellList(pawn, p.Position) && !AdvancedAI_TendUtility.TreatmentImmpossible(p)
				select p;
				bool flag11 = !enumerable.EnumerableNullOrEmpty<Pawn>();
				if (flag11)
				{
					Pawn pawn2 = enumerable.MinBy((Pawn closest) => pawn.Position.DistanceTo(closest.Position));
					bool flag12 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag12)
					{
						Log.Message(string.Format("{0} {1} InjuriedAllyPawn. Found closest injuried pawn: {2} on {3}", new object[]
						{
							pawn,
							pawn.Position,
							pawn2,
							pawn2.Position
						}));
					}
					return pawn2;
				}
			}
			return null;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x0000CD74 File Offset: 0x0000AF74
		public static Pawn IncapableAllyPawn(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			bool flag = lord != null;
			if (flag)
			{
				bool flag2 = AdvancedAI_TendUtility.TreatmentImmpossible(pawn);
				if (flag2)
				{
					CompDoctorRole comp = pawn.GetComp<CompDoctorRole>();
					bool flag3 = comp != null && comp.Patient != null;
					if (flag3)
					{
						comp.patient = null;
					}
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} {1} IncapableAllyPawn. I'am in dangerous close combat, can't tend now. Patient goes to null.", pawn, pawn.Position));
					}
					return null;
				}
				IEnumerable<IntVec3> source = from cell in GenRadial.RadialCellsAround(pawn.Position, SkyAiCore.Settings.downedAlliesScanRange, true)
				where cell.InBounds(pawn.Map)
				select cell;
				IEnumerable<Pawn> source2 = from c in source
				select c.GetFirstPawn(pawn.Map) into p
				where p != null && p.Downed && p.Faction != null && AdvancedAI_TendUtility.<IncapableAllyPawn>g__canHealPlayerPawn|7_0(p) && base.<IncapableAllyPawn>g__canHealEnemyFactionPawn|1(p) && !pawn.HostileTo(p) && (AdvancedAI_TendUtility.RequireTreatment(p) || (!AdvancedAI_TendUtility.RequireTreatment(p) && AdvancedAI_TendUtility.RequireConsciousnessBuff(pawn, p)))
				select p;
				IEnumerable<Pawn> enumerable = from p in source2
				where !AdvancedAI.InDangerousCellList(pawn, p.Position) && !AdvancedAI_TendUtility.TreatmentImmpossible(p)
				select p;
				bool flag5 = !enumerable.EnumerableNullOrEmpty<Pawn>();
				if (flag5)
				{
					Pawn pawn2 = enumerable.MinBy((Pawn closest) => pawn.Position.DistanceTo(closest.Position));
					bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag6)
					{
						Log.Message(string.Format("{0} {1} IncapableAllyPawn. Found closest incapable patient: {2} on {3}", new object[]
						{
							pawn,
							pawn.Position,
							pawn2,
							pawn2.Position
						}));
					}
					return pawn2;
				}
			}
			return null;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000CF40 File Offset: 0x0000B140
		public static List<Pawn> RaidDoctors(Pawn pawn)
		{
			RaidData raidData = AdvancedAI.PawnRaidData(pawn);
			bool flag = raidData != null;
			List<Pawn> result;
			if (flag)
			{
				result = (from doctor in raidData.raidDoctors
				where doctor != pawn && AdvancedAI.IsActivePawn(doctor)
				select doctor).ToList<Pawn>();
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x0000CF94 File Offset: 0x0000B194
		public static bool AnyAllyDoctorNearby(Pawn pawn, float radius, bool checkMedicineAvailable)
		{
			bool result = false;
			IEnumerable<IntVec3> enumerable = from c in GenRadial.RadialCellsAround(pawn.Position, radius, true)
			where c.InBounds(pawn.Map)
			select c;
			foreach (IntVec3 c2 in enumerable)
			{
				Pawn firstPawn = c2.GetFirstPawn(pawn.Map);
				bool flag = firstPawn != null && AdvancedAI.IsAlly(pawn, firstPawn, false);
				if (flag)
				{
					CompDoctorRole comp = firstPawn.GetComp<CompDoctorRole>();
					bool flag2 = comp != null;
					if (flag2)
					{
						result = (!checkMedicineAvailable || comp.DoctorHasMedicine);
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000D074 File Offset: 0x0000B274
		public static bool IsReservedForTreatment(Pawn pawn, bool useForRemoteTreatment = false, bool waitDoctor = false)
		{
			bool result = false;
			List<Pawn> list = AdvancedAI_TendUtility.RaidDoctors(pawn);
			bool flag = !list.NullOrEmpty<Pawn>();
			if (flag)
			{
				bool flag2 = !waitDoctor && SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: isReservedForTreatment. Check for doctors.", pawn, pawn.Position));
				}
				foreach (Pawn pawn2 in list)
				{
					CompDoctorRole compDoctorRole = pawn2.TryGetComp<CompDoctorRole>();
					bool flag3 = compDoctorRole != null && compDoctorRole.DoctorHasMedicine;
					if (flag3)
					{
						bool flag4 = !waitDoctor && SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn2));
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: isReservedForTreatment could be reserved by {2} {3} treatmentType: {4}", new object[]
							{
								pawn,
								pawn.Position,
								pawn2,
								pawn2.Position,
								compDoctorRole.GetTreatmentType
							}));
						}
						bool flag5 = compDoctorRole.Patient == pawn;
						if (flag5)
						{
							bool flag6 = waitDoctor && pawn.Position.DistanceTo(pawn2.Position) <= 2f;
							if (flag6)
							{
								result = true;
								break;
							}
							if (useForRemoteTreatment)
							{
								result = false;
								break;
							}
							bool flag7 = !waitDoctor && SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(pawn) || SkyAiCore.SelectedPawnDebug(pawn2));
							if (flag7)
							{
								Log.Message(string.Format("{0} {1}: isReservedForTreatment2 by {2} {3}", new object[]
								{
									pawn,
									pawn.Position,
									pawn2,
									pawn2.Position
								}));
							}
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000D288 File Offset: 0x0000B488
		public static bool ReservedByDoctor(Pawn patient, out Pawn doctor)
		{
			List<Pawn> list = AdvancedAI_TendUtility.RaidDoctors(patient);
			bool flag = !list.NullOrEmpty<Pawn>();
			if (flag)
			{
				foreach (Pawn pawn in list)
				{
					bool flag2 = patient != pawn;
					if (flag2)
					{
						CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
						bool flag3 = compDoctorRole != null;
						if (flag3)
						{
							bool flag4 = compDoctorRole.Patient != null && compDoctorRole.Patient == patient;
							if (flag4)
							{
								doctor = pawn;
								return true;
							}
						}
					}
				}
			}
			doctor = null;
			return false;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x0000D340 File Offset: 0x0000B540
		public static bool ReservedByAnotherDoctor(Pawn doctor, Pawn Patient)
		{
			List<Pawn> list = AdvancedAI_TendUtility.RaidDoctors(doctor);
			bool flag = !list.NullOrEmpty<Pawn>();
			if (flag)
			{
				foreach (Pawn pawn in list)
				{
					bool flag2 = pawn != doctor;
					if (flag2)
					{
						CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
						bool flag3 = compDoctorRole != null;
						if (flag3)
						{
							bool flag4 = compDoctorRole.Patient != null && compDoctorRole.Patient == Patient;
							if (flag4)
							{
								bool flag5 = SkyAiCore.Settings.debugLog && !list.NullOrEmpty<Pawn>() && (SkyAiCore.SelectedPawnDebug(doctor) || SkyAiCore.SelectedPawnDebug(Patient));
								if (flag5)
								{
									bool flag6 = list != null;
									if (flag6)
									{
										Log.Message(string.Format("{0} {1}: Patient {2} is already reserved by {3} Doctors in list: {4}", new object[]
										{
											doctor,
											doctor.Position,
											Patient,
											pawn,
											GeneralExtensions.Join<Pawn>(list, null, ", ").ToString()
										}));
									}
									else
									{
										Log.Message(string.Format("{0} {1}: Patient {2} is already reserved by {3} Doctors count: {4}", new object[]
										{
											doctor,
											doctor.Position,
											Patient,
											pawn,
											list.Count
										}));
									}
								}
								return true;
							}
						}
					}
				}
			}
			else
			{
				bool flag7 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(doctor) || SkyAiCore.SelectedPawnDebug(Patient));
				if (flag7)
				{
					Log.Message(string.Format("{0} {1}: doctors list null or empty.", doctor, doctor.Position));
				}
			}
			bool flag8 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(doctor) || SkyAiCore.SelectedPawnDebug(Patient));
			if (flag8)
			{
				bool flag9 = list != null;
				if (flag9)
				{
					Log.Message(string.Format("{0} {1}: Patient {2} ReservedByAnotherDoctor? False. Doctors in list: {3}", new object[]
					{
						doctor,
						doctor.Position,
						Patient,
						GeneralExtensions.Join<Pawn>(list, null, ", ").ToString()
					}));
				}
			}
			return false;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x0000D594 File Offset: 0x0000B794
		public static bool RequireTreatment(Pawn pawn, out AdvancedAI_TendUtility.InjurySeverity injurySeverity)
		{
			Pawn_HealthTracker health = pawn.health;
			bool flag = health != null && AdvancedAI_TendUtility.HasHediffsNeedingTend(pawn);
			if (flag)
			{
				bool flag2 = health.hediffSet.BleedRateTotal >= 3f || (double)health.hediffSet.PainTotal >= 0.6;
				if (flag2)
				{
					injurySeverity = AdvancedAI_TendUtility.InjurySeverity.extreme;
					return true;
				}
				bool flag3 = health.hediffSet.BleedRateTotal >= 2f || (double)health.hediffSet.PainTotal >= 0.5;
				if (flag3)
				{
					injurySeverity = AdvancedAI_TendUtility.InjurySeverity.severe;
					return true;
				}
				bool flag4 = health.hediffSet.BleedRateTotal > 0f;
				if (flag4)
				{
					injurySeverity = AdvancedAI_TendUtility.InjurySeverity.minor;
					return true;
				}
			}
			injurySeverity = AdvancedAI_TendUtility.InjurySeverity.none;
			return false;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x0000D660 File Offset: 0x0000B860
		public static bool CanUseFirstAidSkills(Pawn pawn, bool selfTend)
		{
			bool flag = !pawn.RaceProps.Humanlike || pawn.InAggroMentalState;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !AdvancedAI.CanMove(pawn) && !selfTend;
					result = !flag3;
				}
			}
			return result;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x0000D6C0 File Offset: 0x0000B8C0
		public static bool RequireTreatment(Pawn pawn)
		{
			AdvancedAI_TendUtility.InjurySeverity injurySeverity;
			AdvancedAI_TendUtility.RequireTreatment(pawn, out injurySeverity);
			bool flag = !AdvancedAI_TendUtility.CanUseFirstAidSkills(pawn, true);
			bool result;
			if (flag)
			{
				result = (injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.minor);
			}
			else
			{
				result = (injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.extreme);
			}
			return result;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x0000D6FC File Offset: 0x0000B8FC
		public static bool PawnShouldUseStandartMedicalTreatment(Pawn patient, IntVec3 focusCell)
		{
			bool flag = patient != null && patient.RaceProps.Animal;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = !AdvancedAI.IsValidLoc(focusCell);
				result = flag2;
			}
			return result;
		}

		// Token: 0x060000AE RID: 174 RVA: 0x0000D73C File Offset: 0x0000B93C
		public static bool IsReadyForSelfTend(Pawn pawn)
		{
			bool flag = pawn.Downed || pawn.InBed();
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = Find.TickManager.TicksGame < pawn.mindState.lastHarmTick + 300;
				result = !flag2;
			}
			return result;
		}

		// Token: 0x060000AF RID: 175 RVA: 0x0000D790 File Offset: 0x0000B990
		public static bool RequireConsciousnessBuff(Pawn doctor, Pawn patient)
		{
			bool flag = patient.Faction == null || doctor.Faction == null || patient.RaceProps.Animal;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !AdvancedAI.IsBioHumanlikeOnly(patient);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = patient.CurJob != null && patient.jobs.curDriver.asleep;
					if (flag3)
					{
						result = false;
					}
					else
					{
						HediffDef hediffDef = AdvancedAI_TendUtility.CureDef(doctor.Faction);
						bool flag4 = patient.Downed && hediffDef != null && !patient.health.hediffSet.HasHediff(hediffDef, false) && doctor.Faction == patient.Faction;
						result = flag4;
					}
				}
			}
			return result;
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x0000D84C File Offset: 0x0000BA4C
		public static bool HasAlreadyStartTendJob(Pawn pawn, bool checkBuffJob)
		{
			Job job;
			if (pawn == null)
			{
				job = null;
			}
			else
			{
				Pawn_JobTracker jobs = pawn.jobs;
				job = ((jobs != null) ? jobs.curJob : null);
			}
			Job job2 = job;
			bool flag = job2 != null;
			bool result;
			if (flag)
			{
				bool flag2 = checkBuffJob && job2.def == JobDefOfAI.AddConsciousnessBuff;
				result = (flag2 || job2.def == JobDefOfAI.AITend || job2.def == JobDefOf.TendPatient);
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x0000D8C0 File Offset: 0x0000BAC0
		public static Thing MedicineInInventory(Pawn doctor)
		{
			Thing result = null;
			CompInventory compInventory = doctor.TryGetComp<CompInventory>();
			ThingOwner container = compInventory.container;
			bool flag = container != null && compInventory.container != null;
			if (flag)
			{
				foreach (Thing thing in ((IEnumerable<Thing>)container))
				{
					bool flag2 = thing != null && !thing.def.thingCategories.NullOrEmpty<ThingCategoryDef>() && thing.def.thingCategories.Contains(ThingCategoryDefOf.Medicine) && thing.GetStatValue(StatDefOf.MedicalPotency, true) > 0f;
					if (flag2)
					{
						result = thing;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x0000D990 File Offset: 0x0000BB90
		public static Thing CarryMedicine(Pawn doctor)
		{
			Thing result = null;
			bool flag = doctor.carryTracker != null;
			if (flag)
			{
				ThingOwner<Thing> innerContainer = doctor.carryTracker.innerContainer;
				bool flag2 = innerContainer != null;
				if (flag2)
				{
					foreach (Thing thing in innerContainer)
					{
						bool flag3 = thing != null && !thing.def.thingCategories.NullOrEmpty<ThingCategoryDef>() && thing.def.thingCategories.Contains(ThingCategoryDefOf.Medicine) && thing.GetStatValue(StatDefOf.MedicalPotency, true) > 0f;
						if (flag3)
						{
							result = thing;
							break;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x0000DA64 File Offset: 0x0000BC64
		public static Thing StartCarryMedicine(Pawn pawn, Pawn patient, bool takeMedicine)
		{
			Thing thing = null;
			bool flag = patient == null;
			Thing result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = Medicine.GetMedicineCountToFullyHeal(patient) <= 0;
				if (flag2)
				{
					bool flag3 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag3)
					{
						Log.Message(string.Format("{0}: CarriedMedicine. GetMedicineCountToFullyHeal for {1}: {2}", pawn, patient, Medicine.GetMedicineCountToFullyHeal(patient)));
					}
					result = null;
				}
				else
				{
					Thing thing2 = AdvancedAI_TendUtility.MedicineInInventory(pawn);
					bool flag4 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
					if (flag4)
					{
						Log.Message(string.Format("{0} {1}: CarriedMedicine. getFromInventory", pawn, pawn.Position));
					}
					bool flag5 = thing2 != null;
					if (flag5)
					{
						bool flag6 = !takeMedicine;
						if (flag6)
						{
							return thing2;
						}
						thing = AdvancedAI_TendUtility.CarryMedicine(pawn);
						bool flag7 = thing == null;
						if (flag7)
						{
							int num = Mathf.Min(thing2.stackCount, Medicine.GetMedicineCountToFullyHeal(patient));
							Thing thing3 = pawn.inventory.innerContainer.Take(thing2, num);
							pawn.carryTracker.TryStartCarry(thing3);
							thing = thing3;
							bool flag8 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag8)
							{
								Log.Message(string.Format("{0} {1}: CarriedMedicine. Medicine found: {2}, {3}", new object[]
								{
									pawn,
									pawn.Position,
									thing,
									num
								}));
							}
						}
						else
						{
							bool flag9 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
							if (flag9)
							{
								Log.Message(string.Format("{0} {1}: CarriedMedicine. Already carry: {2}", pawn, pawn.Position, thing));
							}
						}
					}
					result = thing;
				}
			}
			return result;
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x0000DC20 File Offset: 0x0000BE20
		public static HediffDef CureDef(Faction faction)
		{
			HediffDef result;
			switch (faction.def.techLevel)
			{
			case TechLevel.Animal:
				result = HediffDefOfAI.Berserk_herb_high;
				break;
			case TechLevel.Neolithic:
				result = HediffDefOfAI.Berserk_herb_high;
				break;
			case TechLevel.Medieval:
				result = HediffDefOfAI.Berserk_herb_high;
				break;
			case TechLevel.Industrial:
				result = HediffDefOfAI.IbuprofenHigh;
				break;
			case TechLevel.Spacer:
				result = HediffDefOfAI.IbuprofenHigh;
				break;
			case TechLevel.Ultra:
				result = HediffDefOfAI.IbuprofenHigh;
				break;
			case TechLevel.Archotech:
				result = HediffDefOfAI.IbuprofenHigh;
				break;
			default:
				result = HediffDefOfAI.IbuprofenHigh;
				break;
			}
			return result;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x0000DCA4 File Offset: 0x0000BEA4
		public static ThingDef MedicineDef(Faction faction)
		{
			ThingDef named;
			switch (faction.def.techLevel)
			{
			case TechLevel.Animal:
				named = DefDatabase<ThingDef>.GetNamed("MedicineHerbal", true);
				break;
			case TechLevel.Neolithic:
				named = DefDatabase<ThingDef>.GetNamed("MedicineHerbal", true);
				break;
			case TechLevel.Medieval:
				named = DefDatabase<ThingDef>.GetNamed("HerbMedicine", true);
				break;
			case TechLevel.Industrial:
				named = DefDatabase<ThingDef>.GetNamed("MedicineIndustrial", true);
				break;
			case TechLevel.Spacer:
				named = DefDatabase<ThingDef>.GetNamed("MedicineIndustrial", true);
				break;
			case TechLevel.Ultra:
				named = DefDatabase<ThingDef>.GetNamed("MedicineUltratech", true);
				break;
			case TechLevel.Archotech:
				named = DefDatabase<ThingDef>.GetNamed("MedicineUltratech", true);
				break;
			default:
				named = DefDatabase<ThingDef>.GetNamed("MedicineIndustrial", true);
				break;
			}
			return named;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x0000DD58 File Offset: 0x0000BF58
		public static void AddMedicineToInventory(Pawn doctor, ThingDef medDef, IntRange intRange)
		{
			int randomInRange = intRange.RandomInRange;
			CompInventory compInventory = doctor.TryGetComp<CompInventory>();
			bool flag = compInventory == null || compInventory.container == null || doctor.inventory == null;
			if (!flag)
			{
				float num = medDef.GetStatValueAbstract(StatDefOf.Mass, null) * (float)randomInRange;
				bool flag2 = compInventory.currentWeight + num > compInventory.capacityWeight;
				if (flag2)
				{
					IOrderedEnumerable<Thing> orderedEnumerable = from item in doctor.inventory.innerContainer
					where item.def != medDef
					select item into mass
					orderby mass.GetStatValue(StatDefOf.Mass, true) * (float)mass.stackCount descending
					select mass;
					bool flag3 = !orderedEnumerable.EnumerableNullOrEmpty<Thing>();
					if (flag3)
					{
						for (int i = 0; i < orderedEnumerable.Count<Thing>(); i++)
						{
							bool flag4 = compInventory.currentWeight + num >= compInventory.capacityWeight;
							if (!flag4)
							{
								break;
							}
							doctor.inventory.innerContainer.Remove(orderedEnumerable.FirstOrDefault<Thing>());
						}
					}
				}
				bool flag5 = compInventory.currentWeight + num <= compInventory.capacityWeight;
				if (flag5)
				{
					for (int j = 0; j < randomInRange; j++)
					{
						Thing item2 = ThingMaker.MakeThing(medDef, null);
						doctor.inventory.innerContainer.TryAdd(item2, 1, true);
					}
				}
			}
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x0000DED5 File Offset: 0x0000C0D5
		[CompilerGenerated]
		internal static bool <InjuriedAllyPawn>g__canHealPlayerPawn|6_0(Pawn p)
		{
			return SkyAiCore.Settings.allyDoctorCanHealPlayerPawns || p.Faction != Faction.OfPlayer;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x0000DED5 File Offset: 0x0000C0D5
		[CompilerGenerated]
		internal static bool <IncapableAllyPawn>g__canHealPlayerPawn|7_0(Pawn p)
		{
			return SkyAiCore.Settings.allyDoctorCanHealPlayerPawns || p.Faction != Faction.OfPlayer;
		}

		// Token: 0x04000058 RID: 88
		public static AdvancedAI_TendUtility.InjurySeverity injurySeverity;

		// Token: 0x0200008D RID: 141
		public enum InjurySeverity
		{
			// Token: 0x040001F8 RID: 504
			none,
			// Token: 0x040001F9 RID: 505
			minor,
			// Token: 0x040001FA RID: 506
			severe,
			// Token: 0x040001FB RID: 507
			extreme
		}
	}
}
