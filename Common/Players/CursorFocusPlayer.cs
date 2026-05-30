using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Cursor Focus camera ability (unlocked by the Magnification
/// Heart). Kept separate from the heart-consumption ledger because it is a pure client preference —
/// flipping it never touches HP or world progression. Defaults to on so the ability is felt the
/// moment the heart is consumed; the player turns it off from the Heart Log.
/// </summary>
public sealed class CursorFocusPlayer : ModPlayer
{
	public bool Enabled = true;

	public override void SaveData(TagCompound tag)
	{
		// Only the off state is worth persisting; absence means the default (on).
		if (!Enabled)
			tag["cursorFocusOff"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = !tag.GetBool("cursorFocusOff");
	}
}
