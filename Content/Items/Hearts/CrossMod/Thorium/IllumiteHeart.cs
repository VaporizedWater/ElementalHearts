using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.CrossMod.Thorium;

public sealed class IllumiteHeart : ThoriumHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void AddRecipes() =>
		RegisterModRecipe("IllumiteBar", 20, TileID.MythrilAnvil);
}
