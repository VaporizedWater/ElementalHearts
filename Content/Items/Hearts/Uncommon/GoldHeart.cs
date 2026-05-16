using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class GoldHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.GoldBar, 30)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
