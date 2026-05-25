using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class MercuryHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.MercuryBrick, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<LegendaryLifeShard>(), 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
	}
}
