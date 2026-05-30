using Microsoft.Xna.Framework;
using Terraria.Graphics.CameraModifiers;

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

	public void Update(ref CameraInfo cameraInfo)
	{
		Vector2 offset = CursorFocusSystem.SmoothedOffset;
		if (offset != Vector2.Zero)
			cameraInfo.CameraPosition += offset;
	}

	public void Finish() => Finished = true;
}
