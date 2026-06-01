using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Server-side tuning for the idle Life Shard generator: the master toggle and how
/// large the unclaimed-shard reservoir grows, both as a flat base and per World Tier.
/// Labels and tooltips live in the localization file like every other config.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsIdleConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsIdleConfig Instance => ModContent.GetInstance<ElementalHeartsIdleConfig>();

	[Header("IdleGameSettings")]
	[DefaultValue(true)]
	public bool EnableIdleGame { get; set; }

	[DefaultValue(50)] [Range(10, 1000)] [Increment(10)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int BaseCapacity { get; set; }

	[DefaultValue(50)] [Range(10, 500)] [Increment(10)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int CapacityPerTier { get; set; }
}
