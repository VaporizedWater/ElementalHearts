using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Client-side, per-player interface preferences: the Heart Log checklist and its
/// detail widgets, the recoloured Elemental HP bar, permanent-buff icons, and the
/// global kill-switch for the Jack-O'-Lantern dash ability. Purely presentational —
/// nothing here touches gameplay or needs to agree with other players.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsClientConfig Instance => ModContent.GetInstance<ElementalHeartsClientConfig>();

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

	/// <summary>Global kill-switch for the Jack-O'-Lantern Heart's dash burst. When off, dashing
	/// never spawns a lantern regardless of the per-character Heart Log toggle.</summary>
	[Header("Abilities")]
	[DefaultValue(true)]
	public bool EnableJackOLanternDash { get; set; } = true;
}
