using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class ArcheryHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Archery;
	public override int PotionItemId => ItemID.ArcheryPotion;
	public override string PermanentEffectText => "Permanently increases arrow damage and velocity";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
