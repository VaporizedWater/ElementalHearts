using System.IO;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Network;
using ElementalHearts.Common.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace ElementalHearts;

public sealed class ElementalHearts : Mod
{
	public ElementalHearts()
	{
		// Skip music autoload when audio is unavailable (dedicated server, -nosound launch,
		// or no audio device). tML's MusicLoader crashes on the first AddMusic in that state
		// because MusicID.Search ends up partially populated. Falling back to no custom
		// music is fine — GetMusicSlot returns 0 and vanilla boss music plays instead.
		// Must be set in the constructor: Mod.Autoload() runs before Mod.Load().
		MusicAutoloadingEnabled = !Main.dedServ && Main.audioSystem is LegacyAudioSystem;
	}

	public override void PostSetupContent()
	{
		HeartRegistry.Build();
		PotionHeartRegistry.Build();
		BossHeartDropRegistry.Build();
	}

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		MessageType type = (MessageType)reader.ReadByte();
		switch (type)
		{
			case MessageType.HeartConsumed:
				HeartConsumptionWorld.ReceiveConsumption(reader, whoAmI);
				break;

			case MessageType.HeartsCleared:
				HeartConsumptionWorld.ReceiveClear(whoAmI);
				break;
		}
	}
}
