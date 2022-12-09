using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public static class DubUtils
	{
		private static float SMALL_NUMBER = 1E-05f;

		public static void drawLEDfade(Vector3 pos, Vector3 s, Quaternion q, Material mat, float alpha)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, q, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, FadedMaterialPool.FadedVersionOf(mat, alpha), 0);
		}

		public static float GetSkill(this Pawn p, SkillDef def)
		{
			if (p.skills != null)
			{
				return p.skills.GetSkill(def).Level;
			}
			if (p.IsColonyMech)
			{
				return p.RaceProps.mechFixedSkillLevel;
			}
			return 1f;
		}

		public static void drawLED(Vector3 p, Quaternion r, Material mat)
		{
			Graphics.DrawMesh(MeshPool.plane10, p, r, mat, 0);
		}

		public static void drawLED(Vector3 p, Vector3 s, Quaternion r, Material mat)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(p, r, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		public static float VerticalSlider(Rect rect, float value, float leftValue, float rightValue, float roundTo = -1f)
		{
			float num = GUI.VerticalSlider(rect, value, leftValue, rightValue);
			if (roundTo > 0f)
			{
				num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
			}
			return num;
		}

		public static bool IsRobot(Pawn pawn)
		{
			if (pawn.RaceProps.IsMechanoid || DubDef.RobotFilters.RobotBodyDefs.Contains(pawn.def.race.body.defName))
			{
				return true;
			}
			return false;
		}

		public static float Rinterp(float r, float to, float speed)
		{
			return FInterpTo(r, to, 0.0016f, speed);
		}

		public static float InterpEaseOut(float A, float B, float Alpha, float Exp)
		{
			float t = 1f - Mathf.Pow(1f - Alpha, Exp);
			return Mathf.Lerp(A, B, t);
		}

		public static float InterpEaseIn(float A, float B, float Alpha, float Exp)
		{
			float t = Mathf.Pow(Alpha, Exp);
			return Mathf.Lerp(A, B, t);
		}

		public static float InterpEaseInOut(float A, float B, float Alpha, float Exp)
		{
			return Mathf.Lerp(A, B, (Alpha < 0.5f) ? (InterpEaseIn(0f, 1f, Alpha * 2f, Exp) * 0.5f) : (InterpEaseOut(0f, 1f, Alpha * 2f - 1f, Exp) * 0.5f + 0.5f));
		}

		public static float FInterpTo(float Current, float Target, float DeltaTime, float InterpSpeed)
		{
			if (InterpSpeed <= 0f)
			{
				return Target;
			}
			float num = Target - Current;
			if (Mathf.Sqrt(num) < SMALL_NUMBER)
			{
				return Target;
			}
			float num2 = num * Mathf.Clamp(DeltaTime * InterpSpeed, 0f, 1f);
			return Current + num2;
		}

		public static void emitRadiation(IntVec3 Position, float strengthIN, float range, Map map)
		{
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(Position, map, range, useCenter: true))
			{
				if (item is Building_RadDetector building_RadDetector)
				{
					float strength = strengthIN;
					float num = Position.DistanceToSquared(building_RadDetector.Position);
					if (Rand.Chance(strengthIN / num) && LineOfSightRad(ref strength, Position, building_RadDetector.Position, map, skipFirstCell: true))
					{
						building_RadDetector.DetectRads(strength);
					}
				}
				if (item is Pawn pawn)
				{
					float strength2 = strengthIN;
					float num2 = Position.DistanceToSquared(pawn.Position);
					if (Rand.Chance(strengthIN / num2) && LineOfSightRad(ref strength2, Position, pawn.Position, map, skipFirstCell: true))
					{
						applyRads(pawn, strength2);
					}
				}
			}
		}

		public static bool LineOfSightRad(ref float strength, IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false)
		{
			if (!start.InBounds(map) || !end.InBounds(map))
			{
				return false;
			}
			if (start.AdjacentTo8Way(end) && (skipFirstCell || start.CanBePenetratedRad(map, ref strength)))
			{
				return true;
			}
			bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
			int num = Mathf.Abs(end.x - start.x);
			int num2 = Mathf.Abs(end.z - start.z);
			int num3 = start.x;
			int num4 = start.z;
			int num5 = 1 + num + num2;
			int num6 = ((end.x > start.x) ? 1 : (-1));
			int num7 = ((end.z > start.z) ? 1 : (-1));
			int num8 = num - num2;
			num *= 2;
			num2 *= 2;
			IntVec3 intVec = default(IntVec3);
			while (num5 > 1)
			{
				intVec.x = num3;
				intVec.z = num4;
				if ((!skipFirstCell || !(intVec == start)) && !intVec.CanBePenetratedRad(map, ref strength))
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

		public static bool CanBePenetratedRad(this IntVec3 c, Map map, ref float strength)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice == null || edifice.CanBeSeenOver())
			{
				return true;
			}
			strength *= GenMath.LerpDoubleClamped(10f, 1000f, 1f, 0f, edifice.HitPoints);
			return strength > 0f;
		}

		public static void applyRads(Pawn pawn, float strength)
		{
			strength *= RimatomicsMod.Settings.RadiationStrength;
			float num = GenMath.LerpDoubleClamped(0f, 1f, strength, 0f, pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance));
			if (pawn.IsColonist && num > 0.1f && (DubDef.Geigercounter.IsFinished || DebugSettings.godMode))
			{
				DubDef.geigerTick.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, num.ToString("0.00"), Color.green);
			}
			float num2 = 0.00287583331f;
			num2 *= num;
			Log.Warning(pawn.Name?.ToString() + " filteredStrength   " + num);
			if (IsRobot(pawn))
			{
				HealthUtility.AdjustSeverity(pawn, DubDef.RadiationMechanoid, num2);
				return;
			}
			HediffSet hediffSet = pawn.health.hediffSet;
			bool flag = hediffSet.HasHediff(DubDef.RimatomicsRadiation);
			bool flag2 = hediffSet.HasHediff(DubDef.FatalRad);
			if (!flag2)
			{
				Log.Warning(pawn.Name?.ToString() + "  " + num2);
				HealthUtility.AdjustSeverity(pawn, DubDef.RimatomicsRadiation, num2);
				HealthUtility.AdjustSeverity(pawn, DubDef.RadiationIncurable, num2 / 10f);
			}
			else
			{
				HealthUtility.AdjustSeverity(pawn, DubDef.FatalRad, num2);
			}
			if (flag && hediffSet.GetFirstHediffOfDef(DubDef.RimatomicsRadiation).Severity >= 0.9f && !flag2)
			{
				HealthUtility.AdjustSeverity(pawn, DubDef.RimatomicsRadiation, -2f);
				HealthUtility.AdjustSeverity(pawn, DubDef.FatalRad, 0.1f);
			}
		}

		public static RimatomicsResearch GetResearch()
		{
			return RimatomicsResearch._instance;
		}

		public static MapComponent_Rimatomics Rimatomics(this Map map)
		{
			if (MapComponent_Rimatomics.loccachecomp != null && MapComponent_Rimatomics.loccachecomp.map.uniqueID == map.uniqueID)
			{
				return MapComponent_Rimatomics.loccachecomp;
			}
			MapComponent_Rimatomics.loccachecomp = MapComponent_Rimatomics.CompCache[map.uniqueID];
			return MapComponent_Rimatomics.loccachecomp;
		}

		public static void ThrowSmoke(Vector3 loc, Map map, float size)
		{
			if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				Rand.PushState();
				MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(DubDef.Mote_SmokeRocket);
				obj.Scale = Rand.Range(1.5f, 2.5f) * size;
				obj.rotationRate = Rand.Range(-30f, 30f);
				obj.exactPosition = loc;
				obj.SetVelocity(Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
				GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
				Rand.PopState();
			}
		}

		public static void ThrowFireGlow(IntVec3 c, Map map, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (vector.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
			{
				vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
				if (vector.InBounds(map))
				{
					Rand.PushState();
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(DubDef.Mote_FireGlowRocket);
					obj.Scale = Rand.Range(4f, 6f) * size;
					obj.rotationRate = Rand.Range(-3f, 3f);
					obj.exactPosition = vector;
					obj.SetVelocity(Rand.Range(0, 360), 0.12f);
					GenSpawn.Spawn(obj, vector.ToIntVec3(), map);
					Rand.PopState();
				}
			}
		}

		public static void FiddleDesignation(Map map, Thing t, DesignationDef des)
		{
			SoundDefOf.FlickSwitch.PlayOneShot(t);
			Designation designation = map.designationManager.DesignationOn(t, des);
			if (designation == null)
			{
				map.designationManager.AddDesignation(new Designation(t, des));
			}
			else
			{
				map.designationManager.RemoveDesignation(designation);
			}
		}

		public static bool IsPrototype(ThingDef defin)
		{
			if (defin is RimatomicsThingDef rimatomicsThingDef && rimatomicsThingDef.StepsThatUnlock.Any((ResearchStepDef x) => x.GetParentProject().IsFinished))
			{
				return false;
			}
			return true;
		}
	}
}
