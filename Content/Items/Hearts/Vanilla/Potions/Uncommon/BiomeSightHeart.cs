using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Uncommon;

public sealed class BiomeSightHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;
	public override int BuffType => BuffID.BiomeSight;
	public override int PotionItemId => ItemID.BiomeSightPotion;
	public override string PermanentEffectText => "Permanently highlights nearby blocks belonging to evil biomes and the Hallow";
}
