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
			// First kill: always drop the heart
			if (isFirstKill)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
				continue;
			}

			// Subsequent kills: 10% chance
			if (Main.rand.NextFloat() < 0.1f)
			{
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, heartType);
			}
		}
	}
}
