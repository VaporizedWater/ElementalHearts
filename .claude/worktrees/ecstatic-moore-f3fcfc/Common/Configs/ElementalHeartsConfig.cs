using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;

	public static ElementalHeartsConfig Instance => ModContent.GetInstance<ElementalHeartsConfig>();

	[Header("HeartHP")]
	[DefaultValue(2)]  [Range(0, 1000)] [Slider] [Increment(1)] public int Common;
	[DefaultValue(4)]  [Range(0, 1000)] [Slider] [Increment(1)] public int Uncommon;
	[DefaultValue(6)]  [Range(0, 1000)] [Slider] [Increment(1)] public int Rare;
	[DefaultValue(8)]  [Range(0, 1000)] [Slider] [Increment(1)] public int Epic;
	[DefaultValue(10)] [Range(0, 1000)] [Slider] [Increment(1)] public int Legendary;
	[DefaultValue(10)] [Range(0, 1000)] [Slider] [Increment(1)] public int Exotic;
	[DefaultValue(50)] [Range(0, 1000)] [Slider] [Increment(1)] public int Mythic;

	[Header("Recipes")]
	[DefaultValue(10)] [Range(1, 100)] [Slider] [Increment(1)] public int RecipeDifficulty;

	[Header("BossHearts")]
	[DefaultValue(true)] public bool GuaranteedFirstKill;
	[DefaultValue(10)] [Range(1, 100)] [Slider] [Increment(1)] public int BossHeartDropChance;
}
