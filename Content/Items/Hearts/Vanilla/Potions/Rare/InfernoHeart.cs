using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class InfernoHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Inferno;
	public override int PotionItemId => ItemID.InfernoPotion;
	public override string PermanentEffectText => "Permanently surrounds you with a ring of fire that damages nearby enemies";
}
