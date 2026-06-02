using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

public sealed class TheDestroyerHeart : BossHeartItem
{
	protected override int AnimationFrameCount => 10;
	public override bool IsActiveAbility => true;

	public override int HpGain => 0;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<DestroyerAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled)
	{
		if (enabled && !Main.LocalPlayer.GetModPlayer<DestroyerAbilityPlayer>().Player.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled)
		{
			Main.NewText("The Destroyer Probe refuses to awaken without its master's eye...", Microsoft.Xna.Framework.Color.Red);
			return;
		}
		Main.LocalPlayer.GetModPlayer<DestroyerAbilityPlayer>().Enabled = enabled;
	}
}
