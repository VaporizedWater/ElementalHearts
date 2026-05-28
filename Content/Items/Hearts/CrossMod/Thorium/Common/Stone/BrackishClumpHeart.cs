using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium.Common.Stone;

public sealed class BrackishClumpHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("BrackishClump", 50, TileID.WorkBenches, ModContent.ItemType<CommonLifeShard>(), 5);
}
