using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Hearts;

public static class HeartTierExtensions
{
	/// <summary>
	/// Colour of the inner particle ring in the consumption effect. Mirrors the
	/// rarity colour ladder (white → green → blue → purple → gold → red → radiant)
	/// so a heart's tier reads at a glance.
	/// </summary>
	public static Color GetEffectColor(this HeartTier tier) => tier switch
	{
		HeartTier.Common    => new Color(255, 205, 218),
		HeartTier.Uncommon  => new Color(150, 230, 150),
		HeartTier.Rare      => new Color(110, 170, 255),
		HeartTier.Epic      => new Color(200, 130, 255),
		HeartTier.Legendary => new Color(255, 200, 90),
		HeartTier.Exotic    => new Color(255, 120, 110),
		HeartTier.Mythic    => new Color(255, 235, 150),
		_ => Color.White,
	};

	public static int GetHpGain(this HeartTier tier)
	{
		var cfg = ElementalHeartsHPConfig.Instance;
		return tier switch
		{
			HeartTier.Common    => cfg.Common,
			HeartTier.Uncommon  => cfg.Uncommon,
			HeartTier.Rare      => cfg.Rare,
			HeartTier.Epic      => cfg.Epic,
			HeartTier.Legendary => cfg.Legendary,
			HeartTier.Exotic    => cfg.Exotic,
			HeartTier.Mythic    => cfg.Mythic,
			_ => 0,
		};
	}

	public static int GetRarityType(this HeartTier tier) => tier switch
	{
		HeartTier.Common    => ModContent.RarityType<CommonHeartRarity>(),
		HeartTier.Uncommon  => ModContent.RarityType<UncommonHeartRarity>(),
		HeartTier.Rare      => ModContent.RarityType<RareHeartRarity>(),
		HeartTier.Epic      => ModContent.RarityType<EpicHeartRarity>(),
		HeartTier.Legendary => ModContent.RarityType<LegendaryHeartRarity>(),
		HeartTier.Exotic    => ModContent.RarityType<ExoticHeartRarity>(),
		HeartTier.Mythic    => ModContent.RarityType<MythicHeartRarity>(),
		_ => ModContent.RarityType<CommonHeartRarity>(),
	};
}
