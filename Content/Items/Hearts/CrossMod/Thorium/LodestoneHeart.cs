using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class LodestoneHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("LodestoneOre", 20, TileID.MythrilAnvil);
}
