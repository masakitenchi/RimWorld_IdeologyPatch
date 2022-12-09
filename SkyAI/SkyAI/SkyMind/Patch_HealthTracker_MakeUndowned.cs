using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000052 RID: 82
	[HarmonyPatch(typeof(Pawn_HealthTracker))]
	[HarmonyPatch("MakeUndowned")]
	public class Patch_HealthTracker_MakeUndowned
	{
		// Token: 0x060001FC RID: 508 RVA: 0x0002C3C4 File Offset: 0x0002A5C4
		public static void Postfix(ref Pawn_HealthTracker __instance, PawnHealthState ___healthState, Pawn ___pawn)
		{
			bool flag = __instance == null || ___pawn == null;
			if (!flag)
			{
				bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
				if (!debugDisableSkyAI)
				{
					bool flag2 = ___pawn.Map == null || !___pawn.RaceProps.Humanlike || ___healthState != PawnHealthState.Mobile || ___pawn.IsPrisoner || ___pawn.IsSlave || ___pawn.Faction.IsPlayer || ___pawn.trader != null;
					if (!flag2)
					{
						bool flag3 = ___pawn.story != null && ___pawn.WorkTagIsDisabled(WorkTags.Violent);
						if (!flag3)
						{
							bool flag4 = true;
							bool flag5 = SkyAiCore.Settings.downedEnemyAfterHealInheritAlliesDuty && AdvancedAI.HasExitJob(___pawn);
							if (flag5)
							{
								try
								{
									Pawn pawn = AdvancedAI_TendUtility.ClosestDoctor(___pawn);
									bool flag6 = pawn != null;
									if (flag6)
									{
										bool flag7 = ___pawn.Position.DistanceTo(pawn.Position) <= 4f;
										Lord lord = pawn.GetLord();
										bool flag8 = flag7 && lord != null && ___pawn.GetLord() != lord;
										if (flag8)
										{
											bool flag9 = ___pawn.GetLord() != null;
											if (flag9)
											{
												AdvancedAI_LordUtility.RemovePawnFromCurrentLord(___pawn);
											}
											___pawn.jobs.StopAll(false, true);
											___pawn.jobs.ClearQueuedJobs(true);
											flag4 = false;
											lord.AddPawn(___pawn);
											bool flag10 = pawn.mindState.duty != null;
											if (flag10)
											{
												___pawn.mindState.duty = pawn.mindState.duty;
											}
											bool flag11 = ___pawn.mindState != null && ___pawn.mindState.mentalStateHandler.InMentalState;
											if (flag11)
											{
												___pawn.mindState.mentalStateHandler.Reset();
											}
										}
									}
									AdvancedAI_TendUtility.InjurySeverity injurySeverity;
									AdvancedAI_TendUtility.RequireTreatment(___pawn, out injurySeverity);
									bool flag12 = injurySeverity < AdvancedAI_TendUtility.InjurySeverity.severe || (Rand.Chance(0.2f) && injurySeverity >= AdvancedAI_TendUtility.InjurySeverity.minor);
									if (flag12)
									{
										bool flag13 = ___pawn.GetLord() != null;
										if (flag13)
										{
											AdvancedAI_LordUtility.RemovePawnFromCurrentLord(___pawn);
											___pawn.jobs.StopAll(false, true);
											___pawn.jobs.ClearQueuedJobs(true);
											flag4 = false;
										}
										Pawn pawn2 = null;
										foreach (Pawn pawn3 in ___pawn.Map.mapPawns.AllPawnsSpawned)
										{
											bool flag14 = pawn3.Faction != null && pawn3.Faction == ___pawn.Faction && pawn3.GetLord() != null;
											if (flag14)
											{
												pawn2 = pawn3;
												break;
											}
										}
										bool flag15 = pawn2 != null;
										if (flag15)
										{
											Lord lord2 = pawn2.GetLord();
											bool flag16 = lord2 != null && lord2 != ___pawn.GetLord();
											if (flag16)
											{
												lord2.AddPawn(___pawn);
												bool flag17 = pawn2.mindState.duty != null;
												if (flag17)
												{
													___pawn.mindState.duty = pawn2.mindState.duty;
												}
											}
										}
									}
								}
								catch (Exception arg)
								{
									Log.Error(string.Format("{0} {1}: DownedEnemyAfterHealInheritAlliesDuty exception: {2}", ___pawn, ___pawn.Position, arg));
								}
							}
							try
							{
								ThingWithComps thingWithComps = AdvancedAI.PrimaryWeapon(___pawn);
								bool flag18 = thingWithComps == null;
								if (flag18)
								{
									bool flag19 = AdvancedAI.HasExitJob(___pawn) && flag4;
									if (flag19)
									{
										___pawn.jobs.StopAll(false, true);
										___pawn.jobs.ClearQueuedJobs(true);
									}
									float radius = 4f;
									List<Thing> list = GenRadial.RadialDistinctThingsAround(___pawn.Position, ___pawn.Map, radius, true).ToList<Thing>();
									MapComponent_SkyAI mapComponent_SkyAI = AdvancedAI_Classes.MapComp(___pawn);
									bool flag20 = mapComponent_SkyAI != null && !mapComponent_SkyAI.pawnThings.NullOrEmpty<PawnThingsOwner>();
									if (flag20)
									{
										foreach (PawnThingsOwner pawnThingsOwner in mapComponent_SkyAI.pawnThings)
										{
											bool flag21 = pawnThingsOwner != null && pawnThingsOwner.owner == ___pawn;
											if (flag21)
											{
												foreach (ThingCountClass thingCountClass in pawnThingsOwner.thingCount)
												{
													for (int i = 0; i < list.Count<Thing>(); i++)
													{
														Thing thing = list[i];
														bool flag22 = thingCountClass.thing != null && thingCountClass.thing.Equals(thing);
														if (flag22)
														{
															bool flag23 = AdvancedAI.PrimaryWeapon(___pawn) == null && thing.def.IsWeapon;
															if (flag23)
															{
																___pawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(JobDefOf.Equip, thing), null);
															}
															else
															{
																Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
																job.expiryInterval = 300;
																job.checkOverrideOnExpire = true;
																job.count = Mathf.Min(thing.stackCount, thingCountClass.Count);
																___pawn.jobs.jobQueue.EnqueueFirst(job, null);
															}
															list.Remove(thing);
															bool flag24 = SkyAiCore.Settings.debugLog && SkyAiCore.SelectedPawnDebug(___pawn);
															if (flag24)
															{
																Log.Message(string.Format("{0} {1}: Undowned. Add to job queue: {2} count: {3}", new object[]
																{
																	___pawn,
																	___pawn.Position,
																	thing,
																	thing.stackCount
																}));
															}
														}
													}
												}
											}
										}
									}
								}
							}
							catch (Exception arg2)
							{
								Log.Error(string.Format("{0} {1}: Pawn_HealthTracker.MakeUndowned patch exception: {2}", ___pawn, ___pawn.Position, arg2));
							}
						}
					}
				}
			}
		}
	}
}
