using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ElementalHearts.Common.Configs;

/// <summary>
/// Client-side, per-player interface preferences and visual settings.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsClientConfig Instance => ModContent.GetInstance<ElementalHeartsClientConfig>();

	[Header("UI")]
	[SeparatePage]
	public UISettings UI = new UISettings();

	[Header("Camera")]
	[SeparatePage]
	public CameraSettings Camera = new CameraSettings();

	[Header("Idle")]
	[SeparatePage]
	public IdleSettings Idle = new IdleSettings();

	[Header("Tips")]
	[SeparatePage]
	public TipSettings Tips = new TipSettings();

	[Header("Visuals")]
	[SeparatePage]
	public VisualSettings Visuals = new VisualSettings();

	[Header("Abilities")]
	[SeparatePage]
	public AbilitySettings Abilities = new AbilitySettings();
}

public class UISettings
{
	[DefaultValue(true)]
	public bool EnableHeartChecklist { get; set; } = true;

	[DefaultValue(true)]
	public bool ShowDetailedHeartStats { get; set; } = true;

	[DefaultValue(true)]
	public bool EnableElementalHP { get; set; } = true;

	[DefaultValue(true)]
	public bool ShowPermanentBuffs { get; set; } = true;

	[DefaultValue(true)]
	public bool HideImpossibleHearts { get; set; } = true;

	[DefaultValue(false)]
	public bool DraggableUI;

	[DefaultValue(typeof(Vector2), "0, 0")]
	[Range(0f, 3840f)]
	[Increment(1f)]
	public Vector2 UIPosition;
}

public class AbilitySettings
{
	[DefaultValue(true)]
	public bool EnableJackOLanternDash { get; set; } = true;
}

public class CameraSettings
{
	[Header("General")]
	[DefaultValue(true)]
	public bool EnableCursorFocus;

	[Header("Strength")]
	[Range(0, 600)]
	[Increment(10)]
	[Slider]
	[DrawTicks]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(160)]
	public int MaxPanDistance;

	[Range(0, 200)]
	[Increment(5)]
	[Slider]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(100)]
	public int HorizontalStrength;

	[Range(0, 200)]
	[Increment(5)]
	[Slider]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(100)]
	public int VerticalStrength;

	[Header("Feel")]
	[Range(0, 100)]
	[Increment(5)]
	[Slider]
	[DrawTicks]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(35)]
	public int Smoothing;

	[Range(0, 90)]
	[Increment(5)]
	[Slider]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(10)]
	public int Deadzone;

	[Range(1f, 3f)]
	[Increment(0.1f)]
	[Slider]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(1.5f)]
	public float ResponseCurve;

	[Header("Behaviour")]
	[DefaultValue(false)]
	public bool OnlyWhileUsingItem;

	[DefaultValue(true)]
	public bool RecenterInMenus;

	[DefaultValue(false)]
	public bool InvertHorizontal;

	[DefaultValue(false)]
	public bool InvertVertical;
}

public class IdleSettings
{
	[Header("IdleGameSettings")]
	[DefaultValue(true)]
	public bool EnableIdleGame { get; set; } = true;

	[DefaultValue(50)] [Range(10, 1000)] [Increment(10)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int BaseCapacity { get; set; } = 50;

	[DefaultValue(50)] [Range(10, 500)] [Increment(10)] [Slider] [SliderColor(150, 230, 150, 255)]
	public int CapacityPerTier { get; set; } = 50;
}

public class TipSettings
{
	[Header("Tips")]
	[DefaultValue(true)]
	public bool EnableTips { get; set; } = true;
}

public class VisualSettings
{
	[Header("Effects")]
	[DefaultValue(3)]
	[Range(1, 5)]
	[Increment(1)]
	[Slider]
	[DrawTicks]
	[SliderColor(255, 130, 160, 255)]
	public int ConsumptionEffectStrength;
}
