using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SkyMind
{
	// Token: 0x0200003B RID: 59
	[HarmonyPatch(typeof(Pawn_MindState), "MindStateTick")]
	public class Patch_MindState_AnyCloseHostilesChecker
	{
		// Token: 0x060001CB RID: 459 RVA: 0x00028C88 File Offset: 0x00026E88
		public static void AnyCloseHostilesChecker(Pawn_MindState mindState)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				Pawn pawn = mindState.pawn;
				bool flag = pawn != null && pawn.Map != null && !pawn.IsColonist && pawn.CurJob != null && pawn.HostileTo(Find.FactionManager.OfPlayer);
				if (flag)
				{
					bool flag2 = pawn.CurJob.def == JobDefOf.AttackMelee || pawn.CurJob.def == JobDefOf.UseVerbOnThing || pawn.CurJob.def == JobDefOf.Wait_Combat;
					if (flag2)
					{
						bool flag3 = pawn.CurJob.targetA != null && pawn.Position.DistanceTo(pawn.CurJob.targetA.Cell) <= 30f;
						if (flag3)
						{
							mindState.anyCloseHostilesRecently = true;
							return;
						}
					}
					IEnumerable<IntVec3> source = GenRadial.RadialCellsAround(pawn.Position, 16f, false);
					Func<IntVec3, bool> <>9__0;
					Func<IntVec3, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((IntVec3 cell) => cell.InBounds(pawn.Map)));
					}
					foreach (IntVec3 c in source.Where(predicate))
					{
						List<Thing> list = pawn.Map.thingGrid.ThingsListAtFast(c);
						bool flag4 = list.NullOrEmpty<Thing>();
						if (flag4)
						{
							break;
						}
						foreach (Thing thing in list)
						{
							bool flag5 = thing != null && thing.Faction != null && thing.Faction == Faction.OfPlayer;
							if (flag5)
							{
								mindState.anyCloseHostilesRecently = true;
								return;
							}
						}
					}
				}
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00028ED0 File Offset: 0x000270D0
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			FieldInfo anyCloseHostilesRecently = AccessTools.Field(typeof(Pawn_MindState), "anyCloseHostilesRecently");
			MethodInfo get_Spawned = AccessTools.Method(typeof(Thing), "get_Spawned", null, null);
			MethodInfo anyCloseHostilesChecker = AccessTools.Method(typeof(Patch_MindState_AnyCloseHostilesChecker), "AnyCloseHostilesChecker", null, null);
			List<CodeInstruction> c = instructions.ToList<CodeInstruction>();
			bool skip = false;
			int num;
			for (int i = 0; i < c.Count; i = num + 1)
			{
				bool flag = i > 2 && c[i].opcode == OpCodes.Ldarg_0 && c[i - 1].opcode == OpCodes.Brfalse_S && c[i - 2].opcode == OpCodes.Callvirt && c[i - 2].operand == get_Spawned && c[i + 1].opcode == OpCodes.Ldfld && c[i + 1].operand == anyCloseHostilesRecently;
				if (flag)
				{
					yield return c[i];
					yield return new CodeInstruction(OpCodes.Call, anyCloseHostilesChecker);
					skip = true;
				}
				bool flag2 = skip;
				if (flag2)
				{
					bool flag3 = c[i].opcode == OpCodes.Br_S && c[i - 1].opcode == OpCodes.Stfld && c[i - 1].operand == anyCloseHostilesRecently;
					if (flag3)
					{
						skip = false;
					}
				}
				bool flag4 = !skip;
				if (flag4)
				{
					yield return c[i];
				}
				num = i;
			}
			yield break;
		}
	}
}
