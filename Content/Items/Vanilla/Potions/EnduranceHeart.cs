using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class EnduranceHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	public override int BuffType => BuffID.Endurance;
	public override int PotionItemId => ItemID.EndurancePotion;
	public override string PermanentEffectText => "Permanently reduces damage taken by 10%";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 1;
}
