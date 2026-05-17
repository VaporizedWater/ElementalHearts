using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class PearlHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes() =>
		RegisterModRecipe("Pearl", 60, TileID.HeavyWorkBench);
}
