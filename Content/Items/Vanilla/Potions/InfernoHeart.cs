using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class InfernoHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Inferno;
	public override int PotionItemId => ItemID.InfernoPotion;
	public override string PermanentEffectText => "Permanently surrounds you with a ring of fire that damages nearby enemies";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
