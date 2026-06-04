using ElementalHearts.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Handles the logic for Progression Gates, determining the player's current maximum HP capacity
/// from elemental hearts based on defeated bosses.
/// </summary>
public sealed class HeartCapacitySystem : ModSystem
{
	private static uint _cachedCapacityTick = uint.MaxValue;
	private static int? _cachedMaxCapacity;

	/// <summary>
	/// Returns the maximum HP bonus the player is allowed to have from elemental hearts
	/// based on current world progression, or null if there is no limit. Capacity is read
	/// during stat recomputation, so one tick-local cache keeps the hot path cheap while
	/// still reacting on the next frame to boss kills or config changes.
	/// </summary>
	public static int? GetMaxCapacity()
	{
		uint currentTick = Main.GameUpdateCount;
		if (_cachedCapacityTick == currentTick)
			return _cachedMaxCapacity;

		_cachedCapacityTick = currentTick;
		_cachedMaxCapacity = ComputeMaxCapacity();
		return _cachedMaxCapacity;
	}

	private static int? ComputeMaxCapacity()
	{
		var config = ElementalHeartsServerConfig.Instance.CapacityLimits;
		if (!config.EnableProgressionGates)
			return null;

		if (config.UnlimitedPostMoonLord && NPC.downedMoonlord)
			return null;

		if (NPC.downedMoonlord)
			return config.PostMoonLordCapacity;

		if (NPC.downedAncientCultist)
			return config.PostCultistCapacity;

		if (NPC.downedGolemBoss)
			return config.PostGolemCapacity;

		if (NPC.downedPlantBoss)
			return config.PostPlanteraCapacity;

		int downedMechs = 0;
		if (NPC.downedMechBoss1) downedMechs++;
		if (NPC.downedMechBoss2) downedMechs++;
		if (NPC.downedMechBoss3) downedMechs++;

		if (downedMechs == 3)
			return config.Post3MechsCapacity;
		if (downedMechs == 2)
			return config.Post2MechsCapacity;
		if (downedMechs == 1)
			return config.Post1MechCapacity;

		if (Main.hardMode)
			return config.PostWoFCapacity;

		if (NPC.downedBoss3) // Skeletron
			return config.PostSkeletronCapacity;

		return config.PreBossCapacity;
	}
}
