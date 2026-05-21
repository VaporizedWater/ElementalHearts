using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Common;

public sealed class MarbleHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Marble, RecipeCost(400))
			.AddIngredient(ModContent.ItemType<CommonLifeShard>(), 4)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
