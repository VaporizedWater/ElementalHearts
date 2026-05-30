using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class CrateHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Crate;
	public override int PotionItemId => ItemID.CratePotion;
	public override string PermanentEffectText => "Permanently increases the chance of fishing up crates";
	public override int PotionsForTwoHours => 40;
	public override int ShardCost => 1;
}
