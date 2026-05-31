using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Gemcorn Heart's gem harvesting ability.
/// Defaults to on so the ability is felt the moment the heart is consumed;
/// the player turns it off from the Heart Log.
/// </summary>
public sealed class GemcornHeartPlayer : ModPlayer
{
	public bool Enabled = true;

	public override void SaveData(TagCompound tag)
	{
		if (!Enabled)
			tag["gemcornHeartOff"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = !tag.GetBool("gemcornHeartOff");
	}
}
