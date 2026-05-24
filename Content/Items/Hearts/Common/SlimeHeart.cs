using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class SlimeHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SlimeBlock, RecipeCost(500))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 1)
			.AddTile(TileID.Solidifier)
			.Register();
	}
}
