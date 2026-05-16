using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class PurpleIceHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PurpleIceBlock, 50)
			.AddTile(TileID.IceMachine)
			.Register();
	}
}
