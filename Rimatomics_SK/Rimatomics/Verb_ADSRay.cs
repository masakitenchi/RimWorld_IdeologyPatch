using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class Verb_ADSRay : Verb_RimatomicsVerb
	{
		public override int ShotsPerBurst
		{
			get
			{
				return verbProps.burstShotCount;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, (!currentTarget.HasThing) ? null : currentTarget.Thing, (base.EquipmentSource == null) ? null : base.EquipmentSource.def, null, burst: false));
			Building_EnergyWeapon getWep = base.GetWep;
			getWep.GatherData("PPCWeapon", 5f);
			getWep.GatherData("PPCADS", 5f);
			getWep.PrototypeBang(getWep.GunProps.EnergyWep.PrototypeFailureChance);
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return (caster as Building_ADS).AOE;
		}

		public static bool HasShield(Pawn p)
		{
			if (p.apparel == null)
			{
				return false;
			}
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				CompShield comp = wornApparel[i].GetComp<CompShield>();
				if (comp != null && comp.ShieldState == ShieldState.Active)
				{
					return true;
				}
			}
			return false;
		}

		public override bool TryCastShot()
		{
			Building_EnergyWeapon wep = base.GetWep;
			if (!wep.top.TargetInSights)
			{
				return false;
			}
			float AOE = (caster as Building_ADS).AOE;
			foreach (Pawn item in GenRadial.RadialDistinctThingsAround(currentTarget.Cell, caster.Map, AOE, useCenter: true).OfType<Pawn>().ToList())
			{
				if (item.RaceProps.IsMechanoid || item.Downed || HasShield(item))
				{
					break;
				}
				IntVec3 fleeDest = CellFinderLoose.GetFleeDest(item, new List<Thing> { wep }, wep.Position.DistanceTo(currentTarget.Cell) + AOE + 2f);
				if (!fleeDest.IsValid)
				{
					break;
				}
				if (item.RaceProps.Humanlike)
				{
					if (item.CurJob.def != DubDef.FleeADS)
					{
						item.jobs.StartJob(new Job(DubDef.FleeADS, fleeDest), JobCondition.InterruptForced);
					}
				}
				else if (Rand.Chance(0.5f))
				{
					Job newJob = new Job(JobDefOf.Goto, fleeDest)
					{
						locomotionUrgency = LocomotionUrgency.Sprint
					};
					item.jobs.StartJob(newJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
				}
			}
			wep.DissipateCharge(wep.PulseSize);
			FleckMaker.ThrowHeatGlow((from x in GenRadial.RadialCellsAround(currentTarget.Cell, AOE, useCenter: true)
				where x.InBounds(wep.Map)
				select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(currentTarget.Cell) / AOE, 1f) + 0.05f), wep.Map, 2f);
			return true;
		}
	}
}
