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

	[DefaultValue(true)]
	public bool ShowDetailedHeartStats { get; set; } = true;

	[DefaultValue(true)]
	public bool EnableElementalHP { get; set; } = true;

	[DefaultValue(true)]
	public bool ShowPermanentBuffs { get; set; } = true;

	[DefaultValue(true)]
	public bool HideImpossibleHearts { get; set; } = true;

	// ── Abilities ────────────────────────────────────────────────────────────────────
	/// <summary>Global kill-switch for the Jack-O'-Lantern Heart's dash burst. When off, dashing
	/// never spawns a lantern regardless of the Heart Log toggle.</summary>
	[Header("Abilities")]
	[DefaultValue(true)]
	public bool EnableJackOLanternDash { get; set; } = true;
}
