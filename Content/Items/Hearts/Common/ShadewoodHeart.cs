using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class ShadewoodHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Shadewood, 50)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
