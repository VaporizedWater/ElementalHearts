using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class SnowCloudHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SnowCloudBlock, RecipeCost(100))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 3)
			.AddTile(TileID.SkyMill)
			.Register();
	}
}
