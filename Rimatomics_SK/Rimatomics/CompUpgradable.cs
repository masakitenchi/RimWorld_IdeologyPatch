using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Rimatomics
{
	public class CompUpgradable : ThingComp
	{
		public Dictionary<ThingDef, UpgradeState> Upgrades = new Dictionary<ThingDef, UpgradeState>();

		private StringBuilder sb = new StringBuilder();

		public CompProperties_Upgradable Props => (CompProperties_Upgradable)props;

		public bool NeedsUpgrade => Upgrades.Any((KeyValuePair<ThingDef, UpgradeState> x) => x.Value == UpgradeState.Install);

		public IEnumerable<ThingDef> ThingsToInstall()
		{
			foreach (KeyValuePair<ThingDef, UpgradeState> upgrade in Upgrades)
			{
				if (upgrade.Value == UpgradeState.Install)
				{
					yield return upgrade.Key;
				}
			}
		}

		public bool HasUpgrade(ThingDef upgrade)
		{
			if (Upgrades.ContainsKey(upgrade) && Upgrades[upgrade] == UpgradeState.Installed)
			{
				return true;
			}
			return false;
		}

		public override void PostDeSpawn(Map map)
		{
			if (map.Rimatomics().Upgradables.Contains(parent))
			{
				map.Rimatomics().Upgradables.Remove(parent);
			}
			base.PostDeSpawn(map);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!parent.Map.Rimatomics().Upgradables.Contains(parent))
			{
				parent.Map.Rimatomics().Upgradables.Add(parent);
			}
			if (respawningAfterLoad)
			{
				return;
			}
			foreach (Upgrades upgrade in Props.upgrades)
			{
				if (upgrade.project.IsFinished && upgrade.project.BlueprintUpgrade)
				{
					Upgrades[upgrade.project.part] = UpgradeState.Installed;
					parent.BroadcastCompSignal("upgraded");
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref Upgrades, "Upgrades");
			if (Upgrades == null)
			{
				Upgrades = new Dictionary<ThingDef, UpgradeState>();
			}
		}

		public void InstallUpgrade(ThingDef added)
		{
			if (Upgrades == null)
			{
				Upgrades = new Dictionary<ThingDef, UpgradeState>();
			}
			Upgrades[added] = UpgradeState.Installed;
			MoteMaker.MakeStaticMote(parent.Position, parent.Map, ThingDef.Named("Mote_Upgraded"), 2f);
			SoundDef.Named("Upgrade").PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			parent.BroadcastCompSignal("upgraded");
		}

		public void RemoveUpgrade(ThingDef removed)
		{
			if (Upgrades.ContainsKey(removed))
			{
				Upgrades[removed] = UpgradeState.None;
				parent.BroadcastCompSignal("upgraded");
			}
		}

		public string MakeStats(Upgrades upgrade)
		{
			sb.Clear();
			foreach (string stat in upgrade.project.stats)
			{
				sb.AppendLine(stat);
			}
			foreach (string stat2 in upgrade.stats)
			{
				sb.AppendLine(stat2);
			}
			return sb.ToString().TrimEndNewlines();
		}

		[SyncMethod(SyncContext.None)]
		public void Toggers(ThingDef t, UpgradeState s)
		{
			Upgrades[t] = s;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			foreach (Upgrades upgrade in Props.upgrades)
			{
				if (!upgrade.project.IsFinished && !DebugSettings.godMode)
				{
					continue;
				}
				if (Upgrades == null)
				{
					Upgrades = new Dictionary<ThingDef, UpgradeState>();
				}
				if (!Upgrades.TryGetValue(upgrade.project.part, out var partState))
				{
					Upgrades.Add(upgrade.project.part, UpgradeState.None);
					continue;
				}
				if (partState == UpgradeState.Installed)
				{
					if (DebugSettings.godMode)
					{
						yield return new Gizmo_Upgrades
						{
							defaultLabel = upgrade.project.part.label,
							defaultDesc = MakeStats(upgrade),
							icon = upgrade.project.PreviewImage,
							upgradeState = partState,
							action = delegate
							{
								Toggers(upgrade.project.part, UpgradeState.None);
								SoundDefOf.FlickSwitch.PlayOneShot(parent);
								parent.BroadcastCompSignal("upgraded");
							}
						};
					}
					else
					{
						yield return new Gizmo_Upgrades
						{
							defaultLabel = upgrade.project.part.label,
							defaultDesc = MakeStats(upgrade),
							icon = upgrade.project.PreviewImage,
							upgradeState = partState,
							disabled = true
						};
					}
				}
				if (partState == UpgradeState.Install)
				{
					yield return new Gizmo_Upgrades
					{
						defaultLabel = upgrade.project.part.label,
						defaultDesc = MakeStats(upgrade),
						icon = upgrade.project.PreviewImage,
						upgradeState = partState,
						action = delegate
						{
							Toggers(upgrade.project.part, UpgradeState.None);
							SoundDefOf.FlickSwitch.PlayOneShot(parent);
						}
					};
				}
				if (partState != 0)
				{
					continue;
				}
				yield return new Gizmo_Upgrades
				{
					defaultLabel = upgrade.project.part.label,
					defaultDesc = MakeStats(upgrade),
					icon = upgrade.project.PreviewImage,
					upgradeState = partState,
					action = delegate
					{
						if (DebugSettings.godMode)
						{
							Toggers(upgrade.project.part, UpgradeState.Installed);
							parent.BroadcastCompSignal("upgraded");
						}
						else
						{
							Toggers(upgrade.project.part, UpgradeState.Install);
						}
						SoundDefOf.FlickSwitch.PlayOneShot(parent);
					}
				};
			}
		}
	}
}
