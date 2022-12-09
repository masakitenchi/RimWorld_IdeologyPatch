using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class greekAlpha
	{
		public static List<string> alphas;

		static greekAlpha()
		{
			alphas = new List<string>(new string[24]
			{
				"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa",
				"Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma", "Tau", "Upsilon",
				"Phi", "Chi", "Psi", "Omega"
			});
		}

		public static string getAlpha(int ID)
		{
			return alphas.ElementAt(ID);
		}

		public static string getAlphaString(int ID, Map map)
		{
			return alphas.ElementAt(ID);
		}

		public static int getInt(string b)
		{
			for (int i = 0; i < alphas.Count(); i++)
			{
				if (alphas.ElementAt(i) == b)
				{
					return i;
				}
			}
			return 0;
		}
	}
}
