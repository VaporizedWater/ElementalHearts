using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Uncommon;

public sealed class GoldHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.GoldBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
	}
}

