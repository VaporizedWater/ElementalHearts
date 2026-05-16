using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class MagmaHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("MagmaOre", 20, TileID.Hellforge);
}
