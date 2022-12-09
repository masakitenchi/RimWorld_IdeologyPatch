using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public class Building_ShieldArray : Building, ICamoSelect
	{
		public CompRimatomicsShield CompShield;

		public Graphic baseGraphic;

		public string camoMode = "base";

		public CompPowerTrader powerComp;

		public Graphic turretMatCache;

		private static string toppath = "Rimatomics/Things/RimatomicsBuildings/Shield_test";

		public string GetCamoMode
		{
			get
			{
				if (!(camoMode == "base"))
				{
					return "-" + camoMode;
				}
				return "";
			}
		}

		public Graphic turretMat
		{
			get
			{
				if (turretMatCache == null)
				{
					_ = Graphic;
				}
				return turretMatCache;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				Vector2 drawSize = new Vector2(def.building.turretTopDrawSize, def.building.turretTopDrawSize);
				if (turretMatCache == null)
				{
					turretMatCache = GraphicDatabase.Get<Graphic_Single>(toppath, ShaderDatabase.DefaultShader, drawSize, DrawColor);
				}
				if (camoMode == "base")
				{
					return base.Graphic;
				}
				if (baseGraphic != null)
				{
					return baseGraphic;
				}
				return baseGraphic = GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath + GetCamoMode, ShaderDatabase.DefaultShader, def.graphicData.drawSize, Color.white);
			}
		}

		public void SpawnedCamo()
		{
		}

		public void SetMode(string mode)
		{
			camoMode = mode;
			baseGraphic = null;
			turretMatCache = null;
			DirtyMapMesh(base.Map);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			CompShield = GetComp<CompRimatomicsShield>();
			DubUtils.GetResearch().NotifyResearch();
			SpawnedCamo();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref camoMode, "camoMode", "base");
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_SetCamoMode(this)
			{
				defaultLabel = "SetCamoMode".Translate(),
				defaultDesc = "SetCamoMode".Translate(),
				icon = ContentFinder<Texture2D>.Get("Rimatomics/UI/CamoMode")
			};
		}

		public override void Draw()
		{
			base.Draw();
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 drawPos = DrawPos;
			drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
			drawPos.z += 0.5f;
			matrix.SetTRS(drawPos, Quaternion.identity, new Vector3(3f, 1f, 3f));
			Graphics.DrawMesh(MeshPool.plane10, matrix, turretMat.MatSingle, 0);
		}
	}
}
