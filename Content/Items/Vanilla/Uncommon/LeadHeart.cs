using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class LeadHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.LeadBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 2)
			.AddTile(TileID.Anvils)
			.Register();
	}
}

