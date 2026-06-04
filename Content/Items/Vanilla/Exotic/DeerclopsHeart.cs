using Terraria;
using Terraria.Audio;
using Terraria.ID;

using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Exotic;

/// <summary>
/// Active heart: consuming it unlocks the winter-beast <b>parry</b> (press the Parry keybind to flare
/// pink and body-check whatever touches you). Grants no HP — the parry is the payoff. All the timing,
/// damage and FX live in <see cref="ParryAbilityPlayer"/>; this stays a declaration that just names
/// its toggle (mirrors <see cref="TheTwinsHeart"/> and friends).
/// </summary>
public sealed class DeerclopsHeart : BossHeartItem
{
	protected override SoundStyle BossConsumeSound => SoundID.DeerclopsScream;

	public override bool IsActiveAbility => true;

	public override int HpGain => 0;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<ParryAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<ParryAbilityPlayer>().Enabled = enabled;
}
