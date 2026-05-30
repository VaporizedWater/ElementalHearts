using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class WrathHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Wrath;
	public override int PotionItemId => ItemID.WrathPotion;
	public override string PermanentEffectText => "Permanently increases damage by 10%";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
