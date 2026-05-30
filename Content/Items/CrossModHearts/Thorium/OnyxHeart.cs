using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.CrossModHearts;
namespace ElementalHearts.Content.Items.CrossModHearts.Thorium;

public sealed class OnyxHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("OnyxBar", 10, TileID.AdamantiteForge, ModContent.ItemType<LegendaryLifeShard>(), 1);
}
