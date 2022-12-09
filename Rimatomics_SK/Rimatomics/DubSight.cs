using UnityEngine;
using Verse;

namespace Rimatomics
{
	public static class DubSight
	{
		public static bool LineOfSightEnergy(IntVec3 start, Pawn target, Map map, bool skipFirstCell = false)
		{
			IntVec3 position = target.Position;
			if (!start.InBounds(map) || !position.InBounds(map))
			{
				return false;
			}
			if (start.AdjacentTo8Way(position) && (skipFirstCell || start.CanBeSeenOverFast(map)))
			{
				return true;
			}
			bool flag = ((start.x != position.x) ? (start.x < position.x) : (start.z < position.z));
			int num = Mathf.Abs(position.x - start.x);
			int num2 = Mathf.Abs(position.z - start.z);
			int num3 = start.x;
			int num4 = start.z;
			int num5 = 1 + num + num2;
			int num6 = ((position.x > start.x) ? 1 : (-1));
			int num7 = ((position.z > start.z) ? 1 : (-1));
			int num8 = num - num2;
			num *= 2;
			num2 *= 2;
			IntVec3 intVec = default(IntVec3);
			while (num5 > 1)
			{
				intVec.x = num3;
				intVec.z = num4;
				if ((!skipFirstCell || !(intVec == start)) && (!intVec.CanBeSeenOverFast(map) || position.GetFirstPawn(map) != target))
				{
					return false;
				}
				if ((float)num8 > 0f || ((float)num8 == 0f && flag))
				{
					num3 += num6;
					num8 -= num2;
				}
				else
				{
					num4 += num7;
					num8 += num;
				}
				num5--;
			}
			return true;
		}

		public static bool IntRange(IntVec3 center, IntVec3 target, FloatRange range)
		{
			if ((center - target).LengthHorizontal < range.min)
			{
				return false;
			}
			if ((center - target).LengthHorizontal > range.max)
			{
				return false;
			}
			return true;
		}

		public static bool LineOfSightProjectile(IntVec3 start, IntVec3 end, IntVec3 center, Map map, FloatRange range, int depth, bool skipFirstCell = false)
		{
			int num = 0;
			if (!start.InBounds(map) || !end.InBounds(map))
			{
				return false;
			}
			if (start.AdjacentTo8Way(end) && (skipFirstCell || IntRange(center, start, range)))
			{
				num++;
			}
			bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
			int num2 = Mathf.Abs(end.x - start.x);
			int num3 = Mathf.Abs(end.z - start.z);
			int num4 = start.x;
			int num5 = start.z;
			int num6 = 1 + num2 + num3;
			int num7 = ((end.x > start.x) ? 1 : (-1));
			int num8 = ((end.z > start.z) ? 1 : (-1));
			int num9 = num2 - num3;
			num2 *= 2;
			num3 *= 2;
			IntVec3 intVec = default(IntVec3);
			while (num6 > 1)
			{
				intVec.x = num4;
				intVec.z = num5;
				if ((!skipFirstCell || !(intVec == start)) && IntRange(center, intVec, range))
				{
					num++;
				}
				if ((float)num9 > 0f || ((float)num9 == 0f && flag))
				{
					num4 += num7;
					num9 -= num3;
				}
				else
				{
					num5 += num8;
					num9 += num2;
				}
				if (num >= depth)
				{
					return true;
				}
				num6--;
			}
			if (num >= depth)
			{
				return true;
			}
			return false;
		}
	}
}
