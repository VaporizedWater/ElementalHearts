using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Epic.Ores;

public sealed class ScoriaHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("ScoriaBar", 15, TileID.AdamantiteForge, ModContent.ItemType<EpicLifeShard>(), 2);
}
