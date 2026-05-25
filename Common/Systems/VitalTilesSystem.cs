using ElementalHearts.Common.Configs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// Load-time wiring for the Vital Tiles system. Currently only manages Life Fruit
/// extractinator acceptance, mirroring how <see cref="LifeShards.LifeShardSystem"/>
/// handles Life Crystal.
/// </summary>
public sealed class VitalTilesSystem : ModSystem
{
	public override void PostSetupContent()
		=> SetLifeFruitExtractable(VitalTilesConfig.Instance.SystemEnabled);

	/// <summary>
	/// Toggles whether vanilla Life Fruit is accepted by the Extractinator. Disabling
	/// the Vital Tiles system reverses this so a Life Fruit can never be consumed for
	/// nothing once the seed feature is gone.
	/// </summary>
	public static void SetLifeFruitExtractable(bool enabled)
	{
		ItemID.Sets.ExtractinatorMode[ItemID.LifeFruit] = enabled ? ItemID.LifeFruit : -1;
	}
}
