using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class FeatherfallHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Featherfall;
	public override int PotionItemId => ItemID.FeatherfallPotion;
	public override string PermanentEffectText => "Permanently slows your falling speed";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 1;
}
