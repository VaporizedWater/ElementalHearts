using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class DangersenseHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Dangersense;
	public override int PotionItemId => ItemID.TrapsightPotion;
	public override string PermanentEffectText => "Permanently reveals nearby hazardous tiles";
}
