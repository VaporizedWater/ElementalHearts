using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class SummoningHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Summoning;
	public override int PotionItemId => ItemID.SummoningPotion;
	public override string PermanentEffectText => "Permanently increases maximum number of minions by 1";
}
