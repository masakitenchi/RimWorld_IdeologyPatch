using System.Collections.Generic;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class MapComponent_Rimatomics : UniversalPipeMapComp
	{
		public List<WeaponsConsole> Consoles = new List<WeaponsConsole>();

		public List<Thing> Facilities = new List<Thing>();

		public List<Building_HEL> HELS = new List<Building_HEL>();

		public List<Building_PPC> PPCs = new List<Building_PPC>();

		public List<Building_Radar> Radars = new List<Building_Radar>();

		public List<Thing> Upgradables = new List<Thing>();

		public string MapCamoMode = "base";

		public static MapComponent_Rimatomics loccachecomp = null;

		public static Dictionary<int, MapComponent_Rimatomics> CompCache = new Dictionary<int, MapComponent_Rimatomics>();

		public bool AtomActive => Radars.Any((Building_Radar x) => x.Working && x.HasATOM);

		public bool RadarActive => Radars.Any((Building_Radar x) => x.Working);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref MapCamoMode, "MapCamoMode", "base");
		}

		public MapComponent_Rimatomics(Map map)
			: base(map)
		{
			if (CompCache.ContainsKey(base.map.uniqueID))
			{
				CompCache[base.map.uniqueID] = this;
			}
			else
			{
				CompCache.Add(base.map.uniqueID, this);
			}
			loccachecomp = null;
		}

		public override void MapRemoved()
		{
			base.MapRemoved();
			CompCache.Remove(map.uniqueID);
			loccachecomp = null;
		}

		public IEnumerable<Thing> AllProjectiles()
		{
			foreach (ThingDef projectileDef in DefExtensions.ProjectileDefs)
			{
				foreach (Thing item in map.listerThings.ThingsOfDef(projectileDef))
				{
					yield return item;
				}
			}
		}

		public void RegisterHEL(Building_HEL hel)
		{
			if (!HELS.Contains(hel))
			{
				HELS.Add(hel);
			}
		}

		public void DeRegisterHEL(Building_HEL hel)
		{
			if (HELS.Contains(hel))
			{
				HELS.Remove(hel);
			}
		}

		public void RegisterConsole(WeaponsConsole console)
		{
			if (!Consoles.Contains(console))
			{
				Consoles.Add(console);
			}
		}

		public void DeRegisterConsole(WeaponsConsole console)
		{
			if (Consoles.Contains(console))
			{
				Consoles.Remove(console);
			}
		}

		public void RegisterRadar(Building_Radar Radar)
		{
			if (!Radars.Contains(Radar))
			{
				Radars.Add(Radar);
			}
		}

		public void DeRegisterRadar(Building_Radar Radar)
		{
			if (Radars.Contains(Radar))
			{
				Radars.Remove(Radar);
			}
		}
	}
}
