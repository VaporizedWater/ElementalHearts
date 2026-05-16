using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class ThoriumHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("ThoriumBar", 15, TileID.AdamantiteForge);
}
