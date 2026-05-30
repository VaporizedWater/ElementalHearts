using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class AmmoReservationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.AmmoReservation;
	public override int PotionItemId => ItemID.AmmoReservationPotion;
	public override string PermanentEffectText => "Permanently grants a 20% chance to not consume ammo";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
