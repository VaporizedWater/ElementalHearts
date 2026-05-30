using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace ElementalHearts.Common.NPCs;

public sealed class BossHeartDropGlobalNPC : GlobalNPC
{
	public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
		var hearts = BossHeartDropRegistry.GetDrops(npc.type);
		foreach (int heartType in hearts)
		{
			npcLoot.Add(new BossHeartDropRule(npc.type, heartType));
		}
	}

	public override void OnKill(NPC npc)
	{
		// Record the boss defeat after drops have been rolled so ModifyNPCLoot
		// can correctly evaluate if it was the first kill.
		BossFirstKillWorld.RecordBossDefeat(npc.type);
	}
}
