using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Epic;

public sealed class MechanicalHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFright, RecipeCost(40))
			.AddIngredient(ItemID.SoulofMight, RecipeCost(40))
			.AddIngredient(ItemID.SoulofSight, RecipeCost(40))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

