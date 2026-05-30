using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class FossilHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.DesertFossil, RecipeCost(300))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 4)
			.AddTile(TileID.Extractinator)
			.Register();
	}
}

