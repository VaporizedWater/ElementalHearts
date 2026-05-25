using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>Controls how boss-themed hearts drop from their boss.</summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsBossConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsBossConfig Instance => ModContent.GetInstance<ElementalHeartsBossConfig>();

	[Header("Drops")]
	[DefaultValue(true)]
	public bool BossHeartsGuaranteedOnFirstKill;

	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 120, 110, 255)]
	public int BossHeartDropChance;
}
