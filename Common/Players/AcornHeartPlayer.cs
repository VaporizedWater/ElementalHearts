using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Acorn Heart's tree loot boost ability.
/// Defaults to on so the ability is felt the moment the heart is consumed;
/// the player turns it off from the Heart Log.
/// </summary>
public sealed class AcornHeartPlayer : ModPlayer
{
	public bool Enabled = true;

	public override void SaveData(TagCompound tag)
	{
		if (!Enabled)
			tag["acornHeartOff"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = !tag.GetBool("acornHeartOff");
	}
}
