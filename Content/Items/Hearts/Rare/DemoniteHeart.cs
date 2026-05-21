using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class DemoniteHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.DemoniteBar, RecipeCost(150))
			.AddIngredient(ModContent.ItemType<RareLifeShard>(), 2)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
