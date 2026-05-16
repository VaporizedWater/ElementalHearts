using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class TungstenHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.TungstenBar, 30)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
