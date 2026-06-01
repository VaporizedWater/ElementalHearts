using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Buffs;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Intercepts the quick-heal key press to consume Life Shards held in the regular inventory
/// before vanilla picks a healing potion. Shards sitting in the dedicated panel slots
/// (<see cref="LifeShardPlayer.Shards"/>) are never touched — only stacks that overflowed
/// into the regular inventory because their tier slot was already full are quick-heal
/// candidates. That makes the panel a structurally safe storage.
/// </summary>
public sealed class LifeShardQuickHealPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!triggersSet.QuickHeal)
			return;

		LifeShardSettings cfg = ElementalHeartsServerConfig.Instance.LifeShards;
		if (!cfg.SystemEnabled || !cfg.ShardsAreConsumable)
			return;

		// Shards share a cooldown with each other but not with vanilla potions.
		if (Player.HasBuff(ModContent.BuffType<ShardSickness>()))
			return;

		// Consume AFTER regular potions: only use if already potion sick.
		if (!Player.HasBuff(BuffID.PotionSickness))
			return;

		int missing = Player.statLifeMax2 - Player.statLife;
		if (missing <= 0)
			return;

		if (!TryFindShard(missing, out int slotIndex, out int healAmount))
			return;

		ConsumeShard(slotIndex, healAmount, cfg.ShardSicknessSeconds);

		// Suppress vanilla quick-heal for this press so a healing potion isn't also
		// consumed on the same frame. ProcessTriggers runs before vanilla's controlQuickHeal
		// check in UpdateInput, so clearing both is enough.
		triggersSet.QuickHeal = false;
		Player.controlQuickHeal = false;
	}

	/// <summary>
	/// Picks the shard in inventory whose heal best matches <paramref name="missing"/> HP:
	/// the smallest stack whose heal value fully covers the missing amount, or — if no
	/// shard can fully cover it — the largest heal available. Mirrors vanilla's
	/// <c>QuickHeal_GetItemToUse</c> selection but limited to Life Shards.
	/// </summary>
	private bool TryFindShard(int missing, out int slotIndex, out int healAmount)
	{
		slotIndex = -1;
		healAmount = 0;

		int bestCoverIndex = -1;
		int bestCoverHeal = int.MaxValue;
		int bestFallbackIndex = -1;
		int bestFallbackHeal = 0;

		for (int i = 0; i < Player.inventory.Length; i++)
		{
			Item item = Player.inventory[i];
			if (item.IsAir || item.ModItem is not LifeShardItem shard)
				continue;

			int heal = shard.Tier.GetHealAmount();
			if (heal <= 0)
				continue;

			if (heal >= missing)
			{
				// Covers the missing HP — prefer the smallest such shard.
				if (heal < bestCoverHeal)
				{
					bestCoverHeal = heal;
					bestCoverIndex = i;
				}
			}
			else if (heal > bestFallbackHeal)
			{
				// Doesn't fully cover — track the largest as fallback.
				bestFallbackHeal = heal;
				bestFallbackIndex = i;
			}
		}

		if (bestCoverIndex >= 0)
		{
			slotIndex = bestCoverIndex;
			healAmount = bestCoverHeal;
			return true;
		}

		if (bestFallbackIndex >= 0)
		{
			slotIndex = bestFallbackIndex;
			healAmount = bestFallbackHeal;
			return true;
		}

		return false;
	}

	private void ConsumeShard(int slotIndex, int healAmount, int sicknessSeconds)
	{
		Item slot = Player.inventory[slotIndex];

		slot.stack--;
		if (slot.stack <= 0)
			slot.TurnToAir();

		Player.statLife += healAmount;
		if (Player.statLife > Player.statLifeMax2)
			Player.statLife = Player.statLifeMax2;

		Player.HealEffect(healAmount);
		Player.AddBuff(ModContent.BuffType<ShardSickness>(), sicknessSeconds * 60);
	}
}
