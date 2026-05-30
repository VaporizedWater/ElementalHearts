using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class SilverHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SilverBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 3)
			.AddTile(TileID.Anvils)
			.Register();
	}
}

