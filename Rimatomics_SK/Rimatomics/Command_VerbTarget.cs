using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	internal class Command_VerbTarget : Command
	{
		public Verb verb;

		public Building_EnergyWeapon wep;

		public override Color IconDrawColor => verb.EquipmentSource?.DrawColor ?? base.IconDrawColor;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Find.Targeter.BeginTargeting(verb);
		}
	}
}
