using System.ComponentModel;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Server-side settings for Elemental Hearts. These affect all players in the world.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsServerConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsServerConfig Instance => ModContent.GetInstance<ElementalHeartsServerConfig>();

	[Header("Boss")]
	[SeparatePage]
	public BossSettings BossDrops = new BossSettings();

	[Header("Capacity")]
	[SeparatePage]
	public CapacitySettings CapacityLimits = new CapacitySettings();

	[Header("HP")]
	[SeparatePage]
	public HPSettings HPScale = new HPSettings();



	[Header("Recipes")]
	[SeparatePage]
	public RecipeSettings Recipes = new RecipeSettings();

	[Header("World")]
	[SeparatePage]
	public WorldSettings WorldGen = new WorldSettings();

	[Header("LifeShards")]
	[SeparatePage]
	public LifeShardSettings LifeShards = new LifeShardSettings();

	[Header("VitalTiles")]
	[SeparatePage]
	public VitalTileSettings VitalTiles = new VitalTileSettings();

	public override void OnChanged()
	{
		if (Main.netMode != NetmodeID.Server && !Main.gameMenu)
		{
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().RecomputeBonus();
		}

		LifeShardSystem.SetLifeCrystalExtractable(LifeShards.SystemEnabled);
		VitalTilesSystem.SetLifeFruitExtractable(VitalTiles.SystemEnabled);
	}
}

public class BossSettings
{
	[DefaultValue(true)]
	public bool BossHeartsGuaranteedOnFirstKill = true;

	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 120, 110, 255)]
	public int BossHeartDropChance = 10;
}

public class CapacitySettings
{
	[DefaultValue(true)]
	public bool EnableProgressionGates = true;

	[DefaultValue(25)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int PreBossCapacity = 25;

	[DefaultValue(50)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 225, 170, 255)]
	public int PostSkeletronCapacity = 50;

	[DefaultValue(75)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int PostWoFCapacity = 75;

	[DefaultValue(90)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(140, 200, 255, 255)]
	public int Post1MechCapacity = 90;

	[DefaultValue(105)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(125, 185, 255, 255)]
	public int Post2MechsCapacity = 105;

	[DefaultValue(120)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Post3MechsCapacity = 120;

	[DefaultValue(150)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int PostPlanteraCapacity = 150;

	[DefaultValue(175)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 150, 255, 255)]
	public int PostGolemCapacity = 175;

	[DefaultValue(200)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int PostCultistCapacity = 200;

	[DefaultValue(200)] [Range(10, 1000)] [Increment(5)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int PostMoonLordCapacity = 200;

	[DefaultValue(true)]
	public bool UnlimitedPostMoonLord = true;
}

public class HPSettings
{
	[DefaultValue(false)]
	public bool ChallengeMode = false;

	[Header("CraftableTiers")]
	[DefaultValue(2)]  [Range(1, 20)]  [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int Common = 2;

	[DefaultValue(4)]  [Range(2, 40)]  [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int Uncommon = 4;

	[DefaultValue(6)]  [Range(3, 60)]  [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Rare = 6;

	[DefaultValue(8)]  [Range(4, 80)]  [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int Epic = 8;

	[DefaultValue(10)] [Range(5, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int Legendary = 10;

	[Header("BossTiers")]
	[DefaultValue(5)] [Range(1, 100)] [Increment(1)] [Slider] [SliderColor(255, 120, 110, 255)]
	public int Exotic = 5;

	[DefaultValue(50)] [Range(25, 500)] [Increment(1)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int Mythic = 50;
}



public class RecipeSettings
{
	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 180, 120, 255)]
	[ReloadRequired]
	public int RecipeDifficulty = 10;
}

public class WorldSettings
{
	[Header("Progression")]
	[DefaultValue(true)]
	public bool SharedProgression { get; set; } = true;

	[Header("Cheats")]
	[DefaultValue(false)]
	public bool AdminMode { get; set; } = false;

	[Header("ResetActions")]
	[DefaultValue(false)]
	public bool ClearHeartRegistry
	{
		get => false;
		set
		{
			if (value)
				HeartConsumptionWorld.ClearAllHearts();
		}
	}

	[DefaultValue(false)]
	public bool ClearElementalTier
	{
		get => false;
		set
		{
			if (value)
				AnimateProgressionSystem.ClearTier();
		}
	}

	[Header("Worldgen")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool GenerateLifeBiomes = true;

	[DefaultValue(2)] [Range(0, 10)] [Increment(1)] [Slider]
	public int SurfaceBiomeCountMultiplier = 2;

	[DefaultValue(2)] [Range(0, 10)] [Increment(1)] [Slider]
	public int JungleBiomeCountMultiplier = 2;

	[Header("NightEvents")]
	[DefaultValue(7)] [Range(0, 100)] [Increment(1)] [Slider]
	public int HeartShootingStarChance = 7;

	[DefaultValue(3)] [Range(1, 10)] [Increment(1)] [Slider]
	public int HeartShootingStarMaxPerNight = 3;

	[DefaultValue(0.5f)] [Range(0.1f, 1f)] [Increment(0.1f)] [Slider]
	public float HeartShootingStarFalloffMultiplier = 0.5f;

	[DefaultValue(100)] [Range(1, 1000)] [Increment(5)] [Slider]
	public int HeartShootingStarFrequency = 100;
}

public class LifeShardSettings
{
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled = true;

	[Header("DropChances")]
	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 205, 218, 255)]
	public float CommonDropChance = 1f;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(150, 230, 150, 255)]
	public float UncommonDropChance = 1f;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(110, 170, 255, 255)]
	public float RareDropChance = 1f;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(200, 130, 255, 255)]
	public float EpicDropChance = 1f;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 200, 90, 255)]
	public float LegendaryDropChance = 1f;

	[Header("DamageThresholds")]
	[DefaultValue(50)] [Range(1, 10_000_000)] [Increment(10)]
	public int CommonDamageThreshold = 50;

	[DefaultValue(250)] [Range(1, 10_000_000)] [Increment(10)]
	public int UncommonDamageThreshold = 250;

	[DefaultValue(2000)] [Range(1, 10_000_000)] [Increment(10)]
	public int RareDamageThreshold = 2000;

	[DefaultValue(7000)] [Range(1, 10_000_000)] [Increment(10)]
	public int EpicDamageThreshold = 7000;

	[DefaultValue(30000)] [Range(1, 10_000_000)] [Increment(10)]
	public int LegendaryDamageThreshold = 30000;

	[Header("Extractinator")]
	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int ExtractinatorCommonMin = 1;

	[DefaultValue(3)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int ExtractinatorCommonMax = 3;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(150, 230, 150, 255)]
	public float ExtractinatorUncommonChance = 10f;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int ExtractinatorUncommonMin = 1;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int ExtractinatorUncommonMax = 1;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(110, 170, 255, 255)]
	public float ExtractinatorRareChance = 10f;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int ExtractinatorRareMin = 1;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int ExtractinatorRareMax = 1;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(200, 130, 255, 255)]
	public float ExtractinatorEpicChance = 1f;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int ExtractinatorEpicMin = 1;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int ExtractinatorEpicMax = 1;

	[DefaultValue(0.1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 200, 90, 255)]
	public float ExtractinatorLegendaryChance = 0.1f;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int ExtractinatorLegendaryMin = 1;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int ExtractinatorLegendaryMax = 1;

	[Header("Consumable")]
	[DefaultValue(false)]
	public bool ShardsAreConsumable = false;

	[DefaultValue(30)] [Range(1, 600)] [Increment(1)]
	public int ShardSicknessSeconds = 30;

	[DefaultValue(10)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 205, 218, 255)]
	public int CommonHealAmount = 10;

