using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class IronskinHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Ironskin;
	public override int PotionItemId => ItemID.IronskinPotion;
	public override string PermanentEffectText => "Permanently increases defense by 8";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 3;
}
