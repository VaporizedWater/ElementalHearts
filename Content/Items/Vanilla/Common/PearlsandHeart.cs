using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class PearlsandHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PearlsandBlock, RecipeCost(1000))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 4)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

