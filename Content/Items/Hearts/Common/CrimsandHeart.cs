using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class CrimsandHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.CrimsandBlock, RecipeCost(800))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
