using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Rare.Special;

public sealed class CogHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Cog, RecipeCost(300))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 3)
			.AddTile(TileID.SteampunkBoiler)
			.Register();
	}
}

