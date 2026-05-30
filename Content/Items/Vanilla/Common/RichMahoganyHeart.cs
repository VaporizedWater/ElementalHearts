using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class RichMahoganyHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.RichMahogany, RecipeCost(750))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 1)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

