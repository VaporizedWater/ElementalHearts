using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class RainbowHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.RainbowBrick, RecipeCost(50))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 5)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

