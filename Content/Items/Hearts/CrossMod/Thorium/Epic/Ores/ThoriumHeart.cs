using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium.Epic.Ores;

public sealed class ThoriumHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("ThoriumBar", 15, TileID.AdamantiteForge, ModContent.ItemType<EpicLifeShard>(), 1);
}
