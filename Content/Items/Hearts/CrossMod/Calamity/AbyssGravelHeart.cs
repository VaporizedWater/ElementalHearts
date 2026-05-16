using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class AbyssGravelHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("AbyssGravel", 50, TileID.WorkBenches);
}
