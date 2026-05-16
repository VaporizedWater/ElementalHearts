using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class CactusHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Cactus, 50)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
