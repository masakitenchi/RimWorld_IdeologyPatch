using RimWorld;

namespace Rimatomics
{
	public class ITab_ShellsRailgun : ITab_Storage
	{
		public override IStoreSettingsParent SelStoreSettingsParent
		{
			get
			{
				IStoreSettingsParent selStoreSettingsParent = base.SelStoreSettingsParent;
				if (selStoreSettingsParent != null)
				{
					return selStoreSettingsParent;
				}
				if (base.SelObject is Building_EnergyWeapon building_EnergyWeapon)
				{
					return GetThingOrThingCompStoreSettingsParent(building_EnergyWeapon.gun);
				}
				return null;
			}
		}

		public override bool IsPrioritySettingVisible
		{
			get
			{
				return false;
			}
		}

		public ITab_ShellsRailgun()
		{
			labelKey = "TabShells";
			tutorTag = "Shells";
		}
	}
}
