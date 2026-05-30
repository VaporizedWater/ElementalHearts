using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class RageHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Rage;
	public override int PotionItemId => ItemID.RagePotion;
	public override string PermanentEffectText => "Permanently increases critical strike chance by 10%";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
