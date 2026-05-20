using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsBossConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsBossConfig Instance => ModContent.GetInstance<ElementalHeartsBossConfig>();

	[DefaultValue(true)]
	public bool BossHeartsGuaranteedOnFirstKill;

	[DefaultValue(10)]
	[Range(1, 100)]
	[Slider]
	[Increment(1)]
	public int BossHeartRNG;
}
