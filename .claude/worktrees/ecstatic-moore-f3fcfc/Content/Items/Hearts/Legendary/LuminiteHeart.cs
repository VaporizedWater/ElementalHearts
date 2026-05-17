using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class LuminiteHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.LunarBar, 20)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
	}
}
