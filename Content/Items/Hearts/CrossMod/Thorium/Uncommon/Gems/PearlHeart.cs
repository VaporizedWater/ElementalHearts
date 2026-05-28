using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium.Uncommon.Gems;

public sealed class PearlHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes() =>
		RegisterModRecipe("Pearl", 30, TileID.HeavyWorkBench, ModContent.ItemType<UncommonLifeShard>(), 4);
}
