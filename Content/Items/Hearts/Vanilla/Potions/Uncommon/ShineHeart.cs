using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class ShineHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Shine;
	public override int PotionItemId => ItemID.ShinePotion;
	public override string PermanentEffectText => "Permanently emits light around you";
}
