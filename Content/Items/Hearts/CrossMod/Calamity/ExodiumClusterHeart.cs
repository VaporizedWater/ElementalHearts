using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class ExodiumClusterHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("ExodiumCluster", 50, TileID.LunarCraftingStation);
}
