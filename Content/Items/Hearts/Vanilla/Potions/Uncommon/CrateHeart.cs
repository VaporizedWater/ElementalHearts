using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class CrateHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Crate;
	public override int PotionItemId => ItemID.CratePotion;
	public override string PermanentEffectText => "Permanently increases the chance of fishing up crates";
	public override int PotionsForTwoHours => 40;
	public override int ShardCost => 1;
}
