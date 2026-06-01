using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class LuckHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	public override int BuffType => BuffID.Lucky;
	public override int PotionItemId => ItemID.LuckPotionGreater;
	public override int PotionsForTwoHours => 8;
	public override int ShardCost => 1;
}
