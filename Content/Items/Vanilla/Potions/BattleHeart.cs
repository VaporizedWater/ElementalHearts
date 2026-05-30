using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class BattleHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Battle;
	public override int PotionItemId => ItemID.BattlePotion;
	public override string PermanentEffectText => "Permanently increases enemy spawn rate";
	public override int PotionsForTwoHours => 18;
	public override int ShardCost => 2;
}
