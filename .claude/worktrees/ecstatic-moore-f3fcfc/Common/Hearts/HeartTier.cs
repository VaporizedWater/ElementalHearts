namespace ElementalHearts.Common.Hearts;

/// <summary>
/// Rarity / power tier of a heart. The integer value is used as the sell-price multiplier
/// (see <see cref="Content.Items.Hearts.ElementalHeartItem.SetDefaults"/>); HP gain is
/// driven entirely by <see cref="Configs.ElementalHeartsConfig"/>.
/// </summary>
public enum HeartTier
{
	Common = 1,
	Uncommon = 3,
	Rare = 5,
	Epic = 7,
	Legendary = 10,
	Exotic = 15,
	Mythic = 30,
}
