using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>Client-side tuning for the heart-consumption effect and HUD elements.</summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsVisualConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsVisualConfig Instance => ModContent.GetInstance<ElementalHeartsVisualConfig>();

	[Header("Effects")]
	[DefaultValue(3)]
	[Range(1, 5)]
	[Increment(1)]
	[Slider]
	[DrawTicks]
	[SliderColor(255, 130, 160, 255)]
	public int ConsumptionEffectStrength;

	[Header("UI")]
	[DefaultValue(false)]
	public bool DraggableUI;

	[DefaultValue(typeof(Vector2), "0, 0")]
	[Range(0f, 3840f)]
	[Increment(1f)]
	public Vector2 UIPosition;
}
