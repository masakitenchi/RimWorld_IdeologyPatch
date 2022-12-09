using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public abstract class GameComp_RimatomicsResearch : WorldComponent
	{
		public static float GlobalProgressFactor = 0.009f;

		public Dictionary<RimatomicResearchDef, bool> Purchased = new Dictionary<RimatomicResearchDef, bool>();

		public Dictionary<ResearchStepDef, bool> StepCompleted = new Dictionary<ResearchStepDef, bool>();

		public Dictionary<ResearchStepDef, float> StepProgress = new Dictionary<ResearchStepDef, float>();

		public List<Building_RimatomicsResearchBench> ActiveBenches = new List<Building_RimatomicsResearchBench>();

		private bool notify;

		private int tickTillCheck;

		private StringBuilder text = new StringBuilder();

		private StringBuilder sb = new StringBuilder();

		public List<RimatomicResearchDef> AllProjects => DefDatabase<RimatomicResearchDef>.AllDefsListForReading;

		public List<ResearchStepDef> AllSteps => DefDatabase<ResearchStepDef>.AllDefsListForReading;

		public GameComp_RimatomicsResearch(World world)
			: base(world)
		{
		}

		public void ResetAllResearch()
		{
			Purchased = new Dictionary<RimatomicResearchDef, bool>();
			StepCompleted = new Dictionary<ResearchStepDef, bool>();
			StepProgress = new Dictionary<ResearchStepDef, float>();
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref Purchased, "Purchased", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref StepProgress, "StepProgress", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref StepCompleted, "StepCompleted", LookMode.Def, LookMode.Value);
		}

		public override void WorldComponentUpdate()
		{
			tickTillCheck++;
			if (tickTillCheck >= 2000 || notify)
			{
				tickTillCheck = 0;
				notify = false;
				CheckAllStepsAllBenches();
			}
		}

		public void RegisterBench(Building_RimatomicsResearchBench bench)
		{
			if (!ActiveBenches.Contains(bench))
			{
				ActiveBenches.Add(bench);
			}
		}

		public void DeregisterBench(Building_RimatomicsResearchBench bench)
		{
			if (ActiveBenches.Contains(bench))
			{
				ActiveBenches.Remove(bench);
			}
		}

		public bool IsActive(RimatomicResearchDef proj)
		{
			return ActiveBenches.Any((Building_RimatomicsResearchBench x) => x.currentProj == proj);
		}

		public void DoCompletionDialog(RimatomicResearchDef proj, Building_RimatomicsResearchBench bench)
		{
			text.Clear();
			text.Append("RimatomicResearchCompleteDialog".Translate(proj.ResearchLabel, proj.ResearchDescDisc));
			if (proj.BlueprintUpgrade)
			{
				text.AppendLine();
				text.AppendLine("BlueprintUpgradeDesc".Translate());
			}
			DiaNode diaNode = new DiaNode(text.ToString());
			diaNode.options.Add(DiaOption.DefaultOK);
			DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
			diaOption.action = delegate
			{
				CameraJumper.TryJumpAndSelect(bench);
			};
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true, radioMode: false, "ResearchComplete".Translate()));
		}

		public RimatomicResearchDef GetParentProject(ResearchStepDef step)
		{
			return AllProjects.FirstOrDefault((RimatomicResearchDef x) => x.Steps.Contains(step));
		}

		public bool StepActive(ResearchStepDef checkstep)
		{
			foreach (Building_RimatomicsResearchBench activeBench in ActiveBenches)
			{
				if (activeBench.currentProj != null && activeBench.currentProj.CurrentStep == checkstep)
				{
					return true;
				}
			}
			return false;
		}

		public void ResetStepProgress(ResearchStepDef step)
		{
			if (GetProgress(step) > 0f)
			{
				StepProgress[step] = 0f;
			}
		}

		public void ResearchPerformed(ResearchStepDef step, float amount, Pawn researcher, Building_RimatomicsResearchBench bench)
		{
			amount *= GlobalProgressFactor;
			if (DebugSettings.fastResearch)
			{
				amount *= 500f;
			}
			researcher?.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
			if (step != null)
			{
				float progress = GetProgress(step);
				StepProgress[step] = Mathf.Min(progress + amount, step.PointCost);
			}
			CheckAllSteps(step.ParentProject, bench);
		}

		public void CheckAllStepsAllBenches()
		{
			foreach (Building_RimatomicsResearchBench activeBench in ActiveBenches)
			{
				if (activeBench.currentProj != null)
				{
					CheckAllSteps(activeBench.currentProj, activeBench);
				}
			}
		}

		public void CheckAllSteps(RimatomicResearchDef proj, Building_RimatomicsResearchBench bench)
		{
			if (Current.ProgramState != ProgramState.Playing || proj.IsFinished)
			{
				return;
			}
			foreach (ResearchStepDef step in proj.Steps)
			{
				if (!CheckStep(step, bench))
				{
					return;
				}
			}
			foreach (Map map in Find.Maps)
			{
				foreach (Thing facility in map.Rimatomics().Facilities)
				{
					facility.DirtyMapMesh(map);
				}
			}
			DoCompletionDialog(proj, bench);
			ActiveBenches.Where((Building_RimatomicsResearchBench x) => x.currentProj == proj).ToList().ForEach(delegate(Building_RimatomicsResearchBench x)
			{
				x.currentProj = null;
			});
		}

		public bool CheckStep(ResearchStepDef step, Building_RimatomicsResearchBench bench)
		{
			if (IsCompleted(step))
			{
				return true;
			}
			if (step.PointCost > 0f && GetProgress(step) < step.PointCost)
			{
				return false;
			}
			if (!step.AgentJohnson(bench))
			{
				return false;
			}
			if (!step.RequiredThings.NullOrEmpty())
			{
				foreach (ThingDef item in step.RequiredThings)
				{
					if (!ActiveBenches.Any((Building_RimatomicsResearchBench x) => x.Map.listerThings.ThingsOfDef(item).Any()))
					{
						return false;
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				foreach (Thing facility in map.Rimatomics().Facilities)
				{
					facility.DirtyMapMesh(map);
				}
			}
			Complete(step);
			sb.Clear();
			sb.AppendLine("RimatomStepComplete".Translate() + " : " + step.GetStepLabel);
			if (step.ParentProject.CurrentStep != null)
			{
				sb.AppendLine();
				sb.AppendLine("NextResearchStep".Translate());
				sb.AppendLine(step.ParentProject.CurrentStep.GetStepLabel);
				sb.AppendLine();
				sb.AppendLine(step.ParentProject.CurrentStep.GetStepDesc);
			}
			Find.LetterStack.ReceiveLetter("RimatomStepComplete".Translate(), sb.ToString(), LetterDefOf.PositiveEvent, bench);
			step.CompleteAction();
			return true;
		}

		public void NotifyResearch()
		{
			notify = true;
		}

		public void GatherData(string data, float f)
		{
			foreach (Building_RimatomicsResearchBench item in ActiveBenches.Where((Building_RimatomicsResearchBench x) => x.currentProj != null))
			{
				ResearchStepDef currentStep = item.currentProj.CurrentStep;
				if (currentStep.GatherDataFor == data)
				{
					AddProgress(currentStep, f);
					return;
				}
			}
			ResearchStepDef researchStepDef = AllSteps.FirstOrDefault((ResearchStepDef x) => x.GatherDataFor == data);
			if (researchStepDef != null)
			{
				AddProgress(researchStepDef, f);
			}
		}

		public bool IsPurchased(RimatomicResearchDef proj)
		{
			if (proj.price == 0)
			{
				return true;
			}
			if (Purchased.TryGetValue(proj, out var value))
			{
				return value;
			}
			Purchased.Add(proj, value: false);
			return false;
		}

		public bool IsCompleted(ResearchStepDef proj)
		{
			if (StepCompleted == null)
			{
				StepCompleted = new Dictionary<ResearchStepDef, bool>();
			}
			if (StepCompleted.TryGetValue(proj, out var value))
			{
				return value;
			}
			StepCompleted.Add(proj, value: false);
			return false;
		}

		public void Complete(ResearchStepDef proj)
		{
			if (StepCompleted.TryGetValue(proj, out var _))
			{
				StepCompleted[proj] = true;
			}
			else
			{
				StepCompleted.Add(proj, value: true);
			}
		}

		public float GetProgress(ResearchStepDef step)
		{
			if (StepProgress.TryGetValue(step, out var value))
			{
				return value;
			}
			StepProgress.Add(step, 0f);
			return 0f;
		}

		public ResearchStepDef AddProgress(ResearchStepDef step, float f)
		{
			if (StepProgress.TryGetValue(step, out var _))
			{
				StepProgress[step] = Mathf.Min(StepProgress[step] + f, step.PointCost);
				return step;
			}
			StepProgress.Add(step, f);
			return step;
		}

		public float GetProgressPct(ResearchStepDef step)
		{
			return GetProgress(step) / step.PointCost;
		}
	}
}
