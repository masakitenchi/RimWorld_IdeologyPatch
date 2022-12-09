using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimatomics
{
	public class ResearchStepDef : Def
	{
		public List<RimatomicsFailureDef> FacilityFailures;

		public string GatherDataFor;

		public float PointCost;

		public List<RecipeDef> RecipeUnlocks = new List<RecipeDef>();

		public List<ThingDef> requiredResearchFacilities;

		public List<ThingDef> RequiredThings = new List<ThingDef>();

		public int RequiredSkillLevel;

		public BuildStandingMode StandMode;

		public List<RimatomicsThingDef> Unlocks = new List<RimatomicsThingDef>();

		public WorkTypeDef WorkType;

		public virtual RimatomicResearchDef ParentProject => GetParentProject();

		public virtual float ProgressReal => DubUtils.GetResearch().GetProgress(this);

		public virtual bool IsFinished => DubUtils.GetResearch().IsCompleted(this);

		public virtual string GetStepLabel
		{
			get
			{
				if (RequiredSkillLevel > 0 && WorkType != null)
				{
					return label + string.Format(" ({0} {1})", RequiredSkillLevel, WorkType.relevantSkills?.FirstOrDefault()?.skillLabel.CapitalizeFirst() ?? "");
				}
				return label;
			}
		}

		public virtual string GetStepDesc => description;

		public bool UsesFacility(ThingDef def)
		{
			if (requiredResearchFacilities != null)
			{
				return requiredResearchFacilities.Any((ThingDef x) => x == def);
			}
			return false;
		}

		public static ResearchStepDef Named(string defName)
		{
			return DefDatabase<ResearchStepDef>.GetNamed(defName);
		}

		public virtual bool CanBeResearchedAt(Building_RimatomicsResearchBench bench, bool ignoreResearchBenchPowerStatus)
		{
			if (!ignoreResearchBenchPowerStatus)
			{
				CompPowerTrader comp = bench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
				{
					return false;
				}
			}
			if (!requiredResearchFacilities.NullOrEmpty())
			{
				for (int i = 0; i < requiredResearchFacilities.Count; i++)
				{
					if (!bench.Map.listerThings.ThingsOfDef(requiredResearchFacilities[i]).OfType<Building>().Any((Building x) => x.GetComp<CompFacility>().CanBeActive))
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual RimatomicResearchDef GetParentProject()
		{
			return DefDatabase<RimatomicResearchDef>.AllDefsListForReading.FirstOrDefault((RimatomicResearchDef x) => x.Steps.Contains(this));
		}

		public virtual bool AgentJohnson(Building_RimatomicsResearchBench bench)
		{
			return true;
		}

		public virtual void CompleteAction()
		{
		}
	}
}
