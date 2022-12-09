using Multiplayer.API;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_LaunchPad : Building, ICamoSelect
	{
		public Graphic baseGraphic;

		public string camoMode = "base";

		public CompPowerTrader powerComp;

		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("Rimatomics/UI/Nuke");

		public int TickCounter;

		public LaunchPhase launchPhase;

		public static ThingDef Mote_SiloFlash = ThingDef.Named("Mote_SiloFlash");

		public static SoundDef rocketEngineLaunch = SoundDef.Named("rocketEngineLaunch");

		public static SoundDef Siren = SoundDef.Named("Siren");

		public virtual int MaxLaunchDistance => 100;

		public virtual bool IsUnderRoof => base.Position.Roofed(base.Map);

		public virtual bool Manned => base.Map.Rimatomics().Consoles.Any((WeaponsConsole x) => x.powerComp.PowerNet == powerComp.PowerNet && x.Manned);

		public override Graphic Graphic
		{
			get
			{
				if (camoMode != "base")
				{
					if (baseGraphic != null)
					{
						return baseGraphic;
					}
					return baseGraphic = GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath + GetCamoMode, ShaderDatabase.DefaultShader, def.graphicData.drawSize, Color.white);
				}
				return base.Graphic;
			}
		}

		public virtual bool CanShoot => base.Position.GetFirstThing<SCUD>(base.Map) != null;

		public string GetCamoMode
		{
			get
			{
				if (!(camoMode == "base"))
				{
					return "-" + camoMode;
				}
				return "";
			}
		}

		public GlobalTargetInfo Target { get; private set; }

		public Vector3 ScatterVec => new Vector3(Rand.Range(-0.25f, 0.25f), 0f, Rand.Range(-0.25f, 0.25f));

		public void SpawnedCamo()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			DubUtils.GetResearch().NotifyResearch();
			SpawnedCamo();
		}

		public virtual void SetMode(string mode)
		{
			baseGraphic = null;
			camoMode = mode;
			DirtyMapMesh(base.Map);
		}

		[SyncMethod(SyncContext.None)]
		public static void SetCamoMode(Thing t, string s)
		{
			if (t is ICamoSelect camoSelect)
			{
				camoSelect.SetMode(s);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref camoMode, "camoMode", "base");
			Scribe_Values.Look(ref launchPhase, "launchPhase", LaunchPhase.idle);
			Scribe_Values.Look(ref TickCounter, "TickCounter", 0);
		}

		public void StartChoosingDestinationShort()
		{
			ChoseWorldTarget(new GlobalTargetInfo(base.Map.Parent));
		}

		public void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
			Find.WorldSelector.ClearSelection();
			int tile = base.Map.Tile;
			Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, TargeterMouseAttachment, closeWorldTabWhenFinished: true, delegate
			{
				if (MaxLaunchDistance < 200)
				{
					GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
				}
			}, delegate(GlobalTargetInfo target)
			{
				if (!target.IsValid)
				{
					return "MissileTargetInvalid".Translate();
				}
				int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
				return (MaxLaunchDistance < 200 && num > MaxLaunchDistance) ? ((string)"MissileRangeBad".Translate()) : null;
			});
		}

		public bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				Messages.Message("MissileTargetInvalid".Translate(), MessageTypeDefOf.RejectInput);
				return false;
			}
			int num = Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, target.Tile);
			if (MaxLaunchDistance < 200 && num > MaxLaunchDistance)
			{
				Messages.Message("MissileRangeBad".Translate(), MessageTypeDefOf.RejectInput);
				return false;
			}
			Map map;
			if (target.WorldObject is MapParent mapParent && mapParent.HasMap)
			{
				map = mapParent.Map;
				Current.Game.CurrentMap = map;
				Targeter targeter = Find.Targeter;
				if (this is MissileSilo missileSilo && missileSilo.WarheadYield < 150f)
				{
					HarmonyPatches.Harmony_Targeter_TargeterUpdate.DrawLowYieldField = true;
				}
				targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), Action, null, ActionWhenFinished, TargeterMouseAttachment);
				return true;
			}
			TryLaunch(target.Tile, null, -1);
			return true;
			void Action(LocalTargetInfo x)
			{
				TryLaunch(target.Tile, x, map.uniqueID);
			}
			void ActionWhenFinished()
			{
				if (Find.Maps.Contains(base.Map))
				{
					Current.Game.CurrentMap = base.Map;
				}
				HarmonyPatches.Harmony_Targeter_TargeterUpdate.DrawLowYieldField = false;
			}
		}

		[SyncMethod(SyncContext.None)]
		public void TryLaunch(int tile, LocalTargetInfo target, int map)
		{
			Target = (target.IsValid ? target.ToGlobalTargetInfo(Find.Maps.FirstOrDefault((Map x) => x.uniqueID == map)) : new GlobalTargetInfo(tile));
			launchPhase = LaunchPhase.hatch;
			TickCounter = 2f.SecondsToTicks();
		}

		public override void Tick()
		{
			base.Tick();
			TickLauncher();
		}

		public virtual void TickLauncher()
		{
			if (powerComp != null && !powerComp.PowerOn)
			{
				launchPhase = LaunchPhase.idle;
				return;
			}
			if (launchPhase == LaunchPhase.hatch)
			{
				TickCounter--;
				if (TickCounter == 0)
				{
					SoundInfo.InMap(this);
					Rand.PushState();
					foreach (IntVec3 item in GenRadial.RadialCellsAround(base.Position, 2.3f, useCenter: false))
					{
						DubUtils.ThrowSmoke(item.ToVector3() + this.TrueCenter() + ScatterVec, base.Map, 1f);
					}
					Rand.PopState();
					TickCounter = 5f.SecondsToTicks();
					launchPhase = LaunchPhase.countdown;
				}
			}
			if (launchPhase == LaunchPhase.countdown)
			{
				if (TickCounter % 60 == 0)
				{
					Siren.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				TickCounter--;
				if (TickCounter == 5)
				{
					SoundInfo info = SoundInfo.InMap(this);
					rocketEngineLaunch.PlayOneShot(info);
					Rand.PushState();
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(Mote_SiloFlash);
					obj.exactScale = new Vector3(10f, 1f, 10f);
					obj.exactPosition = base.Position.ToVector3Shifted() + this.TrueCenter();
					GenSpawn.Spawn(obj, base.Position, base.Map);
					foreach (IntVec3 item2 in GenRadial.RadialCellsAround(base.Position, 2.9f, useCenter: false))
					{
						DubUtils.ThrowSmoke(item2.ToVector3Shifted() + ScatterVec, base.Map, 2f);
					}
					Rand.PopState();
				}
				if (TickCounter == 0)
				{
					TickCounter = 1f.SecondsToTicks();
					Rand.PushState();
					ICBM_Fission obj2 = GenSpawn.Spawn(ThingDef.Named("SCUD_Chem"), base.Position, base.Map) as ICBM_Fission;
					Rand.PopState();
					Vector3 drawPos = DrawPos;
					drawPos += this.TrueCenter();
					obj2?.TryLaunch(Target, drawPos, 250f);
					launchPhase = LaunchPhase.clearTower;
				}
			}
			if (launchPhase == LaunchPhase.clearTower)
			{
				TickCounter--;
				if (TickCounter == 0)
				{
					launchPhase = LaunchPhase.idle;
				}
			}
		}
	}
}
