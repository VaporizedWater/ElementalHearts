using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class OnyxHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;

	public override void AddRecipes() =>
		RegisterModRecipe("OnyxBar", 10, TileID.AdamantiteForge);
}
