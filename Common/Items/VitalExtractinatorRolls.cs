using System;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;

namespace ElementalHearts.Common.Items;

/// <summary>
/// Extractinator yield tables for the two Vital tile items. Mirrors the existing Life
/// Crystal extractinator pattern: a primary result returned via the <c>ref</c> params
/// and zero or more independent bonus rolls spawned straight onto the local player.
/// </summary>
public static class VitalExtractinatorRolls
{
	/// <summary>
	/// Vital Soil yields: Common Life Shards as the primary result, with rare bonus rolls
	/// for gems, herb seeds, and a Life Crystal. Composition aims to make Vital Soil feel
	/// like a "soft" silt-equivalent that also feeds the shard economy.
	/// </summary>
	public static void RollVitalSoil(ref int resultType, ref int resultStack)
	{
		PrimaryShardRoll(ref resultType, ref resultStack);

		TrySpawnGem(5f);
		TrySpawnFromPool(5f, _commonHerbSeeds);
		TrySpawn(1f, ItemID.LifeCrystal, 1, 1);
	}

	/// <summary>
	/// Vital Quartz yields: same shard primary, but jungle-flavoured bonuses — jungle herb
	/// seeds, stingers and vines, and a hardmode-gated Life Fruit drop.
	/// </summary>
	public static void RollVitalQuartz(ref int resultType, ref int resultStack)
	{
		PrimaryShardRoll(ref resultType, ref resultStack);

		TrySpawnGem(5f);
		TrySpawnFromPool(5f, _jungleHerbSeeds);
		TrySpawnFromPool(3f, _vineMaterials);

		// Life Fruit is hardmode-only in vanilla; matching that gate keeps progression intact.
		if (Main.hardMode)
			TrySpawn(2f, ItemID.LifeFruit, 1, 1);
	}

	private static void PrimaryShardRoll(ref int resultType, ref int resultStack)
	{
		LifeShardConfig cfg = LifeShardConfig.Instance;
		if (!cfg.SystemEnabled)
			return;

		resultStack = RollStack(cfg.ExtractinatorCommonMin, cfg.ExtractinatorCommonMax);
		resultType = resultStack > 0 ? LifeShardTier.Common.GetItemType() : 0;
	}

	private static readonly short[] _gems = new[]
	{
		ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald,
		ItemID.Ruby, ItemID.Diamond, ItemID.Amber,
	};

	private static readonly short[] _commonHerbSeeds = new[]
	{
		ItemID.DaybloomSeeds, ItemID.BlinkrootSeeds, ItemID.WaterleafSeeds,
		ItemID.MoonglowSeeds, ItemID.DeathweedSeeds,
	};

	private static readonly short[] _jungleHerbSeeds = new[]
	{
		ItemID.MoonglowSeeds, ItemID.DeathweedSeeds, ItemID.FireblossomSeeds,
		ItemID.ShiverthornSeeds,
	};

	private static readonly short[] _vineMaterials = new[]
	{
		ItemID.Stinger, ItemID.Vine, ItemID.JungleSpores,
	};

	private static void TrySpawnGem(float chancePercent)
		=> TrySpawnFromPool(chancePercent, _gems);

	private static void TrySpawnFromPool(float chancePercent, short[] pool)
	{
		if (pool == null || pool.Length == 0)
			return;

		int type = pool[Main.rand.Next(pool.Length)];
		TrySpawn(chancePercent, type, 1, 1);
	}

	private static void TrySpawn(float chancePercent, int itemType, int min, int max)
	{
		if (Main.netMode == NetmodeID.Server)
			return;
		if (itemType <= 0)
			return;
		if (Main.rand.NextFloat() >= chancePercent / 100f)
			return;

		int stack = RollStack(min, max);
		if (stack <= 0)
			return;

		Player player = Main.LocalPlayer;
		player.QuickSpawnItem(player.GetSource_Misc("VitalExtractinator"), itemType, stack);
	}

	private static int RollStack(int a, int b)
	{
		int min = Math.Max(0, Math.Min(a, b));
		int max = Math.Max(a, b);
		return Main.rand.Next(min, max + 1);
	}
}
