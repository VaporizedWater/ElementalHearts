using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Epic;

public sealed class EnduranceHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	public override int BuffType => BuffID.Endurance;
	public override int PotionItemId => ItemID.EndurancePotion;
}

public sealed class LifeforceHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	public override int BuffType => BuffID.Lifeforce;
	public override int PotionItemId => ItemID.LifeforcePotion;
}
