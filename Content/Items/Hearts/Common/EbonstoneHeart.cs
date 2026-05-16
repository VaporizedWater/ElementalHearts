using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class EbonstoneHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.EbonstoneBlock, 50)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
