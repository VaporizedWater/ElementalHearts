using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class FeatherfallHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Featherfall;
	public override int PotionItemId => ItemID.FeatherfallPotion;
	public override string PermanentEffectText => "Permanently slows your falling speed";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 1;
}
