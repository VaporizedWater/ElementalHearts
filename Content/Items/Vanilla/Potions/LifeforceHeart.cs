using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class LifeforceHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;
	public override int BuffType => BuffID.Lifeforce;
	public override int PotionItemId => ItemID.LifeforcePotion;
	public override string PermanentEffectText => "Permanently increases maximum life by 20%";
	public override int PotionsForTwoHours => 24;
	public override int ShardCost => 1;
}
