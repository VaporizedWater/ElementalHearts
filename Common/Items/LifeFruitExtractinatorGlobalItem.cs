using System;
using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Items;

/// <summary>
/// Makes vanilla Life Fruit accepted by the Extractinator while the Vital Tiles system is
/// enabled. A Life Fruit is the hardmode counterpart to the Life Crystal recycle: it
/// always yields Uncommon Life Shards as the primary result, rolls higher tiers as
/// independent bonuses, and has an independent chance to also produce a Life Fruit Seed.
/// </summary>
public sealed class LifeFruitExtractinatorGlobalItem : GlobalItem
{
	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (item.type != ItemID.LifeFruit || !ElementalHeartsServerConfig.Instance.VitalTiles.SystemEnabled)
			return;

		tooltips.Add(new TooltipLine(Mod, "LifeFruitExtractinator",
			"Can be crushed in the Extractinator"));
	}

	public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
		if (extractType != ItemID.LifeFruit)
			return;

		VitalTileSettings vitalCfg = ElementalHeartsServerConfig.Instance.VitalTiles;
		if (!vitalCfg.SystemEnabled)
			return;

		LifeShardSettings shardCfg = ElementalHeartsServerConfig.Instance.LifeShards;
		if (shardCfg.SystemEnabled)
		{
			// Guaranteed Uncommon shards form the primary Extractinator result —
			// Life Fruit is the hardmode-tier recycle, one step above Life Crystal.
			resultStack = RollStack(shardCfg.ExtractinatorUncommonMin, shardCfg.ExtractinatorUncommonMax);
			resultType = resultStack > 0 ? LifeShardTier.Uncommon.GetItemType() : 0;

			TrySpawnBonus(shardCfg.ExtractinatorRareChance, LifeShardTier.Rare, shardCfg.ExtractinatorRareMin, shardCfg.ExtractinatorRareMax);
			TrySpawnBonus(shardCfg.ExtractinatorEpicChance, LifeShardTier.Epic, shardCfg.ExtractinatorEpicMin, shardCfg.ExtractinatorEpicMax);
			TrySpawnBonus(shardCfg.ExtractinatorLegendaryChance, LifeShardTier.Legendary, shardCfg.ExtractinatorLegendaryMin, shardCfg.ExtractinatorLegendaryMax);
		}

		TrySpawnSeed(vitalCfg);
	}

	/// <summary>
	/// Independent bonus: a Life Fruit Seed for replanting on Vital Soil. Spawned
	/// alongside the shard yield rather than replacing it.
	/// </summary>
	private static void TrySpawnSeed(VitalTileSettings cfg)
	{
		if (Main.netMode == NetmodeID.Server)
			return;
		if (Main.rand.NextFloat() >= cfg.LifeFruitSeedChance / 100f)
			return;

		Player player = Main.LocalPlayer;
		player.QuickSpawnItem(player.GetSource_Misc("LifeFruitSeedExtractinator"),
			ModContent.ItemType<Content.Items.Placeable.LifeFruitSeedItem>(), 1);
	}

	private static int RollStack(int a, int b)
	{
		int min = Math.Max(0, Math.Min(a, b));
		int max = Math.Max(a, b);
		return Main.rand.Next(min, max + 1);
	}

	private static void TrySpawnBonus(float chancePercent, LifeShardTier tier, int min, int max)
	{
		if (Main.netMode == NetmodeID.Server)
			return;
		if (Main.rand.NextFloat() >= chancePercent / 100f)
			return;

		int stack = RollStack(min, max);
		if (stack <= 0)
			return;

		Player player = Main.LocalPlayer;
		player.QuickSpawnItem(player.GetSource_Misc("LifeShardExtractinator"), tier.GetItemType(), stack);
	}
}
