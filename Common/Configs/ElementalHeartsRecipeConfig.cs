using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsRecipeConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsRecipeConfig Instance => ModContent.GetInstance<ElementalHeartsRecipeConfig>();

	[DefaultValue(10)]
	[Range(1, 100)]
	[Slider]
	[Increment(1)]
	[ReloadRequired]
	public int RecipeDifficulty;
}
