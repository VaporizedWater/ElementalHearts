using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class AerialiteHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes() =>
		RegisterModRecipe("AerialiteOre", 200, TileID.Anvils);
}
