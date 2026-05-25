using System.ComponentModel;
using ElementalHearts.Common.Systems;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Server-side tuning for the Vital Soil and Vital Quartz tiles: master toggle, spread
/// rates, the per-tile player buff magnitudes, and the Vital Quartz aura range.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class VitalTilesConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static VitalTilesConfig Instance => ModContent.GetInstance<VitalTilesConfig>();

	// ── General ───────────────────────────────────────────────────────────────
	[Header("General")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool SystemEnabled;

	// ── Spread ────────────────────────────────────────────────────────────────
	// Both tiles spread chlorophyte-style: each RandomUpdate rolls a (rare) chance to
	// convert one adjacent valid neighbour. Setting these to 0 disables spread without
	// affecting placement, breaking, or buffs.
	[Header("Spread")]
	[DefaultValue(0.005f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalSoilSpreadChance;

	[DefaultValue(0.005f)] [Range(0f, 1f)] [Increment(0.001f)] [Slider]
	public float VitalQuartzSpreadChance;

	// ── Buffs ─────────────────────────────────────────────────────────────────
	[Header("Buffs")]
	[DefaultValue(10)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalSoilRegenPercent;

	[DefaultValue(5)] [Range(0, 100)] [Increment(1)] [Slider]
	public int VitalQuartzMaxHpPercent;

	// Vertical range is large, horizontal is short — encouraging players to build
	// columns of Vital Quartz through their arena rather than a single wall.
	[DefaultValue(40)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzVerticalRange;

	[DefaultValue(20)] [Range(1, 200)] [Increment(1)] [Slider]
	public int VitalQuartzHorizontalRange;

	// ── Seeds ─────────────────────────────────────────────────────────────────
	// Chance an extractinated Life Crystal / Life Fruit yields one seed of its kind,
	// independent of the existing Common shard roll. Seeds are pure addition: a roll
	// that fails still produces the normal yield.
	[Header("Seeds")]
	[DefaultValue(25f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeCrystalSeedChance;

	[DefaultValue(25f)] [Range(0f, 100f)] [Increment(0.1f)] [Slider]
	public float LifeFruitSeedChance;

	/// <summary>Keep Life Fruit Extractinator acceptance in sync with the master toggle.</summary>
	public override void OnChanged() => VitalTilesSystem.SetLifeFruitExtractable(SystemEnabled);
}
