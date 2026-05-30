using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class ThornsHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Thorns;
	public override int PotionItemId => ItemID.ThornsPotion;
	public override string PermanentEffectText => "Permanently reflects melee damage back at attackers";
	public override int PotionsForTwoHours => 20;
	public override int ShardCost => 2;
}
