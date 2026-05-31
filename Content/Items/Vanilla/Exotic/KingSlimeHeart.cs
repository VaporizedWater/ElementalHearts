using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

public sealed class KingSlimeHeart : BossHeartItem
{
	public override bool IsActiveAbility => true;

	public override int? ActiveAbilityDailyCost => 3;

	public override int HpGain => 0;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<KingSlimeAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<KingSlimeAbilityPlayer>().Enabled = enabled;
}
