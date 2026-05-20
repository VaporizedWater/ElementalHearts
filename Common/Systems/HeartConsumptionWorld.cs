using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElementalHearts.Common.Network;
using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Authoritative store of which hearts have been consumed in this world,
/// keyed by <see cref="Items.Hearts.ElementalHeartItem.ConsumptionId"/>.
/// In multiplayer the server is authoritative and broadcasts each new consumption.
/// </summary>
public sealed class HeartConsumptionWorld : ModSystem
{
	private static readonly Dictionary<string, int> _consumed = new();

	public static IReadOnlyDictionary<string, int> Consumed => _consumed;

	public static bool IsConsumed(string heartId) => _consumed.ContainsKey(heartId);

	/// <summary>
	/// Attempts to consume a heart in World mode. Routes through the server in multiplayer.
	/// Returns false if already consumed (caller should reject the item use).
	/// </summary>
	public static bool TryConsume(string heartId, int hpGain)
	{
		if (_consumed.ContainsKey(heartId))
			return false;

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			Record(heartId, hpGain);
			return true;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			SendConsumePacket(heartId, hpGain, toClient: -1, ignoreClient: -1);
			// Optimistic: server is authoritative and will broadcast the canonical result.
			return true;
		}

		// Server consumed it directly (e.g. host-as-server use).
		Record(heartId, hpGain);
		BroadcastConsume(heartId, hpGain, ignoreClient: -1);
		return true;
	}

	internal static void Record(string heartId, int hpGain)
	{
		_consumed[heartId] = hpGain;
		if (Main.netMode != NetmodeID.Server)
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ReconcileWorldHp();
	}

	internal static void ReceiveConsumption(BinaryReader reader, int whoAmI)
	{
		string heartId = reader.ReadString();
		int hpGain = reader.ReadInt32();

		if (Main.netMode == NetmodeID.Server)
		{
			// A client is requesting consumption; validate then rebroadcast.
			if (_consumed.ContainsKey(heartId))
				return;

			Record(heartId, hpGain);
			BroadcastConsume(heartId, hpGain, ignoreClient: whoAmI);
		}
		else
		{
			// Client receiving an authoritative announcement from the server.
			Record(heartId, hpGain);
		}
	}

	private static void SendConsumePacket(string heartId, int hpGain, int toClient, int ignoreClient)
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartConsumed);
		packet.Write(heartId);
		packet.Write(hpGain);
		packet.Send(toClient, ignoreClient);
	}

	private static void BroadcastConsume(string heartId, int hpGain, int ignoreClient)
	{
		SendConsumePacket(heartId, hpGain, toClient: -1, ignoreClient: ignoreClient);
	}

	public override void ClearWorld()
	{
		_consumed.Clear();
	}

	public static void ClearAllHearts()
	{
		_consumed.Clear();
		if (Main.netMode != NetmodeID.Server)
		{
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ReconcileWorldHp();
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["ids"] = _consumed.Keys.ToList();
		tag["hp"] = _consumed.Values.ToList();
	}

	public override void LoadWorldData(TagCompound tag)
	{
		_consumed.Clear();
		var ids = tag.GetList<string>("ids");
		var hp = tag.GetList<int>("hp");
		int count = System.Math.Min(ids.Count, hp.Count);
		for (int i = 0; i < count; i++)
			_consumed[ids[i]] = hp[i];
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(_consumed.Count);
		foreach (var (id, hp) in _consumed)
		{
			writer.Write(id);
			writer.Write(hp);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		_consumed.Clear();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			string id = reader.ReadString();
			int hp = reader.ReadInt32();
			_consumed[id] = hp;
		}

		Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ReconcileWorldHp();
	}
}
