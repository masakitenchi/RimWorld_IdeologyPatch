using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000014 RID: 20
	public static class AdvancedAI_FollowUtility
	{
		// Token: 0x0600007E RID: 126 RVA: 0x00008208 File Offset: 0x00006408
		public static Pawn GetFolloweeFromLord(Pawn follower, bool excludeDoctors = true, bool necessarilyRangedFollowee = false, bool excludeLeaders = true)
		{
			Lord lord = follower.GetLord();
			bool flag = lord == null;
			Pawn result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Pawn pawn = null;
				IEnumerable<Pawn> ownedPawns = lord.ownedPawns;
				Func<Pawn, bool> <>9__0;
				Func<Pawn, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((Pawn p1) => AdvancedAI.IsActivePawn(p1) && p1 != follower));
				}
				IEnumerable<Pawn> source = ownedPawns.Where(predicate);
				Func<Pawn, float> <>9__1;
				Func<Pawn, float> keySelector;
				if ((keySelector = <>9__1) == null)
				{
					keySelector = (<>9__1 = ((Pawn p2) => follower.Position.DistanceTo(p2.Position)));
				}
				foreach (Pawn pawn2 in source.OrderBy(keySelector))
				{
					bool flag2 = AdvancedAI_FollowUtility.IsGoodFollowee(follower, pawn2, excludeDoctors, excludeLeaders, necessarilyRangedFollowee);
					if (flag2)
					{
						pawn = pawn2;
						break;
					}
				}
				result = pawn;
			}
			return result;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000082F4 File Offset: 0x000064F4
		public static bool IsGoodFollowee(Pawn follower, Pawn followee, bool excludeDoctors, bool excludeLeaders, bool necessarilyRangedFollowee)
		{
			bool flag = followee.mindState != null && followee.mindState.duty != null;
			if (flag)
			{
				PawnDuty duty = followee.mindState.duty;
				bool flag2 = duty.def == DutyDefOf.Escort || duty.def == DutyDefOf.Follow;
				if (flag2)
				{
					return false;
				}
				bool flag3 = duty.focus != null;
				if (flag3)
				{
					bool flag4 = duty.focus.Equals(follower);
					if (flag4)
					{
						return false;
					}
					Pawn pawn = duty.focus.Pawn;
					bool flag5 = pawn != null && AdvancedAI.IsAlly(pawn, followee, false);
					if (flag5)
					{
						return false;
					}
				}
			}
			bool flag6 = followee.jobs != null && followee.jobs.curJob != null;
			if (flag6)
			{
				bool flag7 = followee.CurJobDef == JobDefOf.Follow || followee.CurJobDef == JobDefOf.FollowClose;
				if (flag7)
				{
					return false;
				}
				LocalTargetInfo targetA = followee.jobs.curJob.targetA;
				bool flag8 = targetA != null && targetA.Thing != null && AdvancedAI.IsAlly(targetA.Thing, followee, false);
				if (flag8)
				{
					return false;
				}
			}
			bool flag9 = excludeDoctors && AdvancedAI.PawnIsDoctor(followee);
			bool result;
			if (flag9)
			{
				result = false;
			}
			else
			{
				bool flag10 = excludeLeaders && AdvancedAI.IsRaidLeaderOrSquadCommander(followee);
				if (flag10)
				{
					result = false;
				}
				else
				{
					bool flag11 = necessarilyRangedFollowee && !AdvancedAI.IsRangedPawn(followee);
					if (flag11)
					{
						result = false;
					}
					else
					{
						bool flag12 = AdvancedAI.IsManningTurret(followee);
						if (flag12)
						{
							result = false;
						}
						else
						{
							bool flag13 = !follower.CanReach(followee, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
							result = !flag13;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000084CC File Offset: 0x000066CC
		public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, float radius)
		{
			bool flag = radius <= 0f;
			bool result;
			if (flag)
			{
				string text = "Checking follow job with radius <= 0. pawn=" + follower.ToStringSafe<Pawn>();
				bool flag2 = follower.mindState != null && follower.mindState.duty != null;
				if (flag2)
				{
					string str = text;
					string str2 = " duty=";
					DutyDef def = follower.mindState.duty.def;
					text = str + str2 + ((def != null) ? def.ToString() : null);
				}
				Log.ErrorOnce(text, follower.thingIDNumber ^ 843254009);
				result = false;
			}
			else
			{
				bool flag3 = !follower.CanReach(followee, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
				if (flag3)
				{
					result = false;
				}
				else
				{
					float radius2 = radius * 1.2f;
					result = (!AdvancedAI_FollowUtility.NearFollowee(follower, followee, radius2) || (!AdvancedAI_FollowUtility.NearDestinationOrNotMoving(follower, followee, radius2) && follower.CanReach(followee.pather.LastPassableCellInPath, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn)));
				}
			}
			return result;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000085C0 File Offset: 0x000067C0
		private static bool NearFollowee(Pawn follower, Pawn followee, float radius)
		{
			return follower.Position.AdjacentTo8WayOrInside(followee.Position) || (follower.Position.InHorDistOf(followee.Position, radius) && GenSight.LineOfSight(follower.Position, followee.Position, follower.Map, false, null, 0, 0));
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00008620 File Offset: 0x00006820
		private static bool NearDestinationOrNotMoving(Pawn follower, Pawn followee, float radius)
		{
			bool flag = !followee.pather.Moving;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				IntVec3 lastPassableCellInPath = followee.pather.LastPassableCellInPath;
				result = (!lastPassableCellInPath.IsValid || follower.Position.AdjacentTo8WayOrInside(lastPassableCellInPath) || follower.Position.InHorDistOf(lastPassableCellInPath, radius));
			}
			return result;
		}
	}
}
