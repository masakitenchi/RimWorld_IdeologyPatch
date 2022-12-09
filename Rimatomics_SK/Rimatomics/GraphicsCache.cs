using UnityEngine;
using Verse;

namespace Rimatomics
{
	[StaticConstructorOnStartup]
	public static class GraphicsCache
	{
		public static Texture2D disco = ContentFinder<Texture2D>.Get("Rimatomics/UI/discord", reportFailure: false);

		public static Texture2D Support = ContentFinder<Texture2D>.Get("Rimatomics/UI/Support", reportFailure: false);

		public static readonly Texture2D DotColonist = ContentFinder<Texture2D>.Get("Rimatomics/FX/DotColonist");

		public static readonly Texture2D DotEnemy = ContentFinder<Texture2D>.Get("Rimatomics/FX/DotEnemy");

		public static readonly Texture2D DotFriendly = ContentFinder<Texture2D>.Get("Rimatomics/FX/DotFriendly");

		public static readonly Texture2D DotProjectile = ContentFinder<Texture2D>.Get("Rimatomics/FX/DotProjectile");

		public static readonly Texture2D rimatomicsFlag = ContentFinder<Texture2D>.Get("Rimatomics/UI/rimatomics");

		public static readonly Texture2D clear = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public static readonly Texture2D grey = SolidColorMaterials.NewSolidColorTexture(Color.grey);

		public static readonly Texture2D blue = SolidColorMaterials.NewSolidColorTexture(new ColorInt(38, 169, 224).ToColor);

		public static readonly Texture2D yellow = SolidColorMaterials.NewSolidColorTexture(new ColorInt(249, 236, 49).ToColor);

		public static readonly Texture2D red = SolidColorMaterials.NewSolidColorTexture(new ColorInt(190, 30, 45).ToColor);

		public static readonly Texture2D green = SolidColorMaterials.NewSolidColorTexture(new ColorInt(41, 180, 115).ToColor);

		public static readonly Texture2D black = SolidColorMaterials.NewSolidColorTexture(new ColorInt(15, 11, 12).ToColor);

		public static readonly Texture2D tempGauge = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/tempGauge");

		public static readonly Texture2D Gauge = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/Guage");

		public static readonly Texture2D Install = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/Install");

		public static readonly Texture2D Uninstall = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/Uninstall");

		public static readonly Texture2D activatedTex = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/activated");

		public static readonly Texture2D moxMtext = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/MoxM");

		public static readonly Texture2D ClearDes = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/ClearDes");

		public static readonly Texture2D crackedSlot = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/crackedSlot");

		public static readonly Texture2D Nuke = ContentFinder<Texture2D>.Get("Rimatomics/UI/Nuke");

		public static readonly Texture2D UIfuelKey = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/fuelRod");

		public static readonly Texture2D UIfuelKeyMOX = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/fuelRodMOX");

		public static readonly Texture2D UImouseKey = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/mouseKey");

		public static readonly Texture2D UIcooling = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/cooling");

		public static readonly Texture2D UIfuel = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/fuel");

		public static readonly Texture2D UIpower = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/power");

		public static readonly Texture2D UIrad = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/rad");

		public static readonly Texture2D UItemp = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/temp");

		public static readonly Texture2D UIturbine = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/turbine");

		public static readonly Texture2D UIatom = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/atom");

		public static readonly Texture2D UIbarBG = SolidColorMaterials.NewSolidColorTexture(new ColorInt(21, 25, 29).ToColor);

		public static readonly Texture2D scram = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/scram");

		public static readonly Texture2D startReactor = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/startReactor");

		public static readonly Texture2D coldRods = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/coldRods");

		public static readonly Texture2D warmRods = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/warmRods");

		public static readonly Texture2D controlRods = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/controlRods");

		public static readonly Texture2D ReactorDead = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/ReactorDead");

		public static readonly Texture2D ReactorCold = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/ReactorCold");

		public static readonly Texture2D ReactorHot = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/ReactorHot");

		public static readonly Texture2D ReactorShutdown = ContentFinder<Texture2D>.Get("Rimatomics/UI/ReactorUI/ReactorShutdown");

		public static readonly Texture2D setID = ContentFinder<Texture2D>.Get("Rimatomics/UI/setid");

		public static readonly Texture2D fuelRodTex = ContentFinder<Texture2D>.Get("Rimatomics/Things/Resources/fuelRod");

		public static readonly Texture2D controlPanelBGTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(34, 31, 31).ToColor);

