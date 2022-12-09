using System.Collections.Generic;
using Verse;

namespace Rimatomics
{
	public class CompProperties_Upgradable : CompProperties
	{
		public List<Upgrades> upgrades = new List<Upgrades>();

		public CompProperties_Upgradable()
		{
			compClass = typeof(CompUpgradable);
		}
	}
}
