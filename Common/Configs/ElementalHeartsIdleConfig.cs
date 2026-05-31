using System.ComponentModel;
using Terraria.ModLoader.Config;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Configs;

[BackgroundColor(15, 20, 35)]
public class ElementalHeartsIdleConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;

	public static ElementalHeartsIdleConfig Instance;

	[Header("IdleGameSettings")]
	
	[DefaultValue(true)]
	[Label("Enable Idle Game")]
	[Tooltip("If true, unlocked and consumed hearts will generate Life Shards over time.")]
	public bool EnableIdleGame { get; set; }

	[DefaultValue(50)]
	[Range(10, 1000)]
	[Label("Base Shard Capacity")]
	[Tooltip("The base maximum amount of unclaimed shards you can hold.")]
	public int BaseCapacity { get; set; }

	[DefaultValue(50)]
	[Range(10, 500)]
	[Label("Capacity Per World Tier")]
	[Tooltip("The additional maximum amount of unclaimed shards you can hold per World Tier.")]
	public int CapacityPerTier { get; set; }
}
