using System;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;

namespace ElementalHearts.Common.Systems;

public class IdleShardWorld : ModSystem
{
	public static long LastClaimTimeTicks;

	public override void OnWorldLoad()
	{
		LastClaimTimeTicks = DateTime.UtcNow.Ticks;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["LastClaimTimeTicks"] = LastClaimTimeTicks;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		if (tag.ContainsKey("LastClaimTimeTicks"))
			LastClaimTimeTicks = tag.GetLong("LastClaimTimeTicks");
		else
			LastClaimTimeTicks = DateTime.UtcNow.Ticks;
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(LastClaimTimeTicks);
	}

	public override void NetReceive(BinaryReader reader)
	{
		LastClaimTimeTicks = reader.ReadInt64();
	}
}
