using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class CursedFlameHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.CursedFlame, RecipeCost(150))
			.AddIngredient(ModContent.ItemType<RareLifeShard>(), 3)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
