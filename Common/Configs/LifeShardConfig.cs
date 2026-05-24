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

	[Header("General")]
	[DefaultValue(true)]
	public bool SystemEnabled;

	// ── Drop chances ──────────────────────────────────────────────────────────
	[Header("DropChances")]
	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float CommonDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float UncommonDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float RareDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float EpicDropChance;

	[DefaultValue(2f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LegendaryDropChance;

	// ── Damage thresholds ─────────────────────────────────────────────────────
	[Header("DamageThresholds")]
	[DefaultValue(50)] [Range(1, 10000000)] [Increment(10)]
	public int CommonDamageThreshold;

	[DefaultValue(250)] [Range(1, 10000000)] [Increment(10)]
	public int UncommonDamageThreshold;

	[DefaultValue(2000)] [Range(1, 10000000)] [Increment(10)]
	public int RareDamageThreshold;

	[DefaultValue(7000)] [Range(1, 10000000)] [Increment(10)]
	public int EpicDamageThreshold;

	[DefaultValue(30000)] [Range(1, 10000000)] [Increment(10)]
	public int LegendaryDamageThreshold;

	// ── Extractinator ─────────────────────────────────────────────────────────
	// A Life Crystal always yields Common shards; the higher tiers are bonus rolls.
	[Header("Extractinator")]
	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorCommonMin;

	[DefaultValue(3)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorCommonMax;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float ExtractinatorUncommonChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorUncommonMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorUncommonMax;

	[DefaultValue(10f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float ExtractinatorRareChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorRareMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorRareMax;

	[DefaultValue(1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float ExtractinatorEpicChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorEpicMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorEpicMax;

	[DefaultValue(0.1f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float ExtractinatorLegendaryChance;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorLegendaryMin;

	[DefaultValue(1)] [Range(0, 100)] [Increment(1)] [Slider]
	public int ExtractinatorLegendaryMax;

	/// <summary>Keep Life Crystal Extractinator acceptance in sync with the master toggle.</summary>
	public override void OnChanged() => LifeShardSystem.SetLifeCrystalExtractable(SystemEnabled);
}
