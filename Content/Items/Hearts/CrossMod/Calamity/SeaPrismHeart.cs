using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class SeaPrismHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("SeaPrism", 20, TileID.MythrilAnvil);
}
