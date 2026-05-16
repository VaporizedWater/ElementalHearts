using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class UelibloomHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("UelibloomOre", 20, TileID.LunarCraftingStation);
}
