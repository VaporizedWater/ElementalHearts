using System.IO;
using ElementalHearts.Common.Network;
using ElementalHearts.Common.Systems;
using Terraria.ModLoader;

namespace ElementalHearts;

public sealed class ElementalHearts : Mod
{
	public override void PostSetupContent() => BossHeartDropRegistry.Build();

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		MessageType type = (MessageType)reader.ReadByte();
		switch (type)
		{
			case MessageType.HeartConsumed:
				HeartConsumptionWorld.ReceiveConsumption(reader, whoAmI);
				break;
		}
	}
}
