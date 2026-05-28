using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Calamity.Legendary.PostMoonLord;

public sealed class TenebrisHeart : CalamityHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("Tenebris", 50, TileID.LunarCraftingStation, ModContent.ItemType<LegendaryLifeShard>(), 1);
}
