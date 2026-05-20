using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Rarities;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Hearts;

public static class HeartTierExtensions
{
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
