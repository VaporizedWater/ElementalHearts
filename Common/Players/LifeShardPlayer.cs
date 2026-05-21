using System;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Holds this character's Life Shard slots — one dedicated slot per tier, kept entirely
/// out of the regular inventory. Shards picked up in the world are routed here by
/// <see cref="LifeShardItem.OnPickup"/>; a slot counts as "unlocked" (and is drawn by the
/// shard panel) for as long as it holds shards, and disappears once emptied.
/// </summary>
public sealed class LifeShardPlayer : ModPlayer
{
	/// <summary>One slot per <see cref="LifeShards.LifeShardTier"/>, indexed by its integer value.</summary>
	public const int SlotCount = 5;

	/// <summary>Backing items for the five shard slots. An air slot is empty and locked.</summary>
	public Item[] Shards { get; private set; }

	public override void Initialize()
	{
		Shards = new Item[SlotCount];
		for (int i = 0; i < SlotCount; i++)
			Shards[i] = new Item();
	}

	/// <summary>
	/// Merges a picked-up shard stack into its tier slot. Returns true when the whole
	/// incoming stack was absorbed; false leaves <paramref name="incoming"/>'s remainder
	/// for normal pickup (only possible if a slot is somehow at max stack).
	/// </summary>
	public bool AbsorbShards(Item incoming)
	{
		if (incoming.ModItem is not LifeShardItem shard)
			return false;

		int index = (int)shard.Tier;
		Item slot = Shards[index];

		if (slot.IsAir)
		{
			Shards[index] = incoming.Clone();
			incoming.stack = 0;
			return true;
		}

		if (slot.type != incoming.type)
			return false;

		int space = slot.maxStack - slot.stack;
		if (space <= 0)
			return false;

		int moved = Math.Min(space, incoming.stack);
		slot.stack += moved;
		incoming.stack -= moved;
		return incoming.stack <= 0;
	}

	/// <summary>
	/// True when the slot for <paramref name="lowerTier"/> holds enough shards to combine
	/// upward into one shard of the next tier.
	/// </summary>
	public bool CanCombine(int lowerTier)
	{
		if (lowerTier < 0 || lowerTier >= SlotCount - 1)
			return false;

		int cost = ((LifeShardTier)(lowerTier + 1)).GetUpgradeCost();
		return cost > 0 && Shards[lowerTier].stack >= cost;
	}

	/// <summary>
	/// Consumes the required number of <paramref name="lowerTier"/> shards from their slot
	/// and adds one shard of the next tier up to its slot. Returns false if there aren't
	/// enough shards to combine.
	/// </summary>
	public bool TryCombine(int lowerTier)
	{
		if (!CanCombine(lowerTier))
			return false;

		int resultIndex = lowerTier + 1;
		var resultTier = (LifeShardTier)resultIndex;

		Shards[lowerTier].stack -= resultTier.GetUpgradeCost();
		if (Shards[lowerTier].stack <= 0)
			Shards[lowerTier].TurnToAir();

		if (Shards[resultIndex].IsAir)
		{
			var result = new Item();
			result.SetDefaults(resultTier.GetItemType());
			Shards[resultIndex] = result;
		}
		else
		{
			Shards[resultIndex].stack++;
		}

		return true;
	}

	public override void SaveData(TagCompound tag)
	{
		for (int i = 0; i < SlotCount; i++)
		{
			if (!Shards[i].IsAir)
				tag[SlotKey(i)] = Shards[i];
		}
	}

	public override void LoadData(TagCompound tag)
	{
		for (int i = 0; i < SlotCount; i++)
		{
			if (tag.ContainsKey(SlotKey(i)))
				Shards[i] = tag.Get<Item>(SlotKey(i));
		}
	}

	private static string SlotKey(int index) => $"shardSlot{index}";
}
