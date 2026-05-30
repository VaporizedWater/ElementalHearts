using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Rare;

public sealed class DiamondHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Diamond, RecipeCost(30))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 1)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}

