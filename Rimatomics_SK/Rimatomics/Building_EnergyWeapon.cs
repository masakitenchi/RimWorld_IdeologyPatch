using System.Collections.Generic;
using System.Text;
using Multiplayer.API;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_EnergyWeapon : Building_Turret, ICamoSelect
	{
		public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("Rimatomics/UI/laserTarget");

		public LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

		public GlobalTargetInfo longTargetInt = GlobalTargetInfo.Invalid;

		public List<ThingDef> magazine = new List<ThingDef>();

		public int burstCooldownTicksLeft;

		public int burstWarmupTicksLeft;

		public float TurretRotation;

		public bool holdFire;

		public Building_EnergyWeaponTop top;

		public Thing gun;

		public CompUpgradable UG;

		public CompPowerTrader powerComp;

		public MapComponent_Rimatomics MapComp;

		public int KillCounter;

		public float DamageDealt;

		public float EnergyLossPerDamage = 40f;

		private Vector3 impactAngleVect;

		public float SCADlife;

		public bool HasSCAD;

		public string camoMode = "base";

		public Graphic baseGraphic;

		public Graphic turretMat;

		private StringBuilder sb = new StringBuilder();

		public virtual GlobalTargetInfo longTarget => longTargetInt;

		public bool AnyConsoles => base.Map.Rimatomics().Consoles.Any((WeaponsConsole x) => x.PowerComp.PowerNet == base.PowerComp.PowerNet && x.powerComp.PowerOn);

		public bool IsConsoleManned
		{
			get
			{
				if (MannedConsole == null)
				{
					return MP.IsInMultiplayer;
				}
				return true;
			}
		}

		public WeaponsConsole MannedConsole => base.Map.Rimatomics().Consoles.FirstOrDefault((WeaponsConsole x) => x.PowerComp.PowerNet == base.PowerComp.PowerNet && x.powerComp.PowerOn && x.Manned);

		public override string Label
		{
			get
			{
				if (DubUtils.IsPrototype(def))
				{
					return base.Label + "rimatomicsProto".Translate();
				}
				return base.Label;
			}
		}

		public RimatomicsThingDef GunProps => gun.def as RimatomicsThingDef;

		public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

		public override LocalTargetInfo CurrentTarget => currentTargetInt;

		public bool WarmingUp => burstWarmupTicksLeft > 0;

		public override Verb AttackVerb => GunCompEq.PrimaryVerb;

		public virtual bool CanSetForcedTarget => true;

		public bool CanToggleHoldFire => true;

		public bool MannedByColonist => false;

		public int VetLevel => Mathf.Min(Mathf.FloorToInt(((float)KillCounter - 25f) / 15f), 4);

		public virtual bool TurretBased => false;

		public virtual Vector3 TipOffset => Vector3.zero;

		public virtual int Damage => GunProps.EnergyWep.Damage;

		public virtual float WarmupForShot => AttackVerb.verbProps.warmupTime;

		public virtual float Range => GunProps.EnergyWep.range;

		public virtual float RangeMin => GunProps.EnergyWep.minRange;

		public virtual int WorldRange => GunProps.EnergyWep.WorldRange;

		public virtual int MagazineCap => GunProps.EnergyWep.magazineCap;

		public virtual int ShotCount => AttackVerb.verbProps.burstShotCount;

		public virtual float CooldownForShot
		{
			get
			{
				float num = GunProps.EnergyWep.cooldownForShot;
				if (UG.HasUpgrade(DubDef.ALC))
				{
					num -= 0.15f * num;
				}
				float num2 = (float)(VetLevel + 1) * 0.02f;
				if (num2 > 0f)
				{
					num -= num2 * num;
				}
				return num;
			}
		}

		public virtual float PulseSize
		{
			get
			{
				float num = GunProps.EnergyWep.PulseSizeScaled;
				if (UG.HasUpgrade(DubDef.ERS))
				{
					num -= 0.15f * num;
				}
				return num;
			}
		}

		public virtual float turretSpeed
		{
			get
			{
				float num = GunProps.EnergyWep.turretSpeed;
				if (UG.HasUpgrade(DubDef.DriveActuator))
				{
					num *= 2f;
				}
				return num;
			}
		}

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

		public override Graphic Graphic
		{
			get
			{
				if (GunProps.EnergyWep.CanCamo && camoMode != "base")
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

		public Graphic TurretGraphic
		{
			get
			{
				if (turretMat != null && turretMat.Color == Graphic.Color)
				{
					return turretMat;
				}
				Vector2 drawSize = new Vector2(def.building.turretTopDrawSize, def.building.turretTopDrawSize);
				if (GunProps.EnergyWep.TurretCamo)
				{
					if (camoMode == "base")
					{
						return GraphicDatabase.Get<Graphic_Single>(gun.def.graphicData.texPath, ShaderDatabase.CutoutComplex, drawSize, DrawColor);
					}
					return turretMat = GraphicDatabase.Get<Graphic_Single>(gun.def.graphicData.texPath + GetCamoMode, ShaderDatabase.DefaultShader, drawSize, Color.white);
				}
				return turretMat = GraphicDatabase.Get<Graphic_Single>(gun.def.graphicData.texPath, ShaderDatabase.DefaultShader, drawSize, Color.white);
			}
		}

		public void SpawnedCamo()
		{
		}

		public virtual void DrawArc(Vector3 A, Vector3 B, Material mat)
		{
			if (!(Mathf.Abs(A.x - B.x) < 0.01f) || !(Mathf.Abs(A.z - B.z) < 0.01f))
			{
				Vector3 pos = (A + B) / 2f;
				if (!(A == B))
				{
					A.y = B.y;
					float z = (A - B).MagnitudeHorizontal();
					Quaternion q = Quaternion.LookRotation(A - B);
					Vector3 s = new Vector3(1f, 1f, z);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(pos, q, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
				}
			}
		}

		public virtual void MuzzleFlash()
		{
		}

		public bool HasCharge(float v)
		{
			return powerComp.PowerNet.HasCharge(v);
		}

		public bool DissipateCharge(float v)
		{
			return powerComp.PowerNet.DissipateCharge(v);
		}

		public override void Tick()
		{
			base.Tick();
			TickGuns();
		}

		public Building_EnergyWeapon()
		{
			if (TurretBased)
			{
				top = new Building_EnergyWeaponTop(this);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			UG = GetComp<CompUpgradable>();
			MapComp = base.Map.Rimatomics();
			HasSCAD = UG.HasUpgrade(DubDef.SCAD);
			DubUtils.GetResearch().NotifyResearch();
			SpawnedCamo();
		}

		public override void PostMake()
		{
			base.PostMake();
			MakeGun();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			ResetCurrentTarget();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref TurretRotation, "TurretRotation", 0f);
			Scribe_Values.Look(ref SCADlife, "SCADlife", 0f);
			Scribe_Values.Look(ref KillCounter, "KillCounter", 0);
			Scribe_Collections.Look(ref magazine, "magazine", LookMode.Def);
			Scribe_Values.Look(ref camoMode, "camoMode", "base");
			Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
			Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
			Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
			Scribe_TargetInfo.Look(ref longTargetInt, "longTargetInt");
			Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
			Scribe_Deep.Look(ref gun, "gun");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				UpdateGunVerbs();
			}
		}

		public void MakeGun()
		{
			gun = ThingMaker.MakeThing(def.building.turretGunDef);
			UpdateGunVerbs();
		}

		public void UpdateGunVerbs()
		{
			foreach (Verb allVerb in gun.TryGetComp<CompEquippable>().AllVerbs)
			{
				allVerb.caster = this;
				allVerb.castCompleteCallback = BurstComplete;
			}
		}

		public override void Draw()
		{
			top?.DrawTurret();
			base.Draw();
			if (RimatomicsMod.Settings.ShowVetPatches && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && KillCounter > 25)
			{
				Matrix4x4 matrix = default(Matrix4x4);
				Vector3 pos = this.OccupiedRect().BottomLeft.ToVector3();
				pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				pos += new Vector3(0.5f, 0f, 0.5f);
				matrix.SetTRS(pos, Quaternion.identity, new Vector3(0.7f, 1f, 0.7f));
				Graphics.DrawMesh(MeshPool.plane10, matrix, GraphicsCache.VetPatches[VetLevel], 0);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			if (signal == "upgraded")
			{
				bool hasSCAD = HasSCAD;
				HasSCAD = GetComp<CompUpgradable>().HasUpgrade(DubDef.SCAD);
				if (!hasSCAD && HasSCAD)
				{
					SCADlife = 1000f;
				}
			}
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (HasSCAD)
			{
				float charge = EnergyLossPerDamage * dinfo.Amount;
				if (powerComp.PowerNet != null && powerComp.PowerNet.DissipateCharge(charge))
				{
					dinfo.SetAmount(dinfo.Amount * 0.1f);
					AbsorbedDamage(dinfo);
					absorbed = true;
				}
			}
			base.PreApplyDamage(ref dinfo, out absorbed);
		}

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SCADlife -= dinfo.Amount;
			if (SCADlife <= 0f)
			{
				BreakSCAD();
			}
			DubDef.BulletImpact_SCAD.PlayOneShot(new TargetInfo(base.Position, base.Map));
			impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = this.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			FleckMaker.Static(loc, base.Map, FleckDefOf.ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				FleckMaker.ThrowDustPuff(loc, base.Map, Rand.Range(0.8f, 1.2f));
			}
		}

		public void BreakSCAD()
		{
			UG.RemoveUpgrade(DubDef.SCAD);
		}

		public void GatherData(string data, float f)
		{
			DubUtils.GetResearch().GatherData(data, f);
		}

		public void PrototypeBang(float chance = 0.05f)
		{
			if (Rand.Chance(chance) && DubUtils.IsPrototype(def))
			{
				ShortCircuitUtility.DoShortCircuit(this);
			}
		}

		[SyncMethod(SyncContext.None)]
		public void SetMode(string mode)
		{
			camoMode = mode;
			turretMat = null;
			baseGraphic = null;
			DirtyMapMesh(base.Map);
		}

		public override string GetInspectString()
		{
			sb.Clear();
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				sb.AppendLine(inspectString);
			}
			if (HasSCAD)
			{
				sb.AppendLine("ScadLivesMatter".Translate((SCADlife / 1000f).ToStringPercent("0")));
			}
			if (powerComp.PowerNet != null && !powerComp.PowerNet.HasCharge(1f))
			{
				sb.AppendLine("MissingPPC".Translate());
			}
			else
			{
				sb.AppendLine("PulseSize".Translate(PulseSize, Damage));
			}
			if (!AnyConsoles)
			{
				sb.AppendLine("EnergyWeaponNeedConsole".Translate());
			}
			else
			{
				if (GunProps.EnergyWep.NeedsManning && !IsConsoleManned)
				{
					sb.AppendLine("EnergyWeaponNeedsMannedConsole".Translate());
				}
				if (GunProps.EnergyWep.NeedsRadar && !base.Map.Rimatomics().RadarActive)
				{
					sb.AppendLine("needsRadar".Translate());
				}
			}
			if (KillCounter > 0)
			{
				sb.Append("KillCounter".Translate(KillCounter));
				if (KillCounter > 25)
				{
					sb.AppendLine("VeteranBonus".Translate((VetLevel + 1) * 2));
				}
				else
				{
					sb.AppendLine();
				}
			}
			if (base.Spawned && burstCooldownTicksLeft > 0)
			{
				sb.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
			}
			CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null && !compChangeableProjectile.Loaded)
			{
				sb.AppendLine("ShellNotLoaded".Translate());
			}
			if (MagazineCap > 0)
			{
				sb.AppendLine("critMagazine".Translate(magazine.Count));
			}
			return sb.ToString().TrimEndNewlines();
		}

		public override void DrawExtraSelectionOverlays()
		{
			if (Range < 90f)
			{
				GenDraw.DrawRadiusRing(base.Position, Range);
			}
			float rangeMin = RangeMin;
			if (rangeMin < 90f && rangeMin > 0.1f)
			{
				GenDraw.DrawRadiusRing(base.Position, rangeMin);
			}
			if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
			{
				Vector3 b = ((!forcedTarget.HasThing) ? forcedTarget.Cell.ToVector3Shifted() : forcedTarget.Thing.TrueCenter());
				Vector3 a = this.TrueCenter();
				b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = b.y;
				GenDraw.DrawLineBetween(a, b, Building_TurretGun.ForcedTargetLineMat);
			}
		}

		[SyncMethod(SyncContext.None)]
		public void dumpShells()
		{
			GenPlace.TryPlaceThing(gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near);
			foreach (ThingDef item in magazine)
			{
				GenPlace.TryPlaceThing(ThingMaker.MakeThing(item), base.Position, base.Map, ThingPlaceMode.Near);
			}
			magazine.Clear();
		}

		[SyncMethod(SyncContext.None)]
		public void CommandStopForceAttack()
		{
			ResetForcedTarget();
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		[SyncMethod(SyncContext.None)]
		public void CommandHoldFire()
		{
			holdFire = !holdFire;
			if (holdFire)
			{
				ResetForcedTarget();
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null)
			{
				Command_Action command_Action = new Command_Action
				{
					defaultLabel = "CommandExtractShell".Translate(),
					defaultDesc = "CommandExtractShellDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("Rimatomics/Things/Resources/sabot/sabot_c"),
					alsoClickIfOtherInGroupClicked = false,
					action = dumpShells
				};
				if (compChangeableProjectile.Projectile == null)
				{
					command_Action.Disable("NoSabotToExtract".Translate());
				}
				yield return command_Action;
			}
			if (CanSetForcedTarget && IsConsoleManned)
			{
				yield return new Command_VerbTarget
				{
					defaultLabel = "CommandSetForceAttackTarget".Translate(),
					defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
					verb = GunCompEq.PrimaryVerb,
					wep = this,
					hotKey = KeyBindingDefOf.Misc4
				};
			}
			if (forcedTarget.IsValid || longTarget.IsValid)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandStopForceAttack".Translate(),
					defaultDesc = "CommandStopForceAttackDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
					action = CommandStopForceAttack,
					hotKey = KeyBindingDefOf.Misc5
				};
			}
			if (CanToggleHoldFire)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "CommandHoldFire".Translate(),
					defaultDesc = "CommandHoldFireDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
					hotKey = KeyBindingDefOf.Misc6,
					toggleAction = CommandHoldFire,
					isActive = () => holdFire
				};
			}
			if (GunProps.EnergyWep.CanCamo)
			{
				Command_SetCamoMode command_SetCamoMode = new Command_SetCamoMode(this);
				command_SetCamoMode.defaultLabel = "SetCamoMode".Translate();
				command_SetCamoMode.defaultDesc = "SetCamoMode".Translate();
				command_SetCamoMode.icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/CamoMode");
				yield return command_SetCamoMode;
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Give kills",
					defaultDesc = "",
					action = delegate
					{
						KillCounter += 5;
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "Fill Mag",
					defaultDesc = "",
					action = delegate
					{
						gun.TryGetComp<CompChangeableProjectile>().LoadShell(DubDef.RailgunSabot, 25);
					}
				};
			}
			foreach (Command item in DubDef.BilderbergCommands(def))
			{
				yield return item;
			}
		}

		[SyncMethod(SyncContext.None)]
		public override void OrderAttack(LocalTargetInfo targ)
		{
			longTargetInt = GlobalTargetInfo.Invalid;
			if (!targ.IsValid)
			{
				if (forcedTarget.IsValid)
				{
					ResetForcedTarget();
				}
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal < RangeMin)
			{
				Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput);
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal > Range)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput);
				return;
			}
			if (forcedTarget != targ)
			{
				forcedTarget = targ;
				if (burstCooldownTicksLeft <= 0)
				{
					TryStartShootSomething();
				}
			}
			if (holdFire)
			{
				Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput);
			}
		}

		public virtual void TickGuns()
		{
			if (longTarget.IsValid && longTarget.ThingDestroyed)
			{
				ResetForcedTarget();
			}
			if (longTarget.IsValid && longTarget.Map == null)
			{
				ResetForcedTarget();
			}
			if (forcedTarget.IsValid && !CanSetForcedTarget)
			{
				ResetForcedTarget();
			}
			if (!CanToggleHoldFire)
			{
				holdFire = false;
			}
			if (forcedTarget.ThingDestroyed)
			{
				ResetForcedTarget();
			}
			if ((powerComp == null || powerComp.PowerOn) && AnyConsoles && (!GunProps.EnergyWep.NeedsManning || IsConsoleManned) && base.Spawned)
			{
				if (!stunner.Stunned && GunProps.EnergyWep.HasTurret)
				{
					top.TurretTopTick();
				}
				GunCompEq.verbTracker.VerbsTick();
				if (stunner.Stunned || GunCompEq.PrimaryVerb.state == VerbState.Bursting)
				{
					return;
				}
				if (WarmingUp)
				{
					burstWarmupTicksLeft--;
					if (burstWarmupTicksLeft == 0)
					{
						BeginBurst();
					}
					return;
				}
				if (burstCooldownTicksLeft > 0)
				{
					burstCooldownTicksLeft--;
				}
				if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
				{
					TryStartShootSomething();
				}
			}
			else
			{
				ResetCurrentTarget();
			}
		}

		public virtual bool CanFireWhileRoofed()
		{
			if (GunCompEq.PrimaryVerb.ProjectileFliesOverhead() && base.Position.Roofed(base.Map))
			{
				return false;
			}
			return true;
		}

		public virtual void TryStartShootSomething()
		{
			if (!base.Spawned || (holdFire && CanToggleHoldFire) || (GunProps.EnergyWep.NeedsRadar && !base.Map.Rimatomics().RadarActive) || !CanFireWhileRoofed())
			{
				ResetCurrentTarget();
				return;
			}
			if (longTarget.IsValid)
			{
				if ((!TurretBased || top.TargetInSights) && powerComp.PowerNet.HasCharge(PulseSize))
				{
					burstWarmupTicksLeft = WarmupForShot.SecondsToTicks();
					GunProps.EnergyWep.ChargeUpSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(this)));
				}
				currentTargetInt = LocalTargetInfo.Invalid;
				return;
			}
			bool isValid = currentTargetInt.IsValid;
			if (forcedTarget.IsValid)
			{
				currentTargetInt = forcedTarget;
			}
			else if (!CanShoot())
			{
				currentTargetInt = TryFindNewTarget();
			}
			if (!isValid && currentTargetInt.IsValid)
			{
				SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
			if (currentTargetInt.IsValid)
			{
				if ((!TurretBased || top.TargetInSights) && CanShoot() && powerComp.PowerNet.HasCharge(PulseSize))
				{
					burstWarmupTicksLeft = WarmupForShot.SecondsToTicks();
					GunProps.EnergyWep.ChargeUpSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(this)));
				}
			}
			else
			{
				ResetCurrentTarget();
			}
		}

		public bool CanShoot()
		{
			if (currentTargetInt.IsValid && !currentTargetInt.ThingDestroyed && (!(currentTargetInt.Thing is Pawn pawn) || !pawn.Downed) && InRange())
			{
				ShootLine resultingLine;
				if (AttackVerb.verbProps.requireLineOfSight)
				{
					return AttackVerb.TryFindShootLineFromTo(base.Position, currentTargetInt, out resultingLine);
				}
				return true;
			}
			return false;
		}

		public bool InRange()
		{
			if ((CurrentTarget.Cell - base.Position).LengthHorizontal < RangeMin)
			{
				return false;
			}
			if ((CurrentTarget.Cell - base.Position).LengthHorizontal > Range)
			{
				return false;
			}
			return true;
		}

		public virtual LocalTargetInfo TryFindNewTarget()
		{
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
			if (AttackVerb.verbProps.requireLineOfSight)
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToAll;
			}
			return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, targetScanFlags, IsValidTarget);
		}

		private bool IsValidTarget(Thing t)
		{
			if (!CanFireWhileRoofed() && GunCompEq.PrimaryVerb.ProjectileFliesOverhead())
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
				if (roofDef != null && roofDef.isThickRoof)
				{
					return false;
				}
			}
			if (t is Pawn pawn)
			{
				if (!IsConsoleManned)
				{
					return !GenAI.MachinesLike(base.Faction, pawn);
				}
				if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
			return true;
		}

		public void BeginBurst()
		{
			if (longTarget.IsValid)
			{
				Vector3 vector = Vector3.forward.RotatedBy(TurretRotation);
				IntVec3 intVec = (DrawPos + vector * 500f).ToIntVec3();
				AttackVerb.TryStartCastOn(intVec);
			}
			else if (!AttackVerb.TryStartCastOn(CurrentTarget) && DebugSettings.godMode)
			{
				Log.Warning($"{LabelCap} failed to cast attack on {CurrentTarget} (godmode)");
			}
			OnAttackedTarget(CurrentTarget);
		}

		public void BurstComplete()
		{
			burstCooldownTicksLeft = CooldownForShot.SecondsToTicks();
		}

		public void ResetForcedTarget()
		{
			currentTargetInt = LocalTargetInfo.Invalid;
			longTargetInt = GlobalTargetInfo.Invalid;
			forcedTarget = LocalTargetInfo.Invalid;
			burstWarmupTicksLeft = 0;
			if (burstCooldownTicksLeft <= 0)
			{
				TryStartShootSomething();
			}
		}

		public void ResetCurrentTarget()
		{
			longTargetInt = GlobalTargetInfo.Invalid;
			currentTargetInt = LocalTargetInfo.Invalid;
			burstWarmupTicksLeft = 0;
		}
	}
}
