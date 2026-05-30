using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class MiningHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Mining;
	public override int PotionItemId => ItemID.MiningPotion;
	public override string PermanentEffectText => "Permanently increases mining speed by 25%";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 2;
}
