using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class TitaniumHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.TitaniumBar, RecipeCost(100))
			.AddTile(TileID.AdamantiteForge)
			.Register();
	}
}
