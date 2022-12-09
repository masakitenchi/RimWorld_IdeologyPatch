using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000021 RID: 33
	public class CompSquadCommanderRole : ThingComp
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000134 RID: 308 RVA: 0x0001D9FB File Offset: 0x0001BBFB
		public CompProperties_SquadCommanderRole Props
		{
			get
			{
				return (CompProperties_SquadCommanderRole)this.props;
			}
		}

		// Token: 0x06000135 RID: 309 RVA: 0x0001DA08 File Offset: 0x0001BC08
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look<HediffDef>(ref this.leaderAura, "leaderAura");
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0001DA24 File Offset: 0x0001BC24
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
						bool flag4 = !AdvancedAI.IsActivePawn(pawn);
						if (!flag4)
						{
							bool flag5 = this.leaderAura != null && SkyAiCore.Settings.enableSquadCommandersAura;
							if (flag5)
							{
								HediffLeader hediffLeader = (HediffLeader)pawn.health.hediffSet.GetFirstHediffOfDef(this.leaderAura, false);
								bool flag6 = hediffLeader == null;
								if (flag6)
								{
									hediffLeader = (HediffLeader)HediffMaker.MakeHediff(this.leaderAura, pawn, null);
									hediffLeader.Severity = AdvancedAI_Aura.AuraLevel(pawn);
									pawn.health.AddHediff(hediffLeader, null, null, null);
									bool debugLog = SkyAiCore.Settings.debugLog;
									if (debugLog)
									{
										Log.Message(string.Format("{0} {1}: Added leader aura: {2}. Severity: {3} with stage: {4}", new object[]
										{
											pawn,
											pawn.Position,
											hediffLeader.LabelCap,
											AdvancedAI_Aura.AuraLevel(pawn),
											hediffLeader.CurStage
										}));
									}
								}
							}
							else
							{
								SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Social);
								bool flag7 = skill != null;
								if (flag7)
								{
									int level = skill.Level;
									foreach (SkillRecord skillRecord in pawn.skills.skills)
									{
										bool flag8 = skillRecord.def == SkillDefOf.Social;
										if (flag8)
										{
											skillRecord.Level = Mathf.Max(1, level);
											break;
										}
									}
								}
								this.leaderAura = AdvancedAI_Aura.AuraHediffDef(pawn);
								bool debugLog2 = SkyAiCore.Settings.debugLog;
								if (debugLog2)
								{
									Log.Message(string.Format("{0} {1}: Generate leader aura: {2} Severity: {3}", new object[]
									{
										pawn,
										pawn.Position,
										this.leaderAura.LabelCap,
										AdvancedAI_Aura.AuraLevel(pawn)
									}));
								}
							}
							Lord lord = pawn.GetLord();
							SquadData squadData = AdvancedAI_SquadUtility.PawnSquadData(pawn);
							bool flag9 = pawn.IsHashIntervalTick(500) && squadData != null;
							if (flag9)
							{
								AdvancedAI_SquadUtility.UpdateSquadID(pawn, squadData);
								bool flag10 = !squadData.squadEnteredSiegeCombat;
								if (flag10)
								{
									squadData.squadEnteredSiegeCombat = AdvancedAI_SquadUtility.SquadEnteredSiegeCombat(pawn, lord);
								}
							}
							RaidData raidData = AdvancedAI.PawnRaidData(pawn);
							bool flag11 = raidData != null && squadData != null && raidData.raidStage == RaidData.RaidStage.gathering;
							if (flag11)
							{
								IntVec3 spot = squadData.gatherSpot;
								string squadName = squadData.squadName;
								bool flag12 = squadData.isReserved;
								int num = (from p in lord.ownedPawns
								where p.Position.DistanceTo(spot) <= 20f
								select p).Count<Pawn>();
								int num2 = Mathf.RoundToInt((float)lord.ownedPawns.Count * 0.9f);
								bool flag13 = num >= num2;
								bool flag14 = flag13 && !raidData.raidOrders.ContainsKey(pawn) && !flag12;
								if (flag14)
								{
									raidData.raidOrders.Add(pawn, flag13);
								}
								bool flag15 = flag12 && !AdvancedAI.IsActivePawn(pawn);
								if (flag15)
								{
									flag12 = false;
									raidData.raidIsReady = true;
								}
								squadData.isReady = raidData.raidIsReady;
								bool flag16 = SkyAiCore.Settings.debugLog && (pawn.IsHashIntervalTick(2000) || raidData.raidIsReady);
								if (flag16)
								{
									bool flag17 = raidData.raidIsReady && !flag12;
									if (flag17)
									{
										Log.Message(string.Format("{0} {1}: StageAttack. CompSquadCommanderRole. Squad: {2} is ready! {3}/{4}. Moving forward! Data: raidIsReady: {5} squadIsReserved: {6}", new object[]
										{
											pawn,
											pawn.Position,
											squadName,
											num,
											num2,
											raidData.raidIsReady,
											flag12
										}));
									}
									else
									{
										bool flag18 = flag12;
										if (flag18)
										{
											Log.Message(string.Format("{0} {1}: StageAttack. CompSquadCommanderRole. Waiting on position {2}. Squad: {3} in reserve: {4}/{5}. Waiting for commands.", new object[]
											{
												pawn,
												pawn.Position,
												spot,
												squadName,
												num,
												num2
											}));
										}
										else
										{
											bool flag19 = flag13;
											if (flag19)
											{
												Log.Message(string.Format("{0} {1}: StageAttack. CompSquadCommanderRole. Gathered before attack. Squad: {2} is ready! {3}/{4}. Ready to attack.", new object[]
												{
													pawn,
													pawn.Position,
													squadName,
													num,
													num2
												}));
											}
											else
											{
												Log.Message(string.Format("{0} {1}: StageAttack. CompSquadCommanderRole. Gathering on position {2}. Squad: {3} is ready?: {4}/{5}. Nope.", new object[]
												{
													pawn,
													pawn.Position,
													spot,
													squadName,
													num,
													num2
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
		}

		// Token: 0x06000137 RID: 311 RVA: 0x0001DF80 File Offset: 0x0001C180
		public override void PostDraw()
		{
			base.PostDraw();
			bool enableRoleIcons = SkyAiCore.Settings.enableRoleIcons;
			if (enableRoleIcons)
			{
				Material leaderIconMat = Materials.leaderIconMat;
				Pawn pawn = this.parent as Pawn;
				bool flag = leaderIconMat != null && pawn != null && !pawn.Dead && !pawn.Downed;
				if (flag)
				{
					Vector3 position = this.parent.TrueCenter();
					position.y = AltitudeLayer.WorldClipper.AltitudeFor() + 0.28125f;
					position.z += 1.2f;
					position.x += (float)(this.parent.def.size.x / 2);
					Graphics.DrawMesh(MeshPool.plane08, position, Quaternion.identity, leaderIconMat, 0);
				}
			}
		}

		// Token: 0x0400005C RID: 92
		public HediffDef leaderAura = null;
	}
}
