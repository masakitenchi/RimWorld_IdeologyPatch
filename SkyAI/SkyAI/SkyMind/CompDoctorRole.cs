using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x02000031 RID: 49
	public class CompDoctorRole : ThingComp
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000183 RID: 387 RVA: 0x0002411B File Offset: 0x0002231B
		public CompProperties_DoctorRole Props
		{
			get
			{
				return (CompProperties_DoctorRole)this.props;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000184 RID: 388 RVA: 0x00024128 File Offset: 0x00022328
		public Pawn Doctor
		{
			get
			{
				return this.parent as Pawn;
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00024138 File Offset: 0x00022338
		public JobDef CurrentJob(Pawn doctor)
		{
			bool flag = doctor != null && doctor.CurJob != null;
			JobDef result;
			if (flag)
			{
				result = doctor.CurJob.def;
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000186 RID: 390 RVA: 0x00024170 File Offset: 0x00022370
		public bool PatientNotFound
		{
			get
			{
				bool flag = !this.PatientsAround(this.Doctor).Contains(this.Patient) && !this.PatientOnTheWay;
				bool flag2 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(this.Doctor);
				if (flag2)
				{
					Log.Message(string.Format("{0} {1}: PatientNotFound: {2}", this.Doctor, this.Doctor.Position, flag));
				}
				return flag;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000187 RID: 391 RVA: 0x000241F8 File Offset: 0x000223F8
		// (set) Token: 0x06000188 RID: 392 RVA: 0x000243BB File Offset: 0x000225BB
		public CompDoctorRole.TreatmentType GetTreatmentType
		{
			get
			{
				IntVec3 intVec = (this.Doctor.mindState != null && this.Doctor.mindState.duty != null && this.Doctor.mindState.duty.focus != null) ? this.Doctor.mindState.duty.focus.Cell : IntVec3.Invalid;
				bool flag = this.treatmentType != CompDoctorRole.TreatmentType.standart && AdvancedAI_TendUtility.PawnShouldUseStandartMedicalTreatment(this.Patient, intVec);
				CompDoctorRole.TreatmentType result;
				if (flag)
				{
					bool flag2 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(this.Doctor) || SkyAiCore.SelectedPawnDebug(this.Patient));
					if (flag2)
					{
						Log.Message(string.Format("{0} {1}: PawnShouldUseStandartMedicalTreatment selected: Standart", this.Doctor, this.Doctor.Position));
					}
					result = CompDoctorRole.TreatmentType.standart;
				}
				else
				{
					bool flag3 = this.treatmentType == CompDoctorRole.TreatmentType.unknown;
					if (flag3)
					{
						bool flag4 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(this.Doctor) || SkyAiCore.SelectedPawnDebug(this.Patient));
						if (flag4)
						{
							Log.Message(string.Format("{0} {1}: treatmentType is unknown. Selected: Standart", this.Doctor, this.Doctor.Position));
						}
						result = CompDoctorRole.TreatmentType.standart;
					}
					else
					{
						bool flag5 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(this.Doctor) || SkyAiCore.SelectedPawnDebug(this.Patient));
						if (flag5)
						{
							Log.Message(string.Format("{0} {1}: treatmentType. Selected: {2}", this.Doctor, this.Doctor.Position, this.treatmentType));
						}
						result = this.treatmentType;
					}
				}
				return result;
			}
			set
			{
				this.treatmentType = value;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000189 RID: 393 RVA: 0x000243C8 File Offset: 0x000225C8
		public bool AlreadyTreated
		{
			get
			{
				bool flag = this.patient != null;
				return !flag || !AdvancedAI_TendUtility.HasHediffsNeedingTend(this.patient);
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600018A RID: 394 RVA: 0x000243FC File Offset: 0x000225FC
		public bool PatientOnTheWay
		{
			get
			{
				foreach (IntVec3 c2 in from c in GenAdjFast.AdjacentCells8Way(this.Doctor.Position)
				where c.InBounds(this.Doctor.Map)
				select c)
				{
					Pawn firstPawn = c2.GetFirstPawn(this.Doctor.Map);
					bool flag = firstPawn != null && firstPawn == this.Patient;
					if (flag)
					{
						return false;
					}
				}
				return true;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600018B RID: 395 RVA: 0x00024494 File Offset: 0x00022694
		public bool DoctorIsBusy
		{
			get
			{
				return this.CurrentJob(this.Doctor) != null && this.CurrentJob(this.Doctor) == JobDefOfAI.AITend;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600018C RID: 396 RVA: 0x000244BA File Offset: 0x000226BA
		public Thing Medicine
		{
			get
			{
				return AdvancedAI_TendUtility.MedicineInInventory(this.Doctor);
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600018D RID: 397 RVA: 0x000244C7 File Offset: 0x000226C7
		public bool DoctorHasMedicine
		{
			get
			{
				return this.Doctor != null && (this.Medicine != null || AdvancedAI.ClosestThing(this.Doctor, ThingRequestGroup.Medicine, 3f) != null || AdvancedAI_TendUtility.CarryMedicine(this.Doctor) != null);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600018E RID: 398 RVA: 0x00024504 File Offset: 0x00022704
		// (set) Token: 0x0600018F RID: 399 RVA: 0x000245AA File Offset: 0x000227AA
		public Pawn Patient
		{
			get
			{
				bool flag = AdvancedAI.HasExitJob(this.Doctor);
				Pawn result;
				if (flag)
				{
					this.patient = null;
					result = null;
				}
				else
				{
					bool alreadyTreated = this.AlreadyTreated;
					if (alreadyTreated)
					{
						this.patient = null;
						result = null;
					}
					else
					{
						bool flag2 = !this.patient.Spawned || this.patient.Destroyed || this.patient.Dead || !this.DoctorHasMedicine || (!this.patient.Downed && AdvancedAI.HasExitJob(this.patient));
						if (flag2)
						{
							this.patient = null;
							result = null;
						}
						else
						{
							result = this.patient;
						}
					}
				}
				return result;
			}
			set
			{
				this.patient = value;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000190 RID: 400 RVA: 0x000245B4 File Offset: 0x000227B4
		public bool Reserved
		{
			get
			{
				bool flag = this.Patient != null;
				bool result;
				if (flag)
				{
					bool flag2 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(this.Doctor) || SkyAiCore.SelectedPawnDebug(this.Patient));
					if (flag2)
					{
						Log.Message(string.Format("{0} {1}: Is reserved by: {2}", this.Doctor, this.Doctor.Position, this.Patient));
					}
					result = true;
				}
				else
				{
					bool doctorIsBusy = this.DoctorIsBusy;
					if (doctorIsBusy)
					{
						bool flag3 = SkyAiCore.Settings.debugLog && (SkyAiCore.SelectedPawnDebug(this.Doctor) || (this.Patient != null && SkyAiCore.SelectedPawnDebug(this.Patient)));
						if (flag3)
						{
							LocalTargetInfo targetA = this.Doctor.CurJob.targetA;
							string arg = (targetA != null) ? targetA.ToString() : "";
							Log.Message(string.Format("{0} {1}: Is busy by: {2}", this.Doctor, this.Doctor.Position, arg));
						}
						result = true;
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}

		// Token: 0x06000191 RID: 401 RVA: 0x000246E8 File Offset: 0x000228E8
		public List<Pawn> PatientsAround(Pawn pawn)
		{
			List<Pawn> list = new List<Pawn>();
			foreach (IntVec3 c in GenRadial.RadialCellsAround(pawn.Position, 2f, true))
			{
				bool flag = !c.InBounds(pawn.Map);
				if (!flag)
				{
					foreach (Thing thing in pawn.Map.thingGrid.ThingsListAtFast(c))
					{
						Pawn pawn2 = thing as Pawn;
						bool flag2 = pawn2 != null && pawn2 != pawn;
						if (flag2)
						{
							bool flag3 = AdvancedAI_TendUtility.RequireTreatment(pawn2);
							if (flag3)
							{
								list.Add(pawn2);
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x000247EC File Offset: 0x000229EC
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look<Pawn>(ref this.patient, "patient", false);
			Scribe_Values.Look<IntVec3>(ref this.focusCell, "focusCell", default(IntVec3), false);
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00024830 File Offset: 0x00022A30
		public override void CompTick()
		{
			base.CompTick();
			Pawn pawn = this.parent as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = pawn.IsHashIntervalTick(250);
				if (flag2)
				{
					bool flag3 = pawn.Map == null;
					if (flag3)
					{
						pawn.AllComps.Remove(this);
					}
					else
					{
						bool flag4 = !AdvancedAI.HasExitJob(pawn) && AdvancedAI.IsActivePawn(pawn) && !this.DoctorIsBusy && !AdvancedAI.HasKidnapOrStealJob(pawn) && !AdvancedAI_TendUtility.HasAlreadyStartTendJob(pawn, true);
						if (flag4)
						{
							float radius = (float)((this.Patient == null) ? 15 : 7);
							Job job = AdvancedAI_Jobs.TakeThingsOfGroupJob(pawn, ThingRequestGroup.Medicine, radius);
							bool flag5 = job != null;
							if (flag5)
							{
								bool flag6 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(pawn);
								if (flag6)
								{
									Log.Message(string.Format("{0} {1}: take medicine job!", pawn, pawn.Position));
								}
								pawn.jobs.StartJob(job, JobCondition.InterruptOptional, null, false, true, null, null, false, false);
							}
						}
						bool flag7 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(this.Doctor);
						if (flag7)
						{
							Log.Message(string.Format("{0} {1}: I'm doctor. Patient: {2} Reserved: {3} doctorIsBusy: {4} PatientNotFound: {5} PatientOnTheWay: {6} doctorHasMedicine: {7}", new object[]
							{
								pawn,
								pawn.Position,
								this.Patient,
								this.Reserved,
								this.DoctorIsBusy,
								this.PatientNotFound,
								this.PatientOnTheWay,
								this.DoctorHasMedicine
							}));
						}
					}
				}
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x000249E4 File Offset: 0x00022BE4
		public override void PostDraw()
		{
			base.PostDraw();
			bool enableRoleIcons = SkyAiCore.Settings.enableRoleIcons;
			if (enableRoleIcons)
			{
				Material doctorIconMat = Materials.doctorIconMat;
				Pawn pawn = this.parent as Pawn;
				bool flag = doctorIconMat != null && pawn != null && !pawn.Dead && !pawn.Downed;
				if (flag)
				{
					Vector3 position = this.parent.TrueCenter();
					position.y = AltitudeLayer.WorldClipper.AltitudeFor() + 0.28125f;
					position.z += 1.2f;
					position.x += (float)(this.parent.def.size.x / 2);
					Graphics.DrawMesh(MeshPool.plane08, position, Quaternion.identity, doctorIconMat, 0);
				}
			}
		}

		// Token: 0x04000104 RID: 260
		public CompDoctorRole.TreatmentType treatmentType;

		// Token: 0x04000105 RID: 261
		public Pawn patient;

		// Token: 0x04000106 RID: 262
		public IntVec3 focusCell;

		// Token: 0x020000D1 RID: 209
		public enum TreatmentType
		{
			// Token: 0x04000297 RID: 663
			standart,
			// Token: 0x04000298 RID: 664
			remote,
			// Token: 0x04000299 RID: 665
			unknown
		}
	}
}
