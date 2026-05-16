using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class PerennialHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("PerennialBar", 15, TileID.AdamantiteForge);
}
