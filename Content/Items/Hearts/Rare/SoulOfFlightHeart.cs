using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class SoulOfFlightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFlight, RecipeCost(80))
			.AddIngredient(ModContent.ItemType<RareLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
