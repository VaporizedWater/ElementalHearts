using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.UI;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.LifeShards;

/// <summary>
/// Base class for every Life Shard. Concrete shards only override <see cref="Tier"/>;
/// defaults and rarity are handled here. Life Shards are pure crafting materials — they
/// grant nothing on use. Shards never enter the regular inventory: picked-up shards are
/// routed into the per-tier slots on <see cref="LifeShardPlayer"/>, and combining happens
/// via the shard panel. Each combine is also registered as a recipe so that a shard
/// dropped into Shimmer decrafts back into its lower-tier shards with the normal Shimmer
/// float-up effect.
/// </summary>
public abstract class LifeShardItem : ModItem
{
	public abstract LifeShardTier Tier { get; }

	public override bool IsLoadingEnabled(Mod mod) => ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled;

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 25;
	}

	public override void SetDefaults()
	{
		Item.width = 22;
		Item.height = 22;
		Item.maxStack = Item.CommonMaxStack;
		Item.rare = Tier.GetRarityType();
		Item.material = true;
		Item.value = Item.sellPrice(silver: ((int)Tier + 1) * 5);
	}

	/// <summary>
	/// Registers the combine recipe for this tier (five Common make an Uncommon, four
	/// Uncommon a Rare, and so on). The recipe is what lets a shard decraft in Shimmer
	/// with the normal float-up effect; it never clutters the crafting menu, because
	/// shards live in the shard panel and never the inventory the menu reads from. The
	/// system toggle gates it so disabling the system disables shard crafting too.
	/// </summary>
	public override void AddRecipes()
	{
		if (!Tier.TryGetLowerTier(out LifeShardTier lower))
			return;

		CreateRecipe()
			.AddIngredient(lower.GetItemType(), Tier.GetUpgradeCost())
			.AddCondition(LifeShardSystem.UIUpgradeOnlyCondition)
			.Register();
	}

	/// <summary>
	/// Routes a picked-up shard straight into its dedicated slot on
	/// <see cref="LifeShardPlayer"/> instead of the regular inventory. Any remainder that
	/// doesn't fit falls through to normal pickup.
	/// </summary>
	public override bool OnPickup(Player player)
	{
		if (!ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled)
			return true;

		// Queue the tier's cue rather than playing it now: several shards grabbed on the
		// same frame collapse to a single sound (the last) instead of overlapping.
		LifeShardSystem.QueuePickupSound(Tier);

		int stackBefore = Item.stack;
		bool absorbedFully = player.GetModPlayer<LifeShardPlayer>().AbsorbShards(Item);
		int absorbed = stackBefore - Item.stack;

		// Vanilla's PickupItem spawns the floating "Item Name" popup near the player when
		// an item lands in the inventory. Returning false here skips that path, so we
		// reproduce it for the portion that was routed into the shard slot.
		if (absorbed > 0)
			PopupText.NewText(PopupTextContext.RegularItemPickup, Item, absorbed);

		return !absorbedFully;
	}

	/// <summary>
	/// Every tier shares the name "Life Shard", so the name's colour is the only tier
	/// indicator — colour it from the tier's rarity ladder.
	/// </summary>
	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		foreach (TooltipLine line in tooltips)
		{
			if (line.Mod == "Terraria" && line.Name == "ItemName")
				line.OverrideColor = Tier.GetTextColor();
		}

		if (!LifeShardPanel.IsHoveringShardSlot)
		{
			int healAmount = Tier.GetHealAmount();
			if (healAmount > 0)
			{
				TooltipLine healTooltip = new TooltipLine(Mod, "HealLife", $"Restores {healAmount} life");
				tooltips.Add(healTooltip);
			}
		}
	}
}
