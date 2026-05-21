using ElementalHearts.Common.Configs;
using Terraria;
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
	private static Condition _systemEnabledCondition;

	/// <summary>
	/// Live recipe gate for every Life Shard recipe. The crafting UI re-evaluates it,
	/// so toggling <see cref="LifeShardConfig.SystemEnabled"/> shows or hides the
	/// recipes without a reload.
	/// </summary>
	public static Condition SystemEnabledCondition =>
		_systemEnabledCondition ??= new Condition(
			Language.GetText("Mods.ElementalHearts.Conditions.LifeShardSystemEnabled"),
			() => LifeShardConfig.Instance.SystemEnabled);

	public override void PostSetupContent()
		=> SetLifeCrystalExtractable(LifeShardConfig.Instance.SystemEnabled);

	/// <summary>
	/// Makes a vanilla Life Crystal accepted by the Extractinator only while the system
	/// is enabled, so disabling it can never consume a crystal for nothing.
	/// </summary>
	/// <remarks>
	/// Takes the toggle as an argument rather than reading <see cref="LifeShardConfig.Instance"/>:
	/// the config's <c>OnChanged</c> fires while the config is still being registered, before
	/// <see cref="ModContent.GetInstance{T}"/> can resolve it.
	/// </remarks>
	public static void SetLifeCrystalExtractable(bool enabled)
	{
		ItemID.Sets.ExtractinatorMode[ItemID.LifeCrystal] = enabled ? ItemID.LifeCrystal : -1;
	}
}
