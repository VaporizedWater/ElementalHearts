using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
namespace ElementalHearts.Common.Configs;

/// <summary>Client-side tuning for the heart-consumption effect.</summary>
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

	[DefaultValue(false)]
	public bool DraggableUI;

	public Vector2 UIPosition;
}
