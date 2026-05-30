using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class MagicPowerHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.MagicPower;
	public override int PotionItemId => ItemID.MagicPowerPotion;
	public override string PermanentEffectText => "Permanently increases magic damage by 20%";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 3;
}
