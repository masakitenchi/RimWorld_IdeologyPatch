using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	[DefOf]
	public class DubDef
	{
		[MayRequire("stubbystub")]
		public static ThingDef RimatomicsShieldGenerator;

		public static DubsModOptions FuelBurnRate;

		public static DubsModOptions RimatomicsTraderCooldown;

		public static DubsModOptions RadiationStrength;

		public static DubsModOptions PulseSizeScaling;

		public static DubsModOptions ReactorPipeVisibility;

		public static ThingDef Mote_Gibb_A;

		public static ThingDef Mote_Gibb_B;

		public static ThingDef Mote_Gibb_C;

		public static ThingDef Mote_MeltdownFlash;

		public static DamageDef Bomb_PlasmaToroid;

		public static RobotFilterDef RobotFilters;

		public static ThingDef Mote_NukeCloud;

		public static ThingDef Mote_SmokeRocket;

		public static ThingDef Mote_FireGlowRocket;

		public static ThingDef ATOM;

		public static ThingDef SCAD;

		public static ThingDef ALC;

		public static ThingDef MEPS;

		public static ThingDef DriveActuator;

		public static ThingDef ERS;

		public static ThingDef LenseModule;

		public static ThingDef BeamSplitter;

		public static ThingDef TargetingChip;

		public static FleckDef Mote_CoolingTowerSteamC;

		public static FleckDef Mote_CoolingTowerSteamA;

		public static FleckDef Mote_CoolingTowerSteamB;

		public static ResearchStepDef BuildReactorCore;

		public static ResearchStepDef AdvReactor3;

		public static RimatomicsFailureDef Failure_FacilityBreakdown;

		public static RimatomicsFailureDef Failure_RadiationLeak;

		public static RimatomicsFailureDef Failure_MicrowaveLeak;

		public static RimatomicsFailureDef Failure_AcidLeak;

		public static RimatomicsFailureDef Failure_GasLeak;

		public static RimatomicsFailureDef Failure_BlindingFlash;

		public static RimatomicsFailureDef Failure_ArcDischarge;

		public static RimatomicsFailureDef Failure_LaserDischarge;

		public static RimatomicsFailureDef Failure_Overvoltage;

		public static RimatomicsFailureDef Failure_ShortCircuit;

		public static ThingDef Mote_ArcFlash;

		public static DamageDef ArcDischarge;

		public static ThingDef Mote_Beam;

		public static SoundDef Sizzle;

		public static SoundDef ADS_Discharge;

		public static SoundDef BulletImpact_SCAD;

		public static SoundDef GeothermalPlant_Ambience;

		public static EffecterDef RimatomicsConstructWeld;

		public static EffecterDef RimatomicsConstructDrill;

		public static EffecterDef RimatomicsResearchEffect;

		public static EffecterDef RimatomicsEnergyTestEffect;

		public static StatDef CraftingResearchSpeed;

		public static StatDef ConstructionResearchSpeed;

		public static ResearchProjectDef RimatomicsActivate;

		public static RimatomicResearchDef Geigercounter;

		public static MainButtonDef Rimatomics;

		public static TraderKindDef Orbital_Rimatomics;

		public static ThingDef ResearchReactor;

		public static ThingDef PlutoniumProcessor;

		public static ThingDef RadiationDetector;

		public static ThingDef FuelRods;

		public static ThingDef FuelRodsMOX;

		public static ThingDef NuclearWaste;

		public static ThingDef Plutonium;

		public static ThingDef DepletedUraniumPellets;

		public static ThingDef NuclearResearchBench;

		public static ThingDef ICBMStrike;

		public static ThingDef ReactorSacrophagus;

		public static ThingDef ChunkRadioactiveSlag;

		public static ThingDef ThrownSlag;

		public static SoundDef hugeExplosionDistant;

		public static SoundDef nuclearBlastInMap;

		public static DamageDef CoreBlast;

		public static ThingCategoryDef Mopp;

		public static ThingDef MissileSilo;

		public static ThingDef PPCRailgun;

		public static ThingDef RailgunSabot;

		public static ThingDef FissionWarhead;

		public static WorldObjectDef ICBMfission;

		public static WorkTypeDef Research;

		public static WorkTypeDef Crafting;

		public static WorkTypeDef Construction;

		public static GameConditionDef RadioactiveFallout;

		public static GameConditionDef NuclearFallout;

		public static HediffDef RimatomicsRadiation;

		public static HediffDef RadiationIncurable;

		public static HediffDef FatalRad;

		public static HediffDef RadiationMechanoid;

		public static HediffDef FlashBlindness;

		public static SoundDef geigerTick;

		public static SoundDef obeliskDischarge;

		public static SoundDef teslaDischarge;

		public static JobDef FleeADS;

		public static JobDef RimatomicsResearch;

		public static JobDef SuperviseResearch;

		public static JobDef SuperviseConstruction;

		public static JobDef LoadRailgunMagazine;

		public static JobDef LoadSilo;

		public static JobDef UpgradeBuilding;

		public static JobDef UseReactorConsole;

		static DubDef()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DubDef));
		}

		public static IEnumerable<Thing> AllReactors(Map map)
		{
			foreach (LoomNet item in map.Rimatomics().PipeNets.OfType<LoomNet>())
			{
				foreach (reactorCore core in item.Cores)
				{
					yield return core;
				}
			}
		}

		public static IEnumerable<Thing> NuclearFuel(Map map)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(FuelRodsMOX))
			{
				yield return item;
			}
			foreach (Thing item2 in map.listerThings.ThingsOfDef(FuelRods))
			{
				yield return item2;
			}
		}

		public static IEnumerable<Thing> RadioactiveThings(Map map)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(FuelRodsMOX))
			{
				yield return item;
			}
			foreach (Thing item2 in map.listerThings.ThingsOfDef(FuelRods))
			{
				yield return item2;
			}
			foreach (Thing item3 in map.listerThings.ThingsOfDef(ChunkRadioactiveSlag))
			{
				yield return item3;
			}
		}

		public static IEnumerable<Command> BilderbergCommands(BuildableDef building, bool visible = true)
		{
			if (!(building is RimatomicsThingDef rimatomicsThingDef) || rimatomicsThingDef.Bilderbergs == null)
			{
				yield break;
			}
			foreach (ThingDef bilderberg in rimatomicsThingDef.Bilderbergs)
			{
				Designator_Build designator_Build = BuildCopyCommandUtility.FindAllowedDesignator(bilderberg, mustBeVisible: false);
				if (designator_Build != null)
				{
					yield return designator_Build;
				}
			}
		}
	}
}
