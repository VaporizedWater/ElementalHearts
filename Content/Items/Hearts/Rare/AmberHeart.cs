using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class AmberHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Amber, 20)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}
