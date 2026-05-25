using System.ComponentModel;
using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Maximum life each heart tier grants. Values are read live, so a change here
/// retroactively updates every character's heart bonus.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsHPConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsHPConfig Instance => ModContent.GetInstance<ElementalHeartsHPConfig>();

	[Header("CraftableTiers")]
	[DefaultValue(2)]  [Range(1, 20)]  [Increment(1)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int Common;

	[DefaultValue(4)]  [Range(2, 40)]  [Increment(1)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int Uncommon;

	[DefaultValue(6)]  [Range(3, 60)]  [Increment(1)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Rare;

	[DefaultValue(8)]  [Range(4, 80)]  [Increment(1)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int Epic;

	[DefaultValue(10)] [Range(5, 100)] [Increment(1)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int Legendary;

	[Header("BossTiers")]
	[DefaultValue(10)] [Range(5, 100)] [Increment(1)] [Slider] [SliderColor(255, 120, 110, 255)]
	public int Exotic;

	[DefaultValue(50)] [Range(25, 500)] [Increment(1)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int Mythic;

	/// <summary>
	/// Consumed hearts store only their id, so HP is read live from these values.
	/// Re-derive the local character's bonus whenever they change so already-consumed
	/// hearts retroactively grant the new amount.
	/// </summary>
	public override void OnChanged()
	{
		if (Main.netMode == NetmodeID.Server || Main.gameMenu)
			return;

		Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().RecomputeBonus();
	}
}
