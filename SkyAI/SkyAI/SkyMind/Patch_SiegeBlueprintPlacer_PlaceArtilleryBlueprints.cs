using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000046 RID: 70
	[HarmonyPatch(typeof(SiegeBlueprintPlacer))]
	[HarmonyPatch("PlaceArtilleryBlueprints")]
	public class Patch_SiegeBlueprintPlacer_PlaceArtilleryBlueprints
	{
		// Token: 0x060001E2 RID: 482 RVA: 0x0002A554 File Offset: 0x00028754
		public static bool Prefix(ref IEnumerable<Blueprint_Build> __result, Faction ___faction, IntVec3 ___center, float points, Map map)
		{
			Patch_SiegeBlueprintPlacer_PlaceArtilleryBlueprints.<>c__DisplayClass0_0 CS$<>8__locals1 = new Patch_SiegeBlueprintPlacer_PlaceArtilleryBlueprints.<>c__DisplayClass0_0();
			CS$<>8__locals1.___faction = ___faction;
			bool debugDisableSkyAI = SkyAiCore.Settings.debugDisableSkyAI;
			bool result;
			if (debugDisableSkyAI)
			{
				result = true;
			}
			else
			{
				List<Blueprint_Build> list = new List<Blueprint_Build>();
				int num = Mathf.RoundToInt(points / 60f);
				num = Mathf.Clamp(num, 1, 2);
				Pawn pawn = AdvancedAI_SiegeUtility.FirstPawnOfFaction(___center, CS$<>8__locals1.___faction, map);
				bool flag = pawn == null;
				if (flag)
				{
					__result = list;
					result = false;
				}
				else
				{
					IOrderedEnumerable<KeyValuePair<Pawn, int>> source = from sk in AdvancedAI_SiegeUtility.RaidConstuctionLevelSkills(pawn.GetLord(), true, 14)
					orderby sk.Key.skills.GetSkill(SkillDefOf.Construction).Level descending
					select sk;
					for (int i = 0; i < num; i++)
					{
						int skillLevel = source.ElementAt(i).Key.skills.GetSkill(SkillDefOf.Construction).Level;
						IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
						where def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer")
						select def into th
						where CS$<>8__locals1.<Prefix>g__techLevel|1(th.techLevel) && th.constructionSkillPrerequisite <= skillLevel
						select th;
						bool flag2 = enumerable.EnumerableNullOrEmpty<ThingDef>();
						if (flag2)
						{
							break;
						}
						ThingDef thingDef;
						if (i != 0)
						{
							thingDef = enumerable.RandomElementByWeight((ThingDef tch) => (float)tch.techLevel);
						}
						else
						{
							thingDef = (from t in enumerable
							where AdvancedAI_SiegeUtility.ProjectileFlyOverhead(t)
							select t).RandomElementByWeight((ThingDef tch) => (float)tch.techLevel);
						}
						ThingDef thingDef2 = thingDef;
						ThingDef stuff = AdvancedAI_SiegeUtility.GetStuff(thingDef2, CS$<>8__locals1.___faction);
						bool flag3 = AdvancedAI_SiegeUtility.ProjectileFlyOverhead(thingDef2);
						IntVec3 intVec = AdvancedAI_SiegeUtility.FindArtySpot(thingDef2, AdvancedAI_SiegeUtility.GetRot(___center, map), map, ___center, stuff, flag3);
						bool flag4 = !intVec.IsValid;
						if (flag4)
						{
							bool flag5 = !flag3;
							if (!flag5)
							{
								break;
							}
							thingDef2 = (from turret in enumerable
							where AdvancedAI_SiegeUtility.ProjectileFlyOverhead(turret)
							select turret).RandomElementByWeight((ThingDef tch) => (float)tch.techLevel);
							bool flag6 = thingDef2 != null;
							if (!flag6)
							{
								break;
							}
							stuff = AdvancedAI_SiegeUtility.GetStuff(thingDef2, CS$<>8__locals1.___faction);
							intVec = AdvancedAI_SiegeUtility.FindArtySpot(thingDef2, AdvancedAI_SiegeUtility.GetRot(___center, map), map, ___center, stuff, false);
							bool flag7 = !intVec.IsValid;
							if (flag7)
							{
								break;
							}
						}
						bool isValid = intVec.IsValid;
						if (isValid)
						{
							Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(thingDef2, intVec, map, AdvancedAI_SiegeUtility.GetRot(intVec, map), CS$<>8__locals1.___faction, stuff);
							list.Add(blueprint_Build);
							bool debugLog = SkyAiCore.Settings.debugLog;
							if (debugLog)
							{
								Log.Message(string.Format("Siege prepare. Added thingDef to spawn: {0}", blueprint_Build));
							}
							points -= 60f;
						}
					}
					__result = list;
					result = false;
				}
			}
			return result;
		}
	}
}
