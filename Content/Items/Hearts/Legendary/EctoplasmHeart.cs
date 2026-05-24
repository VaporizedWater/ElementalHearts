using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Legendary;

public sealed class EctoplasmHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Ectoplasm, RecipeCost(100))
			.AddIngredient(ModContent.ItemType<LegendaryLifeShard>(), 1)
			.AddTile(TileID.Bookcases)
			.Register();
	}
}
