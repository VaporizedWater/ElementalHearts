using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Items;

/// <summary>
/// Lets a vanilla Life Crystal be "recycled" in the Extractinator: every crystal yields
/// a configurable handful of Common Life Shards, with independent bonus chances for one
/// of each higher tier. Life Crystal acceptance is toggled by
/// <see cref="LifeShards.LifeShardSystem.SetLifeCrystalExtractable"/>.
/// </summary>
public sealed class LifeCrystalExtractinatorGlobalItem : GlobalItem
{
	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (item.type == ItemID.Extractinator || item.type == ItemID.ChlorophyteExtractinator)
		{
			tooltips.Add(new TooltipLine(Mod, "ExtractinatorLifeCrystal", "Crushing a Life Crystal in the Extractinator turns it into something equally as useful"));
		}
		else if (item.type == ItemID.LifeCrystal)
		{
			tooltips.Add(new TooltipLine(Mod, "LifeCrystalExtractinator", "Can be crushed in the Extractinator"));
		}
	}

	public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
		if (extractType != ItemID.LifeCrystal)
			return;

		LifeShardSettings config = ElementalHeartsServerConfig.Instance.LifeShards;
		if (!config.SystemEnabled)
			return;

		// Guaranteed Common shards form the primary Extractinator result.
		resultStack = VitalExtractinatorRolls.RollStack(config.ExtractinatorCommonMin, config.ExtractinatorCommonMax);
		resultType = resultStack > 0 ? LifeShardTier.Common.GetItemType() : 0;

		// Higher tiers are independent bonus rolls spawned straight onto the player.
		TrySpawnBonus(config.ExtractinatorUncommonChance, LifeShardTier.Uncommon, config.ExtractinatorUncommonMin, config.ExtractinatorUncommonMax);
		TrySpawnBonus(config.ExtractinatorRareChance, LifeShardTier.Rare, config.ExtractinatorRareMin, config.ExtractinatorRareMax);
		TrySpawnBonus(config.ExtractinatorEpicChance, LifeShardTier.Epic, config.ExtractinatorEpicMin, config.ExtractinatorEpicMax);
		TrySpawnBonus(config.ExtractinatorLegendaryChance, LifeShardTier.Legendary, config.ExtractinatorLegendaryMin, config.ExtractinatorLegendaryMax);

		TrySpawnSeed();
	}

	/// <summary>
	/// Independent bonus: a Life Crystal Seed for replanting the crystal on Vital Quartz.
	/// Spawned alongside the shard yield rather than replacing it — the seed roll never
	/// reduces shard output.
	/// </summary>
	private static void TrySpawnSeed()
	{
		VitalTileSettings vitalCfg = ElementalHeartsServerConfig.Instance.VitalTiles;
		if (!vitalCfg.SystemEnabled)
			return;
		if (Main.netMode == NetmodeID.Server)
			return;
		if (Main.rand.NextFloat() >= vitalCfg.LifeCrystalSeedChance / 100f)
			return;

		Player player = Main.LocalPlayer;
		player.QuickSpawnItem(player.GetSource_Misc("LifeCrystalSeedExtractinator"),
			ModContent.ItemType<Content.Items.Placeable.LifeCrystalSeedItem>(), 1);
	}

	private static void TrySpawnBonus(float chancePercent, LifeShardTier tier, int min, int max)
		=> VitalExtractinatorRolls.TrySpawn(chancePercent, tier.GetItemType(), min, max, "LifeShardExtractinator");
}
