using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class DiscordHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PixieDust, RecipeCost(300))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 3)
			.AddTile(TileID.CrystalBall)
			.Register();
	}
}

