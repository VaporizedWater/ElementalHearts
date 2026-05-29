using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class GravitationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Gravitation;
	public override int PotionItemId => ItemID.GravitationPotion;
	public override string PermanentEffectText => "Permanently allows you to control gravity";
}
