using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class ObsidianSkinHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.ObsidianSkin;
	public override int PotionItemId => ItemID.ObsidianSkinPotion;
	public override string PermanentEffectText => "Permanently grants immunity to lava";
	public override int PotionsForTwoHours => 30;
	public override int ShardCost => 2;
}
