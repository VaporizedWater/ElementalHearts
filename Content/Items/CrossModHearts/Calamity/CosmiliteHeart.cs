using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Calamity;

public sealed class CosmiliteHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Exotic;

	public override void AddRecipes() =>
		RegisterModRecipe("CosmiliteBar", 10, TileID.LunarCraftingStation);
}
