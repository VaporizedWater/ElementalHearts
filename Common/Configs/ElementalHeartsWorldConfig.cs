using System.ComponentModel;
using ElementalHearts.Common.Systems;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>Per-world maintenance actions for the consumed-heart registry.</summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsWorldConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsWorldConfig Instance => ModContent.GetInstance<ElementalHeartsWorldConfig>();

	/// <summary>
	/// Write-only trigger: enabling it wipes the world's consumed-heart registry and
	/// refunds the max life those hearts granted, then immediately resets — so the
	/// getter always reports false.
	/// </summary>
	[Header("ResetActions")]
	[DefaultValue(false)]
	public bool ClearHeartRegistry
	{
		get => false;
		set
		{
			if (value)
				HeartConsumptionWorld.ClearAllHearts();
		}
	}

	[DefaultValue(false)]
	public bool ClearElementalTier
	{
		get => false;
		set
		{
			if (value)
				AnimateProgressionSystem.ClearTier();
		}
	}

	// ── Worldgen ──────────────────────────────────────────────────────────────
	// Mini-biome generation only runs on world creation; toggling these on an existing
	// world has no retroactive effect. ReloadRequired so the GenPass insertion happens
	// at mod load — disabling avoids the pass running on a fresh world.
	[Header("Worldgen")]
	[DefaultValue(true)]
	[ReloadRequired]
	public bool GenerateLifeBiomes;

	[DefaultValue(1)] [Range(0, 10)] [Increment(1)] [Slider]
	public int SurfaceBiomeCountMultiplier;

	[DefaultValue(1)] [Range(0, 10)] [Increment(1)] [Slider]
	public int JungleBiomeCountMultiplier;
}
