using System;
using RimWorld;
using Verse.AI.Group;

namespace SkyMind
{
	// Token: 0x02000023 RID: 35
	public class LordJob_ExitMapPanicFlee : LordJob
	{
		// Token: 0x0600013B RID: 315 RVA: 0x0001E07C File Offset: 0x0001C27C
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			stateGraph.AddToil(new LordToil_PanicFlee
			{
				useAvoidGrid = false
			});
			return stateGraph;
		}
	}
}
