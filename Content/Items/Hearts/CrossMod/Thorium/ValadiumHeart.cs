using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class ValadiumHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("ValadiumBar", 10, TileID.LunarCraftingStation);
}
