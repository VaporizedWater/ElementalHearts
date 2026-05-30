using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class WaterWalkingHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.WaterWalking;
	public override int PotionItemId => ItemID.WaterWalkingPotion;
	public override string PermanentEffectText => "Permanently allows you to walk on water";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 1;
}
