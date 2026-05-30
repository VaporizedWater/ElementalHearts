using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Rare;

public sealed class DemoniteHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.DemoniteBar, RecipeCost(150))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 2)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}

