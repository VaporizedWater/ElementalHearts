using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class SwiftnessHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Swiftness;
	public override int PotionItemId => ItemID.SwiftnessPotion;
	public override string PermanentEffectText => "Permanently increases movement speed by 25%";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 3;
}
