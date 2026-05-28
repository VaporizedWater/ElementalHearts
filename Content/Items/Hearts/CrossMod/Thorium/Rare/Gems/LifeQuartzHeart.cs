using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium.Rare.Gems;

public sealed class LifeQuartzHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("LifeQuartz", 20, TileID.CrystalBall, ModContent.ItemType<RareLifeShard>(), 2);
}
