// Architecture scaffold only. Fill behavior in the implementation pass.
using System.Collections.Generic;

namespace ElementalHearts.Core;

/// <summary>Semantic extensions for BossId: NPC identity, first-kill state keys, and heart drops.</summary>
public static class BossIdExtensions
{
	public static int GetNpcType(this BossId id)
	{
		// Resolve the tML NPC type for this boss, including cross-mod bosses when loaded.
		return 0;
	}

	public static IEnumerable<HeartId> GetHeartDrops(this BossId id)
	{
		// Yield every heart that can drop from this boss.
		yield break;
	}
}
