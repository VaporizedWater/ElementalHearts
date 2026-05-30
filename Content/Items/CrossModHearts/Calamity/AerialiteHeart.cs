using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Calamity;

public sealed class AerialiteHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void AddRecipes() =>
		RegisterModRecipe("AerialiteOre", 50, TileID.Anvils, ModContent.ItemType<UncommonLifeShard>(), 4);
}
