using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>Client-side toggles for in-game tip hints (e.g. the Life Shard panel's Animate tip).</summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsTipsConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsTipsConfig Instance => ModContent.GetInstance<ElementalHeartsTipsConfig>();

	[Header("Tips")]
	[DefaultValue(true)]
	public bool EnableTips;
}
