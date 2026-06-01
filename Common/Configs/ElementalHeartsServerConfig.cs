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

	[Header("Potions")]
	[SeparatePage]
	public PotionSettings Potions = new PotionSettings();

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
	public bool BossHeartsGuaranteedOnFirstKill;

	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 120, 110, 255)]
	public int BossHeartDropChance;
}

public class CapacitySettings
{
	[DefaultValue(true)]
	public bool EnableProgressionGates;

	[DefaultValue(25)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int PreBossCapacity;

	[DefaultValue(50)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 225, 170, 255)]
	public int PostSkeletronCapacity;

	[DefaultValue(100)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int PostWoFCapacity;

	[DefaultValue(115)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(140, 200, 255, 255)]
	public int Post1MechCapacity;

	[DefaultValue(130)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(125, 185, 255, 255)]
	public int Post2MechsCapacity;

	[DefaultValue(150)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Post3MechsCapacity;

	[DefaultValue(200)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int PostPlanteraCapacity;

	[DefaultValue(250)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 150, 255, 255)]
	public int PostGolemCapacity;

	[DefaultValue(275)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int PostCultistCapacity;

	[DefaultValue(300)] [Range(10, 1000)] [Increment(5)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int PostMoonLordCapacity;

	[DefaultValue(false)]
	public bool UnlimitedPostMoonLord;
}

public class HPSettings
{
	[DefaultValue(false)]
	public bool ChallengeMode;

	[Header("CraftableTiers")]
	[DefaultValue(2)]  [Range(1, 20)]  [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int Common;

	[DefaultValue(4)]  [Range(2, 40)]  [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int Uncommon;

	[DefaultValue(6)]  [Range(3, 60)]  [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Rare;

	[DefaultValue(8)]  [Range(4, 80)]  [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int Epic;

	[DefaultValue(10)] [Range(5, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int Legendary;

	[Header("BossTiers")]
	[DefaultValue(5)] [Range(1, 100)] [Increment(1)] [Slider] [SliderColor(255, 120, 110, 255)]
	public int Exotic;

	[DefaultValue(50)] [Range(25, 500)] [Increment(1)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int Mythic;
}

public class PotionSettings
{
	[DefaultValue(true)]
	public bool WorldwidePotionEffectsEnabled;
}

public class RecipeSettings
{
	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 180, 120, 255)]
	[ReloadRequired]
	public int RecipeDifficulty;
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
	public bool GenerateLifeBiomes;

	[DefaultValue(1)] [Range(0, 10)] [Increment(1)] [Slider]
	public int SurfaceBiomeCountMultiplier;

	[DefaultValue(1)] [Range(0, 10)] [Increment(1)] [Slider]
	public int JungleBiomeCountMultiplier;

	[Header("NightEvents")]
	[DefaultValue(7)] [Range(0, 100)] [Increment(1)] [Slider]
	public int HeartShootingStarChance;

	[DefaultValue(1)] [Range(1, 10)] [Increment(1)] [Slider]
	public int HeartShootingStarMaxPerNight;

	[DefaultValue(100)] [Range(1, 1000)] [Increment(5)] [Slider]
	public int HeartShootingStarFrequency;
}

public class LifeShardSettings
{
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled;

	[Header("DropChances")]
	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 205, 218, 255)]
	public float CommonDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(150, 230, 150, 255)]
	public float UncommonDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(110, 170, 255, 255)]
	public float RareDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(200, 130, 255, 255)]
	public float EpicDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 200, 90, 255)]
	public float LegendaryDropChance;

	[Header("DamageThresholds")]
	[DefaultValue(50)] [Range(1, 10_000_000)] [Increment(10)]
	public int CommonDamageThreshold;

	[DefaultValue(250)] [Range(1, 10_000_000)] [Increment(10)]
	public int UncommonDamageThreshold;

	[DefaultValue(2000)] [Range(1, 10_000_000)] [Increment(10)]
	public int RareDamageThreshold;

	[DefaultValue(7000)] [Range(1, 10_000_000)] [Increment(10)]
	public int EpicDamageThreshold;

	[DefaultValue(30000)] [Range(1, 10_000_000)] [Increment(10)]
	public int LegendaryDamageThreshold;

	[Header("Extractinator")]
	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int ExtractinatorCommonMin;

	[DefaultValue(3)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int ExtractinatorCommonMax;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(150, 230, 150, 255)]
	public float ExtractinatorUncommonChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int ExtractinatorUncommonMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int ExtractinatorUncommonMax;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(110, 170, 255, 255)]
	public float ExtractinatorRareChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int ExtractinatorRareMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int ExtractinatorRareMax;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(200, 130, 255, 255)]
	public float ExtractinatorEpicChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int ExtractinatorEpicMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int ExtractinatorEpicMax;

	[DefaultValue(0.1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider] [SliderColor(255, 200, 90, 255)]
	public float ExtractinatorLegendaryChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int ExtractinatorLegendaryMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int ExtractinatorLegendaryMax;

	[Header("Consumable")]
	[DefaultValue(true)]
	public bool ShardsAreConsumable;

	[DefaultValue(30)] [Range(1, 600)] [Increment(1)]
	public int ShardSicknessSeconds;

	[DefaultValue(10)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 205, 218, 255)]
	public int CommonHealAmount;

	[DefaultValue(25)] [Range(1, 10_000)] [Increment(1)] [SliderColor(150, 230, 150, 255)]
	public int UncommonHealAmount;

	[DefaultValue(50)] [Range(1, 10_000)] [Increment(1)] [SliderColor(110, 170, 255, 255)]
	public int RareHealAmount;

	[DefaultValue(75)] [Range(1, 10_000)] [Increment(1)] [SliderColor(200, 130, 255, 255)]
	public int EpicHealAmount;

	[DefaultValue(100)] [Range(1, 10_000)] [Increment(1)] [SliderColor(255, 200, 90, 255)]
	public int LegendaryHealAmount;
}

public class VitalTileSettings
{
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled;

	[Header("Spread")]
	[DefaultValue(0.005f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalSoilSpreadChance;

	[DefaultValue(0.005f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalQuartzSpreadChance;

	[Header("Buffs")]
	[DefaultValue(10)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalSoilRegenPercent;

	[DefaultValue(5)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalQuartzMaxHpPercent;

	[DefaultValue(40)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzVerticalRange;

	[DefaultValue(20)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzHorizontalRange;

	[Header("Seeds")]
	[DefaultValue(25f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeCrystalSeedChance;

	[DefaultValue(33f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeFruitSeedChance;
}
