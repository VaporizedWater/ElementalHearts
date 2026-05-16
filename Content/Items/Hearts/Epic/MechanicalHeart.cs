using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class MechanicalHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFright, 5)
			.AddIngredient(ItemID.SoulofMight, 5)
			.AddIngredient(ItemID.SoulofSight, 5)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
