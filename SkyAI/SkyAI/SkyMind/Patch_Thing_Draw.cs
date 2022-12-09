using System;
using HarmonyLib;
using RimWorld;
using SK;
using UnityEngine;
using Verse;

namespace SkyMind
{
	// Token: 0x0200004A RID: 74
	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("Draw")]
	public class Patch_Thing_Draw
	{
		// Token: 0x060001EB RID: 491 RVA: 0x0002B974 File Offset: 0x00029B74
		private static void Postfix(ref Thing __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				Pawn pawn = __instance as Pawn;
				bool flag = pawn != null && pawn.def.drawerType == DrawerType.RealtimeOnly;
				if (flag)
				{
					Material material = null;
					bool flag2 = SkyAiCore.Settings.enableTraderIcons && pawn.CanTradeNow;
					if (flag2)
					{
						material = Materials.traderIconMat;
					}
					bool flag3 = SkyAiCore.Settings.enableCrazyTimeIcons && pawn.jobs != null && pawn.jobs.curJob != null;
					if (flag3)
					{
						bool flag4 = pawn.CurJobDef == JobDefOfLocal.CrazyTime || pawn.CurJobDef == JobDefOfLocal.ScratchTarget;
						if (flag4)
						{
							material = Materials.lolIconMat;
						}
					}
					bool inAggroMentalState = pawn.InAggroMentalState;
					if (inAggroMentalState)
					{
						bool enableAgroMentalStateIcons = SkyAiCore.Settings.enableAgroMentalStateIcons;
						if (enableAgroMentalStateIcons)
						{
							material = Materials.agroIconMat;
						}
						bool enableAnimalAgroMentalStateIcons = SkyAiCore.Settings.enableAnimalAgroMentalStateIcons;
						if (enableAnimalAgroMentalStateIcons)
						{
							material = Materials.animalagroIconMat;
						}
					}
					bool flag5 = material != null && !pawn.Dead && !pawn.Downed;
					if (flag5)
					{
						Vector3 position = pawn.TrueCenter();
						position.y = AltitudeLayer.WorldClipper.AltitudeFor() + 0.28125f;
						position.z += 1.2f;
						position.x += (float)(pawn.def.size.x / 2);
						Graphics.DrawMesh(MeshPool.plane08, position, Quaternion.identity, material, 0);
					}
				}
			}
		}
	}
}
