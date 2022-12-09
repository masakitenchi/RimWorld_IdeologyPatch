using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Rimatomics
{
	public class CompResearchFacility : CompFacility
	{
		public int TicksToRadiate;

		public bool Overcurrent;

		public CompPowerTrader powerComp;

		public Sustainer wickSustainer;

		public bool IsSafe
		{
			get
			{
				JobFailReason.Is("UnsafeFacility".Translate());
				if (TicksToRadiate < 10 && !Overcurrent && !parent.IsBrokenDown())
				{
					return !parent.IsBurning();
				}
				return false;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			parent.Map.Rimatomics().Facilities.Add(parent);
			powerComp = parent.GetComp<CompPowerTrader>();
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			previousMap.Rimatomics().Facilities.Remove(parent);
			base.PostDestroy(mode, previousMap);
		}

		public bool ThrowLaserBeam()
		{
			IntVec3 c = GenRadial.RadialCellsAround(parent.Position, 7f, useCenter: false).InRandomOrder().FirstOrDefault((IntVec3 x) => GenSight.LineOfSight(parent.Position, x, parent.Map));
			if (!c.IsValid)
			{
				return false;
			}
			foreach (Thing thing in c.GetThingList(parent.Map))
			{
				thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, Rand.Range(10, 20)));
				if (thing is Pawn pawn && pawn.Dead)
				{
					DubDef.Sizzle.PlayOneShot(SoundInfo.InMap(new TargetInfo(thing)));
					CompRottable compRottable = pawn.Corpse.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress = 1E+10f;
					}
				}
			}
			Vector3 loc = c.ToVector3Shifted();
			for (int i = 0; i < 3; i++)
			{
				FleckMaker.ThrowSmoke(loc, parent.Map, 1.5f);
				FleckMaker.ThrowMicroSparks(loc, parent.Map);
			}
			Mote_Beam obj = (Mote_Beam)ThingMaker.MakeThing(DubDef.Mote_Beam);
			obj.SetupMoteBeam(GraphicsCache.obeliskBeam, parent.OccupiedRect().RandomVector3, c.ToVector3Shifted());
			obj.Attach(parent);
			GenSpawn.Spawn(obj, parent.Position, parent.Map);
			FireUtility.TryStartFireIn(c, parent.Map, Rand.Range(0.1f, 1.75f));
			DubDef.obeliskDischarge.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			return true;
		}

		public bool ThrowArcDischarge()
		{
			IntVec3 c = GenRadial.RadialCellsAround(parent.Position, 7f, useCenter: false).InRandomOrder().FirstOrDefault((IntVec3 x) => GenSight.LineOfSight(parent.Position, x, parent.Map));
			if (!c.IsValid)
			{
				return false;
			}
			foreach (Thing thing in c.GetThingList(parent.Map))
			{
				thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, Rand.Range(10, 20)));
				if (thing is Pawn pawn && pawn.Dead)
				{
					DubDef.Sizzle.PlayOneShot(SoundInfo.InMap(new TargetInfo(thing)));
					CompRottable compRottable = pawn.Corpse.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress = 1E+10f;
					}
				}
			}
			Vector3 loc = c.ToVector3Shifted();
			FleckMaker.ThrowLightningGlow(loc, parent.Map, 1.5f);
			for (int i = 0; i < 3; i++)
			{
				FleckMaker.ThrowSmoke(loc, parent.Map, 1.5f);
				FleckMaker.ThrowMicroSparks(loc, parent.Map);
			}
			Mote_ArcFlash obj = (Mote_ArcFlash)ThingMaker.MakeThing(DubDef.Mote_ArcFlash);
			obj.SetupMoteArcFlash(GraphicsCache.bolts.RandomElement(), parent.DrawPos, c.ToVector3Shifted());
			obj.Attach(parent);
			GenSpawn.Spawn(obj, parent.Position, parent.Map);
			FireUtility.TryStartFireIn(c, parent.Map, Rand.Range(0.1f, 1.75f));
			DubDef.teslaDischarge.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			return true;
		}

		public bool Used(float skill, RimatomicResearchDef Proj)
		{
			if (parent is ResearchBuilding researchBuilding)
			{
				researchBuilding.LastTickUsed = Find.TickManager.TicksGame;
			}
			ResearchStepDef currentStep = Proj.CurrentStep;
			float mtb = GenMath.LerpDoubleClamped(0f, 20f, 0.5f, 3f, skill);
			if (parent.IsHashIntervalTick(60) && Rand.MTBEventOccurs(mtb, 60000f, 60f))
			{
				RimatomicsFailureDef rimatomicsFailureDef = currentStep.FacilityFailures.RandomElement();
				_ = DubDef.Failure_AcidLeak;
				if (rimatomicsFailureDef == DubDef.Failure_ArcDischarge)
				{
					ThrowArcDischarge();
				}
				if (rimatomicsFailureDef == DubDef.Failure_BlindingFlash)
				{
					Rand.PushState();
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_MeltdownFlash"));
					obj.Scale = 10f;
					obj.exactPosition = parent.Position.ToVector3();
					GenSpawn.Spawn(obj, parent.Position, parent.Map);
					Rand.PopState();
					FleckMaker.ThrowMicroSparks(parent.DrawPos + new Vector3(-1.2f, 0f, 1f), parent.Map);
					foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, 5f, useCenter: true))
					{
						if (!(item is Pawn pawn))
						{
							continue;
						}
						foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, BodyPartTagDefOf.SightSource))
						{
							pawn.health.AddHediff(DubDef.FlashBlindness, notMissingPart);
						}
					}
				}
				if (rimatomicsFailureDef == DubDef.Failure_FacilityBreakdown)
				{
					parent.GetComp<CompBreakdownable>().DoBreakdown();
				}
				_ = DubDef.Failure_GasLeak;
				if (rimatomicsFailureDef == DubDef.Failure_LaserDischarge)
				{
					ThrowLaserBeam();
				}
				_ = DubDef.Failure_MicrowaveLeak;
				if (rimatomicsFailureDef == DubDef.Failure_Overvoltage)
				{
					FireUtility.TryStartFireIn(parent.CellsAdjacent8WayAndInside().RandomElement(), parent.Map, Rand.Range(0.1f, 1.75f));
				}
				if (rimatomicsFailureDef == DubDef.Failure_RadiationLeak)
				{
					TicksToRadiate = Rand.Range(200, 2000);
				}
				if (rimatomicsFailureDef == DubDef.Failure_ShortCircuit)
				{
					Overcurrent = true;
				}
				Find.LetterStack.ReceiveLetter(rimatomicsFailureDef.label, rimatomicsFailureDef.description, LetterDefOf.NegativeEvent, parent);
				return true;
			}
			return false;
		}

		public void StartWickSustainer()
		{
			SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
			wickSustainer = SoundDef.Named("transformerDischarge").TrySpawnSustainer(info);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (TicksToRadiate > 0 && parent.IsHashIntervalTick(6))
			{
				TicksToRadiate -= 6;
				DubUtils.emitRadiation(parent.Position, 2f, 8f, parent.Map);
			}
			if (!Overcurrent)
			{
				return;
			}
			if (wickSustainer == null)
			{
				StartWickSustainer();
			}
			else if (wickSustainer.Ended)
			{
				StartWickSustainer();
			}
			else
			{
				wickSustainer.Maintain();
			}
			if (!parent.GetComp<CompPowerTrader>().PowerOn)
			{
				Overcurrent = false;
			}
			if (parent.IsHashIntervalTick(15))
			{
				FleckMaker.ThrowSmoke(parent.TrueCenter() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), parent.Map, 1.5f);
				FleckMaker.ThrowMicroSparks(parent.TrueCenter() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), parent.Map);
			}
			if (parent.IsHashIntervalTick(3))
			{
				FleckMaker.ThrowLightningGlow(parent.TrueCenter(), parent.Map, 1f);
				parent.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1f));
				if (parent.HitPoints <= 1)
				{
					GenExplosion.DoExplosion(parent.Position, parent.Map, 5f, DamageDefOf.Flame, parent);
					parent.Destroy(DestroyMode.KillFinalize);
				}
			}
		}
	}
}
