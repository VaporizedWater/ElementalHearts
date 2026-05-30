using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class FlipperHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Flipper;
	public override int PotionItemId => ItemID.FlipperPotion;
	public override string PermanentEffectText => "Permanently allows you to swim";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
