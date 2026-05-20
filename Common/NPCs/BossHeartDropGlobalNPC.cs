using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

public sealed class BossHeartDropGlobalNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		bool isFirstKill = BossFirstKillWorld.IsFirstKill(npc.type);
		BossFirstKillWorld.RecordBossDefeat(npc.type);

		var hearts = BossHeartDropRegistry.GetDrops(npc.type);
		foreach (int heartType in hearts)
		{
			// First kill: potentially always drop the heart depending on config
			if (isFirstKill && ElementalHeartsBossConfig.Instance.BossHeartsGuaranteedOnFirstKill)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
				continue;
			}

			// Based on RNG config (default 10%)
			float dropChance = ElementalHeartsBossConfig.Instance.BossHeartDropChance / 100f;
			if (Main.rand.NextFloat() < dropChance)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
			}
		}
	}
}
