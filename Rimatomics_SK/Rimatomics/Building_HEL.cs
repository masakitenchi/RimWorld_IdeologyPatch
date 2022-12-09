using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_HEL : Building_EnergyWeapon
	{
		public IEnumerable<IntVec3> cellsInRange;

		public Material beam;

		public float beamAlpha;

		public override bool TurretBased => true;

		public override float PulseSize
		{
			get
			{
				float num = base.GunProps.EnergyWep.PulseSizeScaled;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num *= 1.5f;
				}
				if (UG.HasUpgrade(DubDef.ERS))
				{
					num -= 0.15f * num;
				}
				return num;
			}
		}

		public override int ShotCount
		{
			get
			{
				int num = AttackVerb.verbProps.burstShotCount;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num -= 4;
				}
				return num;
			}
		}

		public override bool CanSetForcedTarget => false;

		public override Vector3 TipOffset
		{
			get
			{
				Vector3 drawPos = DrawPos;
				Vector3 v = new Vector3(0f, 1f, 1f);
				v = v.RotatedBy(TurretRotation);
				return drawPos + v;
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Map.Rimatomics().DeRegisterHEL(this);
			base.DeSpawn(mode);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			base.Map.Rimatomics().RegisterHEL(this);
			cellsInRange = GenRadial.RadialCellsAround(base.Position, Range, useCenter: false);
		}

		public override LocalTargetInfo TryFindNewTarget()
		{
			List<Building_HEL> list = MapComp.HELS.Where((Building_HEL x) => x.powerComp.PowerNet == powerComp.PowerNet).ToList();
			foreach (ThingDef projectileDef in DefExtensions.ProjectileDefs)
			{
				foreach (Thing projectile in base.Map.listerThings.ThingsOfDef(projectileDef))
				{
					if (list.Any((Building_HEL x) => x.CurrentTarget.Thing == projectile))
					{
						continue;
					}
					Thing thing = null;
					Vector3 vect;
					if (projectile is Projectile projectile2)
					{
						thing = projectile2.launcher;
						vect = projectile2.destination;
					}
					else
					{
						try
						{
							Traverse traverse = Traverse.Create(projectile);
							LocalTargetInfo value = traverse.Field("currentTarget").GetValue<LocalTargetInfo>();
							thing = traverse.Field("launcher").GetValue<Thing>();
							vect = value.CenterVector3;
						}
						catch (Exception)
						{
							continue;
						}
					}
					if (thing != null && (thing.Faction != Faction.OfPlayer || DubUtils.GetResearch().BuggerMe) && DubSight.LineOfSightProjectile(vect.ToIntVec3(), projectile.Position, base.Position, base.Map, new FloatRange(RangeMin, Range), 20))
					{
						return projectile;
					}
				}
			}
			return null;
		}

		public override void Draw()
		{
			base.Draw();
			if (beam == null)
			{
				beam = MaterialPool.MatFrom(GraphicsCache.HELBeam, ShaderDatabase.MoteGlow, Color.white);
			}
			if (AttackVerb.Bursting)
			{
				beamAlpha = Mathf.Clamp01(beamAlpha + 0.1f);
			}
			else
			{
				beamAlpha = Mathf.Clamp01(beamAlpha - 0.2f);
			}
			if (beam != null && beamAlpha > 0f)
			{
				Color white = Color.white;
				white.a *= beamAlpha;
				beam.color = white;
				DrawArc(TipOffset, currentTargetInt.Thing.DrawPos, beam);
			}
		}
	}
}
