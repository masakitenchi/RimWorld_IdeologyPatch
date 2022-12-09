using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class MissileSilo : Building_LaunchPad
	{
		public float WarheadYield = 250f;

		public static SoundDef siloDoor = SoundDef.Named("siloDoor");

		public static ThingDef ICBM_Fission = ThingDef.Named("ICBM_Fission");

		public static ThingDef MissileFuselage = ThingDef.Named("MissileFuselage");

		public static ThingDef RocketEngine = ThingDef.Named("RocketEngine");

		public static ThingDef FissionWarhead = ThingDef.Named("FissionWarhead");

		public static ThingDef Chemfuel = ThingDef.Named("Chemfuel");

		public float doorSlider;

		public int Engines;

		public int EnginesCap = 4;

		public int Fuel;

		public int FuelCap = 250;

		public int Fuselage;

		public int FuselageCap = 4;

		public RimatomicResearchDef ICBMDef = RimatomicResearchDef.Named("ICBM");

		public Vector3 icbmSize = new Vector3(1.5f, 1f, 6f);

		public int icbmTickProgress;

		public float LightRot;

		public int magazine;

		public int magazineCap = 5;

		public Material MAT_ICBM;

		public RimatomicResearchDef ResearchGuidenceSystemDef = RimatomicResearchDef.Named("ResearchGuidenceSystem");

		public Graphic turretMat;

		public int Warheads;

		public int WarheadsCap = 1;

		private StringBuilder sb = new StringBuilder();

		public static readonly Texture2D HighYieldTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/HighYield");

		public static readonly Texture2D LowYieldTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/LowYield");

		public static readonly Texture2D LaunchMapTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/LaunchMap");

		public static readonly Texture2D LaunchWorldTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/LaunchWorld");

		public static readonly Texture2D LaunchAbortTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/scram");

		public virtual float MaxYield => 250f;

		public virtual float MinYield => 10f;

		public Vector3 icbmTuckPos
		{
			get
			{
				if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
				{
					return new Vector3(2f, 1f, 2f);
				}
				return new Vector3(0f, 1f, 0f);
			}
		}

		public Vector3 MissileOffset
		{
			get
			{
				if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
				{
					return new Vector3(2f, 0f, 0f);
				}
				return new Vector3(0f, 0f, -2f);
			}
		}

		public IntVec3 tubePos
		{
			get
			{
				if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
				{
					return base.Position + new IntVec3(2, 0, 1);
				}
				return base.Position + new IntVec3(0, 0, -2);
			}
		}

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
					return baseGraphic = GraphicDatabase.Get<Graphic_Multi>(def.graphicData.texPath + base.GetCamoMode, def.graphicData.shaderType.Shader, def.graphicData.drawSize, Color.white);
				}
				return base.Graphic;
			}
		}

		public Graphic siloLid
		{
			get
			{
				Vector2 drawSize = new Vector2(6f, 6f);
				if (turretMat != null)
				{
					return turretMat;
				}
				if (camoMode == "base")
				{
					return turretMat = GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/siloLid", ShaderDatabase.CutoutComplex, drawSize, DrawColor);
				}
				return turretMat = GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/siloLid" + base.GetCamoMode, ShaderDatabase.DefaultShader, drawSize, Color.white);
			}
		}

		public override int MaxLaunchDistance
		{
			get
			{
				int num = 100;
				if (!ICBMDef.IsFinished)
				{
					num -= 50;
				}
				if (ResearchGuidenceSystemDef.IsFinished)
				{
					return 9999;
				}
				return num;
			}
		}

		public override bool CanShoot => magazine > 0;

		public override void SetMode(string mode)
		{
			baseGraphic = null;
			camoMode = mode;
			turretMat = null;
			DirtyMapMesh(base.Map);
		}

		public void TakePart(Thing ding, int count)
		{
			if (ding.def == MissileFuselage)
			{
				Fuselage += count;
			}
			if (ding.def == RocketEngine)
			{
				Engines += count;
			}
			if (ding.def == FissionWarhead)
			{
				Warheads += count;
			}
			if (ding.def == Chemfuel)
			{
				Fuel += count;
			}
		}

		public ThingDef NextPart(out int count)
		{
			if (Fuselage < FuselageCap)
			{
				count = FuselageCap - Fuselage;
				return MissileFuselage;
			}
			if (Engines < EnginesCap)
			{
				count = EnginesCap - Engines;
				return RocketEngine;
			}
			if (Warheads < WarheadsCap)
			{
				count = WarheadsCap - Warheads;
				return FissionWarhead;
			}
			if (Fuel < FuelCap)
			{
				count = FuelCap - Fuel;
				return Chemfuel;
			}
			count = 0;
			return null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref Engines, "Engines", 0);
			Scribe_Values.Look(ref Fuel, "Fuel", 0);
			Scribe_Values.Look(ref Warheads, "Warheads", 0);
			Scribe_Values.Look(ref Fuselage, "Fuselage", 0);
			Scribe_Values.Look(ref magazine, "magazine", 0);
			Scribe_Values.Look(ref WarheadYield, "WarheadYield", 0f);
			Scribe_Values.Look(ref icbmTickProgress, "icbmTickProgress", 0);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			if (mode == DestroyMode.Deconstruct)
			{
				for (int i = 0; i < magazine; i++)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
					thing.stackCount = FuelCap;
					GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near);
					Thing thing2 = ThingMaker.MakeThing(MissileFuselage);
					thing2.stackCount = FuselageCap;
					GenPlace.TryPlaceThing(thing2, base.Position, map, ThingPlaceMode.Near);
					Thing thing3 = ThingMaker.MakeThing(FissionWarhead);
					thing3.stackCount = WarheadsCap;
					GenPlace.TryPlaceThing(thing3, base.Position, map, ThingPlaceMode.Near);
					Thing thing4 = ThingMaker.MakeThing(RocketEngine);
					thing4.stackCount = EnginesCap;
					GenPlace.TryPlaceThing(thing4, base.Position, map, ThingPlaceMode.Near);
				}
				if (Fuel > 0)
				{
					Thing thing5 = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
					thing5.stackCount = Fuel;
					GenPlace.TryPlaceThing(thing5, base.Position, map, ThingPlaceMode.Near);
				}
				if (Engines > 0)
				{
					Thing thing6 = ThingMaker.MakeThing(RocketEngine);
					thing6.stackCount = Engines;
					GenPlace.TryPlaceThing(thing6, base.Position, map, ThingPlaceMode.Near);
				}
				if (Warheads > 0)
				{
					Thing thing7 = ThingMaker.MakeThing(FissionWarhead);
					thing7.stackCount = Warheads;
					GenPlace.TryPlaceThing(thing7, base.Position, map, ThingPlaceMode.Near);
				}
				if (Fuselage > 0)
				{
					Thing thing8 = ThingMaker.MakeThing(MissileFuselage);
					thing8.stackCount = Fuselage;
					GenPlace.TryPlaceThing(thing8, base.Position, map, ThingPlaceMode.Near);
				}
			}
		}

		public override string GetInspectString()
		{
			sb.Clear();
			sb.Append(base.GetInspectString());
			sb.AppendLine();
			sb.Append("SiloMagazine".Translate(magazine, magazineCap));
			int count;
			ThingDef thingDef = NextPart(out count);
			if (thingDef != null)
			{
				sb.AppendLine();
				sb.Append("NextPart".Translate(thingDef.LabelCap, count));
			}
			if (WarheadYield > 150f)
			{
				sb.AppendLine();
				sb.Append("HighYieldWarhead".Translate());
			}
			else
			{
				sb.AppendLine();
				sb.Append("LowYieldWarhead".Translate());
			}
			return sb.ToString();
		}

		public override void TickLauncher()
		{
			if (powerComp != null && !powerComp.PowerOn)
			{
				launchPhase = LaunchPhase.idle;
			}
			if (powerComp != null && powerComp.PowerOn && this.IsHashIntervalTick(60))
			{
				int count;
				ThingDef thingDef = NextPart(out count);
				if (magazine < magazineCap && thingDef == null)
				{
					magazine++;
					Fuselage = 0;
					Fuel = 0;
					Warheads = 0;
					Engines = 0;
					Messages.Message("ICBMReady".Translate(), this, MessageTypeDefOf.TaskCompletion);
				}
			}
			if (launchPhase == LaunchPhase.hatch)
			{
				TickCounter--;
				if (TickCounter == 0)
				{
					SoundInfo info = SoundInfo.InMap(this);
					siloDoor.PlayOneShot(info);
					Rand.PushState();
					foreach (IntVec3 item in GenRadial.RadialCellsAround(tubePos, 2.3f, useCenter: false))
					{
						DubUtils.ThrowSmoke(item.ToVector3Shifted() + base.ScatterVec, base.Map, 1f);
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
					Building_LaunchPad.Siren.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				TickCounter--;
				if (TickCounter == 5)
				{
					SoundInfo info2 = SoundInfo.InMap(this);
					Building_LaunchPad.rocketEngineLaunch.PlayOneShot(info2);
					Rand.PushState();
					MoteThrown obj = (MoteThrown)ThingMaker.MakeThing(Building_LaunchPad.Mote_SiloFlash);
					obj.exactScale = new Vector3(13f, 1f, 13f);
					obj.exactPosition = DrawPos + MissileOffset;
					GenSpawn.Spawn(obj, tubePos, base.Map);
					foreach (IntVec3 item2 in GenRadial.RadialCellsAround(tubePos, 2.9f, useCenter: false))
					{
						DubUtils.ThrowSmoke(item2.ToVector3Shifted() + base.ScatterVec, base.Map, 2f);
					}
					Rand.PopState();
				}
				if (TickCounter == 0)
				{
					magazine--;
					TickCounter = 1f.SecondsToTicks();
					Rand.PushState();
					ICBM_Fission obj2 = GenSpawn.Spawn(ICBM_Fission, tubePos, base.Map) as ICBM_Fission;
					Rand.PopState();
					obj2?.TryLaunch(base.Target, DrawPos + icbmTuckPos, WarheadYield);
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
			if (launchPhase == LaunchPhase.countdown || launchPhase == LaunchPhase.clearTower)
			{
				doorSlider = DubUtils.FInterpTo(doorSlider, 4f, 0.16f, 0.2f);
			}
			else
			{
				doorSlider = DubUtils.FInterpTo(doorSlider, 0f, 0.16f, 0.1f);
			}
		}

		public void DrawMissile(Vector3 pos, Vector3 size, Quaternion rot, Material mat)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, rot, size);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
		}

		public override void Draw()
		{
			base.Draw();
			Vector3 drawPos = DrawPos;
			if (base.Rotation == Rot4.North || base.Rotation == Rot4.South)
			{
				drawPos.x += 2f;
				drawPos.x -= doorSlider;
				drawPos.y += 1.1f;
			}
			else
			{
				drawPos.z -= 2f;
				drawPos.z += doorSlider;
				drawPos.y += 1.1f;
			}
			siloLid.Draw(drawPos, base.Rotation, this);
			if (MAT_ICBM == null)
			{
				MAT_ICBM = new Material(GraphicsCache.ICBM_MasterMAT);
				MAT_ICBM.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
				MAT_ICBM.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.37f));
			}
			if (launchPhase == LaunchPhase.countdown)
			{
				Vector3 pos = DrawPos + icbmTuckPos;
				DrawMissile(pos, icbmSize, Quaternion.identity, MAT_ICBM);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (launchPhase != 0)
			{
				yield return new Command_Action
				{
					defaultLabel = "CancelICBMLaunch".Translate(),
					defaultDesc = "CancelICBMLaunchDesc".Translate(),
					icon = LaunchAbortTex,
					action = delegate
					{
						launchPhase = LaunchPhase.idle;
						TickCounter = 0;
						Messages.Message("ICBMLaunchCancelled".Translate(), MessageTypeDefOf.RejectInput);
					}
				};
			}
			if (WarheadYield > 150f)
			{
				yield return new Command_Action
				{
					defaultLabel = "HighYieldWarhead".Translate(),
					defaultDesc = "HighYieldWarheadDesc".Translate(),
					icon = HighYieldTex,
					action = delegate
					{
						WarheadYield = 55f;
					}
				};
			}
			else
			{
				yield return new Command_Action
				{
					defaultLabel = "LowYieldWarhead".Translate(),
					defaultDesc = "LowYieldWarheadDesc".Translate(),
					icon = LowYieldTex,
					action = delegate
					{
						WarheadYield = 250f;
					}
				};
			}
			if (launchPhase == LaunchPhase.idle)
			{
				yield return new Command_Action
				{
					defaultLabel = "ShortRangeStratLaunch".Translate(),
					defaultDesc = "ShortRangeStratLaunchDesc".Translate(),
					icon = LaunchMapTex,
					action = delegate
					{
						if (!Manned)
						{
							Messages.Message("ICBMNeedConsole".Translate(), MessageTypeDefOf.RejectInput);
						}
						else if (!CanShoot)
						{
							Messages.Message("ICBMNeedMissile".Translate(), MessageTypeDefOf.RejectInput);
						}
						else if (launchPhase != 0)
						{
							Messages.Message("ICBMAlreadyLaunching".Translate(), MessageTypeDefOf.RejectInput);
						}
						else
						{
							StartChoosingDestinationShort();
						}
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "LongRangeStratLaunch".Translate(),
					defaultDesc = "LongRangeStratLaunchDesc".Translate(),
					icon = LaunchWorldTex,
					action = delegate
					{
						if (!Manned)
						{
							Messages.Message("ICBMNeedConsole".Translate(), MessageTypeDefOf.RejectInput);
						}
						else if (!CanShoot)
						{
							Messages.Message("ICBMNeedMissile".Translate(), MessageTypeDefOf.RejectInput);
						}
						else if (launchPhase != 0)
						{
							Messages.Message("ICBMAlreadyLaunching".Translate(), MessageTypeDefOf.RejectInput);
						}
						else
						{
							StartChoosingDestination();
						}
					}
				};
			}
			Command_SetCamoMode jippo = new Command_SetCamoMode(this)
			{
				defaultLabel = "SetCamoMode".Translate(),
				defaultDesc = "SetCamoMode".Translate(),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/CamoMode")
			};
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "fill",
					action = delegate
					{
						magazine = 5;
					}
				};
			}
			yield return jippo;
		}
	}
}
