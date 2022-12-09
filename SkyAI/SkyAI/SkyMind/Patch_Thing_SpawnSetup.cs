using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x02000055 RID: 85
	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("SpawnSetup")]
	internal class Patch_Thing_SpawnSetup
	{
		// Token: 0x06000205 RID: 517 RVA: 0x0002D1C8 File Offset: 0x0002B3C8
		[HarmonyPostfix]
		public static void Postfix(Thing __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				Pawn pawn = __instance as Pawn;
				bool flag = pawn != null;
				if (flag)
				{
					RaidData raidData = AdvancedAI.PawnRaidData(pawn);
					bool flag2 = raidData != null;
					if (flag2)
					{
						bool flag3 = raidData.raidDoctors == null || (raidData.raidDoctors != null && !raidData.raidDoctors.Contains(pawn));
						if (flag3)
						{
							CompDoctorRole compDoctorRole = pawn.TryGetComp<CompDoctorRole>();
							bool flag4 = compDoctorRole != null;
							if (flag4)
							{
								pawn.AllComps.Remove(compDoctorRole);
							}
						}
						bool flag5 = raidData.raidLeader == null || (raidData.raidLeader != null && raidData.raidLeader != pawn);
						if (flag5)
						{
							CompLeaderRole compLeaderRole = pawn.TryGetComp<CompLeaderRole>();
							bool flag6 = compLeaderRole != null;
							if (flag6)
							{
								pawn.AllComps.Remove(compLeaderRole);
							}
						}
						bool flag7 = raidData.squadCommanders == null || (raidData.squadCommanders != null && (!raidData.squadCommanders.Contains(pawn) || pawn == raidData.raidLeader));
						if (flag7)
						{
							CompSquadCommanderRole compSquadCommanderRole = pawn.TryGetComp<CompSquadCommanderRole>();
							bool flag8 = compSquadCommanderRole != null;
							if (flag8)
							{
								pawn.AllComps.Remove(compSquadCommanderRole);
							}
						}
						bool flag9 = raidData.leaderGuards == null || (raidData.leaderGuards != null && !raidData.leaderGuards.Contains(pawn));
						if (flag9)
						{
							CompGuardRole compGuardRole = pawn.TryGetComp<CompGuardRole>();
							bool flag10 = compGuardRole != null;
							if (flag10)
							{
								pawn.AllComps.Remove(compGuardRole);
							}
						}
					}
					else
					{
						CompDoctorRole compDoctorRole2 = pawn.TryGetComp<CompDoctorRole>();
						bool flag11 = compDoctorRole2 != null;
						if (flag11)
						{
							pawn.AllComps.Remove(compDoctorRole2);
						}
						CompLeaderRole compLeaderRole2 = pawn.TryGetComp<CompLeaderRole>();
						bool flag12 = compLeaderRole2 != null;
						if (flag12)
						{
							pawn.AllComps.Remove(compLeaderRole2);
						}
						CompSquadCommanderRole compSquadCommanderRole2 = pawn.TryGetComp<CompSquadCommanderRole>();
						bool flag13 = compSquadCommanderRole2 != null;
						if (flag13)
						{
							pawn.AllComps.Remove(compSquadCommanderRole2);
						}
						CompGuardRole compGuardRole2 = pawn.TryGetComp<CompGuardRole>();
						bool flag14 = compGuardRole2 != null;
						if (flag14)
						{
							pawn.AllComps.Remove(compGuardRole2);
						}
					}
					bool packAnimal = pawn.RaceProps.packAnimal;
					if (packAnimal)
					{
						Pawn_InventoryTracker inventory = pawn.inventory;
						ThingOwner<Thing> thingOwner = (inventory != null) ? inventory.innerContainer : null;
						float num = 0f;
						foreach (Thing thing in thingOwner)
						{
							float statValue = thing.GetStatValue(StatDefOf.Mass, true);
							num += statValue;
						}
						float num2 = MassUtility.Capacity(pawn, null);
						float num3 = MassUtility.EncumbrancePercent(pawn);
						bool flag15 = num3 >= 1f;
						if (flag15)
						{
							IOrderedEnumerable<Thing> source = from t in thingOwner
							orderby t.GetStatValue(StatDefOf.Mass, true) * (float)t.stackCount descending
							select t;
							List<Thing> list = new List<Thing>();
							float num4 = 0f;
							for (int i = 0; i < source.Count<Thing>(); i++)
							{
								Thing thing2 = source.ElementAt(i);
								bool flag16 = thing2.def.Equals(ThingDefOf.Silver);
								if (!flag16)
								{
									float statValue2 = thing2.GetStatValue(StatDefOf.Mass, true);
									float num5 = (float)thing2.stackCount * statValue2;
									bool flag17 = num4 + num5 > num2;
									if (flag17)
									{
										float num6 = num2 - num4;
										int num7 = Mathf.FloorToInt(num6 / statValue2);
										bool flag18 = num7 <= 1;
										if (flag18)
										{
											list.Add(thing2);
										}
										else
										{
											num4 += statValue2 * (float)num7;
											Thing item = thing2.SplitOff(thing2.stackCount - num7);
											list.Add(item);
										}
									}
									else
									{
										num4 += num5;
									}
								}
							}
							bool flag19 = !list.NullOrEmpty<Thing>();
							if (flag19)
							{
								for (int j = 0; j < list.Count<Thing>(); j++)
								{
									Thing thing3 = list[j];
									thingOwner.Remove(list[j]);
								}
							}
						}
					}
				}
			}
		}
	}
}
