using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class RegenerationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Regeneration;
	public override int PotionItemId => ItemID.RegenerationPotion;
	public override string PermanentEffectText => "Permanently increases life regeneration";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 3;
}
