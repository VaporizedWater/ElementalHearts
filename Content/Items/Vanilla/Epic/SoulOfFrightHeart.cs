using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Epic;

public sealed class SoulOfFrightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	protected override int AnimationFrameCount => 4;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFright, RecipeCost(80))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}


