namespace ElementalHearts.Common.LifeShards;

/// <summary>
/// Rarity tier of a Life Shard. The integer value is used both as a bit index for the
/// per-enemy "already rolled" mask (see <see cref="NPCs.LifeShardDropGlobalNPC"/>) and
/// to step between adjacent tiers, so the values must stay contiguous from zero.
/// </summary>
public enum LifeShardTier
{
	Common = 0,
	Uncommon = 1,
	Rare = 2,
	Epic = 3,
	Legendary = 4,
}
