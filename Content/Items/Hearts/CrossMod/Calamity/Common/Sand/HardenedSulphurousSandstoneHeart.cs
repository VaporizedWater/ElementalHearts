using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Common.Sand;

public sealed class HardenedSulphurousSandstoneHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("HardenedSulphurousSandstone", 50, TileID.WorkBenches, ModContent.ItemType<CommonLifeShard>(), 4);
}
