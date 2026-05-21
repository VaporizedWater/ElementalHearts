using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class PinkIceHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PinkIceBlock, RecipeCost(800))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 4)
			.AddTile(TileID.IceMachine)
			.Register();
	}
}
