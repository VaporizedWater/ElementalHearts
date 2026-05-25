using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>Scales the crafting cost of every heart recipe.</summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsRecipeConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsRecipeConfig Instance => ModContent.GetInstance<ElementalHeartsRecipeConfig>();

	[Header("Crafting")]
	[DefaultValue(10)]
	[Range(1, 100)]
	[Increment(1)]
	[Slider]
	[SliderColor(255, 180, 120, 255)]
	[ReloadRequired]
	public int RecipeDifficulty;
}
