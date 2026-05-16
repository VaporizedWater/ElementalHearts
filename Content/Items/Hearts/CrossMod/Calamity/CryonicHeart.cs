using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity;

public sealed class CryonicHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("CryonicBar", 15, TileID.MythrilAnvil);
}
