using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.BossSpawns;
using Terraria;
using Terraria.ModLoader;
using System;
using ElementalHearts.Content.Items.LifeShards;

namespace ElementalHearts.Common.Systems;

public sealed class TieredItemDecraftPlayer : ModPlayer
{
	public override void PostUpdate()
	{
		int maxTier = AnimateProgressionSystem.UnlockedTier;

		// Decraft Life Shards that are above the unlocked tier
		var shardPlayer = Player.GetModPlayer<LifeShardPlayer>();
		for (int i = LifeShardPlayer.SlotCount - 1; i > maxTier; i--)
		{
			Item higherSlot = shardPlayer.Shards[i];
			if (!higherSlot.IsAir && higherSlot.stack > 0)
			{
				int downgradeYield = ((LifeShardTier)i).GetUpgradeCost();
				int lowerTierIndex = i - 1;
				
				int amountToDowngrade = higherSlot.stack;
				higherSlot.TurnToAir();

				if (lowerTierIndex >= 0)
				{
					int totalYield = amountToDowngrade * downgradeYield;
					Item lowerSlot = shardPlayer.Shards[lowerTierIndex];
					
					if (lowerSlot.IsAir)
					{
						lowerSlot.SetDefaults(((LifeShardTier)lowerTierIndex).GetItemType());
						lowerSlot.stack = totalYield;
					}
					else
					{
						lowerSlot.stack += totalYield;
					}
				}
			}
		}
	}
}

public sealed class TieredItemDecraftGlobalItem : GlobalItem
{
	public override void UpdateInventory(Item item, Player player)
	{
		if (item.ModItem is MenacingHeartItem menacingHeart)
		{
			if ((int)menacingHeart.Tier > AnimateProgressionSystem.UnlockedTier)
			{
				// Decraft into 10 shards of the same tier. 
				// The shards will then be automatically down-crafted by the TieredItemDecraftPlayer on subsequent ticks.
				int shardType = menacingHeart.Tier.GetItemType();
				int amount = item.stack * 10;
				
				item.TurnToAir(); // Remove the heart
				
				// Try to add shards to the player's LifeShard slots directly
				var shardPlayer = player.GetModPlayer<LifeShardPlayer>();
				Item shardItem = new Item();
				shardItem.SetDefaults(shardType);
				shardItem.stack = amount;
				
				if (!shardPlayer.AbsorbShards(shardItem))
				{
					// If they couldn't be absorbed (e.g., slot full), drop them or put them in regular inventory
					// Actually, LifeShardItem never goes into regular inventory. If AbsorbShards fails to absorb all, it drops.
					if (shardItem.stack > 0)
					{
						player.QuickSpawnItem(player.GetSource_ItemUse(item), shardItem, shardItem.stack);
					}
				}
			}
		}
	}
}
