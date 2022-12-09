using Verse;

namespace Rimatomics
{
	public class EnergyWep
	{
		public float range = 10f;

		public float minRange;

		private float PulseSize;

		public int Damage;

		public int WorldRange = 400;

		public float turretSpeed = 20f;

		public float TurretAimSnapAngle = 1.5f;

		public float cooldownForShot = 5f;

		public bool NeedsRadar;

		public bool HasTurret;

		public bool NeedsManning;

		public bool canEverForceTarget;

		public int magazineCap;

		public bool TurretCamo;

		public bool CanCamo;

		public float PrototypeFailureChance = 0.01f;

		public SoundDef ChargeUpSound;

		public SoundDef FailSound;

		public float AOE = 10f;

		public float PulseSizeScaled => PulseSize * RimatomicsMod.Settings.PulseSizeScaling;
	}
}
