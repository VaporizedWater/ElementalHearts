using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class HardenedSulphurousSandstoneHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("HardenedSulphurousSandstone", 50, TileID.WorkBenches);
}
