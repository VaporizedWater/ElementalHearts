using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Rare;

public sealed class SoulOfNightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofNight, 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}
