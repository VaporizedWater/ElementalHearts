using ElementalHearts.Common.Configs;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Common.LifeShards;

/// <summary>
/// Load-time wiring for the Life Shard system: the live recipe gate and the
/// Extractinator acceptance of vanilla Life Crystals.
/// </summary>
public sealed class LifeShardSystem : ModSystem
{


	private static Condition _uiUpgradeOnlyCondition;

	/// <summary>
	/// Condition that is never met, used to disable standard recipe crafting
	/// while preserving Shimmer decrafting capabilities.
	/// </summary>
	public static Condition UIUpgradeOnlyCondition =>
		_uiUpgradeOnlyCondition ??= new Condition(
			Language.GetOrRegister("Mods.ElementalHearts.Conditions.UIUpgradeOnly", () => "Can only be upgraded via the Life Shard UI"),
			() => false);

	public override void PostSetupContent()
		=> SetLifeCrystalExtractable(ElementalHeartsServerConfig.Instance.LifeShards.SystemEnabled);



	// ── Pickup-sound de-duplication ──────────────────────────────────────────────
	// Several shards picked up on the same frame — an Extractinator can yield a few
	// tiers at once — would otherwise stack their cues into a muddy chord. Each
	// pickup only records its tier; the latest one is played once, at end of frame.
	private static LifeShardTier? _pendingPickupSound;

	/// <summary>
	/// Queues a shard pickup cue. Repeated calls within a frame overwrite each other,
	/// so only the last shard picked up that frame is heard.
	/// </summary>
	public static void QueuePickupSound(LifeShardTier tier) => _pendingPickupSound = tier;

	public override void PostUpdateEverything()
	{
		if (_pendingPickupSound is not LifeShardTier tier)
			return;

		_pendingPickupSound = null;
		SoundEngine.PlaySound(tier.GetPickupSound(), Main.LocalPlayer.Center);
	}

	/// <summary>
	/// Makes a vanilla Life Crystal accepted by the Extractinator only while the system
	/// is enabled, so disabling it can never consume a crystal for nothing.
	/// </summary>
	/// <remarks>
	/// Takes the toggle as an argument rather than reading <see cref="ElementalHeartsServerConfig.Instance.LifeShards"/>:
	/// the config's <c>OnChanged</c> fires while the config is still being registered, before
	/// <see cref="ModContent.GetInstance{T}"/> can resolve it.
	/// </remarks>
	public static void SetLifeCrystalExtractable(bool enabled)
	{
		ItemID.Sets.ExtractinatorMode[ItemID.LifeCrystal] = enabled ? ItemID.LifeCrystal : -1;
	}
}
