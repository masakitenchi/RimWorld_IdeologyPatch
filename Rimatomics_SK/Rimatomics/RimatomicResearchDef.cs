using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	public class RimatomicResearchDef : Def
	{
		public ResearchProjectDef activate;

		public bool BlueprintUpgrade;

		public bool HideOnComplete;

		public ThingDef part;

		public List<RimatomicResearchDef> prerequisites;

		private string previewImageURL = "";

		public int price;

		public RimatomicsThingDef PrimaryBuilding;

		public string ProjTypeLabel = "";

		public bool Repeating;

		public string ResearchDesc = "";

		public string ResearchDescDisc = "";

		public string ResearchLabel = "";

		private string Screenshot = "";

		public List<string> stats = new List<string>();

		public List<ResearchStepDef> Steps = new List<ResearchStepDef>();

		public ResearchStepDef UnlockStep;

		public bool Upgrade;

		private StringBuilder sb = new StringBuilder();

		public List<Building_RimatomicsResearchBench> ActiveBenches => DubUtils.GetResearch().ActiveBenches.Where((Building_RimatomicsResearchBench x) => x.currentProj == this).ToList();

		public virtual bool PrerequisitesCompleted
		{
			get
			{
				if (activate != null && !activate.IsFinished)
				{
					return false;
				}
				if (prerequisites != null)
				{
					if (prerequisites.Any((RimatomicResearchDef x) => x.IsFinished))
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public virtual bool CanStartNow
		{
			get
			{
				if (!IsFinished)
				{
					return PrerequisitesCompleted;
				}
				return false;
			}
		}

		public virtual bool IsFinished => Steps.All((ResearchStepDef x) => x.IsFinished);

		public virtual Texture2D screenshot
		{
			get
			{
				Texture2D texture2D = ContentFinder<Texture2D>.Get(Screenshot, reportFailure: false);
				if (!texture2D)
				{
					return PreviewImage;
				}
				return texture2D;
			}
		}

		public virtual Texture2D PreviewImage
		{
			get
			{
				Texture2D texture2D = ContentFinder<Texture2D>.Get(previewImageURL, reportFailure: false);
				if (!texture2D)
				{
					return ContentFinder<Texture2D>.Get("Rimatomics/UI/NoImage", reportFailure: false);
				}
				return texture2D;
			}
		}

		public virtual ResearchStepDef CurrentStep => Steps.FirstOrDefault((ResearchStepDef x) => !x.IsFinished);

		public static RimatomicResearchDef Named(string defName)
		{
			return DefDatabase<RimatomicResearchDef>.GetNamed(defName);
		}

		public virtual string BuildPrereqString(string s, List<ResearchProjectDef> defs)
		{
			sb.Clear();
			sb.AppendLine("StepReaserchRequiredResearchDesc".Translate());
			foreach (ResearchProjectDef def in defs)
			{
				sb.AppendLine(def.label);
			}
			return sb.ToString();
		}

		public virtual void ResearchCompleted()
		{
		}
	}
}
