using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class HellstoneHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.HellstoneBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 2)
			.AddTile(TileID.Hellforge)
			.Register();
	}
}