	[DefaultValue(25)] [Range(1, 10_000)] [Increment(1)] [SliderColor(150, 230, 150, 255)]
	public int UncommonHealAmount = 25;

	[DefaultValue(50)] [Range(1, 10_000)] [Increment(1)] [SliderColor(110, 170, 255, 255)]
	public int RareHealAmount = 50;

	[DefaultValue(75)] [Range(1, 10_000)] [Increment(1)] [SliderColor(200, 130, 255, 255)]
	public int EpicHealAmount = 75;

	[DefaultValue(100)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 200, 90, 255)]
	public int LegendaryHealAmount = 100;

	[Header("PassiveAbilityYields")]
	[DefaultValue(1)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 205, 218, 255)]
	public int CommonPassiveYield = 1;

	[DefaultValue(2)] [Range(1, 10_000)] [Increment(1)] [SliderColor(150, 230, 150, 255)]
	public int UncommonPassiveYield = 2;

	[DefaultValue(3)] [Range(1, 10_000)] [Increment(1)] [SliderColor(110, 170, 255, 255)]
	public int RarePassiveYield = 3;

	[DefaultValue(4)] [Range(1, 10_000)] [Increment(1)] [SliderColor(200, 130, 255, 255)]
	public int EpicPassiveYield = 4;

	[DefaultValue(5)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 200, 90, 255)]
	public int LegendaryPassiveYield = 5;

	[DefaultValue(2)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 120, 110, 255)]
	public int ExoticPassiveYield = 2;

	[DefaultValue(10)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 235, 150, 255)]
	public int MythicPassiveYield = 10;

	[Header("ActiveAbilityCosts")]
	[DefaultValue(1)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 205, 218, 255)]
	public int CommonAbilityCost = 1;

	[DefaultValue(3)] [Range(1, 10_000)] [Increment(1)] [SliderColor(150, 230, 150, 255)]
	public int UncommonAbilityCost = 3;

	[DefaultValue(10)] [Range(1, 10_000)] [Increment(1)] [SliderColor(110, 170, 255, 255)]
	public int RareAbilityCost = 10;

	[DefaultValue(15)] [Range(1, 10_000)] [Increment(1)] [SliderColor(200, 130, 255, 255)]
	public int EpicAbilityCost = 15;

	[DefaultValue(20)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 200, 90, 255)]
	public int LegendaryAbilityCost = 20;

	[DefaultValue(5)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 120, 110, 255)]
	public int ExoticAbilityCost = 5;

	[DefaultValue(30)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 235, 150, 255)]
	public int MythicAbilityCost = 30;
}

public class VitalTileSettings
{
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled = true;

	[Header("Spread")]
	[DefaultValue(0.001f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalSoilSpreadChance = 0.001f;

	[DefaultValue(0.001f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalQuartzSpreadChance = 0.001f;

	[Header("Buffs")]
	[DefaultValue(10)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalSoilRegenPercent = 10;

	[DefaultValue(5)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalQuartzMaxHpPercent = 5;

	[DefaultValue(40)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzVerticalRange = 40;

	[DefaultValue(20)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzHorizontalRange = 20;

	[Header("Seeds")]
	[DefaultValue(25f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeCrystalSeedChance = 25f;

	[DefaultValue(33f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeFruitSeedChance = 33f;
}
