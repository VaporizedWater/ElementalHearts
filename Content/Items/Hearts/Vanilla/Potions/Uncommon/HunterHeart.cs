using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class HunterHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Hunter;
	public override int PotionItemId => ItemID.HunterPotion;
	public override string PermanentEffectText => "Permanently highlights nearby enemies";
}
