using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using ElementalHearts.Common.Systems;

namespace ElementalHearts.Common.Configs;

[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsWorldConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsWorldConfig Instance => ModContent.GetInstance<ElementalHeartsWorldConfig>();

	[DefaultValue(false)]
	public bool ClearHeartRegistry
	{
		get => false;
		set
		{
			if (value)
			{
				HeartConsumptionWorld.ClearAllHearts();
			}
		}
	}
}
