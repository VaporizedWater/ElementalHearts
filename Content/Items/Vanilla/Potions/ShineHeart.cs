using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class ShineHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Shine;
	public override int PotionItemId => ItemID.ShinePotion;
	public override string PermanentEffectText => "Permanently emits light around you";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
