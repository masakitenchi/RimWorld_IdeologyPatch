using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class CompRimatomicsShield : ThingComp
	{
		private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

		public static readonly Material ForceFieldBuzzMat = MaterialPool.MatFrom("Rimatomics/FX/shield", ShaderDatabase.MoteGlow, Color.white, MapMaterialRenderQueues.OrbitalBeam);

		public static readonly Material ForceFieldConeMat = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow, Color.white, MapMaterialRenderQueues.OrbitalBeam);

		public static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

		private CompPowerTrader powercomp;

		private CompPowerBattery batt;

		public bool debugInterceptNonHostileProjectiles;

		public float energy;

		public float EnergyLossPerDamage = 0.033f;

		private CompFlickable flick;

		public float lastInterceptAngle;

		public int lastInterceptTicks = -999999;

		public float Radius;

		private readonly int StartingTicksToReset = 1200;

		public float targetRadius = 8f;

		private int ticksToReset = -1;

		private bool MEPS;

		public float EnergyMax
		{
			get
			{
				if (!MEPS)
				{
					return 25f;
				}
				return 27.5f;
			}
		}

		public float EnergyGainPerTick => (MEPS ? 1.5f : 1.3f) / 60f;

		public bool Active => ShieldState == ShieldState.Active;

		public Color col => Color.red;

		public ShieldState ShieldState
		{
			get
			{
				if (ticksToReset > 0 || !flick.SwitchIsOn || !powercomp.PowerOn || (batt != null && batt.StoredEnergyPct <= 0f))
				{
					return ShieldState.Resetting;
				}
				return ShieldState.Active;
			}
		}

		public CompProperties_ProjectileInterceptor Props => (CompProperties_ProjectileInterceptor)props;

		public float powerman
		{
			get
			{
				if (energy < EnergyMax)
				{
					float num = powercomp.Props.basePowerConsumption;
					if (MEPS)
					{
						num *= 1.5f;
					}
					return 0f - num;
				}
				return -340f;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref targetRadius, "targetRadius", Props.radius);
			Scribe_Values.Look(ref energy, "energy", 0f);
		}

		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "upgraded")
			{
				MEPS = parent.GetComp<CompUpgradable>().HasUpgrade(DubDef.MEPS);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			batt = parent.GetComp<CompPowerBattery>();
			flick = parent.GetComp<CompFlickable>();
			powercomp = parent.GetComp<CompPowerTrader>();
			try
			{
				MEPS = parent.GetComp<CompUpgradable>().HasUpgrade(DubDef.MEPS);
			}
			catch (Exception)
			{
			}
		}

		private void Reset()
		{
			if (parent.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			}
			ticksToReset = -1;
		}

		public override void CompTick()
		{
			base.CompTick();
			Radius = DubUtils.FInterpTo(Radius, targetRadius, 0.016f, 10f);
			if (Active && batt != null)
			{
				batt.storedEnergy = Mathf.Max(batt.storedEnergy - 0.001f * Radius, 0f);
			}
			powercomp.PowerOutput = powerman;
			if (powercomp.PowerOn)
			{
				energy += EnergyGainPerTick;
				if (energy > EnergyMax)
				{
					energy = EnergyMax;
				}
			}
			else
			{
				energy = 0f;
			}
			if (ShieldState == ShieldState.Resetting)
			{
				ticksToReset--;
				if (energy >= EnergyMax)
				{
					Reset();
				}
			}
			else
			{
				_ = ShieldState;
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Find.Selector.SingleSelectedThing == parent)
			{
				yield return new Gizmo_EnergyShieldStatus
				{
					shield = this
				};
			}
			if (MEPS)
			{
				yield return new Command_Action
				{
					defaultLabel = "Increase radius",
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeUp"),
					action = delegate
					{
						targetRadius = Mathf.Min(13f, targetRadius + 1f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Decrease radius",
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeDown"),
					action = delegate
					{
						targetRadius = Mathf.Max(3f, targetRadius - 1f);
					}
				};
			}
			if (!Prefs.DevMode)
			{
				yield break;
			}
			if (ShieldState == ShieldState.Resetting)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Reset cooldown ",
					action = delegate
					{
						lastInterceptTicks = Find.TickManager.TicksGame - Props.cooldownTicks;
					}
				};
			}
			yield return new Command_Toggle
			{
				defaultLabel = "Dev: Intercept non-hostile",
				isActive = () => debugInterceptNonHostileProjectiles,
				toggleAction = delegate
				{
					debugInterceptNonHostileProjectiles = !debugInterceptNonHostileProjectiles;
				}
			};
		}

		private float GetCurrentAlpha_Idle()
		{
			float idlePulseSpeed = Props.idlePulseSpeed;
			float minIdleAlpha = Props.minIdleAlpha;
			if (!Active)
			{
				return 0f;
			}
			if (parent.Faction == Faction.OfPlayer && !debugInterceptNonHostileProjectiles)
			{
				return 0f;
			}
			if (Find.Selector.IsSelected(parent))
			{
				return 0f;
			}
			return Mathf.Lerp(minIdleAlpha, 0.11f, (Mathf.Sin((float)(Gen.HashCombineInt(parent.thingIDNumber, 96804938) % 100) + Time.realtimeSinceStartup * idlePulseSpeed) + 1f) / 2f);
		}

		public float GetCurrentAlpha()
		{
			return Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(GetCurrentAlpha_Idle(), GetCurrentAlpha_Selected()), GetCurrentAlpha_RecentlyIntercepted())), Props.minAlpha);
		}

		public float GetCurrentAlpha_RecentlyIntercepted()
		{
			return Mathf.Clamp01((float)(1.0 - (double)(Find.TickManager.TicksGame - lastInterceptTicks) / 40.0)) * 0.09f;
		}

		public float GetCurrentConeAlpha_RecentlyIntercepted()
		{
			return Mathf.Clamp01((float)(1.0 - (double)(Find.TickManager.TicksGame - lastInterceptTicks) / 40.0)) * 0.82f;
		}

		public float GetCurrentAlpha_Selected()
		{
			float num = Mathf.Max(2f, Props.idlePulseSpeed);
			if (!Find.Selector.IsSelected(parent) || !Active)
			{
				return 0f;
			}
			if (Active)
			{
				return Mathf.Lerp(0.2f, 0.62f, (float)(((double)Mathf.Sin((float)(Gen.HashCombineInt(parent.thingIDNumber, 35990913) % 100) + Time.realtimeSinceStartup * num) + 1.0) / 2.0));
			}
			return 0.41f;
		}

		public override void PostDraw()
		{
			Vector3 pos = parent.Position.ToVector3Shifted();
			pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			float currentAlpha = GetCurrentAlpha();
			if (currentAlpha > 0f)
			{
				Color value = col;
				value.a *= currentAlpha;
				MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(pos, Quaternion.Euler(0f, 0f, 0f), new Vector3(Radius * 2f * 1.16015625f, 1f, Radius * 2f * 1.16015625f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, ForceFieldMat, 0, null, 0, MatPropertyBlock);
			}
			float currentConeAlpha_RecentlyIntercepted = GetCurrentConeAlpha_RecentlyIntercepted();
			if (currentConeAlpha_RecentlyIntercepted > 0f)
			{
				Color value2 = col;
				value2.a *= currentConeAlpha_RecentlyIntercepted;
				MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value2);
				Matrix4x4 matrix2 = default(Matrix4x4);
				matrix2.SetTRS(pos, Quaternion.Euler(0f, lastInterceptAngle - 90f, 0f), new Vector3(Radius * 2f * 1.16015625f, 1f, Radius * 2f * 1.16015625f));
				Graphics.DrawMesh(MeshPool.plane10, matrix2, ForceFieldConeMat, 0, null, 0, MatPropertyBlock);
			}
		}

		public bool CheckIntercept(Projectile projectile, Vector3 lastExactPos, Vector3 newExactPos)
		{
			Vector3 vector = parent.Position.ToVector3Shifted();
			float num = Radius + projectile.def.projectile.SpeedTilesPerTick + 0.1f;
			if ((newExactPos.x - vector.x) * (newExactPos.x - vector.x) + (newExactPos.z - vector.z) * (newExactPos.z - vector.z) > num * num)
			{
				return false;
			}
			if (!Active || ShieldState == ShieldState.Resetting)
			{
				return false;
			}
			if ((projectile.Launcher == null || !projectile.Launcher.HostileTo(parent)) && !debugInterceptNonHostileProjectiles && !Props.interceptNonHostileProjectiles)
			{
				return false;
			}
			if (!Props.interceptOutgoingProjectiles && (new Vector2(vector.x, vector.z) - new Vector2(lastExactPos.x, lastExactPos.z)).sqrMagnitude <= Radius * Radius)
			{
				return false;
			}
			if (!GenGeo.IntersectLineCircleOutline(new Vector2(vector.x, vector.z), Radius, new Vector2(lastExactPos.x, lastExactPos.z), new Vector2(newExactPos.x, newExactPos.z)))
			{
				return false;
			}
			DamageInfo dinfo = new DamageInfo(projectile.def.projectile.damageDef, projectile.DamageAmount);
			lastInterceptAngle = lastExactPos.AngleToFlat(parent.TrueCenter());
			lastInterceptTicks = Find.TickManager.TicksGame;
			if (projectile.def.projectile.damageDef == DamageDefOf.EMP && Props.disarmedByEmpForTicks > 0)
			{
				BreakShield(dinfo);
			}
			Effecter effecter = new Effecter(Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
			effecter.Trigger(new TargetInfo(newExactPos.ToIntVec3(), parent.Map), TargetInfo.Invalid);
			effecter.Cleanup();
			energy -= dinfo.Amount * EnergyLossPerDamage;
			if (energy < 0f)
			{
				BreakShield(dinfo);
			}
			return true;
		}

		private Vector3 bang(int index, float fTheta, Vector3 center)
		{
			return new Vector3(Radius * Mathf.Cos(fTheta * (float)index) + center.x, 0f, Radius * Mathf.Sin(fTheta * (float)index) + center.z);
		}

		public void BreakShield(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.ExplosionFlash, 52f);
			for (int i = 0; i < 6; i++)
			{
				FleckMaker.ThrowDustPuff(parent.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), parent.Map, Rand.Range(0.8f, 1.2f));
			}
			energy = 0f;
			ticksToReset = StartingTicksToReset;
			if (Active)
			{
				SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(parent));
				int num = Mathf.CeilToInt(Radius * 2f);
				float fTheta = (float)Math.PI * 2f / (float)num;
				for (int j = 0; j < num; j++)
				{
					FleckMaker.ConnectingLine(bang(j, fTheta, parent.TrueCenter()), bang((j + 1) % num, fTheta, parent.TrueCenter()), FleckDefOf.LineEMP, parent.Map, 1.5f);
				}
			}
			dinfo.SetAmount((float)Props.disarmedByEmpForTicks / 30f);
		}
	}
}
