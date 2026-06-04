using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// ModPlayer that manages the state of the Chest Heart active ability. It stores
/// the 10 extra inventory slots, handles saving/loading of those items, makes
/// them available for crafting searches, and drops them if the player dies in Mediumcore.
/// </summary>
public sealed class ChestHeartPlayer : ModPlayer
{
	public bool Enabled;
	public Item[] ExtraInventory { get; private set; } = null!;

	public override void Initialize()
	{
		ExtraInventory = new Item[20];
		for (int i = 0; i < ExtraInventory.Length; i++)
		{
			ExtraInventory[i] = new Item();
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag["Enabled"] = Enabled;
		for (int i = 10; i < 20; i++)
		{
			if (!ExtraInventory[i].IsAir)
			{
				tag[$"extraSlot_{i}"] = ExtraInventory[i];
			}
		}
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = tag.GetBool("Enabled");
		for (int i = 0; i < 20; i++)
		{
			ExtraInventory[i] = new Item();
		}
		for (int i = 10; i < 20; i++)
		{
			string key = $"extraSlot_{i}";
			if (tag.ContainsKey(key))
			{
				ExtraInventory[i] = tag.Get<Item>(key);
			}
		}
	}

	public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
	{
		itemConsumedCallback = (item, index) =>
		{
			if (item.stack <= 0)
				item.TurnToAir();
		};
		return Enabled ? ExtraInventory : Array.Empty<Item>();
	}

	/// <summary>
	/// Tries to absorb <paramref name="item"/> into this player's extra inventory, mimicking
	/// vanilla's stack-merge-first, then empty-slot logic from <c>Player.GetItem</c>.
	/// Returns <see langword="true"/> if the entire stack was consumed (caller should mark the
	/// world item as air); returns <see langword="false"/> if some or all of it remains.
	/// </summary>
	public bool TryPutInExtraInventory(Item item)
	{
		if (!Enabled || item.IsAir)
			return false;

		// Pass 1 — stack into identical items that still have room.
		for (int i = 10; i < 20; i++)
		{
			Item slot = ExtraInventory[i];
			if (!slot.IsAir && slot.type == item.type && slot.stack < slot.maxStack)
			{
				int space = slot.maxStack - slot.stack;
				int take  = Math.Min(space, item.stack);
				slot.stack += take;
				item.stack -= take;
				if (item.stack <= 0)
					return true;
			}
		}

		// Pass 2 — place into the first empty slot.
		for (int i = 10; i < 20; i++)
		{
			if (ExtraInventory[i].IsAir)
			{
				ExtraInventory[i] = item.Clone();
				return true;
			}
		}

		return false; // extra inventory also full — let vanilla drop it
	}

	public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
	{
		// Softcore (Classic) is 0, Mediumcore is 1, Hardcore is 2.
		// If Mediumcore, drop all extra inventory items.
		if (Player.difficulty == 1)
		{
			for (int i = 0; i < ExtraInventory.Length; i++)
			{
				if (!ExtraInventory[i].IsAir)
				{
					Player.QuickSpawnItem(Player.GetSource_Death(), ExtraInventory[i], ExtraInventory[i].stack);
					ExtraInventory[i].TurnToAir();
				}
			}
		}
	}
}
