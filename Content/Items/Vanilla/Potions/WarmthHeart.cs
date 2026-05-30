using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class WarmthHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Warmth;
	public override int PotionItemId => ItemID.WarmthPotion;
	public override string PermanentEffectText => "Permanently reduces damage taken from cold sources";
	public override int PotionsForTwoHours => 18;
	public override int ShardCost => 1;
}
