using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class TopazHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Topaz, RecipeCost(50))
			.AddIngredient(ModContent.ItemType<UncommonLifeShard>(), 2)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}
