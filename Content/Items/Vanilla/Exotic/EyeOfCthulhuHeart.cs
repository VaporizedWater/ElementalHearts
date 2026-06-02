using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

public sealed class EyeOfCthulhuHeart : BossHeartItem
{
	public override bool IsActiveAbility => true;



	public override int HpGain => 0;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled = enabled;
}
