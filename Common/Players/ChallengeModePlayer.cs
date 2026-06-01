using ElementalHearts.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Enforces the hardcore "Challenge Mode" health rules (<see cref="HPSettings.ChallengeMode"/>):
/// a character's base max life drops from the vanilla 100 to 1, and each consumed Life Crystal
/// grants 5 instead of 20. Hearts are deliberately left untouched here — they double up in
/// <see cref="Hearts.HeartTierExtensions.GetHpGain"/> — so consuming hearts becomes the real way
/// to grow your health bar.
/// </summary>
public sealed class ChallengeModePlayer : ModPlayer
{
	/// <summary>The max life every vanilla character starts with, before crystals and hearts.</summary>
	private const int VanillaBaseLife = 100;

	/// <summary>Vanilla Life Crystal gain we claw back down to <see cref="ChallengeCrystalLife"/>.</summary>
	private const int VanillaCrystalLife = 20;
	private const int ChallengeCrystalLife = 5;

	public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
	{
		health = StatModifier.Default;
		mana = StatModifier.Default;

		if (!ElementalHeartsServerConfig.Instance.HPScale.ChallengeMode)
			return;

		// `Player.statLifeMax` already bakes in the vanilla base (100) and +20 per crystal, so we
		// subtract the difference as a flat `Base` adjustment. Net floor stays at 1 because each
		// crystal still nets +5, never dropping below the reduced base.
		int reduction = (VanillaBaseLife - 1)
			+ (VanillaCrystalLife - ChallengeCrystalLife) * Player.ConsumedLifeCrystals;

		health = StatModifier.Default with { Base = -reduction };
	}
}
