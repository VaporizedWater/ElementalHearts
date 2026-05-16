using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class SpectreHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SpectreBar, 10)
			.AddTile(TileID.AdamantiteForge)
			.Register();
	}
}
