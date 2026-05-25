using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class HallowedHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HallowedBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

