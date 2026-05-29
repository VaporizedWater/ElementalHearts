using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class WrathHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Wrath;
	public override int PotionItemId => ItemID.WrathPotion;
	public override string PermanentEffectText => "Permanently increases damage by 10%";
}
