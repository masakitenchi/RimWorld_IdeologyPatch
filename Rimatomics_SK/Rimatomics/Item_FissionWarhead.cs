using System.Collections.Generic;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class Item_FissionWarhead : Item_Warhead
	{
		public bool armed;

		public int countdown = 99999;

		public override string Label
		{
			get
			{
				if (armed)
				{
					return base.Label + " " + countdown.TicksToSeconds();
				}
				return base.Label;
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (armed && base.Spawned)
			{
				countdown--;
			}
			if (countdown <= 0)
			{
				(GenSpawn.Spawn(DubDef.ICBMStrike, base.Position, base.Map) as NuclearStrike).Yield = 55f;
				Destroy();
			}
		}

		[SyncMethod(SyncContext.None)]
		public void Arm(int t)
		{
			armed = true;
			countdown = GenTicks.SecondsToTicks(t);
			SoundDefOf.TechMedicineUsed.PlayOneShot(this);
		}

		[SyncMethod(SyncContext.None)]
		public void cancel()
		{
			armed = false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (armed)
			{
				yield return new Command_Action
				{
					icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/det_bomb"),
					defaultLabel = "Cancel",
					action = cancel
				};
			}
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/det_bomb"),
				defaultLabel = "Arm: 2m",
				action = delegate
				{
					Arm(120);
				}
			};
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/det_bomb"),
				defaultLabel = "Arm: 30s",
				action = delegate
				{
					Arm(30);
				}
			};
		}
	}
}
