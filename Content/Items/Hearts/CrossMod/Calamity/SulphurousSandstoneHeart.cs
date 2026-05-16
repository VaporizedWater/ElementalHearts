using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class SulphurousSandstoneHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("SulphurousSandstone", 50, TileID.WorkBenches);
}
