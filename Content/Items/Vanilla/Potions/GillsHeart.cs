using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class GillsHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Gills;
	public override int PotionItemId => ItemID.GillsPotion;
	public override string PermanentEffectText => "Permanently allows you to breathe underwater";
	public override int PotionsForTwoHours => 40;
	public override int ShardCost => 1;
}
