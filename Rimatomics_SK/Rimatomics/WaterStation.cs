namespace Rimatomics
{
	public class WaterStation : CoolingSystem
	{
		public override float coolingCapacity
		{
			get
			{
				if ((powerComp == null || powerComp.PowerOn) && (fuel == null || fuel.HasFuel))
				{
					return ((RimatomicsThingDef)def).CoolingCapacityWatts;
				}
				return 0f;
			}
		}
	}
}
