using System;
using Verse;

namespace SkyMind
{
	// Token: 0x0200003A RID: 58
	public static class SpecialStatic
	{
		// Token: 0x060001C9 RID: 457 RVA: 0x00028C00 File Offset: 0x00026E00
		public static bool EnemyCamperFound(Pawn pawn, Thing target)
		{
			bool flag = target == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				SpecialStatic.delay++;
				bool flag2 = SpecialStatic.delay >= SpecialStatic.maxDelay || !SpecialStatic.enemyCamperFound;
				if (flag2)
				{
					Thing thing;
					SpecialStatic.enemyCamperFound = AdvancedAI.EnemyBehindDefensivePosition(pawn, target.Position, 3f, out thing);
					SpecialStatic.delay = 0;
					result = SpecialStatic.enemyCamperFound;
				}
				else
				{
					result = SpecialStatic.enemyCamperFound;
				}
			}
			return result;
		}

		// Token: 0x04000119 RID: 281
		private static int delay = 0;

		// Token: 0x0400011A RID: 282
		private static int maxDelay = 3;

		// Token: 0x0400011B RID: 283
		private static bool enemyCamperFound = false;
	}
}
