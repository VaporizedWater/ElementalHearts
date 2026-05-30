using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

/// <summary>
/// Shared rendering / feedback helpers for the Rare Animate trio (master + enforcers),
/// factored out so the additive telegraph beam and the range-gated camera punch read
/// identically across all three bodies. Lifted from the conventions in
/// <see cref="UncommonAnimate"/> so the family stays visually consistent.
/// </summary>
internal static class AnimateFx
{
	/// <summary>Range-gated screen punch. No-ops for players too far away to feel it.</summary>
	public static void ShakeCamera(Vector2 center, float strength, float range, int frames, string id)
	{
		if (Main.LocalPlayer?.active != true) return;
		if (!Main.LocalPlayer.WithinRange(center, range)) return;
		Main.instance.CameraModifiers.Add(new PunchCameraModifier(center, Main.rand.NextVector2Unit(), strength, 6f, frames, range, id));
	}

	/// <summary>
	/// Draws the family's signature telegraph laser: a soft additive aura, a glowing muzzle
	/// orb, and a bright white core, all fading in with <paramref name="aimProgress"/> (0..1).
	/// Switches blend modes around itself and restores AlphaBlend on the way out.
	/// </summary>
	public static void DrawLaserBeam(SpriteBatch spriteBatch, Vector2 screenPos, Vector2 startWorld, Vector2 endWorld, Color baseTint, float aimProgress, float baseThickness)
	{
		Color baseColor = baseTint * aimProgress;
		Vector2 startPos = startWorld - screenPos;
		Vector2 endPos = endWorld - screenPos;

		Texture2D magicPixel = TextureAssets.MagicPixel.Value;
		Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
		Vector2 glowOrigin = new(32f, 32f);

		float angle = (endPos - startPos).ToRotation();
		float beamLength = 3000f;

		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		float auraThickness = baseThickness * 4f;
		Color auraColor = baseColor * 0.8f;
		spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), auraColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, auraThickness), SpriteEffects.None, 0f);
		spriteBatch.Draw(glowTex, startPos, null, auraColor, 0f, glowOrigin, auraThickness / 20f, SpriteEffects.None, 0f);

		float coreThickness = baseThickness * 1.5f;
		Color coreColor = Color.White * aimProgress;
		spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), coreColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, coreThickness), SpriteEffects.None, 0f);
		spriteBatch.Draw(glowTex, startPos, null, coreColor, 0f, glowOrigin, coreThickness / 20f, SpriteEffects.None, 0f);

		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
	}
}
