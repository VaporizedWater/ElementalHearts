using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class GravitationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Gravitation;
	public override int PotionItemId => ItemID.GravitationPotion;
	public override string PermanentEffectText => "Permanently allows you to control gravity";
	public override int PotionsForTwoHours => 40;
	public override int ShardCost => 1;
}
