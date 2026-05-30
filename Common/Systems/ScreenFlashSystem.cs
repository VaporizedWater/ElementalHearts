using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Systems;

/// <summary>
/// A faint, fire-and-forget full-screen colour wash used as a consumption flourish. It eases
/// smoothly in and then out on a short envelope (no hard on/off), client-only and cosmetic.
/// </summary>
public sealed class ScreenFlashSystem : ModSystem
{
	private static Color _color = Color.White;
	private static float _peak;    // crest alpha, 0..1
	private static float _age;     // frames since fired
	private static float _fadeIn;  // frames to ease up to the crest
	private static float _fadeOut; // frames to ease back down afterward

	private static ReLogic.Utilities.SlotId _trackedSound;
	private static float _trackedSoundPeak;

	/// <summary>
	/// Fire a wash. <paramref name="strength"/> is the crest alpha (0..1); the envelope is quick
	/// and smooth — <paramref name="fadeIn"/> frames up, then <paramref name="fadeOut"/> down.
	/// If <paramref name="trackedSound"/> is provided, its volume will be smoothly eased along the same envelope.
	/// </summary>
	public static void Flash(Color color, float strength, float fadeIn = 7f, float fadeOut = 20f, ReLogic.Utilities.SlotId trackedSound = default, float trackedSoundPeak = 1f)
	{
		if (Main.dedServ)
			return;

		// Let a stronger wash restart the envelope; never let a weaker one cut a brighter one short.
		if (strength < CurrentIntensity())
			return;

		_color = color;
		_peak = MathHelper.Clamp(strength, 0f, 1f);
		_fadeIn = MathHelper.Max(1f, fadeIn);
		_fadeOut = MathHelper.Max(1f, fadeOut);
		_age = 0f;
		
		_trackedSound = trackedSound;
		_trackedSoundPeak = trackedSoundPeak;
		
		UpdateTrackedSound();
	}

	private static float CurrentIntensity()
	{
		if (_peak <= 0f)
			return 0f;

		float t;
		if (_age <= _fadeIn)
		{
			t = _age / _fadeIn; // easing up
		}
		else
		{
			float outAge = _age - _fadeIn;
			if (outAge >= _fadeOut)
				return 0f; // envelope finished

			t = 1f - (outAge / _fadeOut); // easing down
		}

		// Smoothstep for a soft, polished crest with no hard corners.
		t = MathHelper.Clamp(t, 0f, 1f);
		return _peak * t * t * (3f - (2f * t));
	}

	public override void PostUpdateEverything()
	{
		if (_peak <= 0f)
			return;

		_age += 1f;
		if (_age >= _fadeIn + _fadeOut)
		{
			_peak = 0f; // done — go dormant
			if (_trackedSound.IsValid)
			{
				if (Terraria.Audio.SoundEngine.TryGetActiveSound(_trackedSound, out var sound))
					sound.Stop();
				_trackedSound = ReLogic.Utilities.SlotId.Invalid;
			}
		}
		else
		{
			UpdateTrackedSound();
		}
	}

	private static void UpdateTrackedSound()
	{
		if (!_trackedSound.IsValid)
			return;

		if (Terraria.Audio.SoundEngine.TryGetActiveSound(_trackedSound, out var sound))
		{
			// CurrentIntensity goes 0 -> _peak -> 0, so divide by _peak to get the 0..1 envelope progress.
			float t = _peak > 0f ? CurrentIntensity() / _peak : 0f;
			sound.Volume = _trackedSoundPeak * t;
		}
		else
		{
			_trackedSound = ReLogic.Utilities.SlotId.Invalid;
		}
	}

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		float intensity = CurrentIntensity();
		if (intensity <= 0f)
			return;

		Texture2D pixel = TextureAssets.MagicPixel.Value;
		Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
		spriteBatch.Draw(pixel, screen, _color * intensity);
	}
}
