using System.Collections.Generic;
using System.Linq;
using ElementalHearts.Common.Biomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

/// <summary>
/// While the local player is inside the <see cref="VitalCanopyBiome"/>, monster spawns are
/// reduced by 80% and critter spawns are 10x more likely. The 5x rate cut hits everything,
/// so critter pool weights get a 50x bump (5x to undo the cut, 10x for the net result).
/// </summary>
public sealed class VitalCanopySpawns : GlobalNPC
{
	private static bool LocalPlayerInCanopy(Player player)
		=> player != null && player.active && player.InModBiome<VitalCanopyBiome>();

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (!LocalPlayerInCanopy(player))
			return;

		spawnRate *= 5;
		maxSpawns = System.Math.Max(1, maxSpawns / 5);
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (!LocalPlayerInCanopy(spawnInfo.Player))
			return;

		foreach (int key in pool.Keys.ToList())
		{
			if (key > 0 && key < NPCID.Count && ContentSamples.NpcsByNetId[key].CountsAsACritter)
				pool[key] *= 50f;
		}
	}
}
