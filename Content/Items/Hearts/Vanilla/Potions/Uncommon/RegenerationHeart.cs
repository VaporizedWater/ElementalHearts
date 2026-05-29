using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class RegenerationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Regeneration;
	public override int PotionItemId => ItemID.RegenerationPotion;
	public override string PermanentEffectText => "Permanently increases life regeneration";
}
