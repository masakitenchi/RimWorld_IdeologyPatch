using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_RimatomicsResearchBench : Building
	{
		public MapComponent_Rimatomics mapcomp;

		public RimatomicsResearch Research;

		public CompPowerTrader powerComp;

		public RimatomicResearchDef currentProj;

		public CompAffectedByFacilities facilityComp;

		private Graphic offGraphic;

		private StringBuilder sb = new StringBuilder();

		public bool CanUseConsoleNow
		{
			get
			{
				if (!base.Spawned || !base.Map.gameConditionManager.ElectricityDisabled)
				{
					return powerComp.PowerOn;
				}
				return false;
			}
		}

		public override void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this, 0f);
			if (powerComp.PowerOn)
			{
				if (offGraphic == null)
				{
					offGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, def.graphicData.drawSize, DrawColor, DrawColorTwo);
				}
				offGraphic.Print(layer, this, 0f);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			switch (signal)
			{
			case "PowerTurnedOn":
			case "PowerTurnedOff":
			case "FlickedOn":
			case "FlickedOff":
			case "Refueled":
			case "RanOutOfFuel":
			case "ScheduledOn":
			case "ScheduledOff":
				base.Map?.mapDrawer?.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
				break;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			facilityComp = GetComp<CompAffectedByFacilities>();
			powerComp = GetComp<CompPowerTrader>();
			Research = DubUtils.GetResearch();
			Research.RegisterBench(this);
			mapcomp = base.Map.Rimatomics();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(DestroyMode.Vanish);
			Research.DeregisterBench(this);
		}

		[SyncMethod(SyncContext.None)]
		public void SetProject(RimatomicResearchDef proj)
		{
			currentProj = proj;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref currentProj, "currentProj");
		}

		public void ResearchPerformed(float amount, Pawn researcher, Building_RimatomicsResearchBench bench)
		{
			Research.ResearchPerformed(currentProj.CurrentStep, amount, researcher, bench);
		}

		public override string GetInspectString()
		{
			sb.Clear();
			sb.Append(base.GetInspectString());
			if (Research.TraderCooldown > 0)
			{
				sb.AppendLine();
				sb.Append("TradeShipCooldown".Translate(Research.TraderCooldown.ToStringTicksToPeriod()));
			}
			if (Research.TicksToShipArrive > 0)
			{
				sb.AppendLine();
				sb.Append("TradeShipArrival".Translate(Research.TicksToShipArrive.ToStringTicksToPeriod()));
			}
			return sb.ToString().TrimEndNewlines();
		}

		[SyncMethod(SyncContext.None)]
		public static void BuyProject(int mapID, RimatomicResearchDef project)
		{
			Map map = Find.Maps.FirstOrDefault((Map x) => x.uniqueID == mapID);
			TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, project.price, map, null);
			Purchase(project);
			Messages.Message("ResearchPurchased".Translate(), MessageTypeDefOf.PositiveEvent);
		}

		[SyncMethod(SyncContext.None)]
		public void TradeShipReq()
		{
			if (Research.TraderCooldown > 0)
			{
				Messages.Message("TradeShipWait".Translate(Research.TraderCooldown.ToStringTicksToPeriod()), MessageTypeDefOf.RejectInput);
				return;
			}
			if (!TradeUtility.ColonyHasEnoughSilver(base.MapHeld, RimatomicsResearch.SilverForShip))
			{
				Messages.Message("TradeShipPriceNeed".Translate(RimatomicsResearch.SilverForShip), MessageTypeDefOf.RejectInput);
				return;
			}
			TradeUtility.LaunchSilver(base.MapHeld, RimatomicsResearch.SilverForShip);
			Research.TicksToShipArrive = 120000;
			Research.TraderCooldown = (int)(60000f * RimatomicsMod.Settings.RimatomicsTraderCooldown);
			DiaNode diaNode = new DiaNode("TradeShipReq".Translate());
			diaNode.options.Add(DiaOption.DefaultOK);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
		}

		[SyncMethod(SyncContext.None)]
		public void forceship()
		{
			Research.TicksToShipArrive = 1;
		}

		[SyncMethod(SyncContext.None)]
		public void endcooldown()
		{
			Research.TraderCooldown = 1;
		}

		[SyncMethod(SyncContext.None)]
		public static void DebugFinish(RimatomicResearchDef proj)
		{
			RimatomicsResearch research = DubUtils.GetResearch();
			foreach (ResearchStepDef step in proj.Steps)
			{
				research.GetProgress(step);
				research.StepProgress[step] = step.PointCost;
				research.Complete(step);
			}
			research.ActiveBenches.Where((Building_RimatomicsResearchBench x) => x.currentProj == proj).ToList().ForEach(delegate(Building_RimatomicsResearchBench x)
			{
				x.currentProj = null;
			});
		}

		[SyncMethod(SyncContext.None)]
		public static void Purchase(RimatomicResearchDef proj)
		{
			RimatomicsResearch research = DubUtils.GetResearch();
			if (research.Purchased.TryGetValue(proj, out var _))
			{
				research.Purchased[proj] = true;
			}
			else
			{
				research.Purchased.Add(proj, value: true);
			}
		}

		[SyncMethod(SyncContext.None)]
		public static void ResetProj(RimatomicResearchDef proj)
		{
			RimatomicsResearch research = DubUtils.GetResearch();
			foreach (ResearchStepDef step in proj.Steps)
			{
				research.StepProgress[step] = 0f;
				research.StepCompleted[step] = false;
			}
		}

		[SyncMethod(SyncContext.None)]
		public static void ResetStep(ResearchStepDef Step)
		{
			RimatomicsResearch research = DubUtils.GetResearch();
			research.StepProgress[Step] = 0f;
			research.StepCompleted[Step] = false;
		}

		[SyncMethod(SyncContext.None)]
		public static void FinStep(ResearchStepDef Step)
		{
			DubUtils.GetResearch().StepProgress[Step] = Step.PointCost;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "force ship",
					defaultDesc = "",
					action = forceship
				};
				yield return new Command_Action
				{
					defaultLabel = "end cooldown",
					defaultDesc = "",
					action = endcooldown
				};
			}
			yield return new Command_Action
			{
				defaultLabel = "TradeShipPrice".Translate(RimatomicsResearch.SilverForShip),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/logo"),
				defaultDesc = "TradeShipPrice".Translate(RimatomicsResearch.SilverForShip),
				action = TradeShipReq
			};
		}
	}
}
