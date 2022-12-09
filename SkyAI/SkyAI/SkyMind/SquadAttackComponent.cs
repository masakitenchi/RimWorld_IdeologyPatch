using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x0200000C RID: 12
	public class SquadAttackComponent : MapComponent
	{
		// Token: 0x06000031 RID: 49 RVA: 0x000057F3 File Offset: 0x000039F3
		public SquadAttackComponent(Map map) : base(map)
		{
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00005800 File Offset: 0x00003A00
		public override void MapComponentTick()
		{
			base.MapComponentTick();
			bool flag = Find.TickManager.TicksGame % 300 == 0;
			if (flag)
			{
				MapComponent_SkyAI component = this.map.GetComponent<MapComponent_SkyAI>();
				List<RaidData> raidData = component.raidData;
				foreach (RaidData raidData2 in raidData)
				{
					bool flag2 = raidData2 != null && !raidData2.squads.NullOrEmpty<SquadData>();
					if (flag2)
					{
						foreach (SquadData squadData in raidData2.squads)
						{
							bool debugRaidData = SkyAiCore.Settings.debugRaidData;
							if (debugRaidData)
							{
								Log.Message(string.Format("SquadUpdate1 for id: {0} name: {1} commander: {2} count: {3}", new object[]
								{
									squadData.id,
									squadData.squadName,
									squadData.squadCommander,
									squadData.squadPawns.Count<Pawn>()
								}));
							}
							SquadAttackComponent.SquadUpdate(squadData);
						}
					}
				}
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00005954 File Offset: 0x00003B54
		public void Notify_StageChanged(SquadData squadData)
		{
			squadData.Data.Reset();
			SquadAttackComponent.SquadUpdate(squadData);
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00005954 File Offset: 0x00003B54
		public void Notify_ReachedDutyLocation(SquadData squadData)
		{
			squadData.Data.Reset();
			SquadAttackComponent.SquadUpdate(squadData);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x0000596A File Offset: 0x00003B6A
		public void Notify_BuildingSpawnedOnMap(SquadData squadData, Building b)
		{
			squadData.Data.squadAttackGrid.Notify_BuildingStateChanged(b);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000596A File Offset: 0x00003B6A
		public void Notify_BuildingDespawnedOnMap(SquadData squadData, Building b)
		{
			squadData.Data.squadAttackGrid.Notify_BuildingStateChanged(b);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00005980 File Offset: 0x00003B80
		public static void SquadUpdate(SquadData squadData)
		{
			bool flag = squadData == null || !squadData.Start.IsValid;
			if (!flag)
			{
				bool flag2 = !squadData.squadPawns.Any<Pawn>();
				if (!flag2)
				{
					SquadAttackData data = squadData.Data;
					bool flag3 = !data.dest.IsValid;
					if (flag3)
					{
						bool useAvoidGrid = false;
						data.Reset();
						data.preferMelee = Rand.Chance(0.5f);
						data.start = squadData.Start;
						data.dest = squadData.Destination;
						int num = Mathf.RoundToInt(SquadAttackComponent.RadiusFromNumRaiders.Evaluate((float)squadData.squadPawns.Count));
						int num2 = Mathf.RoundToInt(SquadAttackComponent.WalkMarginFromNumRaiders.Evaluate((float)squadData.squadPawns.Count));
						data.squadAttackGrid.CreatePath(data.start, data.dest, num, num2, useAvoidGrid);
						bool debugRaidData = SkyAiCore.Settings.debugRaidData;
						if (debugRaidData)
						{
							Log.Message(string.Format("SquadUpdate2 for id: {0}. Path created. Data: {1} start: {2} dest: {3} radius: {4} walkMargin: {5}", new object[]
							{
								squadData.id,
								data,
								data.start,
								data.dest,
								num,
								num2
							}));
						}
					}
					SquadAttackData.maxRange = SquadAttackComponent.MaxRangeForShooters;
				}
			}
		}

		// Token: 0x04000014 RID: 20
		private const int UpdateIntervalTicks = 300;

		// Token: 0x04000015 RID: 21
		private const float PreferMeleeChance = 0.5f;

		// Token: 0x04000016 RID: 22
		[TweakValue("SquadAttackData", 0f, 100f)]
		public static float MaxRangeForShooters = 12f;

		// Token: 0x04000017 RID: 23
		private static readonly SimpleCurve RadiusFromNumRaiders = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(25f, 4f),
				true
			},
			{
				new CurvePoint(35f, 5f),
				true
			}
		};

		// Token: 0x04000018 RID: 24
		private static readonly SimpleCurve WalkMarginFromNumRaiders = new SimpleCurve
		{
			{
				new CurvePoint(0f, 2f),
				true
			},
			{
				new CurvePoint(60f, 5f),
				true
			}
		};
	}
}
