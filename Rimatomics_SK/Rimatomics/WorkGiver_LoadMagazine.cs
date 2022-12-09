using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimatomics
{
	public class WorkGiver_LoadMagazine : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DubDef.PPCRailgun);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Building_Railgun building_Railgun))
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (building_Railgun.magazine.Count >= building_Railgun.MagazineCap)
			{
				JobFailReason.Is("critNoSpace".Translate());
				return false;
			}
			if (t.IsBurning())
			{
				return false;
			}
			if (FindAmmo(pawn, building_Railgun) == null)
			{
				JobFailReason.Is("critNoSabotRounds".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Building_Railgun building_Railgun))
			{
				return null;
			}
			Thing thing = FindAmmo(pawn, building_Railgun);
			if (thing == null)
			{
				return null;
			}
			return new Job(DubDef.LoadRailgunMagazine, t, thing)
			{
				count = building_Railgun.MagazineCap - building_Railgun.magazine.Count
			};
		}

		public IEnumerable<Thing> Shooties(ThingFilter filter, Map map)
		{
			foreach (ThingDef allowedThingDef in filter.AllowedThingDefs)
			{
				if (!map.listerThings.listsByDef.TryGetValue(allowedThingDef, out var value))
				{
					continue;
				}
				foreach (Thing item in value)
				{
					yield return item;
				}
			}
		}

		private Thing FindAmmo(Pawn pawn, Building_Railgun railgun)
		{
			StorageSettings allowedShellsSettings = railgun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForUndefined(), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Predicate, Shooties(allowedShellsSettings.filter, pawn.Map));
			bool Predicate(Thing x)
			{
				if (!x.IsForbidden(pawn))
				{
					return pawn.CanReserve(x);
				}
				return false;
			}
		}
	}
}
