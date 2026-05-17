using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class ShroomiteHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.ShroomiteBar, 25)
			.AddTile(TileID.Autohammer)
			.Register();
	}
}
