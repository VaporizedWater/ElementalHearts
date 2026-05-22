using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Buffs;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Potions;

/// <summary>
/// Shared data and lookups for the five-tier Animating Potion line. Each tier grants a
/// timed buff that raises maximum life and — more strongly — the life granted by the mod's
/// elemental hearts. The five tiers reuse <see cref="LifeShardTier"/>, since every potion is
/// gated behind a Life Shard of the matching tier.
/// </summary>
public static class AnimatingPotion
{
	/// <summary>Buff duration in ticks — four minutes, shared by every tier.</summary>
	public const int BuffDuration = 60 * 60 * 4;

	/// <summary>Resolves the registered potion item type for a tier.</summary>
	public static int GetItemType(LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => ModContent.ItemType<CommonAnimatingPotion>(),
		LifeShardTier.Uncommon  => ModContent.ItemType<UncommonAnimatingPotion>(),
		LifeShardTier.Rare      => ModContent.ItemType<RareAnimatingPotion>(),
		LifeShardTier.Epic      => ModContent.ItemType<EpicAnimatingPotion>(),
		LifeShardTier.Legendary => ModContent.ItemType<LegendaryAnimatingPotion>(),
		_ => 0,
	};

	/// <summary>Resolves the buff type granted by a tier's potion.</summary>
	public static int GetBuffType(LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => ModContent.BuffType<CommonAnimatingPotionBuff>(),
		LifeShardTier.Uncommon  => ModContent.BuffType<UncommonAnimatingPotionBuff>(),
		LifeShardTier.Rare      => ModContent.BuffType<RareAnimatingPotionBuff>(),
		LifeShardTier.Epic      => ModContent.BuffType<EpicAnimatingPotionBuff>(),
		LifeShardTier.Legendary => ModContent.BuffType<LegendaryAnimatingPotionBuff>(),
		_ => 0,
	};

	/// <summary>Fraction added to overall maximum life while a tier's buff is active.</summary>
	public static float GetMaxLifePercent(LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => 0.05f,
		LifeShardTier.Uncommon  => 0.05f,
		LifeShardTier.Rare      => 0.05f,
		LifeShardTier.Epic      => 0.05f,
		LifeShardTier.Legendary => 0.05f,
		_ => 0f,
	};

	/// <summary>Fraction added to the elemental-heart life bonus while a tier's buff is active.</summary>
	public static float GetElementalLifePercent(LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => 0.05f,
		LifeShardTier.Uncommon  => 0.10f,
		LifeShardTier.Rare      => 0.15f,
		LifeShardTier.Epic      => 0.20f,
		LifeShardTier.Legendary => 0.25f,
		_ => 0f,
	};
}
