using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class WarmthHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Warmth;
	public override int PotionItemId => ItemID.WarmthPotion;
	public override string PermanentEffectText => "Permanently reduces damage taken from cold sources";
}
