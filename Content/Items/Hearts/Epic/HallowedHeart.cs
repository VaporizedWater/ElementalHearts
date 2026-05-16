using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class HallowedHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HallowedBar, 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
