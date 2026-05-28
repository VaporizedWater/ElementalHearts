using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Epic.Ores;

public sealed class CryonicHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void AddRecipes() =>
		RegisterModRecipe("CryonicBar", 15, TileID.MythrilAnvil, ModContent.ItemType<EpicLifeShard>(), 1);
}
