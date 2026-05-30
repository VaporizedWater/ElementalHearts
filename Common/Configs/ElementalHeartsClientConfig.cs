using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

public class ElementalHeartsClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	public static ElementalHeartsClientConfig Instance => Terraria.ModLoader.ModContent.GetInstance<ElementalHeartsClientConfig>();

	[Header("UI")]
	[DefaultValue(true)]
	public bool EnableHeartChecklist { get; set; } = true;
}
