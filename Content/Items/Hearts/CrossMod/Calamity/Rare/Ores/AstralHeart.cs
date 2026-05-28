using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Rare.Ores;

public sealed class AstralHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("AstralBar", 20, TileID.MythrilAnvil, ModContent.ItemType<RareLifeShard>(), 3);
}
