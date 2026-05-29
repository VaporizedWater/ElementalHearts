using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class SonarHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.Sonar;
	public override int PotionItemId => ItemID.SonarPotion;
	public override string PermanentEffectText => "Permanently reveals the names of fish on the line";
}