		public static readonly Material controlPanelBG = MaterialPool.MatFrom(controlPanelBGTex, ShaderDatabase.DefaultShader, Color.white);

		public static readonly Material GaugeMat = MaterialPool.MatFrom(Gauge, ShaderDatabase.MoteGlow, Color.white);

		public static readonly Graphic smallFlash = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/flash", ShaderDatabase.MoteGlow, new Vector2(5f, 5f), Color.white);

		public static readonly Graphic bigFlash = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/flash", ShaderDatabase.MoteGlow, new Vector2(20f, 20f), Color.white);

		public static readonly Material bigFlashMat = bigFlash.MatSingle;

		public static readonly Material LEDblack = MaterialPool.MatFrom("Rimatomics/FX/LEDwhite", ShaderDatabase.MoteGlow, new ColorInt(20, 20, 20).ToColor);

		public static readonly Material LEDwhite = MaterialPool.MatFrom("Rimatomics/FX/LEDwhite", ShaderDatabase.MoteGlow);

		public static readonly Material LEDred = MaterialPool.MatFrom("Rimatomics/FX/LEDred", ShaderDatabase.MoteGlow);

		public static readonly Material LEDorange = MaterialPool.MatFrom("Rimatomics/FX/LEDorange", ShaderDatabase.MoteGlow);

		public static readonly Material LEDgreen = MaterialPool.MatFrom("Rimatomics/FX/LEDgreen", ShaderDatabase.MoteGlow);

		public static readonly Material LEDblue = MaterialPool.MatFrom("Rimatomics/FX/LEDblue", ShaderDatabase.MoteGlow);

		public static readonly Material LEDoff = MaterialPool.MatFrom("Rimatomics/FX/LEDoff", ShaderDatabase.DefaultShader);

		public static readonly Material BigRedLed = MaterialPool.MatFrom("Rimatomics/FX/BigRedLed", ShaderDatabase.MoteGlow);

		public static readonly Material BigAmberLed = MaterialPool.MatFrom("Rimatomics/FX/BigLed", ShaderDatabase.MoteGlow, new ColorInt(255, 153, 0).ToColor);

		public static readonly Material BigBlueLed = MaterialPool.MatFrom("Rimatomics/FX/BigLed", ShaderDatabase.MoteGlow, new ColorInt(0, 120, 255).ToColor);

		public static readonly Material miniRedLed = MaterialPool.MatFrom("Rimatomics/FX/miniRedLed", ShaderDatabase.MoteGlow);

		public static readonly Material miniGreenLed = MaterialPool.MatFrom("Rimatomics/FX/miniRedLed", ShaderDatabase.MoteGlow, new ColorInt(0, 255, 0).ToColor);

		public static readonly Material damage = MaterialPool.MatFrom("Rimatomics/FX/damage", ShaderDatabase.MoteGlow);

		public static readonly Material overheat = MaterialPool.MatFrom("Rimatomics/FX/overheat", ShaderDatabase.MoteGlow);

		public static readonly Material radiation = MaterialPool.MatFrom("Rimatomics/FX/radiation", ShaderDatabase.MoteGlow);

		public static readonly Texture2D RadIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/Fallout");

		public static readonly Texture2D OverheatIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/overheat");

		public static readonly Material storageArm = MaterialPool.MatFrom("Rimatomics/Things/RimatomicsBuildings/storageArm", ShaderDatabase.DefaultShader);

		public static readonly Material storageGrip = MaterialPool.MatFrom("Rimatomics/Things/RimatomicsBuildings/storageGrip", ShaderDatabase.DefaultShader);

		public static readonly Material storagePoolRods = MaterialPool.MatFrom("Rimatomics/Things/Resources/storagePoolRods", ShaderDatabase.DefaultShader);

		public static readonly Graphic RadDetector = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/RadDetector", ShaderDatabase.MoteGlow, new Vector2(1f, 1f), Color.white);

		public static readonly Graphic Disconnected = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/Disconnected", ShaderDatabase.MoteGlow, new Vector2(7f, 4f), Color.white);

		public static readonly Graphic coremeltscreen1 = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/coremeltscreen1", ShaderDatabase.MoteGlow, new Vector2(7f, 4f), Color.white);

		public static readonly Graphic coremeltscreen2 = GraphicDatabase.Get<Graphic_Single>("Rimatomics/FX/coremeltscreen2", ShaderDatabase.MoteGlow, new Vector2(7f, 4f), Color.white);

		public static readonly Texture2D TrackingScreen = ContentFinder<Texture2D>.Get("Rimatomics/UI/TrackingScreen");

