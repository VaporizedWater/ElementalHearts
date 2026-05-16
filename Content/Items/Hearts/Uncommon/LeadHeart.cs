using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class LeadHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.LeadBar, 30)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
