using Microsoft.Xna.Framework;
using Terraria.Graphics.CameraModifiers;
using ElementalHearts.Common.Configs;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Common.Camera;

/// <summary>
/// Persistent camera modifier that nudges the view toward the cursor. It owns no logic of its own:
/// the offset is computed and smoothed once per game tick by <see cref="CursorFocusSystem"/> (so it
/// stays frame-rate independent) and simply applied here, additively, so it composes cleanly with
/// other modifiers such as the heart-consumption screen shake.
/// </summary>
internal sealed class CursorFocusModifier : ICameraModifier
{
	/// <summary>
	/// Adding a second modifier with this identity clears the first, so a stray double-add can never
	/// stack two cursor offsets.
	/// </summary>
	public string UniqueIdentity => "ElementalHearts:CursorFocus";

	/// <summary>Lives for the whole world session; <see cref="CursorFocusSystem"/> retires it on world unload.</summary>
	public bool Finished { get; private set; }

	private Vector2? _actualCameraPosition;

	public void Update(ref CameraInfo cameraInfo)
	{
		Vector2 targetPos = cameraInfo.CameraPosition + CursorFocusSystem.TargetOffset;

		if (!CursorFocus.IsActive())
		{
			_actualCameraPosition = null;
			return;
		}

		if (_actualCameraPosition == null)
		{
			_actualCameraPosition = targetPos;
		}

		// Calculate smoothing factor based on config
		float smooth01 = System.Math.Clamp(ElementalHeartsCameraConfig.Instance.Smoothing / 100f, 0f, 1f);
		float factor = 0.03f + (0.97f * System.MathF.Pow(1f - smooth01, 5f));

		_actualCameraPosition = Vector2.Lerp(_actualCameraPosition.Value, targetPos, factor);
		
		// Sub-pixel snapping
		if (Vector2.DistanceSquared(_actualCameraPosition.Value, targetPos) < 0.01f)
			_actualCameraPosition = targetPos;

		cameraInfo.CameraPosition = _actualCameraPosition.Value;
	}

	public void Finish() => Finished = true;
}
