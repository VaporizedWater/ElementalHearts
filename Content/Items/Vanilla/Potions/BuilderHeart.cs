using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class BuilderHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Builder;
	public override int PotionItemId => ItemID.BuilderPotion;
	public override string PermanentEffectText => "Permanently increases placement speed and range";
	public override int PotionsForTwoHours => 8;
	public override int ShardCost => 1;
}
