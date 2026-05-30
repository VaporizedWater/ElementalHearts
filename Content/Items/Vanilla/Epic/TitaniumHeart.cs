using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Epic;

public sealed class TitaniumHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.TitaniumBar, RecipeCost(100))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 2)
			.AddTile(TileID.AdamantiteForge)
			.Register();
	}
}

