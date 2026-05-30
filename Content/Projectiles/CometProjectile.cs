using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Projectiles;

/// <summary>
/// Artillery comet lobbed upward by the Rare Animate master during the Phase 2
/// bullet-hell. Each comet launches with its own horizontal velocity and falls back
/// down under a custom (heavier-than-shard) gravity, so a burst of five fans out and
/// rains down across the arena in an arc the player has to weave through.
/// </summary>
public sealed class CometProjectile : ModProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/RareBossProjectile";

	private const float Gravity = 0.32f;
	private const float MaxFallSpeed = 17f;

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.width = 22;
		Projectile.height = 22;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 360;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false;   // bullet-hell readability — comets pass through terrain
		Projectile.scale = 1.3f;
	}

	public override void AI()
	{
		// Custom gravity arc: rises, decelerates, then accelerates back down to a capped fall.
		Projectile.velocity.Y += Gravity;
		if (Projectile.velocity.Y > MaxFallSpeed)
			Projectile.velocity.Y = MaxFallSpeed;

		// Point the sprite along its direction of travel.
		Projectile.rotation = Projectile.velocity.ToRotation();

		Lighting.AddLight(Projectile.Center, 0.25f, 0.45f, 0.95f);

		if (Main.rand.NextBool(2))
		{
			Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Projectile.velocity * -0.15f, 0, default, 1.1f);
			d.noGravity = true;
		}
	}

	public override Color? GetAlpha(Color lightColor) => Color.White;

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 origin = texture.Size() / 2f;

		// Additive afterimage tail — real alpha (intensity) so it actually renders.
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		for (int i = 1; i < Projectile.oldPos.Length; i++)
		{
			if (Projectile.oldPos[i] == Vector2.Zero) continue;
			Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
			float fade = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
			Color trail = new(80, 165, 255);
			trail.A = (byte)(150 * fade);
			Main.EntitySpriteDraw(texture, drawPos, null, trail, Projectile.rotation, origin, Projectile.scale * fade, SpriteEffects.None, 0);
		}
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		// Main sprite, full colour (GetAlpha returns white = full-bright).
		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
		return false;
	}
}
