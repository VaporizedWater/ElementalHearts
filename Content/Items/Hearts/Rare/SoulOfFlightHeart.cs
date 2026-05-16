using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class SoulOfFlightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofFlight, 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
