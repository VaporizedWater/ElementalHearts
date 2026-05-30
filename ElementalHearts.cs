using System.Collections.Generic;
using System.IO;
using ElementalHearts.Common.Biomes;
using ElementalHearts.Common.CrossMod.BossChecklist;
using ElementalHearts.Common.CrossMod.MusicDisplay;
using ElementalHearts.Common.CrossMod.Munchies;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Network;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using Microsoft.Xna.Framework;
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
		MunchiesIntegration.Register(this);
		MusicDisplayIntegration.Register(this);
		BossChecklistIntegration.Register(this);
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

			case MessageType.HeartDeactivated:
				HeartConsumptionWorld.ReceiveDeactivation(reader, whoAmI);
				break;
		}
	}

	// ── BiomeTitles integration (see Biomes.md) ──────────────────────────────
	// Called every frame by the BiomeTitles mod if it's loaded; delegates to the
	// VitalCanopyBiome's own activation check so the title follows the same rules
	// as music, ambience, and spawn modifiers.

	public string BTitlesHook_MiniBiomeChecker(Player player)
	{
		if (player.InModBiome<VitalCanopyBiome>())
			return "vital_canopy";

		return "";
	}

	public IEnumerable<dynamic> BTitlesHook_GetBiomes()
	{
		yield return new
		{
			Key = "vital_canopy",
			Title = "Vital Canopy",
			SubTitle = "Elemental Hearts",
			TitleColor = new Color(120, 230, 130),
			TitleStroke = new Color(20, 60, 30),
		};
	}
}
