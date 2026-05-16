using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Tracks which bosses have been defeated at least once in the world.
/// Used to determine if a heart drop should be 100% (first kill) or 10% (subsequent kills).
/// </summary>
public sealed class BossFirstKillWorld : ModSystem
{
	private static readonly HashSet<int> _defeatedBosses = [];

	public static IReadOnlyCollection<int> DefeatedBosses => _defeatedBosses;

	public static bool IsFirstKill(int npcType) => !_defeatedBosses.Contains(npcType);

	public static void RecordBossDefeat(int npcType)
	{
		_defeatedBosses.Add(npcType);
	}

	public override void OnWorldUnload() => ClearWorld();

	public override void ClearWorld()
	{
		_defeatedBosses.Clear();
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["bosses"] = _defeatedBosses.ToList();
	}

	public override void LoadWorldData(TagCompound tag)
	{
		_defeatedBosses.Clear();
		var bosses = tag.GetList<int>("bosses");
		foreach (int npc in bosses)
			_defeatedBosses.Add(npc);
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(_defeatedBosses.Count);
		foreach (int npc in _defeatedBosses)
			writer.Write(npc);
	}

	public override void NetReceive(BinaryReader reader)
	{
		_defeatedBosses.Clear();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
			_defeatedBosses.Add(reader.ReadInt32());
	}
}
