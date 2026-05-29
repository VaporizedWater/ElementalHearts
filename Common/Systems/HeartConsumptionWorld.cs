using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElementalHearts.Common.Network;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Authoritative store of which hearts have been consumed in this world, keyed by
/// <see cref="ElementalHeartItem.ConsumptionId"/>. Only the set of consumed ids is
/// kept — the HP each heart grants is read live from its definition (see
/// <see cref="Hearts.HeartRegistry"/>), so changing the HP config retroactively
/// updates every character's bonus. In multiplayer the server is authoritative.
/// </summary>
public sealed class HeartConsumptionWorld : ModSystem
{
	private static readonly HashSet<string> _consumed = new();

	public static IReadOnlyCollection<string> Consumed => _consumed;

	public static bool IsConsumed(string heartId) => _consumed.Contains(heartId);

	/// <summary>
	/// Attempts to consume <paramref name="heart"/> in the current world.
	/// Returns false if it has already been consumed.
	/// </summary>
	public static bool TryConsume(ElementalHeartItem heart)
	{
		string id = heart.ConsumptionId;
		if (_consumed.Contains(id))
			return false;

		switch (Main.netMode)
		{
			case NetmodeID.SinglePlayer:
				Record(id);
				return true;

			case NetmodeID.MultiplayerClient:
				// Optimistic local apply so the consuming player's stats update
				// immediately. The server rebroadcasts to *other* clients; it never
				// echoes back to the sender, so we must record here too.
				Record(id);
				SendConsumeRequest(heart.Type, Main.myPlayer);
				return true;

			default:
				// Server consuming directly (e.g. a host-side admin path).
				Record(id);
				BroadcastConsume(heart.Type, consumerWhoAmI: -1, ignoreClient: -1);
				return true;
		}
	}

	internal static void Record(string heartId)
	{
		_consumed.Add(heartId);

		if (Main.netMode != NetmodeID.Server)
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ReconcileWorldHp();
	}

	/// <summary>
	/// Re-enables a toggleable heart's slot in the world registry: drops the id from
	/// <see cref="Consumed"/> and refunds the HP it granted to the local character so
	/// the heart can be consumed again to re-activate. Used by buff Potion Hearts as
	/// their "re-use to disable" action. Returns false if the heart wasn't consumed.
	/// </summary>
	public static bool TryDeactivate(ElementalHeartItem heart)
	{
		string id = heart.ConsumptionId;
		if (!_consumed.Contains(id))
			return false;

		switch (Main.netMode)
		{
			case NetmodeID.SinglePlayer:
				Unrecord(id);
				return true;

			case NetmodeID.MultiplayerClient:
				// Optimistic local apply mirrors the consume path.
				Unrecord(id);
				SendDeactivateRequest(heart.Type);
				return true;

			default:
				Unrecord(id);
				BroadcastDeactivate(heart.Type, ignoreClient: -1);
				return true;
		}
	}

	internal static void Unrecord(string heartId)
	{
		_consumed.Remove(heartId);

		if (Main.netMode != NetmodeID.Server)
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().HandleHeartDeactivated(heartId);
	}

	internal static void ReceiveDeactivation(BinaryReader reader, int whoAmI)
	{
		int itemType = reader.ReadInt32();

		if (!TryResolveHeart(itemType, out ElementalHeartItem heart))
			return;

		if (Main.netMode == NetmodeID.Server)
		{
			if (!_consumed.Contains(heart.ConsumptionId))
				return;

			Unrecord(heart.ConsumptionId);
			BroadcastDeactivate(itemType, ignoreClient: whoAmI);
			return;
		}

		// Client receiving the server's announcement.
		if (!_consumed.Contains(heart.ConsumptionId))
			return;

		Unrecord(heart.ConsumptionId);
	}

