using ElementalHearts.Common.Configs;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

/// <summary>
/// Drops Life Shards from an enemy based on the cumulative damage it has taken from all
/// players, rather than on death. Each tier rolls exactly once, the first time the
/// enemy's running damage total crosses that tier's threshold. Bosses roll at double
/// the configured chance.
/// </summary>
public sealed class LifeShardDropGlobalNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	/// <summary>Running total of damage this enemy has taken.</summary>
	private int _cumulativeDamage;

	/// <summary>Bitmask of <see cref="LifeShardTier"/> values that have already rolled.</summary>
	private int _rolledTiers;

	public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		=> RegisterDamage(npc, damageDone);

	public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		=> RegisterDamage(npc, damageDone);

	private static void RegisterDamage(NPC hitNpc, int damageDone)
	{
		// Drops are authoritative on the server / in single-player; a multiplayer client
		// also runs these hooks for hit prediction, which must not roll its own drops.
		if (Main.netMode == NetmodeID.MultiplayerClient)
			return;
		if (damageDone <= 0)
			return;

		LifeShardSettings config = ElementalHeartsServerConfig.Instance.LifeShards;
		if (!config.SystemEnabled)
			return;

		// Multi-segment enemies (worms) spread hits across segments but share one life
		// pool — funnel all of it onto the life-holding NPC so thresholds add up.
		NPC npc = ResolveLifeNpc(hitNpc);
		if (!IsValidTarget(npc))
			return;

		LifeShardDropGlobalNPC data = npc.GetGlobalNPC<LifeShardDropGlobalNPC>();
		data._cumulativeDamage += damageDone;
		data.RollThresholds(npc, config);
	}

	private void RollThresholds(NPC npc, LifeShardSettings config)
	{
		bool isBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];

		TryTier(npc, isBoss, LifeShardTier.Common,    config.CommonDamageThreshold,    config.CommonDropChance);
		TryTier(npc, isBoss, LifeShardTier.Uncommon,  config.UncommonDamageThreshold,  config.UncommonDropChance);
		TryTier(npc, isBoss, LifeShardTier.Rare,      config.RareDamageThreshold,      config.RareDropChance);
		TryTier(npc, isBoss, LifeShardTier.Epic,      config.EpicDamageThreshold,      config.EpicDropChance);
		TryTier(npc, isBoss, LifeShardTier.Legendary, config.LegendaryDamageThreshold, config.LegendaryDropChance);
	}

	private void TryTier(NPC npc, bool isBoss, LifeShardTier tier, int threshold, float chancePercent)
	{
		int bit = 1 << (int)tier;
		if ((_rolledTiers & bit) != 0)
			return;
		if (_cumulativeDamage < threshold)
			return;

		// Each threshold rolls exactly once, whether or not the shard actually drops.
		_rolledTiers |= bit;

		float chance = chancePercent / 100f;
		if (isBoss)
			chance *= 2f;

		if (Main.rand.NextFloat() < chance)
			Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, tier.GetItemType());
	}

	/// <summary>Resolves a worm segment to the NPC that actually holds the shared life.</summary>
	private static NPC ResolveLifeNpc(NPC npc)
	{
		if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
		{
			NPC real = Main.npc[npc.realLife];
			if (real.active)
				return real;
		}

		return npc;
	}

	/// <summary>Friendly NPCs, town NPCs, and the target dummy are never shard sources.</summary>
	private static bool IsValidTarget(NPC npc)
	{
		if (npc.friendly || npc.townNPC)
			return false;
		if (npc.type == NPCID.TargetDummy)
			return false;

		return true;
	}
}
