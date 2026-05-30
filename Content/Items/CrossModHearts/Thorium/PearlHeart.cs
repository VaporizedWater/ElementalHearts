using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Thorium;

public sealed class PearlHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes() =>
		RegisterModRecipe("Pearl", 30, TileID.HeavyWorkBench, ModContent.ItemType<UncommonLifeShard>(), 4);
}
