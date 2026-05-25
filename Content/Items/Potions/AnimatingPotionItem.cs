using System.Collections.Generic;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Potions;

/// <summary>
/// Base class for the five Animating Potions. A concrete potion only declares its
/// <see cref="Tier"/>; defaults, the tier-specific recipe and the mutually-exclusive buff
/// application all flow from here. Drinking a potion clears every Animating Potion buff
/// before applying its own, so only one tier's buff is ever active at a time.
/// </summary>
public abstract class AnimatingPotionItem : ModItem
{
	public abstract LifeShardTier Tier { get; }

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 30;
	}

	public override void SetDefaults()
	{
		Item.width = 18;
		Item.height = 28;
		Item.maxStack = Item.CommonMaxStack;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
		Item.useAnimation = 17;
		Item.useTime = 17;
		Item.useTurn = true;
		Item.UseSound = SoundID.Item3;
		Item.consumable = true;
		Item.rare = Tier.GetRarityType();
		Item.value = Item.sellPrice(silver: ((int)Tier + 1) * 8);

		// buffType + buffTime make this a standard buff potion: Quick Buff picks it up, and
		// a normal drink applies the buff. Mutual exclusion between the five tiers is then
		// enforced in HeartConsumptionPlayer.PostUpdateBuffs, however the buff was applied.
		Item.buffType = AnimatingPotion.GetBuffType(Tier);
		Item.buffTime = AnimatingPotion.BuffDuration;
	}

	/// <summary>
	/// Every tier shares the name "Animating Potion", so the name's colour is the only tier
	/// indicator — colour it from the tier's rarity ladder, matching the Life Shards.
	/// </summary>
	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		foreach (TooltipLine line in tooltips)
		{
			if (line.Mod == "Terraria" && line.Name == "ItemName")
				line.OverrideColor = Tier.GetTextColor();
		}
	}

	/// <summary>
	/// Each tier brews from a water/herb base plus a Life Shard of the matching tier; the
	/// higher tiers fold the previous potion in, so the line is crafted in sequence. Brewed
	/// at a Bottle (or Alchemy Table) like any vanilla potion.
	/// </summary>
	public override void AddRecipes()
	{
		Recipe recipe = CreateRecipe();

		switch (Tier)
		{
			case LifeShardTier.Common:
				recipe.AddIngredient(ItemID.BottledWater)
					.AddIngredient(ItemID.Daybloom)
					.AddIngredient(ItemID.Shiverthorn);
				break;
			case LifeShardTier.Uncommon:
				recipe.AddIngredient(AnimatingPotion.GetItemType(LifeShardTier.Common))
					.AddIngredient(ItemID.Waterleaf)
					.AddIngredient(ItemID.Moonglow);
				break;
			case LifeShardTier.Rare:
				recipe.AddIngredient(AnimatingPotion.GetItemType(LifeShardTier.Uncommon))
					.AddIngredient(ItemID.Moonglow)
					.AddIngredient(ItemID.Blinkroot);
				break;
			case LifeShardTier.Epic:
				recipe.AddIngredient(AnimatingPotion.GetItemType(LifeShardTier.Rare))
					.AddIngredient(ItemID.Fireblossom)
					.AddIngredient(ItemID.Prismite);
				break;
			case LifeShardTier.Legendary:
				recipe.AddIngredient(AnimatingPotion.GetItemType(LifeShardTier.Epic))
					.AddIngredient(ItemID.PrincessFish)
					.AddIngredient(ItemID.BottledHoney);
				break;
		}

		// The Life Shard of the matching tier — the same tier value indexes both ladders.
		recipe.AddOptionalIngredient(Tier.GetItemType(), 1)
			.AddTile(TileID.Bottles)
			.Register();
	}
}

