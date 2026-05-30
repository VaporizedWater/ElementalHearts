using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class LoveHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Legendary;
	public override int BuffType => 0;
	public override int PotionItemId => ItemID.LovePotion;
	public override int PotionsForTwoHours => 3;
	public override int ShardCost => 1;
}
