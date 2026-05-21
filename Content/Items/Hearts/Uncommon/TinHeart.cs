using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class TinHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.TinBar, RecipeCost(100))
			.AddIngredient(ModContent.ItemType<UncommonLifeShard>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
