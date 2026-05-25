using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class RubyHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Ruby, RecipeCost(40))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 3)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}

