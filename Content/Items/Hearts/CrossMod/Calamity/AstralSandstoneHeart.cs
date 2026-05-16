using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class AstralSandstoneHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("AstralSandstone", 50, TileID.WorkBenches);
}
