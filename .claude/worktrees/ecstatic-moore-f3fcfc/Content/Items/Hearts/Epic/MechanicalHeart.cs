using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Epic;

public sealed class MechanicalHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFright, 20)
			.AddIngredient(ItemID.SoulofMight, 20)
			.AddIngredient(ItemID.SoulofSight, 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
