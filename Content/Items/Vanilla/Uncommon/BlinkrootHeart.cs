using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ID;
using Terraria.ModLoader;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class BlinkrootHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	protected override int AnimationFrameCount => 10;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Blinkroot, RecipeCost(20))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 1)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}


