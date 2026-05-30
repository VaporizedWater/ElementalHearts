using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class SpelunkerHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Spelunker;
	public override int PotionItemId => ItemID.SpelunkerPotion;
	public override string PermanentEffectText => "Permanently highlights nearby treasure";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 2;
}
