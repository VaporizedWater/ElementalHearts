using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class ChlorophyteHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.ChlorophyteBar, 15)
			.AddTile(TileID.AdamantiteForge)
			.Register();
	}
}
