using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class ManaRegenerationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.ManaRegeneration;
	public override int PotionItemId => ItemID.ManaRegenerationPotion;
	public override string PermanentEffectText => "Permanently increases mana regeneration";
}
