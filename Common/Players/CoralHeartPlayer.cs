using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Coral Heart's water movement boost.
/// Defaults to on so the heart immediately feels like catching a friendly reef current.
/// </summary>
public sealed class CoralHeartPlayer : ModPlayer
{
	private const string CoralHeartId = "CoralHeart";
	private const float WaterSpeedMultiplier = 1.1f;

	public bool Enabled = true;

	public override void SaveData(TagCompound tag)
	{
		if (!Enabled)
			tag["coralHeartOff"] = true;
	}

	public override void LoadData(TagCompound tag) => Enabled = !tag.GetBool("coralHeartOff");

	public override void PostUpdateRunSpeeds()
	{
		if (!Enabled || !IsInWater || !Player.GetModPlayer<HeartConsumptionPlayer>().IsUnlockedLocally(CoralHeartId))
			return;

		Player.runAcceleration *= WaterSpeedMultiplier;
		Player.accRunSpeed *= WaterSpeedMultiplier;
		Player.maxRunSpeed *= WaterSpeedMultiplier;
	}

	private bool IsInWater => Player.wet && !Player.lavaWet && !Player.honeyWet;
}
