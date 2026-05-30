using System;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.UI.Checklist;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Camera;

/// <summary>
/// Drives the Cursor Focus camera ability. Each game tick (a fixed 60 Hz, so the easing is
/// frame-rate independent however high the user's FPS) it works out where the camera should sit
/// relative to the cursor and eases <see cref="SmoothedOffset"/> toward it. The lightweight
/// <see cref="CursorFocusModifier"/> just reads that value during the draw loop and applies it.
/// When the ability is switched off, locked, or a menu is up, the target becomes zero and the
/// camera glides smoothly back to centre rather than snapping.
/// </summary>
[Autoload(Side = ModSide.Client)]
public sealed class CursorFocusSystem : ModSystem
{
	/// <summary>The fully-smoothed offset (in world pixels) applied to the camera this frame.</summary>
	internal static Vector2 SmoothedOffset;

	private CursorFocusModifier _modifier;

	public override void OnWorldLoad()
	{
		SmoothedOffset = Vector2.Zero;
		_modifier = new CursorFocusModifier();
		Main.instance.CameraModifiers.Add(_modifier);
	}

	public override void OnWorldUnload()
	{
		_modifier?.Finish(); // the collection drops it once Finished
		_modifier = null;
		SmoothedOffset = Vector2.Zero;
	}

	public override void PostUpdateEverything()
	{
		ElementalHeartsCameraConfig cfg = ElementalHeartsCameraConfig.Instance;
		Vector2 target = ShouldPan(cfg) ? ComputeTargetOffset(cfg) : Vector2.Zero;

		// Per-tick easing factor: 0 smoothing snaps (factor 1); higher smoothing approaches a
		// gentle floor. The power curve keeps the slider feeling linear to the eye.
		float smooth01 = Math.Clamp(cfg.Smoothing / 100f, 0f, 1f);
		float factor = 0.03f + (0.97f * MathF.Pow(1f - smooth01, 5f));

		SmoothedOffset = Vector2.Lerp(SmoothedOffset, target, factor);

		// Kill the last sub-pixel of drift once we're heading home, so the camera truly rests.
		if (target == Vector2.Zero && SmoothedOffset.LengthSquared() < 0.01f)
			SmoothedOffset = Vector2.Zero;
	}

	/// <summary>Whether the camera is allowed to pan this tick, considering the ability state and current UI context.</summary>
	private static bool ShouldPan(ElementalHeartsCameraConfig cfg)
	{
		if (!CursorFocus.IsActive())
			return false;

		Player player = Main.LocalPlayer;
		if (player is null || player.dead)
			return false;

		// Hard recentres: anything that takes the cursor away from "aiming in the world".
		if (Main.mapFullscreen || Main.drawingPlayerChat || Main.gameMenu)
			return false;

		if (cfg.RecenterInMenus && (Main.playerInventory || Main.InGameUI?.CurrentState != null || IsChecklistOpen()))
			return false;

		// Optional: only while actually swinging or aiming an item.
		if (cfg.OnlyWhileUsingItem && !(player.channel || player.itemAnimation > 0 || player.controlUseItem))
			return false;

		return true;
	}

	private static bool IsChecklistOpen()
	{
		ChecklistUISystem sys = ModContent.GetInstance<ChecklistUISystem>();
		return sys?.ChecklistInterface?.CurrentState != null;
	}

	/// <summary>Maps the cursor's distance from screen centre into a camera offset, with deadzone, curve and per-axis scaling.</summary>
	private static Vector2 ComputeTargetOffset(ElementalHeartsCameraConfig cfg)
	{
		float halfW = Main.screenWidth / 2f;
		float halfH = Main.screenHeight / 2f;
		if (halfW <= 0f || halfH <= 0f)
			return Vector2.Zero;

		Vector2 fromCenter = Main.MouseScreen - new Vector2(halfW, halfH);
		float nx = Math.Clamp(fromCenter.X / halfW, -1f, 1f);
		float ny = Math.Clamp(fromCenter.Y / halfH, -1f, 1f);

		float deadzone = cfg.Deadzone / 100f;
		float shapedX = Shape(nx, deadzone, cfg.ResponseCurve);
		float shapedY = Shape(ny, deadzone, cfg.ResponseCurve);

		if (cfg.InvertHorizontal) shapedX = -shapedX;
		if (cfg.InvertVertical) shapedY = -shapedY;

		float max = cfg.MaxPanDistance;
		return new Vector2(
			shapedX * max * (cfg.HorizontalStrength / 100f),
			shapedY * max * (cfg.VerticalStrength / 100f));
	}

	/// <summary>
	/// Turns a normalised axis value (-1..1) into a deadzoned, eased -1..1 response. Values inside the
	/// deadzone return 0; beyond it the remaining travel is renormalised and raised to the response
	/// curve so the camera stays calm near centre and firms up toward the edge.
	/// </summary>
	private static float Shape(float n, float deadzone, float curve)
	{
		float magnitude = Math.Abs(n);
		if (magnitude <= deadzone)
			return 0f;

		float travel = (magnitude - deadzone) / (1f - deadzone);
		return MathF.Sign(n) * MathF.Pow(travel, curve);
	}
}
