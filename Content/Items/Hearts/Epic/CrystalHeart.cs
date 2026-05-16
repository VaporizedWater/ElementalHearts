using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class CrystalHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.CrystalShard, 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
