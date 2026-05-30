using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class SpelunkerHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Spelunker;
	public override int PotionItemId => ItemID.SpelunkerPotion;
	public override string PermanentEffectText => "Permanently highlights nearby treasure";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 2;
}
