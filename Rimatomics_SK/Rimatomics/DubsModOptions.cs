using System.Collections.Generic;
using Verse;

namespace Rimatomics
{
	public class DubsModOptions : Def
	{
		public int ID;

		public List<DubsModOption> options;

		public string tip;

		public float Val(int i)
		{
			return options[i].value;
		}
	}
}
