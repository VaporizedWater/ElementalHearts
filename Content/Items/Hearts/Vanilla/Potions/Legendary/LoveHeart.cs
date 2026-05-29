using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Legendary;

public sealed class LoveHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;
	public override int BuffType => 0;
	public override int PotionItemId => ItemID.LovePotion;
}
