using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

public class DiscordAbilitySystem : ModSystem
{
	public static ModKeybind UseDiscordAbilityKeybind { get; private set; }

	public override void Load()
	{
		UseDiscordAbilityKeybind = KeybindLoader.RegisterKeybind(Mod, "Use Discord Ability", "Q");
	}

	public override void Unload()
	{
		UseDiscordAbilityKeybind = null;
	}
}
