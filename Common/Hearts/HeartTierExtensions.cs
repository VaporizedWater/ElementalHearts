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
		var cfg = ElementalHeartsServerConfig.Instance.HPScale;
		int hp = tier switch
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

		// Challenge mode guts every other HP source, so hearts pay double to stay worth chasing.
		return cfg.ChallengeMode ? hp * 2 : hp;
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

	/// <summary>
	/// 0 (Common) … 1 (Mythic) ramp driving how big and bold a heart's idle glow and consume
	/// flourish read, so rarity reads at a glance — barely-there on a Common heart, unmistakable
	/// on a Mythic one. Mostly a smooth ladder, except Exotic is deliberately placed between Rare
	/// and Epic rather than near the top so its (many) boss hearts stay calm. Lives here, with the
	/// other tier-derived identity, so "look/feel derives from tier" is literally one file.
	/// </summary>
	public static float GetRarityScale(this HeartTier tier) => tier switch
	{
		HeartTier.Common    => 0f,
		HeartTier.Uncommon  => 1f / 6f,
		HeartTier.Rare      => 2f / 6f,
		HeartTier.Exotic    => 0.42f, // intentionally between Rare and Epic
		HeartTier.Epic      => 3f / 6f,
		HeartTier.Legendary => 4f / 6f,
		HeartTier.Mythic    => 1f,
		_ => 0f,
	};

	public static int GetCostMultiplier(this HeartTier tier) => tier switch
	{
		HeartTier.Common => 1,
		HeartTier.Uncommon => 3,
		HeartTier.Rare => 5,
		HeartTier.Epic => 7,
		HeartTier.Legendary => 10,
		HeartTier.Exotic => 15,
		HeartTier.Mythic => 30,
		_ => 0,
	};

	public static int GetCostMultiplier(this HeartTier _, HeartTier tier) => tier switch
	{
		HeartTier.Common => 1,
		HeartTier.Uncommon => 3,
		HeartTier.Rare => 5,
		HeartTier.Epic => 7,
		HeartTier.Legendary => 10,
		HeartTier.Exotic => 15,
		HeartTier.Mythic => 30,
		_ => 0,
	};

	public static bool GreaterThanOrEqualTo(this HeartTier A, HeartTier B) => (int)A >= (int)B;
	public static bool EqualTo(this HeartTier A, HeartTier B) => (int)A == (int)B;

	/// <summary>
	/// Multiplier reining in the on-ground bloom for the showy top tiers so the pulsing glow
	/// doesn't balloon over a dropped heart; 1 (no damping) for everything below Legendary.
	/// Inventory glow, where slots are small and fixed, ignores this and draws at full size.
	/// </summary>
	public static float GetWorldGlowDampen(this HeartTier tier) => tier switch
	{
		HeartTier.Legendary => 0.86f,
		HeartTier.Exotic    => 0.92f,
		HeartTier.Mythic    => 0.71f,
		_ => 1f,
	};

	/// <summary>
	/// Life Shards a consumed heart of this tier generates (or an active-ability heart consumes)
	/// per Terraria day in the idle economy. Climbs with rarity, except Exotic sits low (between
	/// Uncommon and Rare) so its many boss hearts don't flood the economy — the same deliberate
	/// out-of-order Exotic placement as <see cref="GetRarityScale"/>.
	/// </summary>
	public static int GetShardYield(this HeartTier tier)
	{
		var cfg = ElementalHeartsServerConfig.Instance.LifeShards;
		return tier switch
		{
			HeartTier.Common    => cfg.CommonPassiveYield,
			HeartTier.Uncommon  => cfg.UncommonPassiveYield,
			HeartTier.Rare      => cfg.RarePassiveYield,
			HeartTier.Epic      => cfg.EpicPassiveYield,
			HeartTier.Legendary => cfg.LegendaryPassiveYield,
			HeartTier.Exotic    => cfg.ExoticPassiveYield,
			HeartTier.Mythic    => cfg.MythicPassiveYield,
			_ => 1,
		};
	}

	/// <summary>
	/// Life Shards consumed per Terraria day by an active-ability heart.
	/// </summary>
	public static int GetActiveAbilityDailyCost(this HeartTier tier)
	{
		var cfg = ElementalHeartsServerConfig.Instance.LifeShards;
		return tier switch
		{
			HeartTier.Common    => cfg.CommonAbilityCost,
			HeartTier.Uncommon  => cfg.UncommonAbilityCost,
			HeartTier.Rare      => cfg.RareAbilityCost,
			HeartTier.Epic      => cfg.EpicAbilityCost,
			HeartTier.Legendary => cfg.LegendaryAbilityCost,
			HeartTier.Exotic    => cfg.ExoticAbilityCost,
			HeartTier.Mythic    => cfg.MythicAbilityCost,
			_ => 1,
		};
	}
}
