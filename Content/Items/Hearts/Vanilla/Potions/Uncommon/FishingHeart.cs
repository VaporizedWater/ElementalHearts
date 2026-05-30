using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class FishingHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Fishing;
	public override int PotionItemId => ItemID.FishingPotion;
	public override string PermanentEffectText => "Permanently increases fishing power by 15%";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
