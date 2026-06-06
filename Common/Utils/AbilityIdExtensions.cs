// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Semantic extensions for AbilityId: owning heart, default toggle state, and daily idle-shard cost.</summary>
public static class AbilityIdExtensions
{
	public static HeartId GetHeart(this AbilityId id)
	{
		// Resolve the heart that unlocks this ability.
		return HeartId.None;
	}

	public static bool IsEnabledByDefault(this AbilityId id)
	{
		// Most active abilities are enabled when first unlocked unless the player disables them in the Heart Log.
		return true;
	}

	public static int GetDailyShardCost(this AbilityId id)
	{
		// Return idle-shard cost while this ability is enabled.
		return 0;
	}
}
