using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class HeartreachHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Heartreach;
	public override int PotionItemId => ItemID.HeartreachPotion;
	public override string PermanentEffectText => "Permanently increases heart pickup range";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 3;
}
