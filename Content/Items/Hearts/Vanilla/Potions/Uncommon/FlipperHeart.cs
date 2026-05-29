using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class FlipperHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Flipper;
	public override int PotionItemId => ItemID.FlipperPotion;
	public override string PermanentEffectText => "Permanently allows you to swim";
}
