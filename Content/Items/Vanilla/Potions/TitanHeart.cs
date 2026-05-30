using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class TitanHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Titan;
	public override int PotionItemId => ItemID.TitanPotion;
	public override string PermanentEffectText => "Permanently increases knockback";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
