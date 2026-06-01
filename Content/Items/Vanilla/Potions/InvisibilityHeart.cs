using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class InvisibilityHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Invisibility;
	public override int PotionItemId => ItemID.InvisibilityPotion;
	public override int PotionsForTwoHours => 40;
	public override int ShardCost => 2;
}
