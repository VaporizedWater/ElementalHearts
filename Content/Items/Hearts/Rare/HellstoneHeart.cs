using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class HellstoneHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HellstoneBar, 20)
			.AddTile(TileID.Hellforge)
			.Register();
	}
}
