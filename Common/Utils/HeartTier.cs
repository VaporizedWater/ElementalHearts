// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Progression and rarity tier shared by hearts, visuals, HP values, and Life Shard economics.</summary>
public enum HeartTier : byte
{
	None = 0,
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary,
	Exotic,
	Mythic,
}
