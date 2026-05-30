using ElementalHearts.Common.Hearts;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Potions;

public sealed class BiomeSightHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.BiomeSight;
	public override int PotionItemId => ItemID.BiomeSightPotion;
	public override string PermanentEffectText => "Permanently highlights nearby blocks belonging to evil biomes and the Hallow";
	public override int PotionsForTwoHours => 12;
	public override int ShardCost => 1;
}
