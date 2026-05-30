using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Calamity;

public sealed class HardenedAstralSandHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override void AddRecipes() =>
		RegisterModRecipe("HardenedAstralSand", 50, TileID.WorkBenches, ModContent.ItemType<CommonLifeShard>(), 5);
}
