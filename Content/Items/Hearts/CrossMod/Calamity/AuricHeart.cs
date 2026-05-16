using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class AuricHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("AuricOre", 10, TileID.LunarCraftingStation);
}
