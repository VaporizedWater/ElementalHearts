using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Keeps the buff for every consumed potion heart continuously active on the local player.
/// Each tick we re-apply the matching BuffID for a few frames, so the buff effectively
/// behaves as permanent. Gated by <see cref="ElementalHeartsPotionEffectConfig"/> — when
/// the toggle is off this hook returns early and Potion Hearts revert to plain HP grants.
/// </summary>
public sealed class PotionHeartEffectsPlayer : ModPlayer
{
	/// <summary>
	/// Short duration in ticks for each re-applied buff. We re-apply every frame in
	/// <see cref="PostUpdateBuffs"/>, so anything &gt;1 covers a frame where the hook
	/// might be skipped (e.g. paused) without leaving the buff to expire visibly.
	/// </summary>
	private const int BuffRefreshTicks = 5;

	public override void PostUpdateBuffs()
	{
		// Server-side ModPlayer also ticks per connected player; buffs are managed
		// client-side, so applying here from the server doubles up. Each client
		// re-applies for its own local player — that's the whole MP story.
		if (Main.netMode == NetmodeID.Server)
			return;

		if (Player.whoAmI != Main.myPlayer)
			return;

		if (!ElementalHeartsPotionEffectConfig.Instance.WorldwidePotionEffectsEnabled)
			return;

		foreach (string id in HeartConsumptionWorld.Consumed)
		{
			// buffType == 0 is the explicit "no effect" sentinel used by novelty potion
			// hearts (Love, Stink); they only grant HP, never apply a buff.
			if (PotionHeartRegistry.TryGetBuff(id, out int buffType) && buffType > 0)
				Player.AddBuff(buffType, BuffRefreshTicks, quiet: true);
		}
	}
}
