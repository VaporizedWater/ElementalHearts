using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.LifeShards;
using ElementalHearts.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Localization;
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
	/// Tooltip name colour for a shard tier — pulled straight from the matching heart
	/// rarity's <see cref="ModRarity.RarityColor"/>, so a shard's name shows the exact
	/// custom colour the hearts of that tier use. The heart rarities are the single
	/// source of truth: retune one and the shards and potions follow.
	/// </summary>
	public static Color GetTextColor(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => ModContent.GetInstance<CommonHeartRarity>().RarityColor,
		LifeShardTier.Uncommon  => ModContent.GetInstance<UncommonHeartRarity>().RarityColor,
		LifeShardTier.Rare      => ModContent.GetInstance<RareHeartRarity>().RarityColor,
		LifeShardTier.Epic      => ModContent.GetInstance<EpicHeartRarity>().RarityColor,
		LifeShardTier.Legendary => ModContent.GetInstance<LegendaryHeartRarity>().RarityColor,
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
	/// How many shards of <paramref name="from"/> are consumed to craft one shard of the
	/// higher tier <paramref name="to"/> directly, skipping every tier in between (the
	/// per-step costs multiplied together). Returns 0 when <paramref name="to"/> is not
	/// strictly above <paramref name="from"/>.
	/// </summary>
	public static int GetUpgradeCost(this LifeShardTier from, LifeShardTier to)
	{
		if (to <= from)
			return 0;

		int cost = 1;
		for (LifeShardTier step = from + 1; step <= to; step++)
			cost *= step.GetUpgradeCost();

		return cost;
	}

	/// <summary>Localized display name for a tier ("Common", "Rare", …), used in tooltips.</summary>
	public static string GetDisplayName(this LifeShardTier tier)
		=> Language.GetTextValue($"Mods.ElementalHearts.UI.Tier{tier}");

	/// <summary>
	/// The "shard absorbed" cue for a tier — one custom sound per tier, played when a shard
	/// of this tier is picked up into the Life Shard slots. The files live in <c>Sounds/</c>.
	/// </summary>
	public static SoundStyle GetPickupSound(this LifeShardTier tier) => tier switch
	{
		LifeShardTier.Common    => new SoundStyle("ElementalHearts/Sounds/CommonCrystalPickup"),
		LifeShardTier.Uncommon  => new SoundStyle("ElementalHearts/Sounds/UncommonCrystalPickup"),
		LifeShardTier.Rare      => new SoundStyle("ElementalHearts/Sounds/RareCrystalPickup"),
		LifeShardTier.Epic      => new SoundStyle("ElementalHearts/Sounds/EpicCrystalPickup"),
		LifeShardTier.Legendary => new SoundStyle("ElementalHearts/Sounds/LegendaryCrystalPickup"),
		_ => new SoundStyle("ElementalHearts/Sounds/CommonCrystalPickup"),
	};

	/// <summary>
	/// Quick-heal value for one shard of this tier, sourced from <see cref="LifeShardConfig"/>
	/// so the consumable feature can be retuned without recompiling.
	/// </summary>
	public static int GetHealAmount(this LifeShardTier tier)
	{
		LifeShardConfig cfg = LifeShardConfig.Instance;
		return tier switch
		{
			LifeShardTier.Common    => cfg.CommonHealAmount,
			LifeShardTier.Uncommon  => cfg.UncommonHealAmount,
			LifeShardTier.Rare      => cfg.RareHealAmount,
			LifeShardTier.Epic      => cfg.EpicHealAmount,
			LifeShardTier.Legendary => cfg.LegendaryHealAmount,
			_ => 0,
		};
	}

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
