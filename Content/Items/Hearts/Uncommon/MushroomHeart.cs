using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class MushroomHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Mushroom, RecipeCost(20))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 1)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}

