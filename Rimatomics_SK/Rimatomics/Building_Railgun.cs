using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_Railgun : Building_EnergyWeapon
	{
		public static readonly Texture2D FireMissionTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/FireMission");

		public static ThingDef Mote_RailgunMuzzleFlash = ThingDef.Named("Mote_RailgunMuzzleFlash");

		private List<Building_Railgun> selectedRailguns;

		public BiomeDef space = DefDatabase<BiomeDef>.GetNamed("OuterSpaceBiome", errorOnFail: false);

		public override bool TurretBased => true;

		public override Vector3 TipOffset
		{
			get
			{
				Vector3 drawPos = DrawPos;
				Vector3 v = new Vector3(0f, 1f, 5f);
				v = v.RotatedBy(TurretRotation);
				return drawPos + v;
			}
		}

		public float RangeToWorldTarget => Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, longTarget.Tile);

		public override float PulseSize
		{
			get
			{
				float num = base.GunProps.EnergyWep.PulseSizeScaled;
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num *= 1.15f;
				}
				if (UG.HasUpgrade(DubDef.ERS))
				{
					num -= 0.15f * num;
				}
				return num;
			}
		}

		public override int WorldRange
		{
			get
			{
				if (base.Map != null && space != null && base.Map.Biome == space)
				{
					return 99999;
				}
				int num = base.GunProps.EnergyWep.WorldRange;
				if (UG.HasUpgrade(DubDef.TargetingChip))
				{
					num += 10;
				}
				if (UG.HasUpgrade(DubDef.MEPS))
				{
					num += 10;
				}
				return num;
			}
		}

		public int spread
		{
			get
			{
				int num = 6;
				if (UG.HasUpgrade(DubDef.TargetingChip))
				{
					num -= 3;
				}
				return num;
			}
		}

		public override bool CanFireWhileRoofed()
		{
			if (longTargetInt == GlobalTargetInfo.Invalid)
			{
				return true;
			}
			if (base.Position.Roofed(base.Map))
			{
				return false;
			}
			return true;
		}

		public void TryChamberRound()
		{
			if (!magazine.NullOrEmpty())
			{
				CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
				if (!compChangeableProjectile.Loaded)
				{
					compChangeableProjectile.LoadShell(magazine.FirstOrDefault(), 1);
					magazine.RemoveAt(0);
				}
			}
		}

		public override void MuzzleFlash()
		{
			Mote obj = (Mote)ThingMaker.MakeThing(Mote_RailgunMuzzleFlash);
			obj.Scale = 5f;
			obj.exactRotation = TurretRotation;
			obj.exactPosition = TipOffset;
			GenSpawn.Spawn(obj, base.Position, base.Map);
			Vector3 v = new Vector3(1f, 1f, -1f);
			v = v.RotatedBy(TurretRotation);
			FleckMaker.ThrowSmoke(DrawPos + v, base.Map, 1.5f);
			Vector3 v2 = new Vector3(-1f, 1f, -1f);
			v2 = v2.RotatedBy(TurretRotation);
			FleckMaker.ThrowSmoke(DrawPos + v2, base.Map, 1.5f);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "critCommandFireMission".Translate(),
				defaultDesc = "critCommandFireMissionDesc".Translate(),
				icon = FireMissionTex,
				action = StartChoosingDestination
			};
			if (base.Spawned && base.Position.Roofed(base.Map))
			{
				command_Action.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}
			yield return command_Action;
		}

		private void StartChoosingDestination()
		{
			selectedRailguns = Find.Selector.SelectedObjects.OfType<Building_Railgun>().ToList();
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
			Find.WorldSelector.ClearSelection();
			int tile = base.Map.Tile;
			Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: false, FireMissionTex, closeWorldTabWhenFinished: true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, WorldRange);
			});
		}

		public static TargetingParameters ForFireMission()
		{
			if (MP.IsInMultiplayer)
			{
				return new TargetingParameters
				{
					canTargetPawns = false,
					canTargetBuildings = false,
					canTargetLocations = true
				};
			}
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = true,
				canTargetLocations = true
			};
		}

		private bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				Messages.Message("MessageRailgunTargetInvalid".Translate(), MessageTypeDefOf.RejectInput);
				return false;
			}
			if (Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, target.Tile) > WorldRange)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput);
				return false;
			}
			Map myMap;
			if (target.WorldObject is MapParent mapParent && mapParent.HasMap)
			{
				if (mapParent.Map == base.Map)
				{
					Messages.Message("MessageRailgunCantTargetMyMap".Translate(), MessageTypeDefOf.RejectInput);
					return false;
				}
				myMap = base.Map;
				Map map = mapParent.Map;
				Current.Game.CurrentMap = map;
				Find.Targeter.BeginTargeting(ForFireMission(), delegate(LocalTargetInfo x)
				{
					foreach (Building_Railgun selectedRailgun in selectedRailguns)
					{
						selectedRailgun.FireMission(map.Tile, x, map.uniqueID);
					}
				}, null, ActionWhenFinished, FireMissionTex);
				return true;
			}
			Messages.Message("MessageRailgunNeedsMap".Translate(), MessageTypeDefOf.RejectInput);
			return false;
			void ActionWhenFinished()
			{
				if (Find.Maps.Contains(myMap))
				{
					Current.Game.CurrentMap = myMap;
				}
			}
		}

		[SyncMethod(SyncContext.None)]
		public void FireMission(int tile, LocalTargetInfo targ, int map)
		{
			if (!targ.IsValid)
			{
				longTargetInt = GlobalTargetInfo.Invalid;
				return;
			}
			GlobalTargetInfo globalTargetInfo = targ.ToGlobalTargetInfo(Find.Maps.FirstOrDefault((Map x) => x.uniqueID == map));
			if (Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, tile) > WorldRange)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput);
				return;
			}
			if (longTargetInt != globalTargetInfo)
			{
				longTargetInt = globalTargetInfo;
				if (burstCooldownTicksLeft <= 0)
				{
					TryStartShootSomething();
				}
				SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			if (holdFire)
			{
				Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput);
			}
		}
	}
}
