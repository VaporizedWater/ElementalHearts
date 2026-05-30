using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class MiningHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Mining;
	public override int PotionItemId => ItemID.MiningPotion;
	public override string PermanentEffectText => "Permanently increases mining speed by 25%";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 2;
}
