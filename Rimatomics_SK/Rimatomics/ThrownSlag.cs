using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class ThrownSlag : Thing
	{
		public Vector3 currentPoint;

		public Vector3 endPoint;

		public Vector3 startPoint;

		public Vector3 vel = new Vector3(0f, 0f, 100f);

		public override Vector3 DrawPos => currentPoint;

		public override void PostMake()
		{
			base.PostMake();
		}

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override void Tick()
		{
			currentPoint = Vector3.SmoothDamp(currentPoint, endPoint, ref vel, 0.01f, 40f, 0.0166f);
		}

		private void HitRoof()
		{
			if (!base.Position.Roofed(base.Map))
			{
				return;
			}
			RoofCollapserImmediate.DropRoofInCells(this.OccupiedRect().ExpandedBy(1).Cells.Where(delegate(IntVec3 c)
			{
				if (!c.InBounds(base.Map))
				{
					return false;
				}
				if (c == base.Position)
				{
					return true;
				}
				if (base.Map.thingGrid.CellContains(c, ThingCategory.Pawn))
				{
					return false;
				}
				Building edifice = c.GetEdifice(base.Map);
				return edifice == null || !edifice.def.holdsRoof;
			}), base.Map);
		}

		private void Impact()
		{
			for (int i = 0; i < 6; i++)
			{
				FleckMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map, 1.2f);
			}
			FleckMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("ChunkRadioactiveSlag")), base.Position, base.Map, base.Rotation);
			RoofDef roof = base.Position.GetRoof(base.Map);
			if (roof != null)
			{
				if (!roof.soundPunchThrough.NullOrUndefined())
				{
					roof.soundPunchThrough.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				if (roof.filthLeaving != null)
				{
					for (int j = 0; j < 3; j++)
					{
						FilthMaker.TryMakeFilth(base.Position, base.Map, roof.filthLeaving);
					}
				}
			}
			Destroy();
		}
	}
}
