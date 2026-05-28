using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Toggle for the world-wide permanent potion effect granted by consumed Potion Hearts.
/// When disabled, Potion Hearts still grant their HP boost but stop applying the buff
/// to players every tick — they become regular hearts. Server-side because the consumed
/// set lives on the world.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsPotionEffectConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsPotionEffectConfig Instance => ModContent.GetInstance<ElementalHeartsPotionEffectConfig>();

	[Header("PotionHearts")]
	[DefaultValue(true)]
	public bool WorldwidePotionEffectsEnabled;
}
