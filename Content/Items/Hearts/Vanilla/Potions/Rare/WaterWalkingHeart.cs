using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class WaterWalkingHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.WaterWalking;
	public override int PotionItemId => ItemID.WaterWalkingPotion;
	public override string PermanentEffectText => "Permanently allows you to walk on water";
}
