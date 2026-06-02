using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Shared base for the active-ability ModPlayers whose heart is switched on or off per character.
/// Owns the single persisted <see cref="Enabled"/> flag and its save/load, so each concrete
/// ability player is left to implement only its actual behaviour. The flag is written to the tag
/// only when set, so a disabled ability adds nothing to the save.
/// </summary>
public abstract class ToggleAbilityPlayer : ModPlayer
{
	/// <summary>Whether this character currently has the ability switched on.</summary>
	public bool Enabled { get; set; }

	public override void SaveData(TagCompound tag)
	{
		if (Enabled)
			tag["Enabled"] = true;
	}

	public override void LoadData(TagCompound tag) => Enabled = tag.ContainsKey("Enabled");
}
