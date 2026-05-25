using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class ShardHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.CrystalShard, RecipeCost(200))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

