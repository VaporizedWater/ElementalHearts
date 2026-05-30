using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class ManaRegenerationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.ManaRegeneration;
	public override int PotionItemId => ItemID.ManaRegenerationPotion;
	public override string PermanentEffectText => "Permanently increases mana regeneration";
	public override int PotionsForTwoHours => 18;
	public override int ShardCost => 3;
}
