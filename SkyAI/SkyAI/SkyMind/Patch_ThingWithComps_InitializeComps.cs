using System;
using HarmonyLib;
using Verse;

namespace SkyMind
{
	// Token: 0x0200004C RID: 76
	[HarmonyPatch(typeof(ThingWithComps))]
	[HarmonyPatch("InitializeComps")]
	public class Patch_ThingWithComps_InitializeComps
	{
		// Token: 0x060001EF RID: 495 RVA: 0x0002BB2C File Offset: 0x00029D2C
		private static void Postfix(ref ThingWithComps __instance)
		{
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			if (!debugDisableSkyAI)
			{
				Pawn pawn = __instance as Pawn;
				bool flag = pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike;
				if (!flag)
				{
					bool flag2 = Scribe.mode != LoadSaveMode.LoadingVars;
					if (!flag2)
					{
						CompGuardRole compGuardRole = __instance.TryGetComp<CompGuardRole>();
						bool flag3 = compGuardRole == null;
						if (flag3)
						{
							compGuardRole = (CompGuardRole)Activator.CreateInstance(typeof(CompGuardRole));
							compGuardRole.parent = __instance;
							__instance.AllComps.Add(compGuardRole);
							compGuardRole.Initialize(compGuardRole.Props);
						}
						CompSquadCommanderRole compSquadCommanderRole = __instance.TryGetComp<CompSquadCommanderRole>();
						bool flag4 = compSquadCommanderRole == null;
						if (flag4)
						{
							compSquadCommanderRole = (CompSquadCommanderRole)Activator.CreateInstance(typeof(CompSquadCommanderRole));
							compSquadCommanderRole.parent = __instance;
							__instance.AllComps.Add(compSquadCommanderRole);
							compSquadCommanderRole.Initialize(compSquadCommanderRole.Props);
						}
						CompLeaderRole compLeaderRole = __instance.TryGetComp<CompLeaderRole>();
						bool flag5 = compLeaderRole == null;
						if (flag5)
						{
							compLeaderRole = (CompLeaderRole)Activator.CreateInstance(typeof(CompLeaderRole));
							compLeaderRole.parent = __instance;
							__instance.AllComps.Add(compLeaderRole);
							compLeaderRole.Initialize(compLeaderRole.Props);
						}
						CompDoctorRole compDoctorRole = __instance.TryGetComp<CompDoctorRole>();
						bool flag6 = compDoctorRole == null;
						if (flag6)
						{
							compDoctorRole = (CompDoctorRole)Activator.CreateInstance(typeof(CompDoctorRole));
							compDoctorRole.parent = __instance;
							__instance.AllComps.Add(compDoctorRole);
							compDoctorRole.Initialize(compDoctorRole.Props);
						}
					}
				}
			}
		}
	}
}
