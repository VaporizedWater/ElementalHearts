using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class DepthsRockHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("DepthsRock", 50, TileID.WorkBenches);
}
