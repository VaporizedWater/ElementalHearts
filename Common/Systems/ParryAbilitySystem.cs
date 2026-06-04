using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Registers the Parry keybind that the Deerclops Heart's parry ability listens for
/// (<see cref="Common.Players.ParryAbilityPlayer"/>). Mirrors
/// <see cref="DiscordAbilitySystem"/> — one keybind owned by one system, cleared on unload.
/// </summary>
public class ParryAbilitySystem : ModSystem
{
	public static ModKeybind ParryKeybind { get; private set; }

	public override void Load()
	{
		ParryKeybind = KeybindLoader.RegisterKeybind(Mod, "Parry", "C");
	}

	public override void Unload()
	{
		ParryKeybind = null;
	}
}
