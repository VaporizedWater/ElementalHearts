using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class CalmingHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Calm;
	public override int PotionItemId => ItemID.CalmingPotion;
	public override string PermanentEffectText => "Permanently reduces enemy spawn rate";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
