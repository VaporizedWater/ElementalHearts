using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Legendary;

public sealed class HeavenforgeHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HeavenforgeBrick, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<LegendaryLifeShard>(), 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
	}
}