	private static void SendDeactivateRequest(int itemType)
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartDeactivated);
		packet.Write(itemType);
		packet.Send();
	}

	private static void BroadcastDeactivate(int itemType, int ignoreClient)
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartDeactivated);
		packet.Write(itemType);
		packet.Send(toClient: -1, ignoreClient: ignoreClient);
	}

	internal static void ReceiveConsumption(BinaryReader reader, int whoAmI)
	{
		int itemType = reader.ReadInt32();
		int consumerWhoAmI = reader.ReadInt32();

		// The packet only carries the item type; the heart is resolved to its canonical
		// definition on every receiver, so a client can't spoof which heart was used.
		if (!TryResolveHeart(itemType, out ElementalHeartItem heart))
			return;

		if (Main.netMode == NetmodeID.Server)
		{
			if (_consumed.Contains(heart.ConsumptionId))
				return;

			Record(heart.ConsumptionId);
			BroadcastConsume(itemType, consumerWhoAmI, ignoreClient: whoAmI);
			return;
		}

		// Client receiving the server's announcement.
		if (_consumed.Contains(heart.ConsumptionId))
			return;

		Record(heart.ConsumptionId);
		PlayRemoteConsumeEffect(heart, consumerWhoAmI);
	}

	/// <summary>
	/// Wipes the world's consumed-heart registry and revokes the max-HP every character
	/// gained from those hearts. In multiplayer the clear is routed through the server
	/// so every client ends up consistent.
	/// </summary>
	public static void ClearAllHearts()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			// Ask the server to do the authoritative clear; it broadcasts back to everyone.
			SendClearRequest();
			return;
		}

		PerformClear();
		if (Main.netMode == NetmodeID.Server)
			BroadcastClear();
	}

	private static void PerformClear()
	{
		_consumed.Clear();
		if (Main.netMode != NetmodeID.Server)
			Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ClearWorldHp();
	}

	internal static void ReceiveClear(int whoAmI)
	{
		if (Main.netMode == NetmodeID.Server)
		{
			// A client requested the clear — apply it and tell everyone.
			PerformClear();
			BroadcastClear();
		}
		else
		{
			// The server announced the clear.
			PerformClear();
		}
	}

	private static void SendClearRequest()
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartsCleared);
		packet.Send();
	}

	private static void BroadcastClear()
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartsCleared);
		packet.Send(toClient: -1, ignoreClient: -1);
	}

	private static void SendConsumeRequest(int itemType, int consumerWhoAmI)
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartConsumed);
		packet.Write(itemType);
		packet.Write(consumerWhoAmI);
		packet.Send();
	}

	private static void BroadcastConsume(int itemType, int consumerWhoAmI, int ignoreClient)
	{
		ModPacket packet = ModContent.GetInstance<ElementalHearts>().GetPacket();
		packet.Write((byte)MessageType.HeartConsumed);
		packet.Write(itemType);
		packet.Write(consumerWhoAmI);
		packet.Send(toClient: -1, ignoreClient: ignoreClient);
	}

	private static bool TryResolveHeart(int itemType, out ElementalHeartItem heart)
	{
		heart = ModContent.GetModItem(itemType) as ElementalHeartItem;
		return heart != null;
	}

	private static void PlayRemoteConsumeEffect(ElementalHeartItem heart, int consumerWhoAmI)
	{
		// We already played our own effect in UseItem; don't replay it.
		if (consumerWhoAmI == Main.myPlayer)
			return;

		if (consumerWhoAmI < 0 || consumerWhoAmI >= Main.maxPlayers)
			return;

		Player consumer = Main.player[consumerWhoAmI];
		if (!consumer.active)
			return;

		heart.PlayConsumeEffect(consumer.Center);
	}

	public override void ClearWorld() => _consumed.Clear();

	public override void SaveWorldData(TagCompound tag)
	{
		tag["ids"] = _consumed.ToList();
	}

	public override void LoadWorldData(TagCompound tag)
	{
		_consumed.Clear();
		// Older saves also stored a parallel "hp" list; it is intentionally ignored
		// now that HP is always derived live from the heart definition.
		foreach (string id in tag.GetList<string>("ids"))
			_consumed.Add(id);
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(_consumed.Count);
		foreach (string id in _consumed)
			writer.Write(id);
	}

	public override void NetReceive(BinaryReader reader)
	{
		_consumed.Clear();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
			_consumed.Add(reader.ReadString());

		Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().ReconcileWorldHp();
	}
}
