using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class NightOwlHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.NightOwl;
	public override int PotionItemId => ItemID.NightOwlPotion;
	public override string PermanentEffectText => "Permanently improves vision at night";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
