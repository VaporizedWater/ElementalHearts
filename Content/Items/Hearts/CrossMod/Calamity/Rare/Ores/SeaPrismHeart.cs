using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Rare.Ores;

public sealed class SeaPrismHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("SeaPrism", 20, TileID.MythrilAnvil, ModContent.ItemType<RareLifeShard>(), 1);
}
