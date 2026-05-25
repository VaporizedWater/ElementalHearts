using System.ComponentModel;
using ElementalHearts.Common.LifeShards;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Server-side tuning for the Life Shard system: the master toggle, per-tier
/// damage-drop chances and thresholds, and Extractinator yields.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class LifeShardConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static LifeShardConfig Instance => ModContent.GetInstance<LifeShardConfig>();

	// ── General ───────────────────────────────────────────────────────────────
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled;

	// ── Drop chances ──────────────────────────────────────────────────────────
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

	// ── Damage thresholds (number inputs — too wide a range for sliders) ──────
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

	// ── Extractinator ─────────────────────────────────────────────────────────
	// A Life Crystal always yields Common shards; the higher tiers are bonus rolls.
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

	// ── Consumable ────────────────────────────────────────────────────────────
	// Shards routed into the per-tier panel slots are safe; only those that overflow
	// into the regular inventory (slot at max stack) become quick-heal candidates.
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

	/// <summary>Keep Life Crystal Extractinator acceptance in sync with the master toggle.</summary>
	public override void OnChanged() => LifeShardSystem.SetLifeCrystalExtractable(SystemEnabled);
}
