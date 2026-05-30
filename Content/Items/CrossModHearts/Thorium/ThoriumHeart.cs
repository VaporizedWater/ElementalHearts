using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Thorium;

public sealed class ThoriumHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("ThoriumBar", 15, TileID.AdamantiteForge, ModContent.ItemType<EpicLifeShard>(), 1);
}
