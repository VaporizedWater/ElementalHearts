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

	public override void PreUpdateBuffs()
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

	public override void PostUpdateBuffs()
	{
		if (Main.netMode == NetmodeID.Server)
			return;

		if (Player.whoAmI != Main.myPlayer)
			return;

		if (ElementalHeartsClientConfig.Instance.ShowPermanentBuffs)
			return;

		// Remove the buff from the UI if we're hiding permanent buffs.
		// We iterate forwards and use DelBuff, which shifts elements down.
		// We use <= 60 ticks (1 second) to account for other mods potentially modifying the buff time slightly
		// and still ensure it doesn't delete legitimate, newly consumed potions (which have times > 3600).
		for (int i = 0; i < Player.MaxBuffs; i++)
		{
			int buffType = Player.buffType[i];
			if (buffType > 0 && Player.buffTime[i] <= 60)
			{
				// Check if this buff comes from a consumed heart
				foreach (string id in HeartConsumptionWorld.Consumed)
				{
					if (PotionHeartRegistry.TryGetBuff(id, out int heartBuffType) && heartBuffType == buffType)
					{
						Player.DelBuff(i);
						i--; // Re-check this index because a new buff shifted into it
						break;
					}
				}
			}
		}
	}
}
