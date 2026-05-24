using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class HayHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Hay, RecipeCost(2000))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 2)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
