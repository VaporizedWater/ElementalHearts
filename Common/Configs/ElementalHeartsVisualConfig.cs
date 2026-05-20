using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsVisualConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsVisualConfig Instance => ModContent.GetInstance<ElementalHeartsVisualConfig>();

	[DefaultValue(3)]
	[Range(1, 5)]
	[Slider]
	[Increment(1)]
	public int ConsumptionEffectStrength;
}
