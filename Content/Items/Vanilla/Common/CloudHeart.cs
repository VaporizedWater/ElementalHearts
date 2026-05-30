using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

public sealed class CloudHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Cloud, RecipeCost(200))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 3)
			.AddTile(TileID.SkyMill)
			.Register();
	}
}

