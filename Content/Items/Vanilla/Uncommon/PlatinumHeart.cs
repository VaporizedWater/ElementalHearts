using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class PlatinumHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PlatinumBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
	}
}

