using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class EbonstoneHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.EbonstoneBlock, RecipeCost(1000))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 3)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
