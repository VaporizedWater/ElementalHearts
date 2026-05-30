using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class PumpkinHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Pumpkin, RecipeCost(500))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 2)
			.AddTile(TileID.Sawmill)
			.Register();
	}
}