		public static readonly Material ICBM_MasterMAT = MaterialPool.MatFrom("Rimatomics/Things/Projectile/icbmSilo", ShaderDatabase.DefaultShader, Color.white);

		public static readonly Material Engines_MasterMAT = MaterialPool.MatFrom("Rimatomics/FX/engines1", ShaderDatabase.MoteGlow, Color.white);

		public static readonly Graphic God = GraphicDatabase.Get<Graphic_Single>("Rimatomics/God", ShaderDatabase.DefaultShader, new Vector2(1f, 1f), Color.white);

		public static readonly Texture2D obeliskBeam = ContentFinder<Texture2D>.Get("Rimatomics/FX/obeliskBeam");

		public static readonly Texture2D HELBeam = ContentFinder<Texture2D>.Get("Rimatomics/FX/HELbeam");

		public static readonly Texture2D[] bolts = new Texture2D[3]
		{
			ContentFinder<Texture2D>.Get("Rimatomics/FX/bolt1"),
			ContentFinder<Texture2D>.Get("Rimatomics/FX/bolt2"),
			ContentFinder<Texture2D>.Get("Rimatomics/FX/bolt3")
		};

		public static readonly Material underGlow = MaterialPool.MatFrom("Rimatomics/FX/Untitled_BotGlow", ShaderDatabase.MoteGlow, Color.white);

		public static readonly Graphic obeliskCharge = GraphicDatabase.Get<Graphic_Multi>("Rimatomics/FX/obeliskCharge", ShaderDatabase.MoteGlow, new Vector2(5f, 5f), Color.white);

		public static readonly Material[] VetPatches = new Material[5]
		{
			MaterialPool.MatFrom("Rimatomics/UI/vet1", ShaderDatabase.MetaOverlay, Color.white),
			MaterialPool.MatFrom("Rimatomics/UI/vet2", ShaderDatabase.MetaOverlay, Color.white),
			MaterialPool.MatFrom("Rimatomics/UI/vet3", ShaderDatabase.MetaOverlay, Color.white),
			MaterialPool.MatFrom("Rimatomics/UI/vet4", ShaderDatabase.MetaOverlay, Color.white),
			MaterialPool.MatFrom("Rimatomics/UI/vet5", ShaderDatabase.MetaOverlay, Color.white)
		};

		public static Graphic_LinkedPipe[] pipeDick = new Graphic_LinkedPipe[5]
		{
			new Graphic_LinkedPipe(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/HighVoltage_Atlas", ShaderDatabase.Transparent), PipeType.HighVoltage),
			new Graphic_LinkedPipe(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/coolingPipe_Atlas", ShaderDatabase.Transparent), PipeType.Cooling),
			new Graphic_LinkedPipe(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/steamPipe_Atlas", ShaderDatabase.Transparent), PipeType.Steam),
			new Graphic_LinkedPipe(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/coldWaterPipe_Atlas", ShaderDatabase.Transparent), PipeType.ColdWater),
			new Graphic_LinkedPipe(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/Loom_Atlas", ShaderDatabase.Transparent), PipeType.Loom)
		};

		public static Graphic_LinkedPipeOverlay[] pipeOverlayDick = new Graphic_LinkedPipeOverlay[5]
		{
			new Graphic_LinkedPipeOverlay(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/PipeOverlay_Atlas", ShaderDatabase.MetaOverlay, Vector2.one, new Color32(217, 198, 68, 190)), PipeType.HighVoltage),
			new Graphic_LinkedPipeOverlay(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/PipeOverlay_Atlas", ShaderDatabase.MetaOverlay, Vector2.one, new Color32(140, 240, 232, 190)), PipeType.Cooling),
			new Graphic_LinkedPipeOverlay(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/PipeOverlay_Atlas", ShaderDatabase.MetaOverlay, Vector2.one, new Color32(205, 71, 62, 190)), PipeType.Steam),
			new Graphic_LinkedPipeOverlay(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/PipeOverlay_Atlas", ShaderDatabase.MetaOverlay, Vector2.one, new Color32(68, 78, 217, 190)), PipeType.ColdWater),
			new Graphic_LinkedPipeOverlay(GraphicDatabase.Get<Graphic_Single>("Rimatomics/Things/RimatomicsBuildings/PipeOverlay_Atlas", ShaderDatabase.MetaOverlay, Vector2.one, new Color32(68, 217, 93, 190)), PipeType.Loom)
		};
	}
}
