using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Configs;

/// <summary>
/// Client-side tuning for the Cursor Focus camera ability unlocked by the Magnification (Lens)
/// Heart. <see cref="EnableCursorFocus"/> is a global kill-switch; the everyday on/off toggle
/// lives in the Heart Log and is stored per character (see
/// <see cref="Common.Players.CursorFocusPlayer"/>). Everything here only changes the *feel* of
/// the pan, never whether a given character has it switched on.
/// </summary>
[BackgroundColor(40, 40, 40, 220)]
public sealed class ElementalHeartsCameraConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
	public static ElementalHeartsCameraConfig Instance => ModContent.GetInstance<ElementalHeartsCameraConfig>();

	// ── General ─────────────────────────────────────────────────────────────────────
	/// <summary>Global kill-switch. When off the camera never pans, regardless of the Heart Log toggle.</summary>
	[Header("General")]
	[DefaultValue(true)]
	public bool EnableCursorFocus;

	// ── Strength ────────────────────────────────────────────────────────────────────
	/// <summary>How far (in pixels) the camera shifts when the cursor is pushed all the way to a screen edge.</summary>
	[Header("Strength")]
	[Range(0, 600)]
	[Increment(10)]
	[Slider]
	[DrawTicks]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(160)]
	public int MaxPanDistance;

	/// <summary>Per-axis multiplier on the horizontal pan, as a percentage. 0 disables horizontal panning.</summary>
	[Range(0, 200)]
	[Increment(5)]
	[Slider]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(100)]
	public int HorizontalStrength;

	/// <summary>Per-axis multiplier on the vertical pan, as a percentage. 0 disables vertical panning.</summary>
	[Range(0, 200)]
	[Increment(5)]
	[Slider]
	[SliderColor(120, 200, 255, 255)]
	[DefaultValue(100)]
	public int VerticalStrength;

	// ── Feel ────────────────────────────────────────────────────────────────────────
	/// <summary>How floaty the follow is. 0 snaps instantly to the cursor; 100 drifts in lazily.</summary>
	[Header("Feel")]
	[Range(0, 100)]
	[Increment(5)]
	[Slider]
	[DrawTicks]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(35)]
	public int Smoothing;

	/// <summary>Central dead area (percent of the half-screen) the cursor can sit in before the camera reacts.</summary>
	[Range(0, 90)]
	[Increment(5)]
	[Slider]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(10)]
	public int Deadzone;

	/// <summary>
	/// Easing curve of the pan. 1 is linear; higher values keep the camera calmer near the centre and
	/// only ramp up as the cursor nears the edge.
	/// </summary>
	[Range(1f, 3f)]
	[Increment(0.1f)]
	[Slider]
	[SliderColor(170, 150, 255, 255)]
	[DefaultValue(1.5f)]
	public float ResponseCurve;

	// ── Behaviour ───────────────────────────────────────────────────────────────────
	/// <summary>When on, the camera only pans while you are actively swinging/aiming an item.</summary>
	[Header("Behaviour")]
	[DefaultValue(false)]
	public bool OnlyWhileUsingItem;

	/// <summary>When on, opening the inventory (or any of the mod's own menus) glides the camera back to centre.</summary>
	[DefaultValue(true)]
	public bool RecenterInMenus;

	/// <summary>Flip the horizontal pan direction.</summary>
	[DefaultValue(false)]
	public bool InvertHorizontal;

	/// <summary>Flip the vertical pan direction.</summary>
	[DefaultValue(false)]
	public bool InvertVertical;
}
