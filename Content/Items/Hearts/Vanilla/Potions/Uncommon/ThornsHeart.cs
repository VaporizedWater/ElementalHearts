using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class ThornsHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Thorns;
	public override int PotionItemId => ItemID.ThornsPotion;
	public override string PermanentEffectText => "Permanently reflects melee damage back at attackers";
}
