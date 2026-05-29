using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class GillsHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Gills;
	public override int PotionItemId => ItemID.GillsPotion;
	public override string PermanentEffectText => "Permanently allows you to breathe underwater";
}
