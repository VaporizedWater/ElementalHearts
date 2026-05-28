using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Uncommon.Dyes;

public sealed class EnchantedHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.FallenStar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 4)
			.AddTile(TileID.CrystalBall)
			.Register();
	}
}

