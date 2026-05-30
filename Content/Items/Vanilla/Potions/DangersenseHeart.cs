using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class DangersenseHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Dangersense;
	public override int PotionItemId => ItemID.TrapsightPotion;
	public override string PermanentEffectText => "Permanently reveals nearby hazardous tiles";
	public override int PotionsForTwoHours => 12;
	public override int ShardCost => 1;
}
