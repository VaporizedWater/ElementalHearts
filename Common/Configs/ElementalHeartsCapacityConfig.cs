using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Server-side progression gating: the running cap on how much max-life bonus can be
/// banked from hearts at each step of boss progression, so health never outruns the
/// world. Read live by <see cref="Systems.HeartCapacitySystem"/>. The slider colours
/// climb the same pink→gold ladder as the heart tiers to read as a progression curve.
/// Labels and tooltips live in the localization file.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsCapacityConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	public static ElementalHeartsCapacityConfig Instance => ModContent.GetInstance<ElementalHeartsCapacityConfig>();

	[Header("ProgressionGates")]
	[DefaultValue(true)]
	public bool EnableProgressionGates;

	[DefaultValue(25)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 205, 218, 255)]
	public int PreBossCapacity;

	[DefaultValue(50)]  [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 225, 170, 255)]
	public int PostSkeletronCapacity;

	[DefaultValue(100)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int PostWoFCapacity;

	[DefaultValue(115)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(140, 200, 255, 255)]
	public int Post1MechCapacity;

	[DefaultValue(130)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(125, 185, 255, 255)]
	public int Post2MechsCapacity;

	[DefaultValue(150)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(110, 170, 255, 255)]
	public int Post3MechsCapacity;

	[DefaultValue(200)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(200, 130, 255, 255)]
	public int PostPlanteraCapacity;

	[DefaultValue(250)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(220, 150, 255, 255)]
	public int PostGolemCapacity;

	[DefaultValue(275)] [Range(10, 500)]  [Increment(5)] [Slider] [SliderColor(255, 200, 90, 255)]
	public int PostCultistCapacity;

	[DefaultValue(300)] [Range(10, 1000)] [Increment(5)] [Slider] [SliderColor(255, 235, 150, 255)]
	public int PostMoonLordCapacity;

	[DefaultValue(false)]
	public bool UnlimitedPostMoonLord;
}
