using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Exotic.Materials;

public sealed class CosmiliteHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public override void AddRecipes() =>
		RegisterModRecipe("CosmiliteBar", 10, TileID.LunarCraftingStation);
}
