using ElementalHearts.Content.Items.LifeShards;
using ElementalHearts.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ElementalHearts.Common.LifeShards;

public static class LifeShardTierExtensions
{
	/// <summary>Resolves the registered <see cref="LifeShardItem"/> type for a tier.</summary>
	public static int GetItemType(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => ModContent.ItemType<CommonLifeShard>(),
		LifeShardTier.Uncommon  => ModContent.ItemType<UncommonLifeShard>(),
		LifeShardTier.Rare      => ModContent.ItemType<RareLifeShard>(),
		LifeShardTier.Epic      => ModContent.ItemType<EpicLifeShard>(),
		LifeShardTier.Legendary => ModContent.ItemType<LegendaryLifeShard>(),
		_ => 0,
	};

	/// <summary>
	/// Tooltip text colour for a shard tier — the rarity colour ladder (white → green →
	/// blue → purple → gold), so a shard's tier reads from its tooltip at a glance.
	/// </summary>
	public static Color GetTextColor(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => new Color(255, 255, 255),
		LifeShardTier.Uncommon  => new Color(150, 230, 150),
		LifeShardTier.Rare      => new Color(110, 170, 255),
		LifeShardTier.Epic      => new Color(200, 130, 255),
		LifeShardTier.Legendary => new Color(255, 200, 90),
		_ => Color.White,
	};

	/// <summary>Shares the heart rarity colour ladder so shard tiers read at a glance.</summary>
	public static int GetRarityType(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => ModContent.RarityType<CommonHeartRarity>(),
		LifeShardTier.Uncommon  => ModContent.RarityType<UncommonHeartRarity>(),
		LifeShardTier.Rare      => ModContent.RarityType<RareHeartRarity>(),
		LifeShardTier.Epic      => ModContent.RarityType<EpicHeartRarity>(),
		LifeShardTier.Legendary => ModContent.RarityType<LegendaryHeartRarity>(),
		_ => ModContent.RarityType<CommonHeartRarity>(),
	};

	/// <summary>
	/// Number of lower-tier shards consumed to combine into one shard of this tier.
	/// Returns 0 for <see cref="LifeShardTier.Common"/>, which cannot be combined into.
	/// The counts taper (5 → 4 → 3 → 2) so one Legendary shard is worth exactly 120 Commons.
	/// </summary>
	public static int GetUpgradeCost(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Uncommon  => 5,
		LifeShardTier.Rare      => 4,
		LifeShardTier.Epic      => 3,
		LifeShardTier.Legendary => 2,
		_ => 0,
	};

	/// <summary>
	/// Yields the tier one step below this one. Returns false for <see cref="LifeShardTier.Common"/>,
	/// which has no lower tier and therefore no combine recipe.
	/// </summary>
	public static bool TryGetLowerTier(this LifeShardTier tier, out LifeShardTier lower)
	{
		if (tier == LifeShardTier.Common)
		{
			lower = LifeShardTier.Common;
			return false;
		}

		lower = tier - 1;
		return true;
	}
}
