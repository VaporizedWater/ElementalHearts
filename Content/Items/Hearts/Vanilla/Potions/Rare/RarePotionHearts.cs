using ElementalHearts.Common.Hearts;
using Terraria.ID;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Potions.Rare;

public sealed class AmmoReservationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.AmmoReservation;
	public override int PotionItemId => ItemID.AmmoReservationPotion;
}

public sealed class DangersenseHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Dangersense;
	public override int PotionItemId => ItemID.TrapsightPotion;
}

public sealed class GravitationHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Gravitation;
	public override int PotionItemId => ItemID.GravitationPotion;
}

public sealed class InfernoHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Inferno;
	public override int PotionItemId => ItemID.InfernoPotion;
}

public sealed class ObsidianSkinHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.ObsidianSkin;
	public override int PotionItemId => ItemID.ObsidianSkinPotion;
}

public sealed class RageHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Rage;
	public override int PotionItemId => ItemID.RagePotion;
}

public sealed class SummoningHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Summoning;
	public override int PotionItemId => ItemID.SummoningPotion;
}

public sealed class WarmthHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Warmth;
	public override int PotionItemId => ItemID.WarmthPotion;
}

public sealed class WaterWalkingHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.WaterWalking;
	public override int PotionItemId => ItemID.WaterWalkingPotion;
}

public sealed class WrathHeart : PotionHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;
	public override int BuffType => BuffID.Wrath;
	public override int PotionItemId => ItemID.WrathPotion;
}
