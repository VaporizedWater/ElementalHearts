using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class SonarHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Sonar;
	public override int PotionItemId => ItemID.SonarPotion;
	public override string PermanentEffectText => "Permanently reveals the names of fish on the line";
	public override int PotionsForTwoHours => 15;
	public override int ShardCost => 1;
}
