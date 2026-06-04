using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Lightning Bug Heart's movement speed boost ability.
/// Defaults to on so the ability is felt the moment the heart is consumed;
/// the player turns it off from the Heart Log.
/// </summary>
public sealed class LightningBugHeartPlayer : ModPlayer
{
	public bool Enabled = true;

	public override void SaveData(TagCompound tag)
	{
		if (!Enabled)
			tag["lightningBugHeartOff"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = !tag.GetBool("lightningBugHeartOff");
	}

	public override void PostUpdateEquips()
	{
		if (Enabled && Player.GetModPlayer<HeartConsumptionPlayer>().IsConsumedLocally("LightningBugHeart"))
		{
			Player.moveSpeed += 0.08f;
		}
	}

	public override void PostUpdateRunSpeeds()
	{
		if (Enabled && Player.GetModPlayer<HeartConsumptionPlayer>().IsConsumedLocally("LightningBugHeart"))
		{
			Player.maxRunSpeed *= 1.08f;
			Player.accRunSpeed *= 1.08f;
			Player.runAcceleration *= 1.08f;
		}
	}
}
