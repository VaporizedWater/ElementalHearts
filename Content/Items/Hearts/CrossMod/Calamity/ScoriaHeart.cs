using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class ScoriaHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("ScoriaBar", 15, TileID.AdamantiteForge);
}
