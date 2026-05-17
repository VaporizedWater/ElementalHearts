using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

public sealed class BossHeartDropGlobalNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		var config = ElementalHeartsConfig.Instance;
		bool isFirstKill = BossFirstKillWorld.IsFirstKill(npc.type);
		BossFirstKillWorld.RecordBossDefeat(npc.type);

		var hearts = BossHeartDropRegistry.GetDrops(npc.type);
		foreach (int heartType in hearts)
		{
			if (isFirstKill && config.GuaranteedFirstKill)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
				continue;
			}

			if (Main.rand.Next(1, 101) <= config.BossHeartDropChance)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
			}
		}
	}
}
