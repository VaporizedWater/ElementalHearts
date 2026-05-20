using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class EctoplasmHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Ectoplasm, RecipeCost(100))
			.AddTile(TileID.Bookcases)
			.Register();
	}
}
