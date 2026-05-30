using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class HunterHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Hunter;
	public override int PotionItemId => ItemID.HunterPotion;
	public override string PermanentEffectText => "Permanently highlights nearby enemies";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 2;
}
