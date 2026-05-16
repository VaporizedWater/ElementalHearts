using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class EmeraldHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Emerald, 30)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}
