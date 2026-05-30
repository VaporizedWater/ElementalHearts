using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class GlassHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Glass, RecipeCost(500))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 2)
			.AddTile(TileID.GlassKiln)
			.Register();
	}
}

