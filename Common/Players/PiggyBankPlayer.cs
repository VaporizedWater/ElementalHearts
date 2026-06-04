using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.Vanilla.Exotic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

/// <summary>
/// Per-character on/off switch for the Piggy Bank Heart's "passive income" ability: while it's on, the
/// shards you bank can be cashed out as coins (the Heart Log's "Sell" button). Kept out of the
/// heart-consumption ledger because flipping it is a pure preference — it never touches HP or world
/// progression. Defaults to *off*: switching it on (via <see cref="Content.Items.Vanilla.Exotic.PiggyBankHeart.SetAbilityEnabled"/>)
/// first force-claims the existing bank, so the bank you built up before having passive income can't be
/// converted to coins after the fact.
/// </summary>
public sealed class PiggyBankPlayer : ModPlayer
{
	/// <summary>Flat coin value of a single banked Life Shard when sold, in copper (1 gold).</summary>
	public const int ShardCoinValue = 10_000;

	/// <summary><see cref="Content.Items.Hearts.ElementalHeartItem.ConsumptionId"/> of the heart that
	/// grants the ability.</summary>
	public const string HeartId = nameof(PiggyBankHeart);

	public bool Enabled = false;

	/// <summary>Whether the local character has unlocked the heart, honouring the world's
	/// shared-vs-local progression setting exactly the way the Heart Log does.</summary>
	public static bool IsUnlocked(Player player) =>
		ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression
			? HeartConsumptionWorld.IsUnlocked(HeartId)
			: player.GetModPlayer<HeartConsumptionPlayer>().IsUnlockedLocally(HeartId);

	/// <summary>Whether passive income is live for <paramref name="player"/>: heart unlocked and the
	/// toggle switched on. The single gate behind both the Sell button and the 1-gold vendor price.</summary>
	public static bool IsActive(Player player) =>
		player is { active: true } && IsUnlocked(player) && player.GetModPlayer<PiggyBankPlayer>().Enabled;

	public override void SaveData(TagCompound tag)
	{
		// Only the on state is worth persisting; absence means the default (off).
		if (Enabled)
			tag["piggyBankOn"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = tag.GetBool("piggyBankOn");
	}
}
