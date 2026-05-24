using System;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ElementalHearts.Common.Systems;

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
	/// for normal pickup when the slot can't fit all of it.
	/// </summary>
	public bool AbsorbShards(Item incoming)
	{
		if (incoming.ModItem is not LifeShardItem shard)
			return false;

		int index = (int)shard.Tier;
		Item slot = Shards[index];

		if (slot.IsAir)
		{
			// Clone into the empty slot, but never above the shard's max stack: an
			// oversized incoming stack fills the slot to the cap and leaves the rest
			// of its stack for normal pickup, so a slot can't exceed max stack.
			Item clone = incoming.Clone();
			int kept = Math.Min(clone.maxStack, incoming.stack);
			clone.stack = kept;
			Shards[index] = clone;
			incoming.stack -= kept;
			return incoming.stack <= 0;
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
	/// True when the <paramref name="fromTier"/> slot holds enough shards to craft one shard
	/// of the strictly-higher <paramref name="toTier"/> directly — skipping the tiers in
	/// between — and the destination slot has room for it.
	/// </summary>
	public bool CanUpgrade(int fromTier, int toTier)
	{
		if (fromTier < 0 || toTier <= fromTier || toTier >= SlotCount)
			return false;

		if (toTier > Systems.AnimateProgressionSystem.UnlockedTier)
			return false;

		int cost = ((LifeShardTier)fromTier).GetUpgradeCost((LifeShardTier)toTier);
		if (cost <= 0 || Shards[fromTier].stack < cost)
			return false;

		Item destination = Shards[toTier];
		return destination.IsAir || destination.stack < destination.maxStack;
	}

	/// <summary>
	/// Consumes the shards needed to craft one <paramref name="toTier"/> shard directly from
	/// the <paramref name="fromTier"/> slot — skipping intermediate tiers — and adds the
	/// result to the destination slot. Returns false if the upgrade can't be afforded.
	/// </summary>
	public bool TryUpgrade(int fromTier, int toTier)
	{
		if (!CanUpgrade(fromTier, toTier))
			return false;

		int cost = ((LifeShardTier)fromTier).GetUpgradeCost((LifeShardTier)toTier);

		Shards[fromTier].stack -= cost;
		if (Shards[fromTier].stack <= 0)
			Shards[fromTier].TurnToAir();

		if (Shards[toTier].IsAir)
		{
			var result = new Item();
			result.SetDefaults(((LifeShardTier)toTier).GetItemType());
			Shards[toTier] = result;
		}
		else
		{
			Shards[toTier].stack++;
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

	public override System.Collections.Generic.IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
	{
		itemConsumedCallback = (item, index) =>
		{
			if (item.stack <= 0)
				item.TurnToAir();
		};
		return Shards;
	}

	private static string SlotKey(int index) => $"shardSlot{index}";
}
